using System.Text;
using UnityEngine;

namespace Core.Gameplay.Save
{
    public static class SceneObjectSaveId
    {
        public static string Build(Transform transform)
        {
            if (transform == null)
            {
                return string.Empty;
            }

            var builder = new StringBuilder(128);
            AppendPath(builder, transform);
            return $"{transform.gameObject.scene.name}:{builder}";
        }

        private static void AppendPath(StringBuilder builder, Transform current)
        {
            if (current.parent != null)
            {
                AppendPath(builder, current.parent);
                builder.Append('/');
            }

            builder
                .Append(current.name)
                .Append('#')
                .Append(current.GetSiblingIndex());
        }
    }
}
