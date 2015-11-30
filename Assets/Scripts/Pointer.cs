using UnityEngine;
using System.Collections;

public class Pointer : MonoBehaviour
{
    void Start()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldMousePosition.z = 0.0f;
        transform.position = worldMousePosition;
    }
}
