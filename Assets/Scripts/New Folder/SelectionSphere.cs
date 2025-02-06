using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionSphere : MonoBehaviour
{
    public bool insideObject;
    public GameObject touchingObject;

    private void OnTriggerEnter(Collider other)
    {
        insideObject = true;
        touchingObject = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        insideObject = false;
        touchingObject = null;
    }

}
