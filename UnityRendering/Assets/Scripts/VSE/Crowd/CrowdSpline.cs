using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BezierSpline))]
public class CrowdSpline : MonoBehaviour
{
    [Tooltip("A static Mesh used to visualize the crowd entity in the editor as a gizmo.")]
    public Mesh CrowdGizmoMesh;
    [Tooltip("A static Mesh used to visualize the chair entity in the editor as a gizmo.")]
    public Mesh ChairMeshGizmo;
    [Tooltip("What to scale the original crowd entities by.")]
    public float CrowdScale = 1f;
    [Tooltip("What to scale the chairs by.")]
    public float ChairScale = 1f;
    [Tooltip("A position offset relative to the direction the crowd entity is facing.")]
    public Vector3 CrowdPositionOffset = Vector3.zero;

    [Range(0, 50)]
    [Tooltip("Number of crowd instances to spawn.")]
    public int CrowdSpawnCount = 10;

    [Tooltip("A transform in the center of the field used to orient the crowds in the right direction when spawned.")]
    public Transform CrowdLookatTransform;

    public bool CastShadows = true;

    private BezierSpline BezierSpline
    {
        get
        {
            if (bezierSpline == null)
            {
                bezierSpline = GetComponent<BezierSpline>();
            }
            return bezierSpline;
        }
    }
    private BezierSpline bezierSpline;

    void OnDrawGizmos()
    {
        if (CrowdSpawnCount <= 1) return;

        for (int i = 0; i < CrowdSpawnCount; i++)
        {
            float positionOnCurve = (float)(1f / (CrowdSpawnCount - 1)) * i;
            Gizmos.color = Color.red;
            GameObject point = new GameObject();
            point.transform.position = BezierSpline.GetPoint(positionOnCurve);

            Vector3 direction = CrowdLookatTransform.position;
            direction.y = point.transform.position.y;
            point.transform.LookAt(direction);
            Quaternion rotation = point.transform.rotation;

            DestroyImmediate(point);
            Gizmos.DrawMesh(CrowdGizmoMesh, BezierSpline.GetPoint(positionOnCurve), rotation, new Vector3(CrowdScale, CrowdScale, CrowdScale));
            Gizmos.color = Color.blue;
            Gizmos.DrawMesh(ChairMeshGizmo, BezierSpline.GetPoint(positionOnCurve), rotation, new Vector3(ChairScale, ChairScale, ChairScale));
        }
    }

    public void SpawnCrowd(
        GameObject [] crowdEntities, 
        GameObject chair, 
        Transform lookAtTarget,
        Transform parent,
        float percentageEmptySeat,
        float percentageSitting,
        float percentageCheering)
    {
        if (CrowdSpawnCount <= 1) return;

        for (int i = 0; i < CrowdSpawnCount; i++)
        {
            float positionOnCurve = (float)(1f / (CrowdSpawnCount - 1)) * i;

            Vector3 spawnPoint = BezierSpline.GetPoint(positionOnCurve);

            int randomSpawnPercent = Random.Range(0, 100);

            if (randomSpawnPercent > percentageEmptySeat)
            {
                // Select random crowd member
                int randomCrowdIndex = Random.Range(0, crowdEntities.Length);

                GameObject crowdGO = Instantiate(crowdEntities[randomCrowdIndex], spawnPoint, this.transform.rotation);
                crowdGO.transform.localScale = new Vector3(CrowdScale, CrowdScale, CrowdScale);
                crowdGO.transform.parent = parent;

                //Set look at target
                CrowdEntity crowdEntity = crowdGO.GetComponent<CrowdEntity>();

                if (crowdEntity)
                {
                    crowdEntity.LookAtTarget = lookAtTarget;

                    crowdEntity.Init(
                        percentageSitting,
                        percentageCheering);
                }

                Vector3 direction = CrowdLookatTransform.position;
                direction.y = crowdGO.transform.position.y;
                crowdGO.transform.LookAt(direction);
                crowdGO.transform.Translate(CrowdPositionOffset, Space.Self);

                if (!CastShadows)
                {
                    SkinnedMeshRenderer[] meshRenderers = crowdGO.GetComponentsInChildren<SkinnedMeshRenderer>();

                    foreach(SkinnedMeshRenderer mesh in meshRenderers)
                    {
                        mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    }

                    MeshRenderer[] staticMeshRenderers = crowdGO.GetComponentsInChildren<MeshRenderer>();

                    foreach (MeshRenderer mesh in staticMeshRenderers)
                    {
                        mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    }
                }
            }

            GameObject chairInstance = Instantiate(chair, spawnPoint, this.transform.rotation);
            Vector3 chairDirection = CrowdLookatTransform.position;
            chairDirection.y = chairInstance.transform.position.y;
            chairInstance.transform.LookAt(chairDirection);
            chairInstance.transform.parent = parent;

            if (!CastShadows)
            {
                MeshRenderer mesh = chairInstance.GetComponent<MeshRenderer>();
                mesh.receiveShadows = false;
                mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
    }
}
