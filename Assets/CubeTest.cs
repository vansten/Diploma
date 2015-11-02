using UnityEngine;
using System.Collections;

public class CubeTest
{
    private static float _myFloat;

    public static TaskStatus Move(GameObject owner, Blackboard blackboard)
    {
        if (owner == null)
        {
            return TaskStatus.FAILURE;
        }

        Debug.Log("Moving");
        return TaskStatus.RUNNING;
    }

    public static TaskStatus Stay(GameObject owner, Blackboard blackboard)
    {
        if (owner == null)
        {
            return TaskStatus.FAILURE;
        }
        
        if(!blackboard.GetVariable("firstFloat", out _myFloat))
        {
            blackboard.AddVariable("firstFloat", 1.0f);
        }
        else
        {
            Debug.Log(_myFloat);
        }

        Debug.Log("Staying");
        return TaskStatus.SUCCESS;
    }
}
