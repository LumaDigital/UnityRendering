using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utils
{
    public static class UnityExtensions
    {
        // Extension taken from https://loekvandenouweland.com/content/use-linerenderer-in-unity-to-draw-a-circle.html
        public static void DrawCircle(this GameObject container, float radius, float lineWidth)
        {
            var segments = 360;
            LineRenderer line = container.GetComponent<LineRenderer>();
            if (line == null)
            {
                line = container.AddComponent<LineRenderer>();
            }
            if (line != null)
            {
                line.useWorldSpace = false;
                line.startWidth = lineWidth;
                line.endWidth = lineWidth;
                line.positionCount = segments + 1;
                line.startColor = Color.green;
                line.endColor = Color.green;

                var pointCount = segments + 1; // add extra point to make startpoint and endpoint the same to close the circle
                var points = new Vector3[pointCount];

                for (int i = 0; i < pointCount; i++)
                {
                    var rad = Mathf.Deg2Rad * (i * 360f / segments);
                    points[i] = new Vector3(Mathf.Sin(rad) * radius, 0, Mathf.Cos(rad) * radius);
                }

                line.SetPositions(points);
            }
        }

        // Extension taken from https://answers.unity.com/questions/158172/findsceneobjectsoftype-ignoring-the-inactive-gameo.html
        // Find all objects even if disabled by type
        public static List<T> FindObjectsOfTypeAll<T>()
        {
            return SceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(gameObject => gameObject.GetComponentsInChildren<T>(true))
                .ToList();
        }

        public static Transform FindDeepChild(this Transform aParent, string aName)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(aParent);

            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c.name == aName)
                    return c;
                foreach (Transform t in c)
                {
                    queue.Enqueue(t);
                }
            }
            return null;
        }

        /// Build a string that is a token separated list of the elements in an array.
        public static string ToTokenSeparatedString<T>(this T[] arr, string token)
        {
            if (null == arr || arr.Length == 0) return "";

            StringBuilder builder = new StringBuilder();
            int limit = arr.Length - 1;
            // Iterate over the first Length-1 elements appending the element and a token
            for (int i = 0; i < limit; ++i)
            {
                builder.Append(arr[i]);
                builder.Append(token);
            }
            // Add the final element
            builder.Append(arr[limit]);

            return builder.ToString();
        }
    }
}
