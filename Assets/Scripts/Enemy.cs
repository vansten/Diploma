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

    private LayerMask _blockingLayer;

    private int _hp;

    private bool _playerSeen;
    private bool _playerLost;

    private float _attackTimer = 0.0f;

    private Ninja _player;
    private List<Vector3> _wayToPlayerLastPosition;
    private int _currentIndexToPlayer;

    private float _searchingTimer = 0.0f;
    private bool _reachedLastPoint;
    private Vector3 _initialRotation;
    private Vector3 _targetRotation;
    private Vector3 _targetRotation2;

    private Transform _currentTarget;
    private bool _reachedTarget = false;

    private List<Vector3> _wayToCurrentTarget = new List<Vector3>();
    private int _currentWaypoint = 0;
    private int _currentPatrolWaypoint = 0;
    private int _patrolDirection = 1;

    private float _healingTimer = 0.0f;
    private bool _healingPlaceFound = false;
    private bool _healingPlaceReached = false;
    private bool _healing = false;

    private float _idleTimer = 0.0f;
    private bool _idle;
    private UsableObject _someObject;
    private List<Vector3> _wayToSomeObject;
    private int _currentIndexOfWay;
    private bool _usingObject;

    private int _playerLayer;

    void Awake()
    {
        _playerLayer = LayerMask.NameToLayer("Player");
        _blockingLayer = LayerMask.NameToLayer("Obstacle");
    }

    void OnEnable()
    {
        _hp = 100;
    }

    public virtual TaskStatus Flee(GameObject owner, Blackboard blackboard)
    {
        if (_healingPlaceReached)
        {
            return TaskStatus.SUCCESS;
        }

        if (_hp > 50)
        {
            return TaskStatus.FAILURE;
        }

        //Find way to healing place
        if(_currentTarget == null || !_healingPlaceFound)
        {
            _healingPlaceReached = false;
            _healingPlaceFound = true;
            _currentTarget = WorldManager.Instance.FindNearestHealingPlace(transform.position);
            _wayToCurrentTarget = NavigationManager.Instance.FindWay(transform.position, _currentTarget.position);
            _currentWaypoint = 0;
        }
        
        Vector3 dist = _wayToCurrentTarget[_currentWaypoint] - transform.position;
        float magnitude = dist.magnitude;
        dist.Normalize();
        transform.position += dist * Time.deltaTime * 2.5f;
        
        if(magnitude < 0.12f)
        {
            _currentWaypoint += 1;
            if(_currentWaypoint >= _wayToCurrentTarget.Count)
            {
                _healingPlaceReached = true;
                   _healingTimer = 0.0f;
                return TaskStatus.SUCCESS;
            }
        }

        return TaskStatus.RUNNING;
    }

    public virtual TaskStatus Chase(GameObject owner, Blackboard blackboard)
    {
        if(_hp < 30)
        {
            return TaskStatus.FAILURE;
        }

        if (!_playerSeen || _playerLost)
        {
            return TaskStatus.FAILURE;
        }

        if(_player != null && _player.IsDead)
        {
            return TaskStatus.FAILURE;
        }

        Vector3 dir = _player.transform.position - transform.position;
        float currentDistance = dir.magnitude;
        if (currentDistance < 0.5f)
        {
            return TaskStatus.FAILURE;
        }
        dir.Normalize();
        transform.position += dir * Time.deltaTime;

        return TaskStatus.RUNNING;
    }

    public virtual TaskStatus Attack(GameObject owner, Blackboard blackboard)
    {
        if (!_playerSeen || _playerLost)
        {
            _attackTimer = 0.0f;
            return TaskStatus.FAILURE;
        }

        if (_player != null && _player.IsDead)
        {
            _attackTimer = 0.0f;
            return TaskStatus.FAILURE;
        }

        Vector3 dist = _player.transform.position - transform.position;
        float distance = dist.magnitude;
        if(distance > 0.65f)
        {
            _attackTimer = 0.0f;
            return TaskStatus.FAILURE;
        }

        _attackTimer += Time.deltaTime;
        if(_attackTimer > 0.5f)
        {
            _attackTimer = 0.0f;
            _player.GetDamage(2);
            return TaskStatus.RUNNING;
        }

        return TaskStatus.RUNNING;
    }

    public virtual TaskStatus Heal(GameObject owner, Blackboard blackboard)
    {
        if (!_healingPlaceReached)
        {
            return TaskStatus.FAILURE;
        }

        _healing = true;
        _healingTimer += Time.deltaTime;
        if(_healingTimer > 0.5f)
        {
            _healingTimer = 0.0f;
            _hp += 3;
        }

        if(_hp > 75)
        {
            _healingPlaceReached = false;
            _currentTarget = null;
            _healingPlaceFound = false;
            _healing = false;
            return TaskStatus.SUCCESS;
        }

        return TaskStatus.RUNNING;
    }

    public virtual TaskStatus SearchForEnemy(GameObject owner, Blackboard blackboard)
    {
        if (_hp < 30)
        {
            return TaskStatus.FAILURE;
        }

        if (!_playerLost)
        {
            return TaskStatus.FAILURE;
        }

        if(_playerSeen)
        {
            return TaskStatus.SUCCESS;
        }

        if (_player != null && _player.IsDead)
        {
            return TaskStatus.FAILURE;
        }

        if (_reachedLastPoint)
        {
            _searchingTimer += Time.deltaTime;
            if(_searchingTimer > 3.0f)
            {
                float lerpTime = _searchingTimer - 3.0f;
                _enemySprite.transform.rotation = Quaternion.Euler(Vector3.Lerp(_targetRotation2, _initialRotation, lerpTime));
            }
            else if(_searchingTimer > 1.0f)
            {
                float lerpTime = 0.5f * (_searchingTimer - 1.0f);
                _enemySprite.transform.rotation = Quaternion.Euler(Vector3.Lerp(_targetRotation, _targetRotation2, lerpTime));
            }
            else
            {
                _enemySprite.transform.rotation = Quaternion.Euler(Vector3.Lerp(_initialRotation, _targetRotation, _searchingTimer));
            }

            if(_searchingTimer > 4.0f)
            {
                _reachedLastPoint = false;
                _searchingTimer = 0.0f;
                _playerLost = false;
                return TaskStatus.SUCCESS;
            }

            return TaskStatus.RUNNING;
        }

        Vector3 dist = _wayToPlayerLastPosition[_currentIndexToPlayer] - transform.position;
        float distance = dist.magnitude;
        dist.Normalize();
        transform.position += dist * Time.deltaTime * 0.5f;
        if(distance < 0.12f)
        {
            _currentIndexToPlayer += 1;
            if(_currentIndexToPlayer >= _wayToPlayerLastPosition.Count)
            {
                _reachedLastPoint = true;
                _searchingTimer = 0.0f;
                _initialRotation = transform.rotation.eulerAngles;
                _targetRotation = _initialRotation + new Vector3(0.0f, 0.0f, 30.0f);
                _targetRotation2 = _initialRotation - new Vector3(0.0f, 0.0f, 30.0f);
            }
        }

        return TaskStatus.RUNNING;
    }

    public virtual TaskStatus Patrol(GameObject owner, Blackboard blackboard)
    {
        if (_hp < 30)
        {
            return TaskStatus.FAILURE;
        }

        if (_idle)
        {
            return TaskStatus.FAILURE;
        }

        if(_playerSeen)
        {
            return TaskStatus.SUCCESS;
        }

        if (_currentTarget == null || _reachedTarget)
        {
            _reachedTarget = false;
            _currentTarget = _patrolWaypoints[_currentPatrolWaypoint];
            _currentWaypoint = 0;
            _wayToCurrentTarget = NavigationManager.Instance.FindWay(transform.position, _currentTarget.position);
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
                NextPatrolWaypoint();

                _reachedTarget = true;

                return TaskStatus.SUCCESS;
            }
        }

        return TaskStatus.RUNNING;
    }

    public virtual TaskStatus Idle(GameObject owner, Blackboard blackboard)
    {
        if (_hp < 30)
        {
            return TaskStatus.FAILURE;
        }

        if (_playerSeen)
        {
            return TaskStatus.FAILURE;
        }

        if(_usingObject && _idleTimer > 5.0f)
        {
            _idle = false;
            _someObject.ReleaseMe();
            _currentTarget = null;
            return TaskStatus.SUCCESS;
        }

        if(_usingObject)
        {
            _idleTimer += Time.deltaTime;
            return TaskStatus.RUNNING;
        }

        Vector3 dist = _wayToSomeObject[_currentIndexOfWay] - transform.position;
        float magnitude = dist.magnitude;
        dist.Normalize();
        transform.position += dist * Time.deltaTime;
        if(magnitude < 0.12f)
        {
            _currentIndexOfWay += 1;
            if(_currentIndexOfWay >= _wayToSomeObject.Count)
            {
                _usingObject = true;
                _someObject.UseMe();
            }
        }
        
        return TaskStatus.RUNNING;
    }

    public void UseObject(UsableObject usable)
    {
        _idle = true;
        _usingObject = false;
        _currentIndexOfWay = 0;
        _someObject = usable;
        _wayToSomeObject = NavigationManager.Instance.FindWay(transform.position, _someObject.transform.position);
    }

    public void GetDamage(int dmg)
    {
        if(_healing)
        {
            return;
        }

        _hp -= dmg;
        if(_hp < 0)
        {
            _hp = 0;
        }
    }

    private void NextPatrolWaypoint()
    {
        _currentPatrolWaypoint += _patrolDirection;
        if (_currentPatrolWaypoint >= _patrolWaypoints.Count)
        {
            switch (_patrolMode)
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

        if (_currentPatrolWaypoint < 0)
        {
            _currentPatrolWaypoint = 0;
            if (_patrolMode == PatrolMode.ThereAndBack)
            {
                _patrolDirection = 1;
            }
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.layer == _playerLayer)
        {
            Vector3 dir = col.transform.position - transform.position;
            float distanceFromPlayer = dir.magnitude;
            dir.Normalize();
            Debug.DrawRay(transform.position, dir * distanceFromPlayer, Color.red);
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dir, distanceFromPlayer);

            bool wallDetected = false;
            foreach(RaycastHit2D hit in hits)
            {
                int hitLayer = hit.collider.gameObject.layer;
                if(hitLayer == _blockingLayer)
                {
                    wallDetected = true;
                }
            }

            if(wallDetected)
            {
                if(_playerSeen)
                {
                    _playerSeen = false;
                    _playerLost = true;
                    _wayToPlayerLastPosition = NavigationManager.Instance.FindWay(transform.position, col.transform.position);
                    _currentTarget = null;
                    NextPatrolWaypoint();
                    _currentIndexToPlayer = 0;
                }
            }
            else
            {
                if(!_playerSeen)
                {
                    _playerLost = false;
                    _playerSeen = true;
                    if(_player == null)
                    {
                        _player = col.gameObject.GetComponent<Ninja>();
                    }
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if(col.gameObject.layer == _playerLayer && _playerSeen)
        {
            _playerSeen = false;
            _playerLost = true;
            _wayToPlayerLastPosition = NavigationManager.Instance.FindWay(transform.position, col.transform.position);
            _currentTarget = null;
            NextPatrolWaypoint();
            _currentIndexToPlayer = 0;
        }
    }
}
