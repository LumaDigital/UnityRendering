using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(SkinnedMeshRenderer))]
public class ActorPassEntity : MonoBehaviour
{
    [SerializeField]
    private Material actorMaterial;

    public void ApplyActorMaterial(Material material)
    {
        if (material != null)
            GetComponent<SkinnedMeshRenderer>().material = material;
        else
            GetComponent<SkinnedMeshRenderer>().material = actorMaterial;
    }
}
