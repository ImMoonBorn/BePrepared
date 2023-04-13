using MoonBorn.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestScript : Singleton<TestScript>
{
    private static int TestStaticInt = 0;
    private int TestInt = 0;
    public string SceneName = "TestScene";
    public string LoadScene = "TestScene2";

    private void Awake()
    {
        print(SceneName + ": " + TestInt);
        print(SceneName + ": Static : " + TestStaticInt);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            TestInt++;
            TestStaticInt++;
            SceneManager.LoadScene(LoadScene);
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            TestInt += 2;
            TestStaticInt += 2;
            SceneManager.LoadScene(LoadScene);
        }
    }
}
