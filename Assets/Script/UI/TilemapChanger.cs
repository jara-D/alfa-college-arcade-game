using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[System.Serializable]
public class TileMapping
{
    [Header("Default Tile (What to Replace)")]
    [Tooltip("The default/turned off tile (can be TileBase or prefab)")]
    public TileBase defaultTile;
    
    [Tooltip("The default/turned off prefab (alternative to TileBase above)")]
    public GameObject defaultPrefab;
    
    [Header("Colored Tile (What to Replace With)")]
    [Tooltip("The colored/turned on tile (can be TileBase or prefab)")]
    public TileBase coloredTile;
    
    [Tooltip("The colored/turned on prefab (alternative to TileBase above)")]
    public GameObject coloredPrefab;
    
    /// <summary>
    /// Get the effective default tile, prioritizing TileBase over prefab
    /// </summary>
    public TileBase GetDefaultTile()
    {
        if (defaultTile != null)
        {
            return defaultTile;
        }
        
        if (defaultPrefab != null)
        {
            return CreateTileFromPrefab(defaultPrefab);
        }
        
        return null;
    }
    
    /// <summary>
    /// Get the effective colored tile, prioritizing TileBase over prefab
    /// </summary>
    public TileBase GetColoredTile()
    {
        if (coloredTile != null)
        {
            return coloredTile;
        }
        
        if (coloredPrefab != null)
        {
            return CreateTileFromPrefab(coloredPrefab);
        }
        
        return null;
    }
    
    /// <summary>
    /// Creates a tile from a prefab GameObject
    /// </summary>
    private TileBase CreateTileFromPrefab(GameObject prefab)
    {
        if (prefab == null) return null;
        
        // First, check if the prefab itself is a TileBase (for Tile assets made into prefabs)
        TileBase tileComponent = prefab.GetComponent<TileBase>();
        if (tileComponent != null)
        {
            return tileComponent;
        }
        
        // Check if any child has a TileBase component
        tileComponent = prefab.GetComponentInChildren<TileBase>();
        if (tileComponent != null)
        {
            return tileComponent;
        }
        
        // For GameObjects that aren't TileBase components, we need to create a Sprite tile
        // and use the prefab's sprite
        SpriteRenderer spriteRenderer = prefab.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = prefab.GetComponentInChildren<SpriteRenderer>();
        }
        
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            // Create a new Sprite tile using the prefab's sprite
            UnityEngine.Tilemaps.Tile newTile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
            newTile.sprite = spriteRenderer.sprite;
            
            Debug.Log($"Created Sprite tile from prefab {prefab.name} using sprite {spriteRenderer.sprite.name}");
            
            return newTile;
        }
        
        Debug.LogWarning($"Prefab {prefab.name} doesn't have a TileBase component or SpriteRenderer with sprite. Cannot create tile from prefab.");
        return null;
    }
    
    /// <summary>
    /// Validate this tile mapping
    /// </summary>
    public bool IsValid()
    {
        // At least one default source (TileBase or prefab)
        bool hasDefault = defaultTile != null || defaultPrefab != null;
        
        // At least one colored source (TileBase or prefab)  
        bool hasColored = coloredTile != null || coloredPrefab != null;
        
        return hasDefault && hasColored;
    }
    
    /// <summary>
    /// Get description of this mapping for debugging
    /// </summary>
    public string GetDescription()
    {
        string defaultSource = defaultTile != null ? $"TileBase({defaultTile.name})" : 
                              defaultPrefab != null ? $"Prefab({defaultPrefab.name})" : "None";
        
        string coloredSource = coloredTile != null ? $"TileBase({coloredTile.name})" : 
                              coloredPrefab != null ? $"Prefab({coloredPrefab.name})" : "None";
        
        return $"{defaultSource} -> {coloredSource}";
    }
}

public class TilemapChanger : MonoBehaviour
{
    [Header("Tilemap Reference")]
    [SerializeField] private Tilemap targetTilemap;
    
    [Header("Tile Mappings")]
    [Tooltip("List of tile pairs - default tiles will be replaced with colored tiles")]
    [SerializeField] private List<TileMapping> tileMappings = new List<TileMapping>();
    
