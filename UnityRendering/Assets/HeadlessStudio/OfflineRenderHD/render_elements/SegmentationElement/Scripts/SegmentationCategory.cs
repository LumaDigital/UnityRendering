using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SegmentationCategory : MonoBehaviour
{
    public Color categoryColor = new Color(1f, 0.0f, 0f, 1f);

    private void Start()
    {
        SetColor();
    }

    private void OnValidate()
    {
        SetColor();
    }

    public void SetColor()
    {
        var rndr = GetComponent<Renderer>();
        var cat = gameObject.GetComponentInParent<SegmentationManager>();
        if (cat)
        {
            if (!cat.allowOverrides)
                return;
        }

        var propertyBlock = new MaterialPropertyBlock();
        rndr.GetPropertyBlock(propertyBlock);

        propertyBlock.SetColor("_CategoryColor", categoryColor);

        rndr.SetPropertyBlock(propertyBlock);
    }
}
