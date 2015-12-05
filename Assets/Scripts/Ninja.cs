using UnityEngine;
using System.Collections;

public class Ninja : MonoBehaviour
{
    [HideInInspector]
    public bool IsDead;

    [SerializeField]
    private Shuriken _shurikenPrefab;
    [SerializeField]
    private GameObject _ninjaSprite;
    [SerializeField]
    private Transform _pointerTransform;
    [SerializeField]
    private float _sprintMultiplier = 5.0f;
    [SerializeField]
    private int _hp;

    private Animator _myAnimator;
    private float _sprintTimer = 0.0f;
    private Vector3 _initPosition;
    private int _initHP;
    private bool _godMode;

    void Start()
    {
        _myAnimator = GetComponent<Animator>();
        IsDead = false;
        _initPosition = transform.position;
        _initHP = _hp;
        _godMode = false;
    }

    void FixedUpdate()
    {
        if(IsDead)
        {
            return;
        }

        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0.0f);
        if(Input.GetKey(KeyCode.LeftShift))
        {
            _sprintTimer += Time.deltaTime;
        }
        else
        {
            _sprintTimer -= 5.0f * Time.deltaTime;
        }
        _sprintTimer = Mathf.Clamp01(_sprintTimer);
        float multiplier = Mathf.Lerp(1.0f, _sprintMultiplier, _sprintTimer);
        
        transform.position += movement * Time.deltaTime * multiplier;

        if(_myAnimator != null)
        {
            _myAnimator.SetBool("isWalking", movement.magnitude > 0.0f);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            _godMode = !_godMode;
            if (_godMode)
            {
                IsDead = false;
                _hp = _initHP;
            }
        }

        if(IsDead)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                transform.position = _initPosition;
                _hp = _initHP;
                IsDead = false;
            }

            return;
        }

        if(Input.GetMouseButtonDown(0))
        {
            Vector3 dir = _pointerTransform.position - transform.position;
            dir.Normalize();
            Shuriken s = Instantiate(_shurikenPrefab);
            s.transform.position = transform.position;
            s.SetDirection(dir);
        }
    }

    void LateUpdate()
    {
        if(IsDead)
        {
            return;
        }

        Vector3 direction = _pointerTransform.position - transform.position;
        direction.Normalize();
        _ninjaSprite.transform.up = direction;
    }

    public void GetDamage(int dmg)
    {
        if(_godMode)
        {
            return;
        }

        _hp -= dmg;
        if(_hp < 0)
        {
            _hp = 0;
            IsDead = true;
        }
    }
}