    [Header("Terminal Integration")]
    [Tooltip("Check for terminal completion automatically")]
    [SerializeField] private bool autoCheckTerminals = true;
    
    [Tooltip("How often to check terminal status (in seconds)")]
    [SerializeField] private float checkInterval = 1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Performance")]
    [Tooltip("Preload all prefabs at start to avoid lag spikes during gameplay")]
    [SerializeField] private bool preloadPrefabs = true;
    
    private bool hasChangedTiles = false;
    
    // Dictionary to store preloaded prefabs: position -> instantiated GameObject
    private Dictionary<Vector3Int, GameObject> preloadedPrefabs = new Dictionary<Vector3Int, GameObject>();
    
    // Precomputed tile change data for instant execution
    private List<Vector3Int> tilePositionsToChange = new List<Vector3Int>();
    private List<TileBase> newTilesForPositions = new List<TileBase>();
    private List<TileBase> oldTilesForPositions = new List<TileBase>();
    
    void Start()
    {
        // Get tilemap component if not assigned
        if (targetTilemap == null)
        {
            targetTilemap = GetComponent<Tilemap>();
        }
        
        // Validate setup
        if (targetTilemap == null)
        {
            Debug.LogError($"TilemapChanger on {gameObject.name}: No Tilemap found! Please assign a Tilemap or attach this script to a GameObject with a Tilemap component.");
            enabled = false;
            return;
        }
        
        // Start checking terminals if auto-check is enabled
        if (autoCheckTerminals)
        {
            InvokeRepeating(nameof(CheckTerminalStatus), 1f, checkInterval);
        }
        
        // Preload prefabs to avoid lag spikes during gameplay
        if (preloadPrefabs)
        {
            PreloadAllPrefabs();
        }

        if (showDebugLogs)
        {
            Debug.Log($"TilemapChanger initialized with {tileMappings.Count} tile mappings on tilemap: {targetTilemap.name}");
        }
    }
    
    /// <summary>
    /// Preload all prefabs that will be needed, but keep them disabled
    /// Also precompute tile change positions for instant execution
    /// </summary>
    private void PreloadAllPrefabs()
    {
        if (targetTilemap == null || tileMappings == null || tileMappings.Count == 0)
        {
            return;
        }
        
        Debug.Log($"=== PRELOADING PREFABS AND PRECOMPUTING TILE CHANGES ===");
        
        BoundsInt bounds = targetTilemap.cellBounds;
        int preloadedCount = 0;
        int precomputedTileCount = 0;
        
        // Clear previous precomputed data
        tilePositionsToChange.Clear();
        newTilesForPositions.Clear();
        oldTilesForPositions.Clear();
        
        // Iterate through all tiles in the tilemap
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            TileBase currentTile = targetTilemap.GetTile(position);
            
            if (currentTile != null)
            {
                // Check each mapping to see if this tile will need changes
                foreach (TileMapping mapping in tileMappings)
                {
                    if (mapping.IsValid())
                    {
                        TileBase defaultTile = mapping.GetDefaultTile();
                        
                        // If this tile matches a mapping
                        if (defaultTile != null && currentTile.name == defaultTile.name)
                        {
                            if (mapping.coloredPrefab != null)
                            {
                                // Preload prefab for this position
                                Vector3 worldPosition = targetTilemap.CellToWorld(position);
                                worldPosition.x += targetTilemap.cellSize.x * 0.5f;
                                worldPosition.y += targetTilemap.cellSize.y * 0.5f;
                                
                                GameObject preloadedPrefab = Instantiate(mapping.coloredPrefab, worldPosition, Quaternion.identity);
                                preloadedPrefab.transform.SetParent(transform);
                                preloadedPrefab.SetActive(false);
                                
                                preloadedPrefabs[position] = preloadedPrefab;
                                preloadedCount++;
                                
                                if (showDebugLogs && preloadedCount <= 3) // Limit debug spam
                                {
                                    Debug.Log($"Preloaded prefab {mapping.coloredPrefab.name} at {position}");
                                }
                            }
                            else if (mapping.coloredTile != null)
                            {
                                // Precompute tile change data
                                tilePositionsToChange.Add(position);
                                newTilesForPositions.Add(mapping.coloredTile);
                                oldTilesForPositions.Add(currentTile);
                                precomputedTileCount++;
                                
                                if (showDebugLogs && precomputedTileCount <= 3) // Limit debug spam
                                {
                                    Debug.Log($"Precomputed tile change at {position}: {currentTile.name} -> {mapping.coloredTile.name}");
                                }
                            }
                            
                            break; // Found a match, no need to check other mappings
                        }
                    }
                }
            }
        }
        
