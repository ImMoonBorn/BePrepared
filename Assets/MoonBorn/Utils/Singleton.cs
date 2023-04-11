using UnityEngine;

namespace MoonBorn.Utils
{
    public abstract class Singleton : MonoBehaviour
    {
        public abstract void Init();
    }

    public class Singleton<T> : Singleton where T : Component
    {
        public static T Instance => s_Instance;
        private static T s_Instance;

        public override void Init()
        {
            if (!s_Instance)
                s_Instance = this as T;
            else
            {
                Debug.LogWarning(typeof(T) + " tried to create another Instance");
                DestroyImmediate(this);
            }
        }
    }
}
