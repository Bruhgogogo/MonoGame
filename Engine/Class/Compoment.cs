using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MonoEngine.Class
{
    public class Component : MonoObject
    {
        public GameObject Parent;

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

        public T GetOrAddComponent<T>() where T : Component, new()
        {
            return Parent?.GetOrAddComponent<T>();
        }

        public T GetComponent<T>() where T : Component
        {
            return Parent?.GetComponent<T>();
        }

        public List<T> GetComponents<T>() where T : Component
        {
            return Parent?.GetComponents<T>();
        }
    }
}