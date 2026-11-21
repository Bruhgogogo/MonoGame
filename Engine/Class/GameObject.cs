using System.Collections.Generic;

namespace MonoEngine.Class
{
    public sealed class GameObject : MonoObject
    {
        private List<Component> components = new List<Component>();
        public string Name = "GameObject";
        public bool ActiveSelf = true;

        public T AddComponent<T>() where T : Component, new()
        {
            T component = new T();
            component.GameObject = this;
            components.Add(component);
            component.Awake();
            return component;
        }

        public T GetOrAddComponent<T>() where T : Component, new()
        {
            T component = GetComponent<T>();
            if (component == null)
                component = AddComponent<T>();
            return component;
        }

        public T GetComponent<T>() where T : Component
        {
            foreach (var comp in components)
                if (comp is T t && comp)
                    return t;
            return null;
        }

        public List<T> GetComponents<T>() where T : Component
        {
            var result = new List<T>();
            foreach (var comp in components)
                if (comp is T t && comp)
                    result.Add(t);
            return result;
        }

        public override void OnDestroy()
        {
            foreach (var comp in components)
            {
                if (comp)
                {
                    if (comp.Enabled) comp.OnDisable();
                    comp.OnDestroy();
                }
            }
            components.Clear();
        }
    }
}