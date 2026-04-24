using UnityEngine.SceneManagement;

namespace PrismPanic.Core
{
    /// <summary>
    /// Static scene loading helper. Only two scenes: Menu (0) and Main (1).
    /// </summary>
    public static class SceneController
    {
        public static void LoadMenu()
        {
            EventBus.ClearAll();
            SceneManager.LoadScene(0);
        }

        public static void LoadMain()
        {
            EventBus.ClearAll();
            SceneManager.LoadScene(1);
        }
    }
}
