using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformFollow : MonoBehaviour
{
    [SerializeField]
    public Transform FollowTarget;
    [SerializeField]
    [Range(0,1)]
    public float lerpIncrement = 1;
    public bool clampHeight;
    public float Height = 0f;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = FollowTarget.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 targetPosition = FollowTarget.position;
        if (clampHeight) targetPosition.y = Height;
        transform.position = Vector3.Lerp(transform.position, targetPosition, lerpIncrement);
    }
}
