using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdManager : MonoBehaviour
{
    [Header("Crowd properties")]

    [SerializeField]
    private float percentageEmptySeat = 15f;
    [SerializeField]
    private float percentageSitting = 50f;
    [SerializeField]
    private float percentageCheering = 50f;

    [Space(20)]

    [SerializeField]
    private GameObject [] crowdEnitys;
    [SerializeField]
    private GameObject chairInstance;
    [SerializeField]
    private Transform lookAtTarget;
    [SerializeField]
    private Transform crowdInstanceParent;

    private CrowdSpline[] crowdSplines;

    private void Start()
    {
        InitiateCrowd();
    }

    public void InitiateCrowd()
    {
        crowdSplines = FindObjectsOfType<CrowdSpline>();

        foreach(CrowdSpline spline in crowdSplines)
        {
            spline.SpawnCrowd(
                crowdEnitys, 
                chairInstance, 
                lookAtTarget,
                crowdInstanceParent,
                percentageEmptySeat,
                percentageSitting,
                percentageCheering);
        }
    }
}
