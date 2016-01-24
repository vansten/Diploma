using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PatrolMode
{
    Loop,
    ThereAndBack
}

public class Enemy : Humanoid
{
    [SerializeField]
    private List<Transform> _patrolWaypoints;
    [SerializeField]
    private PatrolMode _patrolMode;
    [SerializeField]
    protected GameObject _enemySprite;
    [SerializeField]
    protected float _hpThreshold;

    protected Animator _enemyAnimator;
    protected SpriteRenderer _enemySpriteRenderer;

    protected LayerMask _blockingLayer;

    protected bool _playerSeen;
    protected bool _playerLost;

    protected float _attackTimer = 0.0f;

    protected Ninja _player;
    protected List<Vector3> _wayToPlayerLastPosition;
    protected int _currentIndexToPlayer;

    protected float _searchingTimer = 0.0f;
    protected bool _reachedLastPoint;
    protected Vector3 _initialRotation;
    protected Vector3 _targetRotation;
    protected Vector3 _targetRotation2;

    protected Transform _currentTarget;
    protected bool _reachedTarget = false;

    protected List<Vector3> _wayToCurrentTarget = new List<Vector3>();
    protected int _currentWaypoint = 0;
    protected int _currentPatrolWaypoint = 0;
    protected int _patrolDirection = 1;

    protected float _healingTimer = 0.0f;
    protected bool _healingPlaceFound = false;
    protected bool _healingPlaceReached = false;
    protected bool _healing = false;

    protected float _idleTimer = 0.0f;
    protected bool _idle;
    protected UsableObject _someObject;
    protected List<Vector3> _wayToSomeObject;
    protected int _currentIndexOfWay;
    protected bool _usingObject;

    protected int _playerLayer;

    void Awake()
    {
        _playerLayer = LayerMask.NameToLayer("Player");
        _blockingLayer = LayerMask.NameToLayer("Obstacle");
        _enemySpriteRenderer = _enemySprite.GetComponent<SpriteRenderer>();
        _enemyAnimator = GetComponent<Animator>();
    }

    public TaskStatus Flee(GameObject owner)
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
        _enemyAnimator.SetBool("isWalking", true);
        LookAt(_wayToCurrentTarget[_currentWaypoint]);

        if (magnitude < 0.12f)
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

    public TaskStatus Chase(GameObject owner)
    {
        if(_hp < _hpThreshold)
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
        if (currentDistance < 0.65f)
        {
            return TaskStatus.SUCCESS;
        }
        dir.Normalize();
        LookAt(_player.transform.position);
        transform.position += dir * Time.deltaTime;
        _enemyAnimator.SetBool("isWalking", true);

        return TaskStatus.RUNNING;
    }

    public TaskStatus Attack(GameObject owner)
    {
        if (!_playerSeen || _playerLost)
        {
            _attackTimer = 0.0f;
            _enemyAnimator.SetBool("isWalking", false);
            return TaskStatus.FAILURE;
        }

        if (_player != null && _player.IsDead)
        {
            _attackTimer = 0.0f;
            _enemyAnimator.SetBool("isWalking", false);
            return TaskStatus.SUCCESS;
        }

        Vector3 dist = _player.transform.position - transform.position;
        LookAt(_player.transform.position);
        float distance = dist.magnitude;
        if(distance > 0.65f)
        {
            _attackTimer = 0.0f;
            _enemyAnimator.SetBool("isWalking", false);
            return TaskStatus.FAILURE;
        }

        _enemyAnimator.SetBool("isAttacking", true);

        return TaskStatus.RUNNING;
    }

    public TaskStatus Heal(GameObject owner)
    {
        if (!_healingPlaceReached)
        {
            return TaskStatus.FAILURE;
        }
        _enemyAnimator.SetBool("isWalking", false);

        _healing = true;
        _healingTimer += Time.deltaTime;
        if(_healingTimer > 0.5f)
        {
            _healingTimer = 0.0f;
            HP += 3;
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

    public TaskStatus SearchForEnemy(GameObject owner)
    {
        if (_hp < _hpThreshold)
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
        _enemyAnimator.SetBool("isWalking", true);
        LookAt(_wayToPlayerLastPosition[_currentIndexToPlayer]);
        if (distance < 0.12f)
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

    public TaskStatus Patrol(GameObject owner)
    {
        if (_hp < _hpThreshold)
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
        _enemyAnimator.SetBool("isWalking", true);
        LookAt(_wayToCurrentTarget[_currentWaypoint]);
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

    public TaskStatus Idle(GameObject owner)
    {
        if (_hp < _hpThreshold)
        {
            return TaskStatus.FAILURE;
        }

        if (_playerSeen)
        {
            return TaskStatus.FAILURE;
        }

        if (_usingObject && _idleTimer > 5.0f)
        {
            _idle = false;
            _someObject.ReleaseMe();
            _currentTarget = null;
            return TaskStatus.SUCCESS;
        }

        if (_usingObject)
        {
            _idleTimer += Time.deltaTime;
            _enemyAnimator.SetBool("isWalking", false);
            return TaskStatus.RUNNING;
        }

        Vector3 dist = _wayToSomeObject[_currentIndexOfWay] - transform.position;
        float magnitude = dist.magnitude;
        dist.Normalize();
        transform.position += dist * Time.deltaTime;
        _enemyAnimator.SetBool("isWalking", true);
        LookAt(_wayToSomeObject[_currentIndexOfWay]);
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

    protected void NextPatrolWaypoint()
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

    public void OnAttack()
    {
        _player.GetDamage(2);
        _enemyAnimator.SetBool("isAttacking", false);
    }

    protected void OnTriggerStay2D(Collider2D col)
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
                if (_playerSeen)
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
                if (!_playerSeen)
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

    protected void OnTriggerExit2D(Collider2D col)
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
