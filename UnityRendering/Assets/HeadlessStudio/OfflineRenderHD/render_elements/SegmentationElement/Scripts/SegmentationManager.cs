using HeadlessStudio.Segmentation;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class SegmentationManager : MonoBehaviour
{
    public enum ColorMode
    {
        InstanceId,
        Layer,
        Tag,
        Random,
        Palette,
    }

    [Tooltip("Enabling this option, any SegmentationCategory component in the child objects are considerer.")]
    public bool allowOverrides;
    public ColorMode mode;
    public Color[] palette = new Color[]
    {
        FromHex("FF0000"), FromHex("00FF00"), FromHex("0000FF"), FromHex("FFFF00"), FromHex("FF00FF"), FromHex("00FFFF"), FromHex("000000"),
        FromHex("800000"), FromHex("008000"), FromHex("000080"), FromHex("808000"), FromHex("800080"), FromHex("008080"), FromHex("808080"),
        FromHex("C00000"), FromHex("00C000"), FromHex("0000C0"), FromHex("C0C000"), FromHex("C000C0"), FromHex("00C0C0"), FromHex("C0C0C0"),
        FromHex("400000"), FromHex("004000"), FromHex("000040"), FromHex("404000"), FromHex("400040"), FromHex("004040"), FromHex("404040"),
        FromHex("200000"), FromHex("002000"), FromHex("000020"), FromHex("202000"), FromHex("200020"), FromHex("002020"), FromHex("202020"),
        FromHex("600000"), FromHex("006000"), FromHex("000060"), FromHex("606000"), FromHex("600060"), FromHex("006060"), FromHex("606060"),
        FromHex("A00000"), FromHex("00A000"), FromHex("0000A0"), FromHex("A0A000"), FromHex("A000A0"), FromHex("00A0A0"), FromHex("A0A0A0"),
        FromHex("E00000"), FromHex("00E000"), FromHex("0000E0"), FromHex("E0E000"), FromHex("E000E0"), FromHex("00E0E0"), FromHex("E0E0E0"),
    };

    private void Start()
    {
        SetColors();
    }

    private void OnValidate()
    {
        SetColors();
    }

    private static Color FromHex(string colorStr)
    {
        return new Color(int.Parse(colorStr.Substring(0, 2), NumberStyles.HexNumber) / 255.0f, int.Parse(colorStr.Substring(2, 2), NumberStyles.HexNumber) / 255.0f, int.Parse(colorStr.Substring(4, 2), NumberStyles.HexNumber) / 255.0f);
    }

    private void SetColors()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        ColourGenerator gen = new ColourGenerator();
        int index = 0;
        foreach (var render in renderers)
        {
            if (allowOverrides && render.GetComponent<SegmentationCategory>())
            {
                render.GetComponent<SegmentationCategory>().SetColor();
                continue;
            }
            var propertyBlock = new MaterialPropertyBlock();
            render.GetPropertyBlock(propertyBlock);
            Color color;
            switch (mode)
            {
                case ColorMode.InstanceId:
                    color = ColorEncoding.EncodeIDAsColor(render.gameObject.GetInstanceID());
                    break;
                case ColorMode.Layer:
                    color = ColorEncoding.EncodeLayerAsColor(render.gameObject.layer);
                    break;
                case ColorMode.Tag:
                    color = ColorEncoding.EncodeTagAsColor(render.gameObject.tag);
                    break;
                case ColorMode.Palette:
                    color = palette[index];
                    break;
                case ColorMode.Random:
                default:
                    color = gen.NextColour();
                    break;
            }
            propertyBlock.SetColor("_CategoryColor", color);
            render.SetPropertyBlock(propertyBlock);
            index++;
            if (index >= palette.Length) index = 0;
        }
    }
}
