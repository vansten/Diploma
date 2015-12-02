using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{ 
    public virtual TaskStatus Flee(GameObject owner, Blackboard blackboard)
    {
        return TaskStatus.FAILURE;
    }

    public virtual TaskStatus TakeCover(GameObject owner, Blackboard blackboard)
    {
        return TaskStatus.FAILURE;
    }

    public virtual TaskStatus Attack(GameObject owner, Blackboard blackboard)
    {
        return TaskStatus.FAILURE;
    }

    public virtual TaskStatus Heal(GameObject owner, Blackboard blackboard)
    {
        return TaskStatus.FAILURE;
    }

    public virtual TaskStatus SearchForEnemy(GameObject owner, Blackboard blackboard)
    {
        return TaskStatus.FAILURE;
    }

    public virtual TaskStatus Patrol(GameObject owner, Blackboard blackboard)
    {
        return TaskStatus.FAILURE;
    }

    public virtual TaskStatus Idle(GameObject owner, Blackboard blackboard)
    {
        return TaskStatus.FAILURE;
    }
}
