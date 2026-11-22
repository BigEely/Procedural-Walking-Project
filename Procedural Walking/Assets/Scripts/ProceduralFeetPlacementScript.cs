using UnityEditor.AnimatedValues;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

public class ProceduralFeetPlacementScript : MonoBehaviour
{
    #region Tutorial
    /*
    
            --How To Implement--
    1) Open Character Controller -> Create 2 Float Parameters "IKLFootWeight" & "IKRFootWeight" (Used to determine when to use IK)
    2) In Character Animations FBX create 2 new Curves -> Title them "IKLFootWeight" & "IKRFootWeight" -> Assign curve values (1 = On, 0 = Off)
    3) Put Script onto Character Game Object
    4) Character Tag = Player and Layer = Player
    5) Script LayerMask = Everything Except Player
    6) Tag everything walkable as "Walkable"
    7) Adjust Script Variables to get desired effect

            --Debug Mode--
    1) Green Rays = IK is Active
    2) Red Rays = Ik is Inactive

    */
    #endregion

    #region Variables
    Animator anim;
    CharacterController characterController;

    [Header("Character Settings")]
    public LayerMask layerMask;
    [Range(0, 1f)]
    public float DistanceToGround; //Initial Height Offset for character, Adjust so that feet touches ground

    [Header("Collider Settings")]
    public float centerMax = 1.4f; //How high can the IK collisider move up. Higher = character IK moves downwards
    private float centerMin;
    [Range(1f,1.5f)]
    public float groundCheckDistance = 1.5f; //How far do the Foot IK Rays shoot downwards to find ground (used for uneven terrain) ()
    public float centerChangeSpeed = 4f; //How fast the IK collider adjusts its value, Faster = Instant changes

    [Header("Tools")]
    public bool DebugMode = false; //Set to true if you want to see foot IK rays
    #endregion

    #region Start + Update
    private void Start()
    {
        anim = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        centerMin = characterController.center.y;
    }


    private void Update()
    {
        //If the player doesn't jump delete this, Used to handle falling animations
        if ((Input.GetKeyDown(KeyCode.Space))) 
        {
            anim.SetBool("Grounded", !anim.GetBool("Grounded"));
        }
    }
    #endregion

    #region CharacterCenter
    private void AdjustCharacterCenter()
    {
        bool leftGrounded = CheckGround(AvatarIKGoal.LeftFoot, out RaycastHit leftHit);
        bool rightGrounded = CheckGround(AvatarIKGoal.RightFoot, out RaycastHit rightHit);

        float targetCenter = centerMin;

        if (leftGrounded && rightGrounded)
        {
            float heightDifference = Mathf.Abs(leftHit.point.y - rightHit.point.y);

            targetCenter = Mathf.Clamp(centerMin + heightDifference, centerMin, centerMax);
        }
        else
        { 
            targetCenter = centerMin;
        }
        float newCenter = Mathf.MoveTowards(characterController.center.y, targetCenter, centerChangeSpeed * Time.deltaTime);

        Vector3 center = characterController.center;
        center.y = newCenter;
        characterController.center = center;
    }

    private bool CheckGround(AvatarIKGoal IKGoalFoot, out RaycastHit hit) //Returns true if ray hits the ground with the tag "Walkable"
    {
        Vector3 origin = anim.GetIKPosition(IKGoalFoot) + Vector3.up;

        if (DebugMode)
        {
            Debug.DrawRay(anim.GetIKPosition(IKGoalFoot), Vector3.down * Mathf.Abs(DistanceToGround + 1f - groundCheckDistance), Color.blue);
        }

        return Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance, layerMask) && hit.collider.CompareTag("Walkable");
    }
    #endregion

    #region FootIk
    private void CalculateIK(AvatarIKGoal IKGoalFoot)
    {
        anim.SetIKPositionWeight(IKGoalFoot, anim.GetFloat("IKLFootWeight"));
        anim.SetIKRotationWeight(IKGoalFoot, anim.GetFloat("IKLFootWeight"));

        RaycastHit hit;
        Ray ray = new Ray(anim.GetIKPosition(IKGoalFoot) + Vector3.up, Vector3.down);

        Color debugHitColor = Color.red;

        if (Physics.Raycast(ray, out hit, DistanceToGround + 1f, layerMask))
        {
            if (hit.transform.tag == "Walkable")
            {
                debugHitColor = Color.green;

                Vector3 footPosition = hit.point;
                footPosition.y += DistanceToGround;
                anim.SetIKPosition(IKGoalFoot, footPosition);

                Vector3 footForward = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(footForward, hit.normal);
                anim.SetIKRotation(IKGoalFoot, targetRotation);
                //anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));
            }
            else
            {
                debugHitColor = Color.red;
            }
        }
        if (DebugMode)
        {
            Debug.DrawRay(ray.origin, ray.direction * (DistanceToGround + 1f), debugHitColor);
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (anim)
        {
            CalculateIK(AvatarIKGoal.LeftFoot);
            CalculateIK(AvatarIKGoal.RightFoot);
            AdjustCharacterCenter();
        }
    }
#endregion
}
