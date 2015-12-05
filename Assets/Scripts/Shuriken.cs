using UnityEngine;
using System.Collections;

public class Shuriken : MonoBehaviour
{
    private Vector3 _direction;

    private int _hitCount = 0;

    void Update()
    {
        transform.position += _direction * Time.deltaTime * 6.0f;
    }

    public void SetDirection(Vector3 dir)
    {
        _direction = dir;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Enemy e = col.gameObject.GetComponent<Enemy>();
        if(e != null)
        {
            e.GetDamage(10);
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        _hitCount += 1;
        if(_hitCount > 5)
        {
            Destroy(gameObject);
        }
        Vector3 normal = col.contacts[0].normal;
        if(normal.x != 0.0f)
        {
            _direction.x *= -1.0f;
        }
        else
        {
            _direction.y *= -1.0f;
        }
    }
}
