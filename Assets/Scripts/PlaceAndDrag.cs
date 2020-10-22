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

    private PlacementObject selectedObject = null;

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
            //reference to first touch
            Touch touch = Input.GetTouch(0);
            touchPosition = touch.position;

            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = ARCam.ScreenPointToRay(touch.position);

                if (Physics.Raycast(ray, out RaycastHit hitObject))
                {
                    selectedObject = hitObject.transform.GetComponent<PlacementObject>();
                    if (selectedObject != null)
                    {
                        //get all the PalecemntObjects in the scene...
                        PlacementObject[] allOtherObjects = PlacementObject.FindObjectsOfType<PlacementObject>();

                        //...go through each of them, the ones that are not getting raycasted get their Selected set to false because those do not need to be moved
                        //if the selected object IS the one being selected, that means the one that is being touched is the one we want to move arund
                        foreach (PlacementObject placementObject in allOtherObjects)
                        {
                            placementObject.Selected = placementObject == selectedObject;
                        }
                    }
                }
            }

            if (touch.phase == TouchPhase.Ended)
            {
                selectedObject.Selected = false;
            }
        }


        if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon) && !IsPointerOverUIObject())
        {
            Pose hitPose = s_Hits[0].pose;

            if (selectedObject == null)
            {
                selectedObject = Instantiate(m_PrefabToPlace, hitPose.position, hitPose.rotation).GetComponent<PlacementObject>();
            }

            else
            {
                if (selectedObject.Selected)
                {
                    selectedObject.transform.position = hitPose.position;
                    selectedObject.transform.rotation = hitPose.rotation;
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

