using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Renamer))]
public class RenamerEditor : Editor
{
    public bool buttonDisplayName; //"run" or "generate" for example
    public bool buttonDisplayName2; //supports multiple buttons

    public  override void OnInspectorGUI()
    {
        if (GUILayout.Button("Test"))
        {
            ButtonFunction1();
        }
    }

    void ButtonFunction1()
    {
        DirectoryInfo d = new DirectoryInfo(@"D:\OLD VSE BUILDS\Common");// D:\OLD VSE BUILDS\Common
        FileInfo[] infos = d.GetFiles();
        string [] files  =  Directory.GetFiles(@"D:\OLD VSE BUILDS\Common", "*.ogg*", SearchOption.AllDirectories);

        int i = 0;
        foreach (string f in files)
        {
            FileInfo fi = new FileInfo(f);

            if (fi.Exists)
                File.Move(fi.FullName, fi.FullName.Replace("]", ""));
        }
    }

    void ButtonFunction2()
    {
        //DoStuff
    }
}
