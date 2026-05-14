using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void LoadAimTraining()
    {
        SceneManager.LoadScene("AimTraining");
    }

    public void LoadPrefire()
    {
        SceneManager.LoadScene("Prefire");
    }
}
