using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour {

    public Vector3 Direction
    {
        get
        {
            return _direction;
        }
        set
        {
            _direction = value;
            _direction.Normalize();
            transform.up = _direction;
        }
    }

    private Vector3 _direction;
    private int _playerLayer;
    private int _blockingLayer;

    void Start()
    {
        _playerLayer = LayerMask.NameToLayer("Player");
        _blockingLayer = LayerMask.NameToLayer("Obstacle");
    }

    void Update()
    {
        transform.position += _direction * Time.deltaTime * 3.0f;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.layer == _playerLayer)
        {
            Humanoid h = col.gameObject.GetComponent<Humanoid>();
            if(h != null)
            {
                h.GetDamage(20);
            }
            Destroy(gameObject);
        }
        else if(col.gameObject.layer == _blockingLayer)
        {
            Destroy(gameObject);
        }
    }
}
