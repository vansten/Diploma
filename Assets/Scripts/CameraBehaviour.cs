using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour
{
    public Transform MinPoint;
    public Transform MaxPoint;
    public Transform Target;

    void Update()
    {
        transform.position = Target.position - Vector3.forward;
    }

    void LateUpdate()
    {
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, MinPoint.position.x, MaxPoint.position.x);
        position.y = Mathf.Clamp(position.y, MinPoint.position.y, MaxPoint.position.y);
        transform.position = position;
    }
}
