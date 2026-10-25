using UnityEditor.AnimatedValues;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

public class ProceduralFeetPlacementScript : MonoBehaviour
{
    Animator anim;
    CharacterController characterController;

    public LayerMask layerMask;

    [Range(0, 1f)]
    public float DistanceToGround;

    public float centerMax = 1.5f;
    public float centerMin;
    public float groundCheckDistance = 2f;
    public float centerChangeSpeed = 4f;

    public bool DebugMode = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        centerMin = characterController.center.y;
    }


    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Space)))
        {
            anim.SetBool("Grounded", !anim.GetBool("Grounded"));
        }
    }

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

    private bool CheckGround(AvatarIKGoal foot, out RaycastHit hit)
    {
        Vector3 origin = anim.GetIKPosition(foot) + Vector3.up * .2f;
        return Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance, layerMask) && hit.collider.CompareTag("Walkable");
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (anim)
        {
            //Left Foot
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, anim.GetFloat("IKLFootWeight"));
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, anim.GetFloat("IKLFootWeight"));

            RaycastHit hit;
            Ray ray = new Ray(anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);

            if (Physics.Raycast(ray, out hit, DistanceToGround + 1f, layerMask))
            {
                if (hit.transform.tag == "Walkable")
                {
                    Vector3 footPosition = hit.point;
                    footPosition.y += DistanceToGround;
                    anim.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);

                    Vector3 footForward = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
                    Quaternion targetRotation = Quaternion.LookRotation(footForward, hit.normal);
                    anim.SetIKRotation(AvatarIKGoal.LeftFoot, targetRotation);
                    //anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                }
            }
            if (DebugMode)
            {
                Debug.DrawRay(ray.origin, ray.direction * (DistanceToGround + 1f), Color.blue);
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
            }


            //Right Foot
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, anim.GetFloat("IKRFootWeight"));
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, anim.GetFloat("IKRFootWeight"));

            ray = new Ray(anim.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);

            if (Physics.Raycast(ray, out hit, DistanceToGround + 1f, layerMask))
            {
                if (hit.transform.tag == "Walkable")
                {
                    Vector3 footPosition = hit.point;
                    footPosition.y += DistanceToGround;
                    anim.SetIKPosition(AvatarIKGoal.RightFoot, footPosition);

                    Vector3 footForward = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
                    Quaternion targetRotation = Quaternion.LookRotation(footForward, hit.normal);
                    anim.SetIKRotation(AvatarIKGoal.RightFoot, targetRotation);
                    //anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                }
            }
            if (DebugMode)
            {
                Debug.DrawRay(ray.origin, ray.direction * (DistanceToGround + 1f), Color.blue);
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
            }
        }

        AdjustCharacterCenter();
    }
}
