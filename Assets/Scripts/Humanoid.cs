using UnityEngine;
using System.Collections;

public class Humanoid : MonoBehaviour
{
    [SerializeField]
    protected SpriteRenderer _hpBar;
    [SerializeField]
    protected int _initHP;

    protected int _hp;
    public int HP
    {
        get { return _hp; }
        set
        {
            _hp = value;
            if(_hp <= 0)
            {
                IsDead = true;
            }
            _hpBar.transform.localScale = 8.75f * _hp * 0.01f * Vector3.right + Vector3.up + Vector3.forward;
        }
    }

    protected bool _isDead;
    public bool IsDead
    {
        get
        {
            return _isDead;
        }
        protected set
        {
            _isDead = value;
            if(_isDead)
            {
                gameObject.SetActive(false);
            }
        }
    }

    protected virtual void OnEnable()
    {
        HP = _initHP;
        IsDead = false;
    }

	public virtual void GetDamage(int dmg)
    {
        HP -= dmg;
    }
    
    protected void LookAt(Vector3 point)
    {
        Vector3 dir = point - transform.position;
        Vector3 scale = transform.localScale;
        if (dir.x < 0.0f)
        {
            scale.x = -1.0f;
        }
        else
        {
            scale.x = 1.0f;
        }
        transform.localScale = scale;
    }
}
