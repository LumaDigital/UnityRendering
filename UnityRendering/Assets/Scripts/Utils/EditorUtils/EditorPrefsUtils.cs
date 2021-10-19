using UnityEditor;
using UnityEngine;

namespace Utils
{
    public static class EditorPrefsUtils
    {
        public static void SetVector3(string variableName, Vector3 vector3)
        {
            EditorPrefs.SetFloat(variableName + "_x", vector3.x);
            EditorPrefs.SetFloat(variableName + "_y", vector3.y);
            EditorPrefs.SetFloat(variableName + "_z", vector3.z);
        }

        public static void GetVector3(string variableName, ref Vector3 vector3)
        {
            vector3.x = EditorPrefs.GetFloat(variableName + "_x");
            vector3.y = EditorPrefs.GetFloat(variableName + "_y");
            vector3.z = EditorPrefs.GetFloat(variableName + "_z");
        }

        public static void SetQuaternion(string variableName, Quaternion quaternion)
        {
            EditorPrefs.SetFloat(variableName + "_x", quaternion.x);
            EditorPrefs.SetFloat(variableName + "_y", quaternion.y);
            EditorPrefs.SetFloat(variableName + "_z", quaternion.z);
            EditorPrefs.SetFloat(variableName + "_w", quaternion.w);
        }

        public static void GetQuaternion(string variableName, ref Quaternion quaternion)
        {
            quaternion.x = EditorPrefs.GetFloat(variableName + "_x");
            quaternion.y = EditorPrefs.GetFloat(variableName + "_y");
            quaternion.z = EditorPrefs.GetFloat(variableName + "_z");
            quaternion.w = EditorPrefs.GetFloat(variableName + "_w");
        }

        public static Color GetColour(string variableName)
        {
            return 
                new Color
                {
                    r = EditorPrefs.GetFloat(variableName + "_r"),
                    g = EditorPrefs.GetFloat(variableName + "_g"),
                    b = EditorPrefs.GetFloat(variableName + "_b"),
                    a = EditorPrefs.GetFloat(variableName + "_a")
                }; ;
        }

        public static void SetColour(string variableName, ref Color colour)
        {
            EditorPrefs.SetFloat(variableName + "_r", colour.r);
            EditorPrefs.SetFloat(variableName + "_g", colour.g);
            EditorPrefs.SetFloat(variableName + "_b", colour.b);
            EditorPrefs.SetFloat(variableName + "_a", colour.a);
        }
    }
}

