using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[System.Serializable]
public class TileMapping
{
    [Header("Tile Pair")]
    [Tooltip("The default/turned off tile")]
    public TileBase defaultTile;
    
    [Tooltip("The colored/turned on tile to replace it with")]
    public TileBase coloredTile;
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
    
    private bool hasChangedTiles = false;
    private Dictionary<TileBase, TileBase> tileMap;
    
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
        
        // Build tile mapping dictionary for fast lookup
        BuildTileMappingDictionary();
        
        // Start checking terminals if auto-check is enabled
        if (autoCheckTerminals)
        {
            InvokeRepeating(nameof(CheckTerminalStatus), 1f, checkInterval);
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"TilemapChanger initialized with {tileMappings.Count} tile mappings on tilemap: {targetTilemap.name}");
        }
    }
    
    private void BuildTileMappingDictionary()
    {
        tileMap = new Dictionary<TileBase, TileBase>();
        
        foreach (TileMapping mapping in tileMappings)
        {
            if (mapping.defaultTile != null && mapping.coloredTile != null)
            {
                if (!tileMap.ContainsKey(mapping.defaultTile))
                {
                    tileMap.Add(mapping.defaultTile, mapping.coloredTile);
                }
                else
                {
                    Debug.LogWarning($"TilemapChanger: Duplicate mapping for tile {mapping.defaultTile.name}. Skipping duplicate.");
                }
            }
            else
            {
                Debug.LogWarning("TilemapChanger: Found tile mapping with null tiles. Please assign both default and colored tiles.");
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"Built tile mapping dictionary with {tileMap.Count} valid mappings");
        }
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
    /// Manually trigger the tile change (useful for testing or other triggers)
    /// </summary>
    [ContextMenu("Change Tiles to Colored")]
    public void ChangeTilesToColored()
    {
        if (targetTilemap == null)
        {
            Debug.LogError("TilemapChanger: No tilemap assigned!");
            return;
        }
        
        if (tileMap == null || tileMap.Count == 0)
        {
            Debug.LogWarning("TilemapChanger: No tile mappings configured!");
            return;
        }
        
        int changedTileCount = 0;
        
        // Get the bounds of the tilemap
        BoundsInt bounds = targetTilemap.cellBounds;
        
        // Create arrays to store positions and new tiles
        List<Vector3Int> positionsToChange = new List<Vector3Int>();
        List<TileBase> newTiles = new List<TileBase>();
        
        // Iterate through all tiles in the tilemap
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            TileBase currentTile = targetTilemap.GetTile(position);
            
            // Check if this tile should be replaced
            if (currentTile != null && tileMap.ContainsKey(currentTile))
            {
                positionsToChange.Add(position);
                newTiles.Add(tileMap[currentTile]);
                changedTileCount++;
            }
        }
        
        // Apply all tile changes at once for better performance
        if (positionsToChange.Count > 0)
        {
            targetTilemap.SetTilesBlock(new BoundsInt(0, 0, 0, 1, 1, 1), newTiles.ToArray());
            
            // Alternative method - set tiles individually (use if SetTilesBlock doesn't work)
            for (int i = 0; i < positionsToChange.Count; i++)
            {
                targetTilemap.SetTile(positionsToChange[i], newTiles[i]);
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"TilemapChanger: Changed {changedTileCount} tiles to their colored versions");
        }
        
        hasChangedTiles = true;
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
        
        if (tileMap == null || tileMap.Count == 0)
        {
            Debug.LogWarning("TilemapChanger: No tile mappings configured!");
            return;
        }
        
        int changedTileCount = 0;
        
        // Get the bounds of the tilemap
        BoundsInt bounds = targetTilemap.cellBounds;
        
        // Create arrays to store positions and new tiles
        List<Vector3Int> positionsToChange = new List<Vector3Int>();
        List<TileBase> newTiles = new List<TileBase>();
        
        // Create reverse mapping (colored -> default)
        Dictionary<TileBase, TileBase> reverseTileMap = new Dictionary<TileBase, TileBase>();
        foreach (var kvp in tileMap)
        {
            reverseTileMap[kvp.Value] = kvp.Key;
        }
        
        // Iterate through all tiles in the tilemap
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            TileBase currentTile = targetTilemap.GetTile(position);
            
            // Check if this colored tile should be reset to default
            if (currentTile != null && reverseTileMap.ContainsKey(currentTile))
            {
                positionsToChange.Add(position);
                newTiles.Add(reverseTileMap[currentTile]);
                changedTileCount++;
            }
        }
        
        // Apply all tile changes at once
        for (int i = 0; i < positionsToChange.Count; i++)
        {
            targetTilemap.SetTile(positionsToChange[i], newTiles[i]);
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"TilemapChanger: Reset {changedTileCount} tiles to their default versions");
        }
        
        hasChangedTiles = false;
        
        // Restart terminal checking if auto-check is enabled
        if (autoCheckTerminals && !IsInvoking(nameof(CheckTerminalStatus)))
        {
            InvokeRepeating(nameof(CheckTerminalStatus), 1f, checkInterval);
        }
    }
    
    /// <summary>
    /// Add a tile mapping programmatically
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
        tileMappings.Add(new TileMapping { defaultTile = defaultTile, coloredTile = coloredTile });
        
        // Add to the runtime dictionary
        if (tileMap != null && !tileMap.ContainsKey(defaultTile))
        {
            tileMap.Add(defaultTile, coloredTile);
        }
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
            if (tileMappings[i].defaultTile == null || tileMappings[i].coloredTile == null)
            {
                Debug.LogWarning($"TilemapChanger: Tile mapping {i} has null tiles. Please assign both tiles or remove the mapping.");
            }
        }
    }
}