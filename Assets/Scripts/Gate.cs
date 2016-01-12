using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Gate : MonoBehaviour
{
    [SerializeField]
    private List<BehaviorTreeComponent> _prisoners;

    private int _playerLayer;
    private bool _playerNear;

    void Start()
    {
        _playerLayer = LayerMask.NameToLayer("Player");
    }

    void Update()
    {
        if(_playerNear)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                OpenGate();
            }
        }
    }

    void OpenGate()
    {
        if(!WorldManager.Instance.Player.HasKey)
        {
            return;
        }
        EKey.Instance.Deactivate();
        if (_prisoners != null)
        {
            foreach(BehaviorTreeComponent prisoner in _prisoners)
            {
                prisoner.Run();
            }
        }

        gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.layer == _playerLayer)
        {
            _playerNear = true;
            if(WorldManager.Instance.Player.HasKey)
            {
                EKey.Instance.Activate();
            }
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.layer == _playerLayer)
        {
            _playerNear = false;
            EKey.Instance.Deactivate();
        }
    }
}
