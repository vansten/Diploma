using UnityEngine;
using System.Collections;

public class UsableObject : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    [SerializeField]
    private float _chance;

    private SpriteRenderer _renderer;
    
    private int _enemyLayer;
    private bool _inUse;

    void Awake()
    {
        _inUse = false;
        _enemyLayer = LayerMask.NameToLayer("Enemy");
        _renderer = GetComponent<SpriteRenderer>();
    }
    
    public void ReleaseMe()
    {
        _inUse = false;
        _renderer.enabled = true;
    }

    public void UseMe()
    {
        _renderer.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(_inUse)
        {
            return;
        }

        if (col.gameObject.layer == _enemyLayer)
        {
            float rand = Random.Range(0.0f, 1.0f);
            if(rand > (1.0f - _chance))
            {
                Enemy e = col.gameObject.GetComponent<Enemy>();
                if (e != null)
                {
                    e.UseObject(this);
                    _inUse = true;
                }
            }
        }
    }
}