        Debug.Log($"=== PRELOADING COMPLETE: {preloadedCount} prefabs preloaded, {precomputedTileCount} tile changes precomputed ===");
    }
    

    private void CheckTerminalStatus()
    {
        // Only check if we haven't already changed the tiles
        if (hasChangedTiles) return;
        
        // Check if all terminals are completed using the static method from PowerTerminalMinigame
        if (PowerTerminalMinigame.AreAllTerminalsCompleted())
        {
            if (showDebugLogs)
            {
                Debug.Log("All terminals completed! Changing tilemap to colored tiles...");
            }
            
            ChangeTilesToColored();
            
            // Stop checking since we've completed the change
            CancelInvoke(nameof(CheckTerminalStatus));
            hasChangedTiles = true;
        }
    }
    
    /// <summary>
    /// Instantly trigger the tile change using precomputed data (no lag!)
    /// </summary>
    [ContextMenu("Change Tiles to Colored")]
    public void ChangeTilesToColored()
    {
        if (targetTilemap == null)
        {
            Debug.LogError("TilemapChanger: No tilemap assigned!");
            return;
        }
        
        Debug.Log($"=== INSTANT TILE CHANGE PROCESS ===");
        
        int changedTileCount = 0;
        
        // Step 1: Apply precomputed tile changes using batch operation
        if (tilePositionsToChange.Count > 0)
        {
            targetTilemap.SetTilesBlock(new BoundsInt(0, 0, 0, 1, 1, 1), new TileBase[0]); // Clear any cache
            
            // Use SetTiles for batch operation - much faster than individual SetTile calls
            TileBase[] tilesToSet = newTilesForPositions.ToArray();
            Vector3Int[] positionsArray = tilePositionsToChange.ToArray();
            
            targetTilemap.SetTiles(positionsArray, tilesToSet);
            changedTileCount += tilePositionsToChange.Count;
            
            Debug.Log($"✓ BATCH APPLIED: {tilePositionsToChange.Count} tiles changed instantly");
        }
        
        // Step 2: Activate preloaded prefabs (remove tiles first)
        foreach (var kvp in preloadedPrefabs)
        {
            Vector3Int position = kvp.Key;
            GameObject prefab = kvp.Value;
            
            if (prefab != null)
            {
                // Remove the tile and activate the prefab
                targetTilemap.SetTile(position, null);
                prefab.SetActive(true);
                changedTileCount++;
            }
        }
        
        if (preloadedPrefabs.Count > 0)
        {
            Debug.Log($"✓ PREFABS ACTIVATED: {preloadedPrefabs.Count} prefabs activated instantly");
        }
        
        Debug.Log($"=== INSTANT CHANGE COMPLETE: {changedTileCount} total changes applied ===");
        
        hasChangedTiles = true;
    }

    /// <summary>
    /// Debug method to examine what's currently in the tilemap
    /// </summary>
    [ContextMenu("Debug Current Tilemap")]
    public void DebugCurrentTilemap()
    {
        if (targetTilemap == null)
        {
            Debug.LogError("No tilemap assigned!");
            return;
        }
        
        Debug.Log($"=== DEBUGGING TILEMAP: {targetTilemap.name} ===");
        
        BoundsInt bounds = targetTilemap.cellBounds;
        Debug.Log($"Tilemap bounds: {bounds}");
        
        Dictionary<string, int> tileCount = new Dictionary<string, int>();
        int totalTiles = 0;
        
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            TileBase tile = targetTilemap.GetTile(position);
            if (tile != null)
            {
                totalTiles++;
                string tileName = tile.name;
                
                if (tileCount.ContainsKey(tileName))
                {
                    tileCount[tileName]++;
                }
                else
                {
                    tileCount[tileName] = 1;
                    Debug.Log($"Found tile type: {tileName} (Type: {tile.GetType().Name}, Instance: {tile.GetInstanceID()})");
                }
            }
        }
        
        Debug.Log($"Total tiles found: {totalTiles}");
        Debug.Log("Tile distribution:");
        foreach (var kvp in tileCount)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value} instances");
        }
        
        Debug.Log($"Available mappings: {tileMappings?.Count ?? 0}");
        if (tileMappings != null)
        {
            foreach (var mapping in tileMappings)
            {
                Debug.Log($"  Mapping: {mapping.GetDescription()}");
            }
        }
    }
    
    /// <summary>
    /// Reset tiles back to default state (useful for testing)
    /// </summary>
    [ContextMenu("Reset Tiles to Default")]
    public void ResetTilesToDefault()
    {
        if (targetTilemap == null)
        {
            Debug.LogError("TilemapChanger: No tilemap assigned!");
            return;
        }
        
        Debug.Log($"=== RESETTING TILES TO DEFAULT ===");
        
        int changedTileCount = 0;
        int disabledPrefabCount = 0;
        int destroyedPrefabCount = 0;
        
        // First, disable all preloaded prefabs
        foreach (var kvp in preloadedPrefabs)
        {
            if (kvp.Value != null)
            {
                kvp.Value.SetActive(false);
                disabledPrefabCount++;
            }
        }
        
        if (tileMappings != null && tileMappings.Count > 0)
        {
            // Get the bounds of the tilemap
            BoundsInt bounds = targetTilemap.cellBounds;
            
            // Create arrays to store positions and new tiles
            List<Vector3Int> positionsToChange = new List<Vector3Int>();
            List<TileBase> newTiles = new List<TileBase>();
            
            // Also destroy any non-preloaded instantiated prefabs (children of this transform that aren't in our preloaded dictionary)
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                
                // Check if this child is one of our preloaded prefabs
                bool isPreloaded = false;
                foreach (var kvp in preloadedPrefabs)
                {
                    if (kvp.Value == child)
                    {
                        isPreloaded = true;
                        break;
                    }
                }
                
                // If it's not preloaded, destroy it (it was instantiated during runtime)
                if (!isPreloaded)
                {
                    DestroyImmediate(child);
                    destroyedPrefabCount++;
                }
            }
            
            // Iterate through all tiles in the tilemap to reset colored tiles back to default
            foreach (Vector3Int position in bounds.allPositionsWithin)
            {
                TileBase currentTile = targetTilemap.GetTile(position);
                
                if (currentTile != null)
                {
                    // Check each mapping to see if this colored tile should be reset to default
                    foreach (TileMapping mapping in tileMappings)
                    {
                        if (mapping.IsValid())
                        {
                            // Check if current tile matches the colored tile in this mapping
                            if (mapping.coloredTile != null && currentTile.name == mapping.coloredTile.name)
                            {
                                TileBase defaultTile = mapping.GetDefaultTile();
                                if (defaultTile != null)
                                {
                                    positionsToChange.Add(position);
                                    newTiles.Add(defaultTile);
                                    changedTileCount++;
                                }
                                break; // Found a match, no need to check other mappings
                            }
                        }
                    }
                }
            }
            
            // Apply all tile changes at once
            for (int i = 0; i < positionsToChange.Count; i++)
            {
                targetTilemap.SetTile(positionsToChange[i], newTiles[i]);
            }
        }
        
        Debug.Log($"=== RESET COMPLETE: {changedTileCount} tiles reset, {disabledPrefabCount} prefabs disabled, {destroyedPrefabCount} runtime prefabs destroyed ===");
        
        hasChangedTiles = false;
        
        // Restart terminal checking if auto-check is enabled
        if (autoCheckTerminals && !IsInvoking(nameof(CheckTerminalStatus)))
        {
            InvokeRepeating(nameof(CheckTerminalStatus), 1f, checkInterval);
        }
    }
    
    /// <summary>
    /// Add a regular tile mapping programmatically
    /// </summary>
    /// <param name="defaultTile">The tile to replace</param>
    /// <param name="coloredTile">The tile to replace it with</param>
    public void AddTileMapping(TileBase defaultTile, TileBase coloredTile)
    {
        if (defaultTile == null || coloredTile == null)
        {
            Debug.LogWarning("TilemapChanger: Cannot add tile mapping with null tiles");
            return;
        }
        
        // Add to the serialized list
        tileMappings.Add(new TileMapping 
        { 
            defaultTile = defaultTile, 
            coloredTile = coloredTile 
        });
    }
    
    /// <summary>
    /// Add a mixed tile mapping programmatically (TileBase to prefab or vice versa)
    /// </summary>
    /// <param name="defaultTile">The default tile (can be null if using defaultPrefab)</param>
    /// <param name="defaultPrefab">The default prefab (can be null if using defaultTile)</param>
    /// <param name="coloredTile">The colored tile (can be null if using coloredPrefab)</param>
    /// <param name="coloredPrefab">The colored prefab (can be null if using coloredTile)</param>
    public void AddMixedMapping(TileBase defaultTile, GameObject defaultPrefab, TileBase coloredTile, GameObject coloredPrefab)
    {
        TileMapping newMapping = new TileMapping
        {
            defaultTile = defaultTile,
            defaultPrefab = defaultPrefab,
            coloredTile = coloredTile,
            coloredPrefab = coloredPrefab
        };
        
        if (!newMapping.IsValid())
        {
            Debug.LogWarning("TilemapChanger: Cannot add invalid mapping. Must have at least one default and one colored tile/prefab.");
            return;
        }
        
        tileMappings.Add(newMapping);
    }
    
    /// <summary>
    /// Add a prefab tile mapping programmatically
    /// </summary>
    /// <param name="defaultPrefab">The prefab to replace</param>
    /// <param name="coloredPrefab">The prefab to replace it with</param>
    public void AddPrefabMapping(GameObject defaultPrefab, GameObject coloredPrefab)
    {
        AddMixedMapping(null, defaultPrefab, null, coloredPrefab);
    }
    
    /// <summary>
    /// Get the current tile change status
    /// </summary>
    /// <returns>True if tiles have been changed to colored versions</returns>
    public bool HasChangedToColored()
    {
        return hasChangedTiles;
    }
    
    /// <summary>
    /// Manually set the terminal checking state
    /// </summary>
    /// <param name="enabled">Whether to automatically check terminals</param>
    public void SetAutoCheckTerminals(bool enabled)
    {
        autoCheckTerminals = enabled;
        
        if (enabled && !hasChangedTiles && !IsInvoking(nameof(CheckTerminalStatus)))
        {
            InvokeRepeating(nameof(CheckTerminalStatus), 1f, checkInterval);
        }
        else if (!enabled && IsInvoking(nameof(CheckTerminalStatus)))
        {
            CancelInvoke(nameof(CheckTerminalStatus));
        }
    }
    
    void OnValidate()
    {
        // Validate tile mappings in the inspector
        for (int i = tileMappings.Count - 1; i >= 0; i--)
        {
            TileMapping mapping = tileMappings[i];
            
            if (!mapping.IsValid())
            {
                Debug.LogWarning($"TilemapChanger: Tile mapping {i} is invalid. {mapping.GetDescription()}. Please assign at least one default and one colored tile/prefab.");
            }
            
            // Additional validation for prefab components
            if (mapping.defaultPrefab != null && mapping.defaultPrefab.GetComponent<TileBase>() == null && mapping.defaultPrefab.GetComponentInChildren<TileBase>() == null)
            {
                Debug.LogWarning($"TilemapChanger: Default prefab '{mapping.defaultPrefab.name}' in mapping {i} doesn't have a TileBase component. This may cause issues.");
            }
            
            if (mapping.coloredPrefab != null && mapping.coloredPrefab.GetComponent<TileBase>() == null && mapping.coloredPrefab.GetComponentInChildren<TileBase>() == null)
            {
                Debug.LogWarning($"TilemapChanger: Colored prefab '{mapping.coloredPrefab.name}' in mapping {i} doesn't have a TileBase component. This may cause issues.");
            }
        }
    }
}