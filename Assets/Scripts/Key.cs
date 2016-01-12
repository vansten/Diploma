using UnityEngine;
using System.Collections;

public class Key : MonoBehaviour
{
    private Ninja ninjaComp;
    private int _playerLayer;

    void Start()
    {
        _playerLayer = LayerMask.NameToLayer("Player");
    }

    void Update()
    {
        if(ninjaComp != null)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                ninjaComp.GetKey();
                EKey.Instance.Deactivate();
                gameObject.SetActive(false);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.layer == _playerLayer)
        {
            ninjaComp = col.GetComponent<Ninja>();
            EKey.Instance.Activate();
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if(ninjaComp != null && ninjaComp.gameObject == col.gameObject)
        {
            ninjaComp = null;
            EKey.Instance.Deactivate();
        }
    }
}
