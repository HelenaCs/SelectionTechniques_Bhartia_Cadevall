using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

// Your implemented technique inherits the InteractionTechnique class
public class MyTechnique : InteractionTechnique
{
    // You must implement your technique in this file
    // You need to assign the selected Object to the currentSelectedObject variable
    // Then it will be sent through a UnityEvent to another class for handling

    [SerializeField]
    int raycastMaxDistance = 1000;

    [SerializeField] private GameObject rightController;

    [SerializeField] private GameObject leftController;

    [SerializeField] private GameObject centerEyeAnchor;

    public GameObject selectionSphere;
    SelectionSphere selectionSphereScript;
    GameObject objectToSelect;

    public Material translucentMaterial, selectedMaterial, translucentRed;
    public GameObject highlightedObject;
    Dictionary<GameObject, Material> translucentMaterialsDict = new Dictionary<GameObject, Material>();

    private LineRenderer lineRenderer;

    public List<GameObject> hitObjects = new List<GameObject>();

    public GameObject cameraHolder;

    private void Start()
    {
        lineRenderer = leftController.GetComponent<LineRenderer>();
        selectionSphereScript = selectionSphere.GetComponent<SelectionSphere>();
    }

    private void Update()
    {
        //TODO : Select a GameObject and assign it to the currentSelectedObject variable

        // Make selector sphere visible when pressing left controller index button
        if (OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger) > 0.1f)
        {
            // Sending the selected object hit by the raycast
            selectionSphere.SetActive(true);
        }
        else
        {
            selectionSphere.SetActive(false);
        }

        if (selectionSphere.activeSelf) makeSphereVisibleTranlucentObjects();

        if (selectionSphereScript.insideObject)
        {
            if (!highlightedObject || highlightedObject != selectionSphereScript.touchingObject)
            {
                if (highlightedObject)
                {
                    highlightedObject.GetComponent<MeshRenderer>().materials = new List<Material>() { highlightedObject.GetComponent<MeshRenderer>().materials[0] }.ToArray();
                }
                    
                highlightedObject = selectionSphereScript.touchingObject;
                Material ogMat =  highlightedObject.GetComponent<MeshRenderer>().material;
                List<Material> materialsSelect = new List<Material>();
                materialsSelect.Add(ogMat);
                materialsSelect.Add(selectedMaterial);
                highlightedObject.GetComponent<MeshRenderer>().materials = materialsSelect.ToArray();
            }
        }
        else 
        {
            if (highlightedObject)
            {
                highlightedObject.GetComponent<MeshRenderer>().materials = new List<Material>() { highlightedObject.GetComponent<MeshRenderer>().materials[0] }.ToArray();
            }
            highlightedObject = null;
        }

        if (OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) > 0.1f && selectionSphereScript.insideObject)
        {
            // Sending the selected object hit by the raycast
            currentSelectedObject = selectionSphereScript.touchingObject;
        }

        //  Movement of the camera with the right controller joystick and the head's direction
        Vector2 rawAxisThumbStick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        Quaternion rotation = Quaternion.Euler(0, centerEyeAnchor.transform.eulerAngles.y, 0);
        cameraHolder.transform.position += rotation*new Vector3(rawAxisThumbStick.x, 0, rawAxisThumbStick.y)*0.2f;

        // DO NOT REMOVE
        // If currentSelectedObject is not null, this will send it to the TaskManager for handling
        // Then it will set currentSelectedObject back to null
        base.CheckForSelection();
    }

    private void FixedUpdate()
    {
        Transform leftControllerTransform = leftController.transform;
        Transform rightControllerTransform = rightController.transform;

        // Set the beginning of the line renderer to the position of the controller
        lineRenderer.SetPosition(0, leftControllerTransform.position);

        // Creating a raycast and storing the first hit if existing
        RaycastHit hit;
        bool hasHit = Physics.Raycast(leftControllerTransform.position, leftControllerTransform.forward, out hit, Mathf.Infinity);

        if (hasHit)
        {
            lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            lineRenderer.SetPosition(1, raycastMaxDistance * leftControllerTransform.forward);
        }

        Vector3 relativePos = rightControllerTransform.position - leftControllerTransform.position;
        relativePos = relativePos.magnitude * relativePos.normalized * 8f;

        selectionSphere.transform.position = hit.point + relativePos;
        if (selectionSphereScript.insideObject) objectToSelect = selectionSphereScript.touchingObject;


    }

    public void makeSphereVisibleTranlucentObjects ()
    {
        Vector3 sperarationVector = selectionSphere.transform.position - centerEyeAnchor.transform.position;
        RaycastHit[] raycastHits = Physics.SphereCastAll(centerEyeAnchor.transform.position, 0.5f, sperarationVector.normalized, sperarationVector.magnitude);

        hitObjects.Clear();

        foreach (RaycastHit rch in raycastHits)
        {
            GameObject obj = rch.collider.gameObject;
            if (obj != objectToSelect && obj != selectionSphere && !hitObjects.Contains(obj))  hitObjects.Add(obj);
        }

        foreach (GameObject obj in hitObjects)
        {
            if(translucentMaterialsDict.Keys.Contains(obj)) continue;
            translucentMaterialsDict[obj] = obj.GetComponent<MeshRenderer>().material;
            if(obj.GetComponent<MeshRenderer>().material.name.Contains("Target"))
            {
                obj.GetComponent<MeshRenderer>().material = translucentRed;
                continue;
            }
            obj.GetComponent<MeshRenderer>().material = translucentMaterial;
        }
        if (translucentMaterialsDict.Count == 0) return;

       
        foreach(GameObject obj in translucentMaterialsDict.Keys.ToArray())
        {
            if (!hitObjects.Contains(obj))
            {
                obj.GetComponent<MeshRenderer>().material = translucentMaterialsDict[obj];
                translucentMaterialsDict.Remove(obj);
                hitObjects.Remove(obj);
            }
        }

    }

}