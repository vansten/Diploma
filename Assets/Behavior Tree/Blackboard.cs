using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Blackboard : MonoBehaviour
{
    private Dictionary<string, object> _variables;

    public Blackboard()
    {
        _variables = new Dictionary<string, object>();
    }

    public void AddVariable<T>(string name, T value)
    {
        if(_variables.ContainsKey(name))
        {
            Helper.LogAndBreak("Blackboard named " + gameObject.name + " already contains key: " + name);
        }
        else
        {
            _variables.Add(name, value);
        }
    }

    public bool GetVariable<T>(string name, out T value)
    {
        if (!_variables.ContainsKey(name))
        {
            value = default(T);
            return false;
        }
        else
        {
            value = (T)_variables[name];
            return true;
        }
    }
}
