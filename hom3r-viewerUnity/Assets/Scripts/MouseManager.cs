using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TLeftButtonIteractionMode { iddle, selection, navigation, drag_label};

public class MouseManager : MonoBehaviour
{

    //private bool isNavigationMovement;
    //private bool isLabelClicked;
    private Vector3 clickDownMousePosition;
    private Vector3 previousMousePosition;
    private float movementDetectorMargin;

    private TLeftButtonIteractionMode leftButtonInteractionMode;

    int productRootLayer;       // Layers ID
    int gizmoLayer;             // Layers ID
    int labelsUILayer;          // Layers ID
    Camera gizmoCamera;         // Gizmo Camera

    bool keyControlPressed;     // To know when the control key has been pressed in the web application


    // Start is called before the first frame update
    void Awake()
    {
        this.keyControlPressed = false;
        //this.isNavigationMovement = false;
        //this.isLabelClicked = false;
        this.leftButtonInteractionMode = TLeftButtonIteractionMode.iddle;
        this.clickDownMousePosition = Vector3.zero;
        this.movementDetectorMargin = 10.0f;

        // Initialize the layer masks
        // NameToLayer() returns the layer index 
        //'1 << ...' converts that to a bit mask, turning on the bit associated with that layer
        productRootLayer = 1 << LayerMask.NameToLayer(hom3r.state.productRootLayer);
        gizmoLayer = 1 << LayerMask.NameToLayer("gizmo_layer");
        labelsUILayer = 1 << LayerMask.NameToLayer(hom3r.state.labelsUILayer);        
        gizmoCamera = GameObject.FindWithTag("GizmoCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveMouseInteration())
        {
            EmitMousePosition();                // Emit current mouse position
            LeftButtonManager();                // Capture mouse left button events                
            MouseWheelManager();                // Capture mouse whell events
            CentralButtonManager();             // Capture mouse central button events 
        }
    }


    ////////////////////////
    ///  Mouse Position  ///
    ////////////////////////  
    private void EmitMousePosition()
    {
        Vector3 currentMousePosition = Input.mousePosition;
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.MouseManager_MousePosition, currentMousePosition));
    }

    ////////////////////////////
    ///  Mouse Left Button   ///
    //////////////////////////// 

    /// <summary>
    /// Manage left button interaction
    /// </summary>
    private void LeftButtonManager()
    {
        Vector3 currentMousePosition = Input.mousePosition;
        int mouseSelectionButton = GetMouseSelectionButton();
        
        // Actions just when the mouse left button CLICK DOWN
        if (Input.GetMouseButtonDown(mouseSelectionButton))
        {
            leftButtonInteractionMode = TLeftButtonIteractionMode.selection;
            clickDownMousePosition = Input.mousePosition;
            
            // Check if a label has been clicked
            if (this.IsLabelClicked()) {
                //Debug.Log("IsLabelClicked");
                leftButtonInteractionMode = TLeftButtonIteractionMode.drag_label;
                this.previousMousePosition = this.clickDownMousePosition;   // Init the previous position
            }            
        }

        // Actions when the left button is clicked. DURING
        if ((Input.GetMouseButton(mouseSelectionButton)))
        {                     
            if (this.leftButtonInteractionMode == TLeftButtonIteractionMode.selection)
            {
                // We have to check until we consider that is a REAL movement. 
                if (this.MovementDetector(clickDownMousePosition, Input.mousePosition))
                {
                    this.leftButtonInteractionMode = TLeftButtonIteractionMode.navigation;
                    this.previousMousePosition = this.clickDownMousePosition;   // Init the previous position                    
                }
            }
            if (this.leftButtonInteractionMode == TLeftButtonIteractionMode.navigation)
            {
                this.Navigation_Manager();
            }

            if (this.leftButtonInteractionMode == TLeftButtonIteractionMode.drag_label)
            {
                this.DragLabel_Manager();
            }
            
        }

        //Actions just when the mouse left button CLICK UP
        if (Input.GetMouseButtonUp(mouseSelectionButton))
        {
            if (this.leftButtonInteractionMode == TLeftButtonIteractionMode.selection)
            {
                this.Selection_Manager();   // Make selection
            } else if (this.leftButtonInteractionMode == TLeftButtonIteractionMode.navigation)
            {
                // Nothig to do
            } else if (this.leftButtonInteractionMode == TLeftButtonIteractionMode.drag_label)
            {
                hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.MouseManager_LabelDragGestureEnd));
            }
            this.leftButtonInteractionMode = TLeftButtonIteractionMode.iddle;
        }
    }


    private bool IsLabelClicked()
    {
        Vector3 currentMousePosition = Input.mousePosition;
        // 1. Check if user has clicked in a label object
        GameObject rayCastedGO = Raycast(currentMousePosition, Camera.main, labelsUILayer);        
        if (rayCastedGO != null) {            
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.MouseManager_LabelDragGestureBegin, rayCastedGO));
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks whether a movement has occurred or not
    /// </summary>
    /// <param name="originPosition"></param>
    /// <param name="currentPosition"></param>
    /// <returns></returns>
    private bool MovementDetector(Vector3 originPosition, Vector3 currentPosition)
    {
        Vector3 movement = originPosition - currentPosition;
        if (movement.magnitude > movementDetectorMargin) { return true; }
        return false;
    }

    /// <summary>
    /// Navigation gesture capture
    /// </summary>
    private void Navigation_Manager()
    {        
        
        Vector3 currentMousePosition = Input.mousePosition;                                     // Get current mouse position in pixels 
        float mouseMovementX = (currentMousePosition.x - previousMousePosition.x);              // Get mouse x movement
        float mouseMovementXPercentage = mouseMovementX / Screen.width;                         // Get mouse x movement in screen %
        float mouseMovementY = (currentMousePosition.y - previousMousePosition.y);              // Get mouse y movement         
        float mouseMovementYPercentage = mouseMovementY / Screen.height;                        // Get mouse y movement in screen %            
        previousMousePosition = currentMousePosition;                                           // Save current mouse position in pixels             

        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.MouseManager_LeftButtonDragMovement, currentMousePosition, mouseMovementX, mouseMovementY, mouseMovementXPercentage, mouseMovementYPercentage));
    }

    /// <summary>
    /// Selection Gesture capture
    /// </summary>
    private void Selection_Manager()
    {     
        // 0. Get current mouse position
        Vector3 currentMousePosition = Input.mousePosition;
        // 1. Check if user has clicked in gizmo object
        GameObject gizmoRayCastedGO = Raycast(currentMousePosition, gizmoCamera, gizmoLayer);
        if (gizmoRayCastedGO != null)
        {
            Debug.Log("Click in gizmo");
            //TODO Indicate OFF the indicated object
            //this.GetComponent<Core>().Do((new CNavigationCommand(TNavigationCommands.GizmoSelection, rayCastedGO_current)), Constants.undoNotAllowed);//FIXME after Navigation Refactoring
            return;
        }

        // 2. Check if user has clicked in a label object
        //GameObject labelRayCastedGO = Raycast(currentMousePosition, Camera.main, labelsUILayer);
        //if (labelRayCastedGO != null)
        //{
        //    Debug.Log("Click up in label");                
        //    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.MouseManager_LabelSelectionUp, currentMousePosition, labelRayCastedGO));
        //    return;
        //}

        // 3. Check if user has clicked in a product area
        GameObject rayCastedGO = Raycast(currentMousePosition, Camera.main, productRootLayer);
        bool rightControlKey = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || keyControlPressed;
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.MouseManager_LeftButtonUp, currentMousePosition, rayCastedGO, rightControlKey));
    }

    private void DragLabel_Manager()
    {
        Vector3 currentMousePosition = Input.mousePosition;                                     // Get current mouse position in pixels 
        float mouseMovementX = (currentMousePosition.x - previousMousePosition.x);              // Get mouse x movement
        float mouseMovementXPercentage = mouseMovementX / Screen.width;                         // Get mouse x movement in screen %
        float mouseMovementY = (currentMousePosition.y - previousMousePosition.y);              // Get mouse y movement         
        float mouseMovementYPercentage = mouseMovementY / Screen.height;                        // Get mouse y movement in screen %            
        previousMousePosition = currentMousePosition;                                           // Save current mouse position in pixels             
        
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.MouseManager_LabelDragGesture, currentMousePosition, mouseMovementX, mouseMovementY, mouseMovementXPercentage, mouseMovementYPercentage));
    }


    /////////////////////
    ///  Mouse Wheel  ///
    /////////////////////  
    /// <summary>Get the mouse wheel changes</summary>
    private void MouseWheelManager()
    {
        float mouseWheel = Input.GetAxis("Mouse ScrollWheel");                
        if (mouseWheel != 0)
        {
            if (IsMousePositionInSecundaryCamera())
            {
                hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.MouseManager_WheelMovementSecundaryCamera, mouseWheel));
            } else
            {
                //mouseWheel *= Time.deltaTime;            
                hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.MouseManager_WheelMovement, mouseWheel));
            }                        
        }
    }



    public bool IsMousePositionInSecundaryCamera()
    {
        Camera[] allCameras = Object.FindObjectsOfType<Camera>();

        //GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        Camera secundaryCamera = GameObject.FindGameObjectWithTag("secundarycamera").GetComponent<Camera>();

        if (!secundaryCamera.enabled) return false;

        Vector3 point = secundaryCamera.ScreenToViewportPoint(Input.mousePosition);
              
        if (point.x >= 0 && point.x <= 1 && point.y >= 0 && point.y <= 1)
        {
            //Debug.Log("Mouse Inside of Secundary camera");
            return true;
        }
        return false;
    }
    /// <summary>
    /// Get mouse central button events
    /// </summary>
    private void CentralButtonManager()
    {
        // Capture central mouse button down event
        if (Input.GetMouseButtonDown(2))
        {
            previousMousePosition = Input.mousePosition;   // Get current mouse position in pixels  
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.MouseManager_CentralButtonDown));
        }
        // Capture central mouse button up event
        if (Input.GetMouseButtonUp(2))
        {
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.MouseManager_CentralButtonUp));
        }

        // Capture moude movement when the central button is down
        if ((Input.GetMouseButton(2)))
        {
            //Emit Central Mouse Button DragGesture           
            Vector3 currentMousePosition = Input.mousePosition;
            float mouseMovementX = (currentMousePosition.x - previousMousePosition.x);              // Get mouse x movement
            float mouseMovementXPercentage = mouseMovementX / Screen.width;                         // Get mouse x movement in screen %
            float mouseMovementY = (currentMousePosition.y - previousMousePosition.y);              // Get mouse y movement         
            float mouseMovementYPercentage = mouseMovementY / Screen.height;                        // Get mouse y movement in screen %        
            previousMousePosition = currentMousePosition;                                           // Save current mouse position in pixels             
            
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.MouseManager_CentralButtonDragMovement, currentMousePosition, mouseMovementX, mouseMovementY, mouseMovementXPercentage, mouseMovementYPercentage));
        }
    }


    ////////////////////
    // Others Methods //
    ////////////////////


    /// <summary>
    /// Get the ID of the mouse button used to interaction
    /// </summary>
    /// <returns></returns>
    private int GetMouseSelectionButton()
    {
        TMouseMapping mouseMapping = hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetMouseMapping();

        if (mouseMapping == TMouseMapping.standard)
        {
            return 0;
        }
        else if (mouseMapping == TMouseMapping.inverse)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }


    /// <summary>Method that use Ray Casting technique</summary>
    private GameObject Raycast(Vector3 mouseCurrentPosition, Camera _camera, int _layer)
    {
        // Convert mouse position from screen space to three-dimensional space
        // Build ray, directed from mouse position to “camera forward” way
        Ray ray = _camera.ScreenPointToRay(mouseCurrentPosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, _layer))
        {
            GameObject rayCastedGO_current = hit.collider.gameObject;       // Now, let’s determine intersected GameObject
            return rayCastedGO_current;
        }
        else
        {
            return null;
        }
    }

    

    public void SetKeyControlPressed(bool _keyControlPressed)
    {
        keyControlPressed = _keyControlPressed;
    }
}
