using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DistanceEnemy : Humanoid
{
    [SerializeField]
    private Arrow _arrowPrefab;

    private Animator _myAnimator;
    private List<Vector3> _wayToLastPlayerPosition;
    private List<Vector3> _wayToBasePosition;
    private Vector3 _basePosition;
    private Vector3 _lastKnownPlayerPosition;
    private int _index = 0;
    private int _basePositionIndex = 0;
    private int _playerLayer;
    private int _blockingLayer;
    private bool _playerSeen;
    private bool _playerLost;
    private bool _returning;

    void Awake()
    {
        _myAnimator = GetComponent<Animator>();
        _blockingLayer = LayerMask.NameToLayer("Obstacle");
        _playerLayer = LayerMask.NameToLayer("Player");
        _basePosition = transform.position;
        _playerSeen = false;
        _returning = false;
    }

    public TaskStatus Attack(GameObject owner)
    {
        if(!_playerSeen)
        {
            return TaskStatus.FAILURE;
        }

        _myAnimator.SetBool("isAttacking", true);
        LookAt(WorldManager.Instance.Player.transform.position);
        return TaskStatus.RUNNING;
    }

    public TaskStatus Chase(GameObject owner)
    {
        if(_playerSeen)
        {
            return TaskStatus.SUCCESS;
        }

        if(!_playerLost)
        {
            return TaskStatus.FAILURE;
        }

        if(_returning)
        {
            return TaskStatus.FAILURE;
        }

        float howFarFromBase = (transform.position - _basePosition).magnitude;
        if(howFarFromBase > 5.0f || _index >= _wayToLastPlayerPosition.Count)
        {
            _returning = true;
            SetPlayerSeenLost(false, false);
            _wayToBasePosition = NavigationManager.Instance.FindWay(transform.position, _basePosition);
            _basePositionIndex = 0;
            return TaskStatus.FAILURE;
        }

        Vector3 dir = _wayToLastPlayerPosition[_index] - transform.position;
        transform.position += dir.normalized * Time.deltaTime * 0.75f;
        LookAt(_wayToLastPlayerPosition[_index]);
        _myAnimator.SetBool("isWalking", true);
        if(dir.magnitude < 0.1f)
        {
            _index += 1;
            if(_index >= _wayToLastPlayerPosition.Count)
            {
                return TaskStatus.SUCCESS;
            }
        }

        return TaskStatus.RUNNING;
    }

    public TaskStatus ReturnToBasePosition(GameObject owner)
    {
        if(_playerSeen)
        {
            return TaskStatus.FAILURE;
        }

        Vector3 dist = _basePosition - transform.position;
        if(dist.magnitude < 0.1f)
        {
            _returning = false;
            return TaskStatus.SUCCESS;
        }

        if(_wayToBasePosition == null)
        {
            _wayToBasePosition = NavigationManager.Instance.FindWay(transform.position, _basePosition);
            _basePositionIndex = 0;
        }

        dist = _wayToBasePosition[_basePositionIndex] - transform.position;
        transform.position += dist.normalized * Time.deltaTime * 1.3f;
        LookAt(_wayToBasePosition[_basePositionIndex]);
        _myAnimator.SetBool("isWalking", true);
        if (dist.magnitude < 0.1f)
        {
            _basePositionIndex += 1;
            if(_basePositionIndex >= _wayToBasePosition.Count)
            {
                return TaskStatus.SUCCESS;
            }
        }

        return TaskStatus.RUNNING;
    }

    public TaskStatus Stay(GameObject owner)
    {
        _myAnimator.SetBool("isWalking", false);
        return TaskStatus.SUCCESS;
    }

    public void OnAttack()
    {
        //Shoot at player
        _myAnimator.SetBool("isAttacking", false);
        Arrow a = Instantiate(_arrowPrefab);
        a.transform.position = transform.position;
        a.Direction = WorldManager.Instance.Player.transform.position - transform.position;
    }

    void SetPlayerSeenLost(bool seen, bool lost)
    {
        if(lost && !_playerLost)
        {
            //Last player position
            _lastKnownPlayerPosition = WorldManager.Instance.Player.transform.position;
            _wayToLastPlayerPosition = NavigationManager.Instance.FindWay(transform.position, _lastKnownPlayerPosition);
            _index = 0;
        }
        _playerSeen = seen;
        _playerLost = lost;
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if(col.gameObject.layer == _playerLayer)
        {
            Vector2 dir = col.transform.position - transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir.normalized, dir.magnitude, 1 << _blockingLayer);
            if(hit.collider != null)
            {
                if(_playerSeen)
                {
                    SetPlayerSeenLost(false, true);
                }
            }
            else
            {
                SetPlayerSeenLost(true, false);
            }
        }
    }
    
    protected void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.layer == _playerLayer && _playerSeen)
        {
            SetPlayerSeenLost(false, true);
        }
    }
}