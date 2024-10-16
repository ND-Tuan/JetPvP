using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepOriginRotation : MonoBehaviour
{
    private Quaternion _lastParentRotation;

    void Start()
    {
        _lastParentRotation = transform.parent.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localRotation = Quaternion.Inverse(transform.parent.localRotation) * _lastParentRotation
                                    * transform.localRotation;

        _lastParentRotation = transform.parent.localRotation;
    }
}
