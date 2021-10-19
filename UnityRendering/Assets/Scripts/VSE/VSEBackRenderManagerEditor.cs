using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VSEBackRenderManager))]
public class VSEBackRenderManagerEditor : Editor
{
    private VSERendering VSERendering
    {
        get
        {
            if (vseRendering == null)
            {
                vseRendering = FindObjectOfType<VSERendering>();
            }
            return vseRendering;
        }
    }
    private VSERendering vseRendering;

    private VSEBackRenderManager vseBackRenderManagerTarget;

    private void OnEnable()
    {
        vseBackRenderManagerTarget = target as VSEBackRenderManager;
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Add new background assets to VSE render system"))
        {
            VSERendering.RemainingMeshRenderers.Clear();
            vseRendering.RemainingSkinMeshRenderers.Clear();
            VSERendering.BannerMeshRenderers.Clear();

            List<MeshRenderer> backgroundRenderers = 
                vseBackRenderManagerTarget.GetComponentsInChildren<MeshRenderer>().ToList();

            foreach (MeshRenderer meshRenderer in backgroundRenderers)
            {
                if (meshRenderer == vseBackRenderManagerTarget.BannerRenderer)
                    continue;

                VSERendering.RemainingMeshRenderers.Add(meshRenderer);
            }

            VSERendering.BannerMeshRenderers.Add(vseBackRenderManagerTarget.BannerRenderer);

            Debug.Log("Background render objects added successfully");
        }
    }
}
