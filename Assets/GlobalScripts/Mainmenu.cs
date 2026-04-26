using UnityEngine;
using UnityEngine.SceneManagement;
public class Mainmenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   public void ClickStart()
    {
        SceneManager.LoadScene("DLevel1");
    }   

    public void ClickExit()
        {
            Application.Quit();
    }

    public void ClickManual()
    {
        SceneManager.LoadScene("Manual");
    }
}
