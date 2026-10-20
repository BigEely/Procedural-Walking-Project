using UnityEditor.AnimatedValues;
using UnityEngine;

public class ProceduralFeetPlacementScript : MonoBehaviour
{
    Animator anim;

    public LayerMask layerMask;

    [Range (0, 1f)]
    public float DistanceToGround;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }


    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Space)))
        {
            anim.SetBool("Grounded", !anim.GetBool("Grounded"));
        }
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
                    anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                }
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
                    anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                }
            }
        }
    }
}
