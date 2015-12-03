using UnityEngine;
using System.Collections;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T _instance;
    
    /// <summary>
    /// Get instance of class T
    /// </summary>
    public static T Instance
    {
        get
        {
            if(!_instance)
            {
                _instance = FindObjectOfType<T>();
                if(_instance == null)
                {
                    _instance = new GameObject(typeof(T).ToString(), typeof(T)).GetComponent<T>();
                    if(_instance == null)
                    {
                        Debug.LogError("Error while creating singleton of type " + typeof(T).ToString());
                    }
                }
            }

            return _instance;
        }
    }

    /// <summary>
    /// Override if necessary and call base.Awake() first!
    /// </summary>
    protected virtual void Awake()
    {
        if(_instance == null)
        {
            _instance = (T)this;
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// Override when necessary and call base.OnApplicationQuit() last!
    /// </summary>
    protected virtual void OnApplicationQuit()
    {
        _instance = null;
    }
}
