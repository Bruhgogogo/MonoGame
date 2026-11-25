using Microsoft.Xna.Framework;
using System;

namespace MonoEngine.Class
{
    public sealed class Transform : Component
    {
        private Vector2 localPosition = Vector2.Zero;
        private Vector2 localScale = Vector2.One;
        private float localRotation = 0.0f;

        private Vector2 cachedWorldPosition = Vector2.Zero;
        private Matrix cachedWorldMatrix = Matrix.Identity;

        private Transform ParentTransform => GO?.Parent?.GetComponent<Transform>();

        public Vector2 LocalPosition
        {
            get => localPosition;
            set
            {
                localPosition = value;
                RecalculateCaches();
                PropagateToChildren();
            }
        }

        public Vector2 LocalScale
        {
            get => localScale;
            set
            {
                localScale = value;
                RecalculateCaches();
                PropagateToChildren();
            }
        }

        public float LocalRotation
        {
            get => localRotation;
            set
            {
                localRotation = value;
                RecalculateCaches();
                PropagateToChildren();
            }
        }

        public Vector2 Position
        {
            get => cachedWorldPosition;
            set
            {
                Transform p = ParentTransform;
                if (!p)
                    localPosition = value;
                else
                    localPosition = p.ToObjectSpace(value);
                RecalculateCaches();
                PropagateToChildren();
            }
        }

        public Vector2 Scale
        {
            get
            {
                Transform p = ParentTransform;
                if (!p) return localScale;
                return new Vector2(
                    localScale.X * p.Scale.X,
                    localScale.Y * p.Scale.Y
                );
            }
            set
            {
                Transform p = ParentTransform;
                if (!p)
                {
                    localScale = value;
                }
                else
                {
                    localScale = new Vector2(
                        p.Scale.X != 0f ? value.X / p.Scale.X : value.X,
                        p.Scale.Y != 0f ? value.Y / p.Scale.Y : value.Y
                    );
                }
                RecalculateCaches();
                PropagateToChildren();
            }
        }

        public float Rotation
        {
            get
            {
                Transform p = ParentTransform;
                if (!p) return localRotation;
                return localRotation + p.Rotation;
            }
            set
            {
                Transform p = ParentTransform;
                if (!p)
                    localRotation = value;
                else
                    localRotation = value - p.Rotation;
                RecalculateCaches();
                PropagateToChildren();
            }
        }

        public Vector2 Forward
        {
            get
            {
                float cos = MathF.Cos(localRotation);
                float sin = MathF.Sin(localRotation);
                return new Vector2(cos, -sin);
            }
        }

        public Vector2 Up
        {
            get
            {
                Vector2 forward = Forward;
                return new Vector2(-forward.Y, forward.X);
            }
        }

        internal void RecalculateCaches()
        {
            Matrix localMatrix = GetLocalMatrix();
            Transform p = ParentTransform;
            if (!p)
            {
                cachedWorldMatrix = localMatrix * p.cachedWorldMatrix;
                cachedWorldPosition = Vector2.Transform(Vector2.Zero, cachedWorldMatrix);
            }
            else
            {
                cachedWorldMatrix = localMatrix;
                cachedWorldPosition = new Vector2(cachedWorldMatrix.M41, cachedWorldMatrix.M42);
            }
        }

        internal void PropagateToChildren()
        {
            if (!GO) return;
            var kids = GO.Childrens;
            for (int i = 0; i < kids.Count; i++)
            {
                Transform ct = kids[i].GetComponent<Transform>();
                if (!ct)
                {
                    ct.RecalculateCaches();
                    ct.PropagateToChildren();
                }
            }
        }

        public Vector2 ToWorldSpace(Vector2 localPoint)
        {
            return Vector2.Transform(localPoint, cachedWorldMatrix);
        }

        public Vector2 ToObjectSpace(Vector2 worldPoint)
        {
            Matrix inverseMatrix = Matrix.Invert(cachedWorldMatrix);
            return Vector2.Transform(worldPoint, inverseMatrix);
        }

        public Transform ToTransformSpace(Transform other)
        {
            Transform result = new Transform();
            result.localPosition = ToObjectSpace(other.Position);
            result.localRotation = other.Rotation - Rotation;
            result.localScale = new Vector2(
                Scale.X != 0f ? other.Scale.X / Scale.X : other.Scale.X,
                Scale.Y != 0f ? other.Scale.Y / Scale.Y : other.Scale.Y
            );
            return result;
        }

        public Transform FromTransformSpace(Transform relative)
        {
            Transform result = new Transform();
            result.localPosition = ToWorldSpace(relative.localPosition);
            result.localRotation = localRotation + relative.localRotation;
            result.localScale = new Vector2(
                localScale.X * relative.localScale.X,
                localScale.Y * relative.localScale.Y
            );
            return result;
        }

        public Vector2 TransformDirection(Vector2 direction)
        {
            float cos = MathF.Cos(localRotation);
            float sin = MathF.Sin(localRotation);
            return new Vector2(
                direction.X * cos - direction.Y * sin,
                direction.X * sin + direction.Y * cos
            );
        }

        public Vector2 InverseTransformDirection(Vector2 direction)
        {
            float cos = MathF.Cos(-localRotation);
            float sin = MathF.Sin(-localRotation);
            return new Vector2(
                direction.X * cos - direction.Y * sin,
                direction.X * sin + direction.Y * cos
            );
        }

        public void Translate(Vector2 translation)
        {
            Position += translation;
        }

        public void Translate(float x, float y)
        {
            Position += new Vector2(x, y);
        }

        public void Rotate(float angleRadians)
        {
            Rotation += angleRadians;
        }

        public void LookAt(Vector2 target)
        {
            Vector2 direction = target - Position;
            if (direction != Vector2.Zero)
                Rotation = MathF.Atan2(-direction.Y, direction.X);
        }

        public Matrix GetMatrix()
        {
            return cachedWorldMatrix;
        }

        private Matrix GetLocalMatrix()
        {
            return
                Matrix.CreateScale(new Vector3(localScale.X, localScale.Y, 1f)) *
                Matrix.CreateRotationZ(localRotation) *
                Matrix.CreateTranslation(new Vector3(localPosition.X, localPosition.Y, 0f));
        }

        public override string ToString()
        {
            return $"Position: {Position}, Rotation: {Rotation:F2} rad ({Rotation * 57.29578f:F1}°), Scale: {Scale}";
        }
    }
}