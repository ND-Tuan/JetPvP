using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObserverPattern{

    public class Observer : MonoBehaviour
{
    static Dictionary<EvenID, List<Action<object[]>>> _Listener = new Dictionary<EvenID, List<Action<object[]>>>();

    static public void AddListener(EvenID evenID, Action<object[]> callback)
    {
        if (!_Listener.ContainsKey(evenID))
        {
            _Listener.Add(evenID, new List<Action<object[]>>());
        }
        _Listener[evenID].Add(callback);
    }

    static public void RemoveListener(EvenID evenID, Action<object[]> callback)
    {
        if (_Listener.ContainsKey(evenID))
        {
            _Listener[evenID].Remove(callback);
        }
    }

    public static void PostEvent(EvenID evenID, params object[] data)
    {
        if (_Listener.ContainsKey(evenID))
        {
            foreach (var item in _Listener[evenID])
            {
                item(data);
            }
        }
    }

}
}

