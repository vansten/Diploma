using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldManager : Singleton<WorldManager>
{
    [SerializeField]
    private List<Transform> _healingPlaces;
    
    public Transform FindNearestHealingPlace(Vector3 position)
    {
        int distance = int.MaxValue;
        int minIndex = 0;
        foreach(Transform t in _healingPlaces)
        {
            int dist = NavigationManager.Instance.FindWay(position, t.position).Count;
            if(dist < distance)
            {
                distance = dist;
                minIndex = _healingPlaces.IndexOf(t);
            }
        }

        return _healingPlaces[minIndex];
    }
}
