using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorPassShirtAssistant : MonoBehaviour
{
    [SerializeField]
    private Transform twistJoint;
    [SerializeField]
    private Transform shoulderL;
    [SerializeField]
    private Transform shoulderR;

    private SkinnedMeshRenderer skinnedMeshRenderer;
    private Material material;

    // Start is called before the first frame update
    void Start()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        material = skinnedMeshRenderer.material;
    }

    // Update is called once per frame
    void Update()
    {
        // The rotation maps left twist at 0, the default normal at 0.5, and right twist at 1;
        // localRotation.y will return a value from -0.5f and 0.5f at a -90 degree and 90 degree twist
        float spineRotation = (twistJoint.localRotation.y) + 0.5f;
        float shoulderLRotation = shoulderL.localRotation.y;
        float shoulderRRotation = shoulderR.localRotation.y;

        float twistValue = Mathf.Lerp(0, 1, spineRotation);

        material.SetFloat("Twist", twistValue);

        if (shoulderLRotation > 0 && shoulderLRotation <= 0.5f)
        {
            material.SetFloat("ShoulderForwardL", shoulderLRotation * 2);
        }
        else if (shoulderLRotation > -0.5f && shoulderLRotation < 0f)
        {
            material.SetFloat("ShoulderBackL", shoulderLRotation * 2);
        }

        if (shoulderRRotation > 0 && shoulderRRotation <= 0.5f)
        {
            material.SetFloat("ShoulderBackR", shoulderRRotation * 2);

        }
        else if (shoulderRRotation > -0.5f && shoulderRRotation < 0f)
        {
            material.SetFloat("ShoulderForwardR", shoulderRRotation * 2);
        }
    }
}
