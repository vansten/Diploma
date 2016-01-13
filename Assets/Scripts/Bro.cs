using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bro : Humanoid
{
    [SerializeField]
    private GameObject _book;
    [SerializeField]
    private Transform _startPosition;

    private List<Vector3> _wayToBook;
    private int _index;
    private List<Vector3> _wayToExit;
    private int _exitIndex;

    protected override void OnEnable()
    {
        base.OnEnable();
        _hpBar.enabled = false;
    }

    public TaskStatus GrabBook(GameObject owner)
    {
        if(_book == null)
        {
            _index = 0;
            return TaskStatus.SUCCESS;
        }
        
        if(_wayToBook == null)
        {
            _wayToBook = NavigationManager.Instance.FindWay(_startPosition.position, _book.transform.position);
            _index = 0;
        }

        Vector3 dist = _wayToBook[_index] - transform.position;
        transform.position += dist.normalized * Time.deltaTime * 1.5f;
        LookAt(_wayToBook[_index]);
        if(dist.magnitude < 0.1f)
        {
            _index += 1;
            if(_index >= _wayToBook.Count)
            {
                _index = 0;
                _wayToBook = null;
                _book.SetActive(false);
                _book = null;
                return TaskStatus.SUCCESS;
            }
        }

        return TaskStatus.RUNNING;
    }

    public TaskStatus RunAway(GameObject owner)
    {
        if(_wayToExit == null)
        {
            _wayToExit = NavigationManager.Instance.FindWay(transform.position, WorldManager.Instance.Exit.position);
            _exitIndex = 0;
        }

        Vector3 dist = _wayToExit[_exitIndex] - transform.position;
        transform.position += dist.normalized * Time.deltaTime * 1.5f;
        LookAt(_wayToExit[_exitIndex]);
        if (dist.magnitude < 0.1f)
        {
            _exitIndex += 1;
            if (_exitIndex >= _wayToExit.Count)
            {
                _exitIndex = 0;
                _wayToExit = null;
                owner.SetActive(false);
                return TaskStatus.SUCCESS;
            }
        }

        return TaskStatus.RUNNING;
    }
}
