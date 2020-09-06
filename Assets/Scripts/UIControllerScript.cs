using UnityEngine;
using UnityEngine.SceneManagement;

public class UIControllerScript : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject resumeBtn;
    [SerializeField] private GameObject levelClearTxt;
    [SerializeField] private GameObject buttonsArrow;

    private Scene currActiveScene;

    private bool isPaused = false;

    private void Start()
    {
        currActiveScene = SceneManager.GetActiveScene();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            
            if (isPaused)
                pauseGame();
            else
                resumeGame();
        }
    }

    public void pauseGame()
    {
        Time.timeScale = 0;
        pausePanel.SetActive(true);
    }
    
    public void resumeGame()
    {
        Time.timeScale = 1;
        pausePanel.SetActive(false);
    }
    
    public void restartGame()
    {  
        Time.timeScale = 1;
        SceneManager.LoadScene(currActiveScene.name);
    }

    public void GoToScene(int buildIndex)
    {
        SceneManager.LoadScene(buildIndex);
    }
    
    public void endGame()
    {
        pausePanel.SetActive(true);
        resumeBtn.SetActive(false);

        buttonsArrow.SetActive(true);

        levelClearTxt.SetActive(true);
    }
}
