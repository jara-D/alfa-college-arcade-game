using UnityEngine;
using UnityEngine.SceneManagement;

public class Level_select : MonoBehaviour
{

    [SerializeField]
    private string levelToLoadName;

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
            if (!string.IsNullOrEmpty(levelToLoadName))
            {
                LoadLevel(levelToLoadName);
            }
            else
            {
                // No level assigned
            }
        }
    }

    public void LoadLevel(string levelName)
    {
        if (string.IsNullOrEmpty(levelName))
        {
            Debug.LogWarning("LoadLevel called with empty scene name on " + gameObject.name);
            return;
        }

        SceneManager.LoadScene(levelName);
    }
}
