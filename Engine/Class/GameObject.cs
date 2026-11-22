using System.Collections.Generic;

namespace MonoEngine.Class
{
    public sealed class GameObject : MonoObject
    {
        private static Dictionary<string, GameObject> nameToGameObject = new Dictionary<string, GameObject>();
        private static int nextId = 0;

        public readonly int InstanceId;
        private List<Component> components = new List<Component>();
        private string name;

        public string Name
        {
            get => name;
            set
            {
                string newName = value ?? $"GameObject{InstanceId}";

                if (name == newName) return;

                if (!string.IsNullOrEmpty(name))
                    nameToGameObject.Remove(name);

                string finalName = newName;
                int suffix = 1;
                while (nameToGameObject.ContainsKey(finalName))
                {
                    finalName = $"{newName}_{suffix}";
                    suffix++;
                }

                name = finalName;
                nameToGameObject[name] = this;
            }
        }

        public GameObject()
        {
            InstanceId = nextId++;
            name = $"GameObject{InstanceId}";
            nameToGameObject[name] = this;
        }

        public GameObject(string objectName)
        {
            InstanceId = nextId++;

            if (string.IsNullOrWhiteSpace(objectName))
            {
                name = $"GameObject{InstanceId}";
                nameToGameObject[name] = this;
                return;
            }

            string finalName = objectName;
            int suffix = 1;
            while (nameToGameObject.ContainsKey(finalName))
            {
                finalName = $"{objectName}_{suffix}";
                suffix++;
            }

            name = finalName;
            nameToGameObject[name] = this;
        }

        public static GameObject Find(string name)
        {
            nameToGameObject.TryGetValue(name, out var go);
            return go;
        }

        public T AddComponent<T>() where T : Component, new()
        {
            T component = new T();
            component.Parent = this;
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
            if (!string.IsNullOrEmpty(name))
                nameToGameObject.Remove(name);

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