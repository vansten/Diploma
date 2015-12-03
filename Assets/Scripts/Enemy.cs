using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PatrolMode
{
    Loop,
    ThereAndBack
}

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private List<Transform> _patrolWaypoints;
    [SerializeField]
    private PatrolMode _patrolMode;
    [SerializeField]
    private GameObject _enemySprite;

    private Transform _currentTarget;
    private bool _reachedTarget = false;

    private List<Vector3> _wayToCurrentTarget = new List<Vector3>();
    private int _currentWaypoint = 0;
    private int _currentPatrolWaypoint = 0;
    private int _patrolDirection = 1;

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
        if (_currentTarget == null || _reachedTarget)
        {
            _reachedTarget = false;
            _currentTarget = _patrolWaypoints[_currentPatrolWaypoint];
            FindWay();
        }

        if (_wayToCurrentTarget == null)
        {
            return TaskStatus.FAILURE;
        }

        Vector3 dist = _wayToCurrentTarget[_currentWaypoint] - transform.position;
        float magnitude = dist.magnitude;
        dist.Normalize();
        transform.position += dist * Time.deltaTime;
        if (magnitude < 0.12f)
        {
            _currentWaypoint += 1;
            if (_currentWaypoint >= _wayToCurrentTarget.Count)
            {
                _currentPatrolWaypoint += _patrolDirection;
                if(_currentPatrolWaypoint >= _patrolWaypoints.Count)
                {
                    switch(_patrolMode)
                    {
                        case PatrolMode.Loop:
                            _currentPatrolWaypoint = 1;
                            break;
                        case PatrolMode.ThereAndBack:
                            _currentPatrolWaypoint -= 1;
                            _patrolDirection = -1;
                            break;
                    }
                }

                if(_currentPatrolWaypoint < 0)
                {
                    _currentPatrolWaypoint = 0;
                    if (_patrolMode == PatrolMode.ThereAndBack)
                    {
                        _patrolDirection = 1;
                    }
                }

                _reachedTarget = true;

                return TaskStatus.SUCCESS;
            }
        }

        return TaskStatus.RUNNING;
    }

    public virtual TaskStatus Idle(GameObject owner, Blackboard blackboard)
    {
        return TaskStatus.RUNNING;
    }

    private void FindWay()
    {
        _currentWaypoint = 0;
        _wayToCurrentTarget = NavigationManager.Instance.FindWay(transform.position, _currentTarget.position);
    }
}
