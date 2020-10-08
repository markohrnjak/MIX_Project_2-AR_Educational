using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

public class PlaceAndDrag : MonoBehaviour
{
    [SerializeField] private ARRaycastManager m_RaycastManager;
    [SerializeField] private Camera ARCam;
    [SerializeField] private GameObject m_PrefabToPlace;
    [SerializeField] private Text selectionText;
    [SerializeField] private Button arGreenButton;
    [SerializeField] private Button arRedButton;
    [SerializeField] private Button arBlueButton;

    private Vector2 touchPosition = default;

    private PlacementObject lastSelectedObject = null;

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();


    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();

        // set initial prefab
        ChangePrefabTo("BlueCube");

        arGreenButton.onClick.AddListener(() => ChangePrefabTo("GreenCube"));
        arBlueButton.onClick.AddListener(() => ChangePrefabTo("BlueCube"));
        arRedButton.onClick.AddListener(() => ChangePrefabTo("RedCube"));
    }

    void ChangePrefabTo(string prefabName) //changes which prefab is loaded on different button presses
    {
        m_PrefabToPlace = Resources.Load<GameObject>($"Prefabs/{prefabName}");

        if (m_PrefabToPlace == null)
        {
            Debug.LogError($"Prefab with name {prefabName} could not be loaded, make sure you check the naming of your prefabs...");
        }

        switch (prefabName)
        {
            case "BlueCube":
                selectionText.text = "";
                selectionText.text = $"Selected: <color='blue'>Blue</color>";
                break;
            case "RedCube":
                selectionText.text = "";
                selectionText.text = $"Selected: <color='red'>Red</color>";
                break;
            case "GreenCube":
                selectionText.text = "";
                selectionText.text = $"Selected: <color='green'>Green</color>";
                break;
        }
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);                                                             //reference to first touch
            touchPosition = touch.position;

            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = ARCam.ScreenPointToRay(touch.position);
                RaycastHit hitObject;

                if (Physics.Raycast(ray, out hitObject))
                {
                    lastSelectedObject = hitObject.transform.GetComponent<PlacementObject>();
                    if (lastSelectedObject != null)
                    {
                        //we are looking for objects with the PlacementObject script attached to them
                        PlacementObject[] allOtherObjects = PlacementObject.FindObjectsOfType<PlacementObject>();
                        foreach (PlacementObject placementObject in allOtherObjects)
                        {
                            placementObject.Selected = placementObject == lastSelectedObject;         
                        }
                    }
                }
            }
            if (touch.phase == TouchPhase.Ended)
            {
                lastSelectedObject.Selected = false;
            }
        }

        //create on touch
        if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon) && !IsPointerOverUIObject())  //cast a ray onto the plane that was touched 
        {
            Pose hitPose = s_Hits[0].pose;

            if (lastSelectedObject == null)
            {
                lastSelectedObject = Instantiate(m_PrefabToPlace, hitPose.position, hitPose.rotation).GetComponent<PlacementObject>(); //place object and store in variable only if the placed object does not exist
            }
            else
            {
                if (lastSelectedObject.Selected)
                {
                    lastSelectedObject.transform.position = hitPose.position;
                    lastSelectedObject.transform.rotation = hitPose.rotation;
                }
            }


        }
    }

    //for checking if the touch is on UI or not
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}

