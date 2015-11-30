using UnityEngine;
using System.Collections;

public class FollowTarget : MonoBehaviour
{
    public Transform Target;
    public Vector3 Offset;

    void LateUpdate()
    {
        transform.position = Target.position + Offset;
    }
}
