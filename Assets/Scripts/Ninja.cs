using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ninja : Humanoid
{
    [SerializeField]
    private Shuriken _shurikenPrefab;
    [SerializeField]
    private GameObject _ninjaSprite;
    [SerializeField]
    private Transform _pointerTransform;
    [SerializeField]
    private float _sprintMultiplier = 5.0f;
    
    private Animator _myAnimator;
    private float _sprintTimer = 0.0f;
    private Vector3 _initPosition;
    private bool _godMode;

    public bool HasKey
    {
        get;
        private set;
    }

    void Start()
    {
        _myAnimator = GetComponent<Animator>();
        _initPosition = transform.position;
        _godMode = false;
        HasKey = false;
    }

    void FixedUpdate()
    {
        if(IsDead)
        {
            return;
        }

        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0.0f);
        if (Input.GetKey(KeyCode.LeftShift))
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
            _myAnimator.SetFloat("speed", multiplier);
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
                HP = _initHP;
            }
        }

        if(IsDead)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                transform.position = _initPosition;
                HP = _initHP;
                IsDead = false;
            }

            return;
        }

        if(Input.GetMouseButtonDown(0))
        {
            _myAnimator.SetBool("isAttacking", true);
        }
    }

    void LateUpdate()
    {
        if(IsDead)
        {
            return;
        }

        LookAt(_pointerTransform.position);
    }

    public override void GetDamage(int dmg)
    {
        if (_godMode) return;
        HP -= dmg;
    }

    public void Attack()
    {
        Vector3 dir = _pointerTransform.position - transform.position;
        dir.Normalize();
        Shuriken s = Instantiate(_shurikenPrefab);
        s.transform.position = transform.position;
        s.SetDirection(dir);
        _myAnimator.SetBool("isAttacking", false);
    }

    public void GetKey()
    {
        HasKey = true;
    }
}
