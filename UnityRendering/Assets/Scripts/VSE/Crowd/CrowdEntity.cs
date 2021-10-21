using UnityEngine;

public class CrowdEntity : MonoBehaviour
{

    public Transform LookAtTarget;

    private float percentageSitting = 50f;
    private float percentageCheering = 50f;

    [SerializeField]
    private AnimationClip[] sitAnims;
    [SerializeField]
    private AnimationClip[] sitCheerAnims;
    [SerializeField]
    private AnimationClip[] standAnims;
    [SerializeField]
    private AnimationClip[] standCheerAnims;

    [Header("Selection of meshes variations to randomize crowd members with")]

    [SerializeField]
    private Mesh[] skinMeshes;
    [SerializeField]
    private Mesh[] shirtMeshes;
    [SerializeField]
    private Mesh[] pantsMeshes;
    [SerializeField]
    private Mesh[] shoeMeshes;
    [SerializeField]
    private Mesh[] hairMeshes;

    [SerializeField]
    private SkinnedMeshRenderer shirtMeshRenderer;
    [SerializeField]
    private SkinnedMeshRenderer skinMeshRenderer;
    [SerializeField]
    private SkinnedMeshRenderer pantsMeshRenderer;
    [SerializeField]
    private SkinnedMeshRenderer hairMeshRenderer;
    [SerializeField]
    private SkinnedMeshRenderer shoeMeshRenderer;

    private Animator animator;

    // Use these if you want to use the humanoid look at component to follow the movement of the ball
    private float distanceFromTargetToStand = 8f;
    private float distFromTarget;

    // Start is called before the first frame update
    public void Init(
        float percentSitting, 
        float percentCheering)
    {
        percentageSitting = percentSitting;
        percentageCheering = percentCheering;

        RandomizeMeshes();
    }

    // Animator loses reference to animation state when the game object disabled and re-enabled so we cannot do this on  Awake();
    private void OnEnable()
    {
        float randomPercentageSitting = Random.Range(0, 100);
        float randomPercentageCheering = Random.Range(0, 100);
        bool isSitting = randomPercentageSitting <= percentageSitting;
        bool isCheering = randomPercentageCheering <= percentageCheering;

        animator = GetComponent<Animator>();

        // For models made static
        if (animator == null)
            return;

        if (isSitting)
        {
            if (isCheering)
            {
                PlayRandomAnimation(sitCheerAnims);
            }
            else
            {
                PlayRandomAnimation(sitAnims);
            }
        }
        else
        {
            if (isCheering)
            {
                PlayRandomAnimation(standCheerAnims);
            }
            else
            {
                PlayRandomAnimation(standAnims);
            }
        }
    }

    // Selects random varitations of skinned meshes for the shirt, pants and other extras. These meshes have 2 UV's, 
    // The first to UV references a texture map
    // The second UV works as a colour picker
    // This allows us to use a single material for the entire crowd, saving on draw calls
    private void RandomizeMeshes()
    {
        if (skinMeshRenderer && skinMeshes.Length > 0)
        {
            int randomIndex = Random.Range(0, skinMeshes.Length);
            skinMeshRenderer.sharedMesh = skinMeshes[randomIndex];
        }

        if (shirtMeshRenderer && shirtMeshes.Length > 0)
        {
            int randomIndex = Random.Range(0, shirtMeshes.Length);
            shirtMeshRenderer.sharedMesh = shirtMeshes[randomIndex];
        }

        if (pantsMeshRenderer && pantsMeshes.Length > 0)
        {
            int randomIndex = Random.Range(0, pantsMeshes.Length);
            pantsMeshRenderer.sharedMesh = pantsMeshes[randomIndex];
        }

        if (shoeMeshRenderer && shoeMeshes.Length > 0)
        {
            int randomIndex = Random.Range(0, shoeMeshes.Length);
            shoeMeshRenderer.sharedMesh = shoeMeshes[randomIndex];
        }

        if (hairMeshRenderer && hairMeshes.Length > 0)
        {
            int randomIndex = Random.Range(0, hairMeshes.Length);
            hairMeshRenderer.sharedMesh = hairMeshes[randomIndex];
        }
    }

    /* Uncomment this if you want the humanoid crowd entity's to react to the movement of the mark
    private void OnAnimatorIK(int layerIndex)
    {
        if (LookAtTarget)
        {
            animator.SetLookAtPosition(LookAtTarget.position);
            distFromTarget = Vector3.Distance(transform.position, LookAtTarget.position);

            animator.SetLookAtWeight(1, 5f/ distFromTarget);
        }
    }*/

    public void CrossFadeRandomAnimation(AnimationClip[] clips)
    {
        int index = Random.Range(0, clips.Length);
        animator.CrossFadeInFixedTime(clips[index].name, 0.3f);
    }

    public void PlayRandomAnimation(AnimationClip[] clips)
    {
        int index = Random.Range(0, clips.Length);
        float startPoint = Random.Range(0f, 1f);

        animator.Play(clips[index].name, 0, startPoint);
        animator.speed = Random.Range(0.8f, 1.2f);
    }
}
