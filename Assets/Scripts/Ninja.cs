using UnityEngine;
using System.Collections;

public class Ninja : MonoBehaviour
{
    [SerializeField]
    private GameObject _ninjaSprite;
    [SerializeField]
    private Transform _pointerTransform;
    [SerializeField]
    private float _sprintMultiplier = 5.0f;

    private Animator _myAnimator;
    private float _sprintTimer = 0.0f;

    void Start()
    {
        _myAnimator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
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

    void LateUpdate()
    {
        Vector3 direction = _pointerTransform.position - transform.position;
        direction.Normalize();
        _ninjaSprite.transform.up = direction;
    }
}
