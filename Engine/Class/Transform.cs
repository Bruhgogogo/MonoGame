using Microsoft.Xna.Framework;
using MonoEngine.MonoUtils;
using System;
using System.Collections.Generic;

namespace MonoEngine.Class
{
    public sealed class Transform : Component
    {
        private Vector2 localPosition = Vector2.Zero;
        private Vector2 localScale = Vector2.One;
        private float localRotation = 0.0f;

        private Transform parent;
        private readonly List<Transform> children = new List<Transform>();

        public Transform Parent
        {
            get => parent;
            set
            {
                if (parent == value) return;
                parent?.children.SwapAndRemove(parent.children.IndexOf(this));
                parent = value;
                value?.children.Add(this);
                RecalculateWorldFromParent();
            }
        }

        public Vector2 LocalPosition
        {
            get => localPosition;
            set
            {
                localPosition = value;
                RecalculateWorldFromParent();
            }
        }

        public Vector2 LocalScale
        {
            get => localScale;
            set
            {
                localScale = value;
                RecalculateWorldFromParent();
            }
        }

        public float LocalRotation
        {
            get => localRotation;
            set
            {
                localRotation = value;
                RecalculateWorldFromParent();
            }
        }

        public Vector2 Position
        {
            get
            {
                if (parent == null) return localPosition;
                return parent.ToWorldSpace(localPosition);
            }
            set
            {
                if (parent == null)
                {
                    localPosition = value;
                }
                else
                {
                    localPosition = parent.ToObjectSpace(value);
                }
                PropagateToChildren();
            }
        }

        public Vector2 Scale
        {
            get
            {
                if (parent == null) return localScale;
                return new Vector2(
                    localScale.X * parent.Scale.X,
                    localScale.Y * parent.Scale.Y
                );
            }
            set
            {
                if (parent == null)
                {
                    localScale = value;
                }
                else
                {
                    localScale = new Vector2(
                        parent.Scale.X != 0f ? value.X / parent.Scale.X : value.X,
                        parent.Scale.Y != 0f ? value.Y / parent.Scale.Y : value.Y
                    );
                }
                PropagateToChildren();
            }
        }

        public float Rotation
        {
            get
            {
                if (parent == null) return localRotation;
                return localRotation + parent.Rotation;
            }
            set
            {
                if (parent == null)
                {
                    localRotation = value;
                }
                else
                {
                    localRotation = value - parent.Rotation;
                }
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

        public Vector2 Right
        {
            get
            {
                Vector2 forward = Forward;
                return new Vector2(forward.Y, -forward.X);
            }
        }

        private void RecalculateWorldFromParent()
        {
            PropagateToChildren();
        }

        private void PropagateToChildren()
        {
            for (int i = 0; i < children.Count; i++)
            {
                children[i].OnParentChanged();
            }
        }

        private void OnParentChanged()
        {
            for (int i = 0; i < children.Count; i++)
            {
                children[i].OnParentChanged();
            }
        }

        public Vector2 ToWorldSpace(Vector2 localPoint)
        {
            float cos = MathF.Cos(localRotation);
            float sin = MathF.Sin(localRotation);
            Vector2 scaled = localPoint * localScale;
            Vector2 worldPos = new Vector2(
                scaled.X * cos - scaled.Y * sin,
                scaled.X * sin + scaled.Y * cos
            ) + localPosition;

            if (parent != null)
                worldPos = parent.ToWorldSpace(worldPos);

            return worldPos;
        }

        public Vector2 ToObjectSpace(Vector2 worldPoint)
        {
            if (parent != null)
                worldPoint = parent.ToObjectSpace(worldPoint);

            Vector2 translated = worldPoint - localPosition;
            float cos = MathF.Cos(-localRotation);
            float sin = MathF.Sin(-localRotation);
            Vector2 rotated = new Vector2(
                translated.X * cos - translated.Y * sin,
                translated.X * sin + translated.Y * cos
            );
            return new Vector2(
                localScale.X != 0f ? rotated.X / localScale.X : rotated.X,
                localScale.Y != 0f ? rotated.Y / localScale.Y : rotated.Y
            );
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