using UnityEngine;
using System.Collections;

public class EKey : Singleton<EKey>
{
    [SerializeField]
    private UnityEngine.UI.Image _imageComp;
    private float _timer = 0.0f;
    private float _speed = 2.0f;
    private bool _rise = true;

    void Start()
    {
        Deactivate();
    }

    void OnEnable()
    {
        _timer = 0.0f;
        Color c = _imageComp.color;
        c.a = 0.0f;
        _imageComp.color = c;
        _rise = true;
    }

    void Update()
    {
        if(_rise)
        {
            _timer += Time.deltaTime * _speed;
            if(_timer >= 1.0f)
            {
                _rise = false;
            }
        }
        else
        {
            _timer -= Time.deltaTime * _speed;
            if (_timer <= 0.0f)
            {
                _rise = true;
            }
        }

        Color c = _imageComp.color;
        c.a = _timer;
        _imageComp.color = c;
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}