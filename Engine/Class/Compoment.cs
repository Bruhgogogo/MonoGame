using System.Collections.Generic;

namespace MonoEngine.Class
{
    public class Component : MonoObject
    {
        public GameObject GO;

        private bool enabled = true;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;
                if (value) OnEnable();
                else OnDisable();
                enabled = value;
            }
        }

        public virtual void Awake() { }
        public virtual void OnDisable() { }
        public virtual void OnEnable() { }

        public T AddComponent<T>() where T : Component, new()
        {
            return GO?.AddComponent<T>();
        }

        public T GetOrAddComponent<T>() where T : Component, new()
        {
            return GO?.GetOrAddComponent<T>();
        }

        public T GetComponent<T>() where T : Component
        {
            return GO?.GetComponent<T>();
        }

        public List<T> GetComponents<T>() where T : Component
        {
            return GO?.GetComponents<T>();
        }

        public bool GetComponentInChildren<T>(out T component) where T : Component
        {
            component = null;
            return GO?.GetComponentInChildren(out component) ?? false;
        }

        public GameObject FindChild(string path)
        {
            return GO?.FindChild(path);
        }
    }
}