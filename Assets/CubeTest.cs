using UnityEngine;
using System.Collections;

public class CubeTest
{
    public static TaskStatus Move(GameObject owner)
    {
        if (owner == null)
        {
            return TaskStatus.FAILURE;
        }

        Debug.Log("Moving");
        return TaskStatus.RUNNING;
    }

    public static TaskStatus Stay(GameObject owner)
    {
        if (owner == null)
        {
            return TaskStatus.FAILURE;
        }

        Debug.Log("Staying");
        return TaskStatus.SUCCESS;
    }
}
