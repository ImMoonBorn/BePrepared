using UnityEngine;

namespace MoonBorn.Utils
{
    public class SingletonManager : MonoBehaviour
    {
        private void Awake()
        {
            var singletons = FindObjectsOfType(typeof(Singleton));

            foreach (var s in singletons)
            {
                Singleton singleton = s as Singleton;
                singleton.Init();
            }
        }
    }
}
