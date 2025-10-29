using UnityEditor;
using UnityEngine;

public class Level_select : MonoBehaviour
{
    [SerializeField]
    SceneAsset levelToLoad;

    private bool playerInside = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = false;
        }
    }

    private void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            if (levelToLoad != null)
            {
                Debug.Log("Loading level: " + levelToLoad.name);
                LoadLevel(levelToLoad.name);
            }
            else
            {
                Debug.LogWarning("No level assigned to Level_select on " + gameObject.name);
            }
        }
    }

    public void LoadLevel(string levelName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
    }
}
