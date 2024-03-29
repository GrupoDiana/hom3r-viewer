/*******************SINGLEPOINTMANAGEMENT_SCRIPT****************************************************
 * 
 * Script to place, edit and delete group of points over a 3D model surface
 * 
 * Creation Date: May 2016
 * 
 * UiW European Project
 * Grupo DIANA - University of Malaga
 * 
 * HOM3R VIEWER
 * 
 ***************************************************************************************/
 
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SinglePointsStatus_Type { EmptyMode, DeleteMode, FirstChanges_unsaved, Changes_Unsaved, Changes_Saved};
public enum SinglePointsActions_Type { DataModification, SaveData, ExitMode };
public enum SinglePointSubmode_Type { CaptureSPMode, RemoveSPMode };

public class SinglePointManager : MonoBehaviour
{
    //Dictionary and list
    public struct singlePointGroup_struct
    {
        public GameObject areaGO;
        public List<GameObject> singlePointGO_List;
        public List<GameObject> conectorGO_List;
    };

    enum modeAction { ADD, ADDOVERCONNETOR, REMOVE, REMOVEOVERCONNECTOR, REMOVEFIRSTSP, EDITSP};
    
    struct undoCommand_struct {
       public modeAction action;
       public GameObject areaToPlaceSP;
       public GameObject connectorToPaceSP;
        public GameObject gameObjectSP;
       public Vector3 singlePointPos;
    };

    List<GameObject> singlePoints_GameObjectsList;  //List to store every Game Objects of the introduced single
    List<GameObject> conectors_GameObjectsList;     //List to store every Game Objects of the introduced single  
    List<GameObject> listConfirmedAreas;            //List to store the selected areas
    Dictionary<int, singlePointGroup_struct> singlePointsMap_bySPGroupID;   //Map to store the list of single points, grouped by Revision ID

    //State machine
    SinglePointsStatus_Type singlePointMode_currentStatus;  //Indicate the status of the mode
    SinglePointSubmode_Type singlePoint_currentSubmode;     //Indicate the sub-mode index

    //Class parameters
    //GameObject singlePoints_father;     //Empty object, father of every single point (spheres)
    //GameObject turbineGO;               // Turbine Game Object
    //GameObject orbitPlaneGO;            //Orbit Plane GO (need to get the distance from the camera to the axis)
    //GameObject uiScript;                //Interface script
    Vector3 lastSinglePoint_pos;        //To store the previous added single point local position
    Vector3 newSize;                    //Scale of the SinglePoints
    public bool bDrawConectors;         //Indicate if the connectors between the SP have to be drawn
    public bool firstListComponent;     //Indicate if the SP is the fist component of the singlePointsList
    public bool bShowLabels;            //Show a label each time a single point is added
    float scaleUpdated;
    Vector3 normScaleVector;            //Vector to normalize the scale, depend on the Turbine father scale
    int editingSPGroup_ID;                          //Identifier of the SinglePoint group that is being edited

    //Define constants 
    //*** Pluggy 2018.06.27    
    const float SP_MIN_SCALE = 0.002f;               //Minimum scale of the single point
    //***
    const float CYLINDERCONECTOR_FACTOR = 0.7f;     //Factor to do the connectors smaller than the single points
    const float DISTANCE_FACTOR = 3.0f;             //Factor to scale the distance between the mainCamera and the turbine
    const bool justLastItem = true;                 //Constant to store the value to indicate the SetSinglePointAndConectorList_Scale method to re-scale the last item of the list
    const bool everyItem = false;                   //Constant to store the value to indicate the SetSinglePointAndConectorList_Scale method to re-scale all the items of the list

    //Single Point and connector colours
    Color SP_color;             //Colour of every single points of the list (except the last one)
    Color conector_color;       //Colour of every connector of the list
    Color lastSP_color;         //Colour of the last SinglePoint of the list
    Color editionSP_color;      //Colour of the SP that is changing its position


    //Stack to save previous actions and commands to be undone
    private Stack<undoCommand_struct> undoStack;

    bool blockedByMovingSP;
    public bool SPCapture_restrictedByArea;
    bool startEditingSP;                            //To control when the user is 
    bool addingSP;
    GameObject current_rayCastedSinglePoint;         //Object ray-casting
    int productRootLayer;
    int singlePointsLayer;
    int SPConectorsLayer;

    private void Awake()
    {

        //Initialize Lists and Dictionary
        singlePoints_GameObjectsList = new List<GameObject>();
        conectors_GameObjectsList = new List<GameObject>();
        listConfirmedAreas = new List<GameObject>();
        singlePointsMap_bySPGroupID = new Dictionary<int, singlePointGroup_struct>();
        
        //*** Pluggy 2018.06.27
        //bDrawConectors = true;
        bDrawConectors = false;
        //***
        firstListComponent = true;
        bShowLabels = false;
        //bShowLabels = true;   // TEST LABELS
        //Initialize variables
        normScaleVector = new Vector3(1 / hom3r.quickLinks._3DModelRoot.transform.localScale.x, 1 / hom3r.quickLinks._3DModelRoot.transform.localScale.y, 1 / hom3r.quickLinks._3DModelRoot.transform.localScale.z);
        editingSPGroup_ID = 0;  //If ID is not received from ASP it has to be 0

        //State Machine
        singlePointMode_currentStatus = SinglePointsStatus_Type.EmptyMode;
        singlePoint_currentSubmode = SinglePointSubmode_Type.CaptureSPMode;

        //Initialize Colours
        SP_color = Color.yellow;
        conector_color = Color.yellow;
        lastSP_color = Color.grey;
        editionSP_color = Color.grey;

        //Initialize stack of commands to be undone
        undoStack = new Stack<undoCommand_struct>();


        blockedByMovingSP = false;
        SPCapture_restrictedByArea = true;
        startEditingSP = true;
        addingSP = false;
        // Initialize the layer masks
        // NameToLayer() returns the layer index 
        //'1 << ...' converts that to a bit mask, turning on the bit associated with that layer
        productRootLayer = 1 << LayerMask.NameToLayer(hom3r.state.productRootLayer);
        singlePointsLayer = 1 << LayerMask.NameToLayer("singlePoints_layer");
    }

    /// <summary>
    /// method to capture and edit position of the single points
    /// </summary>// TODO TEST IT, HAS BEEN MOVED FROM MOUSEMANAGER WITHOUT TEST IT
    public void CaptureSinglePoint(Vector3 _mousePosition)
    {
        //Build ray, directed from mouse position to “camera forward” way
        Ray ray = Camera.main.ScreenPointToRay(_mousePosition);
        RaycastHit hit, conectorHit, areaHit, singlePointHit;
        conectorHit = new RaycastHit();
        areaHit = new RaycastHit();
        singlePointHit = new RaycastHit();
        //Get list of confirmed objects (areas). Copy the list to allow the list access
        List<GameObject> listConfirmedAreas = new List<GameObject>(this.GetComponent<SelectionManager>().GetListOfConfirmedObjects());

        //If Ray-cast over a SP and Button pressed
        if (Input.GetMouseButton(0) && !blockedByMovingSP && Physics.Raycast(ray, out singlePointHit, Mathf.Infinity, singlePointsLayer))
        {
            // Now, let’s determine intersected GameObject
            current_rayCastedSinglePoint = singlePointHit.collider.gameObject;
            blockedByMovingSP = true;
        }

        //EDIT SP. If a SP has been selected and Ray cast over an area
        if (Input.GetMouseButton(0) && blockedByMovingSP && Physics.Raycast(ray, out areaHit, Mathf.Infinity, productRootLayer))
        {
            if (SPCapture_restrictedByArea)
            {
                if (listConfirmedAreas.Contains(areaHit.collider.gameObject))
                {
                    if (startEditingSP)
                    {
                        bool undo = startEditingSP & !addingSP;
                        this.GetComponent<SinglePointManager>().EditSinglePoint(current_rayCastedSinglePoint, areaHit.point, undo);
                        startEditingSP = false;
                    }
                    else
                    {
                        this.GetComponent<SinglePointManager>().EditSinglePoint(current_rayCastedSinglePoint, areaHit.point);
                    }
                }
                else { Debug.Log("SinglePoint can not be placed over this area"); }
            }
            else
            { // if the SPMode is not restricted by area, just edit
                if (startEditingSP)
                {
                    //Save the undo state just when starting the SP edition. Also, don't save the undo state when the user is editing the SP because he is adding it
                    bool undo = startEditingSP & !addingSP;
                    this.GetComponent<SinglePointManager>().EditSinglePoint(current_rayCastedSinglePoint, areaHit.point, undo);
                    startEditingSP = false;
                }
                else
                {
                    this.GetComponent<SinglePointManager>().EditSinglePoint(current_rayCastedSinglePoint, areaHit.point);
                }
            }
        }

        //If Ray-cast over a connector and a SP is not being moved
        else if (Input.GetMouseButtonDown(0) && !blockedByMovingSP && Physics.Raycast(ray, out conectorHit, Mathf.Infinity, SPConectorsLayer))
        {
            //Get the area where the connector is placed
            Physics.Raycast(ray, out areaHit, Mathf.Infinity, productRootLayer);
            //current_rayCastedComponent = conectorHit.collider.gameObject;
            //Add a new single point between two existing single points and modify connectors
            this.GetComponent<SinglePointManager>().AddSinglePointToAList_overConnector(conectorHit.collider.gameObject, areaHit.collider.gameObject, areaHit.point);
        }

        //If the user select a point the a component surface
        else if (Input.GetMouseButtonDown(0) && !blockedByMovingSP && Physics.Raycast(ray, out hit, Mathf.Infinity, productRootLayer))
        {
            addingSP = true;
            if (SPCapture_restrictedByArea)
            {
                if (listConfirmedAreas.Contains(hit.collider.gameObject))
                {
                    GameObject current_rayCastedArea = hit.collider.gameObject;
                    Vector3 pointlocal = current_rayCastedArea.transform.InverseTransformPoint(hit.point);
                    this.GetComponent<SinglePointManager>().AddSinglePoint(current_rayCastedArea, pointlocal);
                }
                else { Debug.Log("SinglePoint can not be placed over this area"); }
            }
            else
            {
                GameObject current_rayCastedArea = hit.collider.gameObject;
                Vector3 pointlocal = current_rayCastedArea.transform.InverseTransformPoint(hit.point);
                this.GetComponent<SinglePointManager>().AddSinglePoint(current_rayCastedArea, pointlocal);
            }
        }

        // If Mouse Button 0 goes UP
        if (blockedByMovingSP && Input.GetMouseButtonUp(0))
        {
            blockedByMovingSP = false;
        }

        if (Input.GetMouseButtonUp(0))
        {
            startEditingSP = true;
            addingSP = false;
        }
    }


    /// <summary>
    /// Delete the SinglePoints using Mouse Ray-casting
    /// </summary> // TODO TEST IT, HAS BEEN MOVED FROM MOUSEMANAGER WITHOUT TEST IT
    public void DeleteSinglePoint(Vector3 _mousePosition)
    {
        GameObject current_rayCastedSinglePoint;         //Object ray-casted

        //Convert mouse position from screen space to three-dimensional space
        //Build ray, directed from mouse position to “camera forward” way
        Ray ray = Camera.main.ScreenPointToRay(_mousePosition);

        //Find ray intersection with any singlePoint in scene
        //int singlePointsLayer = 4 << LayerMask.NameToLayer("singlePoints_layer");
        RaycastHit hit, areaHit;

        //If intersection exists, function «Physics.Raycast» returns «true» and sets «hit» variable
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, singlePointsLayer))
        {
            //Store the intersected GameObject
            current_rayCastedSinglePoint = hit.collider.gameObject;
            Physics.Raycast(ray, out areaHit, Mathf.Infinity, productRootLayer);

            //Select the point when mouse-click
            if (Input.GetMouseButtonDown(0))
            {
                this.GetComponent<SinglePointManager>().RemoveSinglePointAndConectors(current_rayCastedSinglePoint, areaHit.collider.gameObject);
            }
        }
    }

    //************************************************************************************************
    // Capture and Edit Single Points Methods
    //*************************************************************************************************

    /// <summary> Add a Single Point to the scene and the corresponding connector in the Capture SinglePoint Mode</summary>
    /// <param name="_obj">         GameObject where the single point is placed</param>
    /// <param name="_position">    SinglePoint GameObject LOCAL position</param>
    /// <param name="undo">         true when the action has to be saved in the undo stack</param>
    public void AddSinglePoint(GameObject _obj, Vector3 _position, bool undo=true)
    {
        //** Place and draw the SP
        //Create the Single Point Object
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //Add to a layer
        sphere.layer = LayerMask.NameToLayer("singlePoints_layer");
        //Get the component where the SP is placed, move the sphere to be a chidren (in order to get the local position) and save the father in the temp struct
        sphere.transform.parent = _obj.transform;
        //Get the local position respect the component where the user is drawing the SP
        sphere.transform.localPosition = _position;

        //** Add the new SinglePoint data to the list
        singlePoints_GameObjectsList.Add(sphere);

        //** Draw the Single Point  
        ///Set the sphere size
        SetSinglePointAndConectorList_Scale(justLastItem);
        //SP Colour
        sphere.GetComponent<Renderer>().material.color = lastSP_color;

        //** Concatenated points
        if (bDrawConectors)
        {
            if (singlePoints_GameObjectsList.Count > 1)
            {
                //Create a cylinder beetwen the previous sphere and the current one. IMP: Parameters are global postions
                AddSPConnector(singlePoints_GameObjectsList[singlePoints_GameObjectsList.Count - 2].gameObject.transform.position, sphere.transform.position, _obj);
            }
        }

        //Change the colour of the last single point in the list
        if (singlePoints_GameObjectsList.Count > 1)
        {
            singlePoints_GameObjectsList[singlePoints_GameObjectsList.Count - 2].GetComponent<Renderer>().material.color = SP_color;
        }

        // Create Label. Debugger method. Note that the method gives the same ID to every label.
        if (bShowLabels)
        {
            //hom3r.quickLinks.scriptsObject.GetComponent<LabelManager>().AddLabelToSpecificPoint("1",_obj, _position, _obj.name);
        }

        //Change Mode status with a data Modification action
        SinglePointMode_statusManager(SinglePointsActions_Type.DataModification);

        //**Save in the undo stack
        if (undo)
        {
            undoCommand_struct command;
            command.action = modeAction.ADD;
            command.areaToPlaceSP = _obj;
            command.connectorToPaceSP = null;
            command.gameObjectSP = sphere;
            command.singlePointPos = _position;
            undoStack.Push(command);
        }
    }
        
    /// <summary> Add a single Point over a connector, remove the previous connector and create two new ones in the Capture SinglePoint Mode </summary>
    /// <param name="_conectorGO">  Connector GameObject where the new SinglePoint is placed</param>
    /// <param name="_areaGO">      GameObject where the single point is placed</param>
    /// <param name="_position">    SinglePoint GameObject GLOBAL position</param>
    /// <param name="undo">         true when the action has to be saved in the undo stack</param>
    public void AddSinglePointToAList_overConnector(GameObject _conectorGO, GameObject _areaGO, Vector3 _position, bool undo = true)
    {
        //    if (conectors_GameObjectsList.Contains(_conector)){
        //** Get connector sorted List index
        int conector_listIndex = conectors_GameObjectsList.IndexOf(_conectorGO);

        int newSP_listIxdex = conector_listIndex + 1;

        //** Create a Single Point
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //Add to a layer
        sphere.layer = LayerMask.NameToLayer("singlePoints_layer");
        //Get the component where the SP is placed, move the sphere to be a chidren (in order to get the local position) and save the father in the temp struct
        sphere.transform.parent = _areaGO.transform;
        //Get the local position respect the component where the user is drawing the SP
        sphere.transform.position = _position;

        //** Add new SinglePoint to the list in the correspond position
        singlePoints_GameObjectsList.Insert(newSP_listIxdex, sphere);

        // **Draw SinglePoint
        //Set scale
        SetSinglePointAndConectorList_Scale(everyItem);
        //Set Colour
        sphere.GetComponent<Renderer>().material.color = SP_color;

        //** Concatenate points
        //Delete previous connector
        DeleteSPConector(_conectorGO);

        //Add two new connectors
        //Previous connector to the new singlePoint      
        AddSPConnector(singlePoints_GameObjectsList[newSP_listIxdex - 1].gameObject.transform.position, sphere.transform.position, _areaGO, conector_listIndex);
        //Next connector to the new singlePoint
        AddSPConnector(singlePoints_GameObjectsList[newSP_listIxdex + 1].gameObject.transform.position, sphere.transform.position, _areaGO, conector_listIndex + 1);

        //Change Mode status with a data Modification action
        SinglePointMode_statusManager(SinglePointsActions_Type.DataModification);

        //**Save in the undo stack
        if (undo)
        {
            undoCommand_struct command;
            command.action = modeAction.ADDOVERCONNETOR;
            command.areaToPlaceSP = _areaGO;
            command.connectorToPaceSP = _conectorGO;
            command.gameObjectSP = sphere;
            command.singlePointPos = _position;
            undoStack.Push(command);
        }
    }

    /// <summary>
    /// Add single point at the beginning of the list
    /// </summary>
    /// <param name="_areaGO"> Area where the single point is going to be placed</param>
    /// <param name="_position"> Global position of the single Point</param>
    public void AddSinglePointToAList_atTheBeginning(GameObject _areaGO, Vector3 _position)
    {
        int newSP_listIxdex = 0;

        //** Create a Single Point
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //Add to a layer
        sphere.layer = LayerMask.NameToLayer("singlePoints_layer");
        //Get the component where the SP is placed, move the sphere to be a chidren (in order to get the local position) and save the father in the temp struct
        sphere.transform.parent = _areaGO.transform;
        //Get the local position respect the component where the user is drawing the SP
        sphere.transform.position = _position;

        //** Add new SinglePoint to the list in the correspond position
        singlePoints_GameObjectsList.Insert(newSP_listIxdex, sphere);

        // **Draw SinglePoint
        //Set scale
        SetSinglePointAndConectorList_Scale(everyItem);
        //Set Colour
        sphere.GetComponent<Renderer>().material.color = SP_color;
                
        //Add connector to the new singlePoint
        AddSPConnector(singlePoints_GameObjectsList[newSP_listIxdex+1].gameObject.transform.position, sphere.transform.position, _areaGO);

        //Change Mode status with a data Modification action
        SinglePointMode_statusManager(SinglePointsActions_Type.DataModification);
    }

    /// <summary>
    /// Method to add connector between two singlePoints
    /// </summary>
    public void AddSPConnector(Vector3 startPos, Vector3 endPos, GameObject areaGO)
    {
        //** Place and draw the SPConector
        //Create the Single Point Object
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        //Add to a layer
        cylinder.layer = LayerMask.NameToLayer("SPConectors_layer");
        //Set the cylinder position and dimensions
        Vector3 dir = endPos - startPos;
        cylinder.transform.position = dir / 2.0f + startPos; //middle point between two spheres
        cylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        //Get the component where the SP is placed, move the cylinder to be a chidren.
        cylinder.transform.parent = areaGO.transform;

        //** Add to a list
        conectors_GameObjectsList.Add(cylinder);

        //** Draw Connector
        //Set scale
        cylinder.transform.localScale = new Vector3(1.0f, dir.magnitude * normScaleVector.y * 0.5f, 1.0f); // Y scale depends on the start and end points and it has to be normalised with the Turbine scale      
        SetSinglePointAndConectorList_Scale(justLastItem);
        //Set colour
        cylinder.GetComponent<Renderer>().material.color = conector_color;
    }

    /// <summary>
    /// OVERLOAD Method to add connector between two singlePoints in a specific index
    /// </summary>
    public void AddSPConnector(Vector3 startPos, Vector3 endPos, GameObject areaGO, int index)
    {
        //** Place and draw the SPConector
        //Create the Single Point Object
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        //Add to a layer
        cylinder.layer = LayerMask.NameToLayer("SPConectors_layer");
        //Set the cylinder position and dimensions
        Vector3 dir = endPos - startPos;
        cylinder.transform.position = dir / 2.0f + startPos; //middle point between two spheres
        cylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        //Get the component where the SP is placed, move the cylinder to be a chidren.
        cylinder.transform.parent = areaGO.transform;

        //** Add to a list
        conectors_GameObjectsList.Insert(index, cylinder);

        //** Draw connector
        //Set scale
        cylinder.transform.localScale = new Vector3(1.0f, dir.magnitude * normScaleVector.y * 0.5f, 1.0f); // Y scale depends on the start and end points and it has to be normalised with the Turbine scale
        SetSinglePointAndConectorList_Scale(everyItem);
        //Set Colour
        cylinder.GetComponent<Renderer>().material.color = conector_color;
    }
    
    /// <summary>
    ///  Method to change the position of a single points using the mouse position over the selected area
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="surfaceHit"></param>
    public void EditSinglePoint(GameObject _obj, Vector3 _surfaceHit, bool undo = false)
    {
        //Update Single point position
        _obj.transform.position = _surfaceHit;

        //obj.GetComponent<Renderer>().material.color = Color.grey;

        if (conectors_GameObjectsList.Count != 0)
        {
            //Update connector position
            int listPos = singlePoints_GameObjectsList.IndexOf(_obj);

            if (listPos == 0) //First SinglePoint of the chain
            {
                //Change position of the connector next to the single point
                EditSPConector(singlePoints_GameObjectsList[listPos].gameObject.transform.position, singlePoints_GameObjectsList[listPos + 1].gameObject.transform.position, conectors_GameObjectsList[listPos]);
            }

            else if (listPos == singlePoints_GameObjectsList.Count - 1) //Last SinglePoint of the chain
            {
                //Change position of the connector previous to the single point
                EditSPConector(singlePoints_GameObjectsList[listPos - 1].gameObject.transform.position, singlePoints_GameObjectsList[listPos].gameObject.transform.position, conectors_GameObjectsList[listPos - 1]);
            }
            else
            {
                //Change position of the connector previous to the single point
                EditSPConector(singlePoints_GameObjectsList[listPos - 1].gameObject.transform.position, singlePoints_GameObjectsList[listPos].gameObject.transform.position, conectors_GameObjectsList[listPos - 1]);
                //Change position of the connector next to the single point
                EditSPConector(singlePoints_GameObjectsList[listPos].gameObject.transform.position, singlePoints_GameObjectsList[listPos + 1].gameObject.transform.position, conectors_GameObjectsList[listPos]);
            }

            //Change Mode status with a data Modification action
            SinglePointMode_statusManager(SinglePointsActions_Type.DataModification);

            if (undo)
            {
                undoCommand_struct command;
                command.action = modeAction.EDITSP;
                command.areaToPlaceSP = _obj;
                command.connectorToPaceSP = null;
                command.gameObjectSP = null;
                command.singlePointPos = _surfaceHit;
                undoStack.Push(command);
            }
        }
        else
        {
            //Debug.Log("No connectors in the conectors_GameObjectsList");
        }
    }

    /// <summary>
    /// Method to change the position of a connector according with the single point position
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <param name="conector"></param>
    public void EditSPConector(Vector3 startPos, Vector3 endPos, GameObject conector)
    {
        //Set the cylinder position and dimensions
        Vector3 dir = endPos - startPos;
        conector.transform.position = dir / 2.0f + startPos; //middle point between two spheres
        conector.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        conector.transform.localScale = new Vector3(conector.transform.localScale.x, dir.magnitude * 0.5f * normScaleVector.y, conector.transform.localScale.z);
        //Change Mode status with a data Modification action
        SinglePointMode_statusManager(SinglePointsActions_Type.DataModification);
    }

    /// <summary>
    /// Public method called from MouseKeyboardmanager to delete singlePoints using Raycasting
    /// </summary>
    /// <param name="_singlePointGO">   singlePoint gameObject pointer</param>
    /// <param name="_areaGO">          GameObject where the single point is placed</param>
    /// <param name="undo">             true when the action has to be saved in the undo stack</param>  
    public void RemoveSinglePointAndConectors(GameObject _singlePointGO, GameObject _areaGO, bool undo=true)
    {
        bool removeOverConnector = false;
        bool removefirstSP = false;

        // Get connector sorted List index
        int sp_listIndex = singlePoints_GameObjectsList.IndexOf(_singlePointGO);

        //If it is the fist SP of the list
        if (sp_listIndex == 0)
        {
            removefirstSP = true;
            //If there is more than one SP, there is a connector
            if (singlePoints_GameObjectsList.Count != 1)
            {
                //Delete connectors
                DeleteSPConector(sp_listIndex);
            }
            //Delete Single Point
            DeleteSinglePoint(_singlePointGO);

        }
        //if it is the last SP of the list
        else if (sp_listIndex == singlePoints_GameObjectsList.Count - 1)
        {
            //Delete connectors
            DeleteSPConector(sp_listIndex - 1);

            //Delete Single Point
            DeleteSinglePoint(_singlePointGO);

            //Change colour of the last SP of the list
            singlePoints_GameObjectsList[singlePoints_GameObjectsList.Count - 1].GetComponent<Renderer>().material.color = lastSP_color;
        }
        else
        {
            removeOverConnector = true;
            //Delete connectors
            DeleteSPConector(sp_listIndex);
            DeleteSPConector(sp_listIndex - 1);

            //Add a new connector
            AddSPConnector(singlePoints_GameObjectsList[sp_listIndex - 1].gameObject.transform.position, singlePoints_GameObjectsList[sp_listIndex + 1].gameObject.transform.position, _areaGO, sp_listIndex - 1);

            //Delete Single Point
            DeleteSinglePoint(_singlePointGO);
        }

        //Change Mode status with a data Modification action
        SinglePointMode_statusManager(SinglePointsActions_Type.DataModification);
        
        //**Save in the undo stack
        if (undo)
        {
            undoCommand_struct command;
            command.areaToPlaceSP = _areaGO;
            command.gameObjectSP = _singlePointGO;

            if (removeOverConnector)
            {
                command.action = modeAction.REMOVEOVERCONNECTOR;
                command.connectorToPaceSP = conectors_GameObjectsList[sp_listIndex - 1];
                command.singlePointPos = _singlePointGO.transform.position;             //Global position is needed in this action
            }
            else if (removefirstSP) {
                command.action = modeAction.REMOVEFIRSTSP;
                command.connectorToPaceSP = null;
                command.singlePointPos = _singlePointGO.transform.position;
            }
            else
            {
                command.action = modeAction.REMOVE;
                command.connectorToPaceSP = null;
                command.singlePointPos = _singlePointGO.transform.localPosition;
            }

            undoStack.Push(command);
        }
    }

    /// <summary>
    /// Method to delete a given single point
    /// </summary>
    /// <param name="obj"></param>
    public void DeleteSinglePoint(GameObject obj)
    {
        if (singlePoints_GameObjectsList.Count != 0)
        {
            //Delete single point fom the list
            singlePoints_GameObjectsList.Remove(obj);
            //Delete GameObject
            Destroy(obj);
            //Change Mode status with a data Modification action
            SinglePointMode_statusManager(SinglePointsActions_Type.DataModification);
        }
        else
        {
            Debug.LogError("Single Point NOT DELETED, singlePoints_GameObjectsList is empty");
        }

    }

    /// <summary> Method to remove a given connector </summary>
    /// <param name="obj"> GameObject to remove</param>
    public void DeleteSPConector(GameObject obj)
    {
        if (conectors_GameObjectsList.Count != 0)
        {
            //Delete from the list
            conectors_GameObjectsList.Remove(obj);
            //Delete GameObject
            Destroy(obj);
        }
        else
        {
            Debug.LogError("Connector NOT DELETED, conectors_GameObjectsList is empty");
        }
    }

    /// <summary> OVERLOAD Method to delete a connector by list index </summary>
    /// <param name="index"> Index of the connector to remove</param>
    public void DeleteSPConector(int index)
    {
        if (conectors_GameObjectsList.Count != 0)
        {
            GameObject tempCopy = conectors_GameObjectsList[index];

            //Delete from the list
            conectors_GameObjectsList.RemoveAt(index);

            //Delete GameObject
            Destroy(tempCopy);

            //Indicate that a new SinglePoint Mode action has not been saved 
            //SP_saved(false);
            //Change Mode status with a data Modification action
            SinglePointMode_statusManager(SinglePointsActions_Type.DataModification);
        }
        else
        {
            Debug.LogError("Connector NOT DELETED, conectors_GameObjectsList is empty");
        }
    }

    /// <summary>
    /// Calculate the scale (size) of the SinglePoint Game Object according to the distance between the MainCamera and the Axis intersection (which depend on the navigation system)
    /// </summary>
    /// <returns> the GameObject size</returns>
    float GetSinglePoint_scale()
    {
        //Distance from camera to the component where the Single Point is located
        //float distance = hom3r.quickLinks.orbitPlane.GetComponent<NavigationManager>().GetDistanceToAxis(); //FIXME after Navigation Refactoring
        //float scale = DISTANCE_FACTOR * distance / 1000;

        //return scale;

        //*** Pluggy 2018.06.27 
        float newSize= 2.0f;
  

        if (hom3r.state.platform == THom3rPlatform.Android || hom3r.state.platform == THom3rPlatform.IOS)
        {
            newSize = 0.0015f;       //AR_APP Hard code
        }
        else if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            //newSize = 0.002f;       //AR_APP Hard code
            newSize = 0.02f;       //hom3r web Hard code
        }

        return newSize;

        //***
    }
    /// <summary> Update the scale of every connectors and single points in the Capture Mode</summary>
    void SetSinglePointAndConectorList_Scale(bool justLastItem)
    {
        //Get the size of the Single Points
        scaleUpdated = GetSinglePoint_scale();

        if (justLastItem)
        {
            //Limit the small size of the Single Point
            if (scaleUpdated < SP_MIN_SCALE)
            {
                if (singlePoints_GameObjectsList.Count != 0)
                {
                    singlePoints_GameObjectsList[singlePoints_GameObjectsList.Count - 1].transform.localScale = SP_MIN_SCALE * normScaleVector;
                }
                if (conectors_GameObjectsList.Count != 0)
                {
                    conectors_GameObjectsList[conectors_GameObjectsList.Count - 1].transform.localScale = new Vector3(CYLINDERCONECTOR_FACTOR * SP_MIN_SCALE * normScaleVector.x, conectors_GameObjectsList[conectors_GameObjectsList.Count - 1].transform.localScale.y, CYLINDERCONECTOR_FACTOR * SP_MIN_SCALE * normScaleVector.z);
                }
            }
            else
            {
                if (singlePoints_GameObjectsList.Count != 0)
                {
                    singlePoints_GameObjectsList[singlePoints_GameObjectsList.Count - 1].transform.localScale = scaleUpdated * normScaleVector;
                }
                if (conectors_GameObjectsList.Count != 0)
                {
                    conectors_GameObjectsList[conectors_GameObjectsList.Count - 1].transform.localScale = new Vector3(CYLINDERCONECTOR_FACTOR * scaleUpdated * normScaleVector.x, conectors_GameObjectsList[conectors_GameObjectsList.Count - 1].transform.localScale.y /** normScaleVector.y*/, CYLINDERCONECTOR_FACTOR * scaleUpdated * normScaleVector.z);
                }
            }
        }

        else // Iterate throught the whole list
        {
            //Re-scale every singlePoints
            foreach (GameObject sp in singlePoints_GameObjectsList)
            {
                //Normalize with the turbine father gameobject scale
                //sp.transform.localScale = new Vector3(sp.transform.localScale.x / turbineScale.x, sp.transform.localScale.y / turbineScale.y, sp.transform.localScale.z / turbineScale.z);

                //Limit the small size of the Single Point
                if (scaleUpdated < SP_MIN_SCALE)
                {
                    sp.transform.localScale = SP_MIN_SCALE * normScaleVector; //* Vector3.one;
                }
                else
                {
                    sp.transform.localScale = scaleUpdated * normScaleVector; //* Vector3.one;
                }
            }//end foreach singlePoints

            //Re-scale every connector
            foreach (GameObject conector in conectors_GameObjectsList)
            {

                //Limit the small size of the Single Point
                if (scaleUpdated < SP_MIN_SCALE)
                {
                    conector.transform.localScale = new Vector3(CYLINDERCONECTOR_FACTOR * SP_MIN_SCALE * normScaleVector.x, conector.transform.localScale.y/* * normScaleVector.y*/, CYLINDERCONECTOR_FACTOR * SP_MIN_SCALE * normScaleVector.z);
                }
                else
                {
                    conector.transform.localScale = new Vector3(CYLINDERCONECTOR_FACTOR * scaleUpdated * normScaleVector.x, conector.transform.localScale.y /** normScaleVector.y*/, CYLINDERCONECTOR_FACTOR * scaleUpdated * normScaleVector.z);

                }
            }//end foreach connector
        }
    }

    public void SendSinglePointsList()
    {
        //Create a new list to send the info
        Dictionary<string, List<Vector3>> singlePoints_byParentID = new Dictionary<string, List<Vector3>>();

        //Fill the list        
        foreach (GameObject sp in singlePoints_GameObjectsList)
        {
            //Get parent (areaID) and position of the single point 
            string _areaID = sp.transform.parent.GetComponent<ObjectStateManager>().areaID;
            Vector3 _position = sp.transform.localPosition;

            //Introduce in a Dictionary to order by areasID
            List<Vector3> positions_List = new List<Vector3>();
            //Check if this area node is already in the dictionary.
            if (singlePoints_byParentID.TryGetValue(_areaID, out positions_List))
            {
                //It is, so update the parent with the new kid
                if (!positions_List.Contains(_position)) { positions_List.Add(_position); }
            }
            else
            {
                //It not, so is the first time that this parent appear. We have to introduce it in the dictionary                               
                List<Vector3> temppositions_List = new List<Vector3>();
                temppositions_List.Add(_position);
                singlePoints_byParentID.Add(_areaID, temppositions_List);
            }
        }
        //Send the list for each areaID
        foreach (KeyValuePair<string, List<Vector3>> pair in singlePoints_byParentID)
        {
            CSinglePointIO singlePointsReceived = new CSinglePointIO();
            singlePointsReceived.areaID = pair.Key;
            singlePointsReceived.singlepointgroupID = editingSPGroup_ID;
            singlePointsReceived.positions = new List<CIOPositionVector>();     
            foreach (var item in pair.Value)
            {
                CIOPositionVector temp =new CIOPositionVector();
                temp.x = (double)item.x;
                temp.y = (double)item.y;
                temp.z = (double)item.z;
                singlePointsReceived.positions.Add(temp);
            }            
            this.GetComponent<Core>().Do(new CIOCommand(TIOCommands.SendSinglePointToWebApp, singlePointsReceived), Constants.undoNotAllowed);
        }
    }

    /// <summary> Method to remove every singlePoint in the Capture Mode </summary>
    public void DeleteSinglePointGroups()
    {
        //1. Destroy every SinglePoints GameObjects and colectors
        foreach (GameObject sp in singlePoints_GameObjectsList)
        {
            Destroy(sp);
        }
        foreach (GameObject conector in conectors_GameObjectsList)
        {
            Destroy(conector);
        }

        //2. Clear the list and re-start booleans
        singlePoints_GameObjectsList.Clear();
        conectors_GameObjectsList.Clear();
        firstListComponent = true;

        //3. Indicate that a new SinglePoint Mode action has not been saved 
        //SP_saved(false);
        //Change Mode status with a data Modification action
        SinglePointMode_statusManager(SinglePointsActions_Type.DataModification);
    }

    /// <summary> Method to send and delete the captured single points </summary>
    public void SendAndDeleteSinglePoints()
    {
        //1. Send singlePointsList to the ASP.NET side        
        SendSinglePointsList();

        //2. Destroy every SinglePoints GameObjects and colectors
        DeleteSinglePointGroups();
    }


    //************************************************************************************************
    // SP Visualization Mode Methods
    //*************************************************************************************************

    /// <summary>
    /// Add singlePoint and the corresponding conector to the dictionary in the SP Visualization Mode
    /// </summary>
    /// <param name="_spGroupID"> ID to identify the singlePoint Group</param>
    /// <param name="_areaGO"> GameObject where the singlePoint have to be placed</param>
    /// <param name="_singlePointsList"> List of positions of the different SinglePoints</param>
    void AddSinglePointGroup_ToDictionary(int _spGroupID, GameObject _areaGO, List<Vector3> _singlePointsList)
    {
        //Temporary struct to save the data to store in the dictionary
        singlePointGroup_struct current_SPgroup;
        current_SPgroup.areaGO = _areaGO;
        current_SPgroup.singlePointGO_List = new List<GameObject>();
        current_SPgroup.conectorGO_List = new List<GameObject>();

        //Check if this area node is already in the dictionary.
        singlePointGroup_struct temp_SPgroup;
        if (singlePointsMap_bySPGroupID.TryGetValue(_spGroupID, out temp_SPgroup))
        {
            Debug.Log("The SP Group with the revision ID: " + _spGroupID + " is already in the dictionary");
        }
        else
        {
            foreach (var singlePoint_position in _singlePointsList)
            {
                //** Create the GameObject
                //Create the Single Point Object
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //Add to a layer
                sphere.layer = LayerMask.NameToLayer("singlePoints_layer");
                //Set the father of the SinglePoint. Every singlePoint of this list has the same father
                sphere.transform.parent = _areaGO.transform;
                //Set the local position of the singlePoint
                sphere.transform.localPosition = singlePoint_position;

                //Add SinglePoint GameObject to the map
                current_SPgroup.singlePointGO_List.Add(sphere);

                //** Draw the Single Point  
                //Set the sphere size
                SetGameObjectScale(sphere);
                //SP Color
                sphere.GetComponent<Renderer>().material.color = SP_color;

                //** Concatenated points
                //Create a cylinder beetwen the previous sphere and the current one. IMP: Parameters are global postions
                if (bDrawConectors)
                {
                    if (current_SPgroup.singlePointGO_List.Count > 1)
                    {
                        //Begin and end of the conector
                        Vector3 startPos = current_SPgroup.singlePointGO_List[current_SPgroup.singlePointGO_List.Count - 2].transform.position;
                        Vector3 endPos = current_SPgroup.singlePointGO_List[current_SPgroup.singlePointGO_List.Count - 1].transform.position;

                        //** Place and draw the SPConector
                        //Create the Single Point Object
                        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        //Add to a layer
                        cylinder.layer = LayerMask.NameToLayer("SPConectors_layer");
                        //Set the cylinder position and dimensions
                        Vector3 dir = endPos - startPos;
                        cylinder.transform.position = dir / 2.0f + startPos; //middle point between two spheres
                        cylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
                        //Get the component where the SP is placed, move the cylinder to be a chidren.
                        cylinder.transform.parent = current_SPgroup.areaGO.transform;

                        //** Draw Conector
                        //Set scale
                        cylinder.transform.localScale = new Vector3(1.0f, dir.magnitude * 0.5f * normScaleVector.y, 1.0f); // Y scale depends on the start and end points and it has to be normalised with the Turbine scale      
                        SetGameObjectScale(cylinder);
                        //Set color
                        cylinder.GetComponent<Renderer>().material.color = conector_color;

                        //** Add to the list of the struct
                        current_SPgroup.conectorGO_List.Add(cylinder);
                    }
                }
            }//end foreach

            //Change the colour of the last singlepoint (NOT in SP Visualizer mode)
            //current_SPgroup.singlePointGO_List[current_SPgroup.singlePointGO_List.Count-1].GetComponent<Renderer>().material.color = lastSP_color;

            //Add the single point group to the disctionary                               
            singlePointsMap_bySPGroupID.Add(_spGroupID, current_SPgroup);
        }
    }

    public void DeleteASinglePointGroup_byAreaGO(GameObject areaGO)
    {
        //Remove every singlePoint
        foreach (var pair in singlePointsMap_bySPGroupID)
        {
            if (pair.Value.areaGO == areaGO)
            {
                foreach (var go in pair.Value.singlePointGO_List)
                {
                    Destroy(go);
                }
                foreach (var go in pair.Value.conectorGO_List)
                {
                    Destroy(go);
                }
            }
        }
    }
    

    /// <summary> Set the localScale of a specific SinglePoint or conector </summary>
    /// <param name="go"> GameObject to be re-scaled</param>
    void SetGameObjectScale(GameObject go) {

        float scaleUpdated = GetSinglePoint_scale();

        //Limit the small size of the Single Point
        if (scaleUpdated < SP_MIN_SCALE)
        {
            if (go.name == "Sphere")
            {
                go.transform.localScale = SP_MIN_SCALE * normScaleVector;
            }

            if (go.name == "Cylinder")
            {
                go.transform.localScale = new Vector3(CYLINDERCONECTOR_FACTOR * SP_MIN_SCALE * normScaleVector.x, go.transform.localScale.y, CYLINDERCONECTOR_FACTOR * SP_MIN_SCALE * normScaleVector.z); // Y scale depends on the start and end points and it has to be normalised with the Turbine scale      
            }

        }
        else
        {
            if (go.name == "Sphere")
            {
                go.transform.localScale = scaleUpdated * normScaleVector;
            }

            if (go.name == "Cylinder")
            {
                go.transform.localScale = new Vector3(CYLINDERCONECTOR_FACTOR * scaleUpdated * normScaleVector.x, go.transform.localScale.y, CYLINDERCONECTOR_FACTOR * scaleUpdated * normScaleVector.z); // Y scale depends on the start and end points and it has to be normalised with the Turbine scale      
            }
        }
    }

    /// <summary> Set the localScale of every singlePoint and Conector stored in the dictionaty </summary>
    void SetSinglePointDictionary_Scale()
    {
        //Iterate throught the dictionary  to re-scale every gameobject
        foreach (var pair in singlePointsMap_bySPGroupID)
        {
            foreach (var go in pair.Value.singlePointGO_List) {
                SetGameObjectScale(go);
            }
            foreach (var go in pair.Value.conectorGO_List)
            {
                SetGameObjectScale(go);
            }
        }
    }

    /// <summary>  Remove a specific singlePoint group of the dictionary in SP Visualization Mode </summary>
    /// <param name="_spGroupID"> SinglePoints Group ID </param>
    void RemoveSinglePointGroup_FromDictionary(int _spGroupID)
    {
        //Remove Gameobjects
        singlePointGroup_struct value;
        singlePointsMap_bySPGroupID.TryGetValue(_spGroupID, out value);

            foreach (var go in value.singlePointGO_List)
            {
                Destroy(go);
            }
            foreach (var go in value.conectorGO_List)
            {
                Destroy(go);
            }

        //Remove dictionary value
        singlePointsMap_bySPGroupID.Remove(_spGroupID);
    }

    /// <summary> Method to receive a singlePoint group to add to the dictionary </summary>
    /// <param name="_spGroupID"> SinglePoints Group Identifier </param>
    /// <param name="_areaGO"> GameObject where place the singlePoint</param>
    /// <param name="_singlePointsList"> List of position of all the single points of the group</param>
    public void ReceiveSinglePoints_ToAdd(int _spGroupID, GameObject _areaGO, List<Vector3> _singlePointsList)
    {
        AddSinglePointGroup_ToDictionary(_spGroupID, _areaGO, _singlePointsList);
    }

    /// <summary> Method to receive a singlePoint group to remove from the dictionary</summary>
    /// <param name="_spGroupID"> SinglePoints Group Identifier </param>
    public void ReceiveSinglePoints_ToHide(int _spGroupID)
    {
        RemoveSinglePointGroup_FromDictionary(_spGroupID);
    }

    /// <summary> Method to iterate throught the singlePoint Dictionary and remove every cells </summary>
    public void RemoveAllSinglePoints()
    {
        //Remove every singlePoint
        foreach (var pair in singlePointsMap_bySPGroupID)
        {
            foreach (var go in pair.Value.singlePointGO_List)
            {
                Destroy(go);
            }
            foreach (var go in pair.Value.conectorGO_List)
            {
                Destroy(go);
            }
        }

        singlePointsMap_bySPGroupID.Clear();
    }

    public bool ContainArea_SinglePointDictiorary(GameObject area) {
        //Iterate throught the dictionary  to find a given area
        foreach (var pair in singlePointsMap_bySPGroupID)
        {
            if (area == pair.Value.areaGO)
            {
                return true;
            }
        }
        return false;
    }


    //************************************************************************************************
    // SP Edit Mode Methods
    //*************************************************************************************************

    public void ReceiveSinglePoints_ToEdit(int _spGroupID, GameObject _areaGO, List<Vector3> _singlePointsList)
    {
        singlePointMode_currentStatus = SinglePointsStatus_Type.Changes_Unsaved;
        editingSPGroup_ID = _spGroupID;

        foreach (var singlePoint_position in _singlePointsList)
        {
            AddSinglePoint(_areaGO, singlePoint_position);
        }
    }

    //************************************************************************************************
    // SinglePoints state machine
    //*************************************************************************************************

    public SinglePointsStatus_Type GetSinglePointMode_Status()
    {
        return singlePointMode_currentStatus;
    }

    public SinglePointSubmode_Type GetSinglePoint_SubMode()
    {
        return singlePoint_currentSubmode;
    }

    public void SetSinglePoint_SubMode(SinglePointSubmode_Type newSubmode)
    {
        singlePoint_currentSubmode = newSubmode;
    }

    public void SinglePointMode_statusManager(SinglePointsActions_Type action)
    {
        if (action == SinglePointsActions_Type.DataModification)
        {
            //Manage mode according with actions
            if (singlePointMode_currentStatus == SinglePointsStatus_Type.EmptyMode)
            {
                singlePointMode_currentStatus = SinglePointsStatus_Type.FirstChanges_unsaved;
            }

            else if (singlePointMode_currentStatus == SinglePointsStatus_Type.Changes_Saved)
            {
                singlePointMode_currentStatus = SinglePointsStatus_Type.Changes_Unsaved;
            }

        }

        else if (action == SinglePointsActions_Type.SaveData)
        {
            singlePointMode_currentStatus = SinglePointsStatus_Type.Changes_Saved;
        }

        else if (action == SinglePointsActions_Type.ExitMode)
        {
            singlePointMode_currentStatus = SinglePointsStatus_Type.EmptyMode;
        }
        //Change icon according to the new status
        hom3r.quickLinks.uiObject.GetComponent<UIManager>().SPSave_IconManager();

    }


    //************************************************************************************************
    // SinglePoints I/O management Methods
    //*************************************************************************************************

    /// <summary>
    /// Re-sacle ingle points in SP Capture and SP Visualizer Mode
    /// </summary>
    public void UpdateSinglePointScale()
    {
        //Update scale if there is any single point
        if (singlePoints_GameObjectsList.Count != 0)
        {
            SetSinglePointAndConectorList_Scale(everyItem);
        }

        if (singlePointsMap_bySPGroupID.Count != 0)
        {
            SetSinglePointDictionary_Scale();
        }
    }

    /// <summary>
    /// Method that inform if a GameObject has a Single Point as a child
    /// </summary>
    /// <param name="areaGO"> GameObject parent</param>
    /// <returns> Position of the child. If there is no child return Vector3.zero</returns>
    public Vector3 HasSinglePoint(GameObject areaGO)
    {
        Vector3 position;
        position = Vector3.zero;
        if (areaGO.transform.childCount != 0)
        {
            position = areaGO.transform.GetChild(0).transform.localPosition;
        }
        return position;
    }

    /// <summary>
    /// Method to set the identifier of the single point group that is being edited
    /// </summary>
    public void SetSinglePointGroupID(int _id)
    {
        editingSPGroup_ID = _id;
    }
    
    /// <summary> Method to get the identifier of the single point group that is being edited </summary>
    public int GetSinglePointGroupID()
    {
        return editingSPGroup_ID;
    }

    /// <summary>
    /// Check if a given gameObject is a single point or a connector
    /// </summary>
    /// <param name="obj"> gameObject to be checked</param>
    /// <returns></returns>
    public bool IsASinglePoint(GameObject obj)
    {
        return (obj.name == "Sphere" || obj.name == "Cylinder");
    }

    /// <summary> Undo a previous action </summary>
    public void Undo_SinglePointAction()
    {
        if (undoStack.Count > 0)
        {
            undoCommand_struct command = undoStack.Pop();

            if (command.action == modeAction.ADD)
            {
                RemoveSinglePointAndConectors(command.gameObjectSP, command.areaToPlaceSP, false);
            }
            else if (command.action == modeAction.ADDOVERCONNETOR)
            {
                RemoveSinglePointAndConectors(command.gameObjectSP, command.areaToPlaceSP, false);
            }
            else if (command.action == modeAction.REMOVE)
            {
                AddSinglePoint(command.areaToPlaceSP, command.singlePointPos, false);
                ResetUndoStack();
            }
            else if (command.action == modeAction.REMOVEOVERCONNECTOR)
            {
                AddSinglePointToAList_overConnector(command.connectorToPaceSP, command.areaToPlaceSP, command.singlePointPos, false);
                ResetUndoStack();
            }
            else if (command.action == modeAction.REMOVEFIRSTSP)
            {
                AddSinglePointToAList_atTheBeginning(command.areaToPlaceSP, command.singlePointPos);
                ResetUndoStack();
            }
            else if (command.action == modeAction.EDITSP)
            {
                EditSinglePoint(command.areaToPlaceSP, command.singlePointPos);
                //ResetUndoStack();
            }
        }
    }

    /// <summary> Clear the undo stack </summary>
    public void ResetUndoStack() {
        undoStack.Clear();
    }

    //************************************************************************************************
    // Single Points (anchors) for Labeling Methods
    //*************************************************************************************************
    //    /// <summary>
    ///// Method to add a point to any point in the surface of an object (in world space)
    ///// </summary>
    //public void AddLabelToSurface(string labelID, GameObject _obj, string text, bool largeText = false)
    //{
    //    //** Compute position
    //    // Find point and direction for casting ray
    //    Vector3 bbCenter = _obj.GetComponent<Collider>().bounds.center;         //BoundingBox center in world space
    //    Vector3 bbCenterProjection = new Vector3(bbCenter.x, 0.0f, 0.0f);
    //    Vector3 castDirection = bbCenter - bbCenterProjection;
    //    castDirection.Normalize();

    //    float castDistance = _obj.GetComponent<Collider>().bounds.size.y + _obj.GetComponent<Collider>().bounds.size.z;
    //    Vector3 castPoint = bbCenterProjection + castDirection * castDistance;
    //    bool objRaycasted = false; //To control if the _obj has been raycasted

    //    // Ray cast
    //    RaycastHit[] hits;
    //    hits = Physics.RaycastAll(castPoint, -castDirection, Mathf.Infinity);

    //    // Select first raycasted point in our object
    //    Vector3 position = _obj.transform.InverseTransformPoint(bbCenter); //default value (Transform from world to local position)
    //    foreach (RaycastHit hit in hits)
    //    {
    //        if (hit.collider.gameObject == _obj)
    //        {
    //            //Get the obg hit point in local position
    //            position = hit.point;
    //            //Transform from world to local position, for ShowLabel
    //            position = _obj.transform.InverseTransformPoint(position);
    //            //text = "Number of flaws: 3";//"Hit Point";
    //            objRaycasted = true;
    //            break;
    //        }
    //    }

    //    //If _obj has not been raycasted, then use a mesh vertex position to place the label
    //    if (!objRaycasted)
    //    {
    //        //Get the furthest vertex
    //        Mesh objMesh = _obj.GetComponent<MeshFilter>().mesh;
    //        //Vertices positions in local space
    //        Vector3[] vertices = objMesh.vertices;
    //        // Transform from world to local position, for get the distance
    //        bbCenter = _obj.transform.InverseTransformPoint(bbCenter);
    //        //Get the Furthest position
    //        position = SortDistances(vertices, bbCenter);          
    //    }
    //    // CREATE LABEL
    //    AddLabelToSinglePoint(labelID, _obj, position, text, largeText);            

    //}
    
    ///// <summary>
    ///// Method to add a point to any point in the surface of an object (in world space)
    ///// </summary>
    //public void AddLabelToGameObjectGroup(string labelID, List<GameObject> _objects, string text = "", bool largeText = false)
    //{

    //    //******** Compute position
    //    // Bounding box of every child
    //    Bounds goListBounds = orbitPlaneGO.GetComponent<NavigationManager>().ComputeBoundingBox(_objects);
    //    Vector3 bbCenter = goListBounds.center;                                 // BoundingBox center in world space
    //    Vector3 bbCenterProjection = new Vector3(bbCenter.x, 0.0f, 0.0f);       // BB center X axis projection
    //    Vector3 castDirection = bbCenter - bbCenterProjection;                  // Ray direction
    //    castDirection.Normalize();
    //    float castDistance = goListBounds.size.y + goListBounds.size.z + 500.0f;
    //    Vector3 castPoint = bbCenterProjection + castDirection * castDistance;  //Ray origin point

    //    bool objRaycasted = false; //To control if the _obj has been raycasted
    //    //Defuals value: first component of the list
    //    GameObject surfaceGO = _objects[0];
    //    //Vector3 closestVertex = surfaceGO.transform.InverseTransformPoint(bbCenter); 
    //    Vector3 closestVertex = bbCenterProjection; // Fix bug with some components
        
    //    // Ray cast
    //    RaycastHit[] hits;
    //    hits = Physics.RaycastAll(castPoint, -castDirection, Mathf.Infinity);
        
    //    // Select first raycasted point in our list of objects
    //    foreach (RaycastHit hit in hits)
    //    {
    //        if ( _objects.Contains( hit.collider.gameObject) )
    //        {
    //            surfaceGO = hit.collider.gameObject;
    //            //Get the obj hit and Transform from world to local position
    //            if (Vector3.Distance(hit.point, bbCenterProjection) > Vector3.Distance(closestVertex, bbCenterProjection))  // Fix bug with some components               
    //                closestVertex = surfaceGO.transform.InverseTransformPoint(hit.point);        
    //            objRaycasted = true;
    //        }            
    //    }

    //    //If _obj has not been raycasted, then use a mesh vertex position to place the label
    //    if (!objRaycasted)
    //    {            
    //        float minDistance = 0.0f; ;
    //        bool fistTime = true;
    //        float dist;
    //        // Go throught every GameObject of the list to: (1)Get the closest vertex to the Bounding Box center for each GameObject. (2) Get the closest vertex from the closest vertex for each GO list.   
    //        foreach (GameObject go in _objects)
    //        {
    //            //Get the furthest vertex
    //            Mesh objMesh = go.GetComponent<MeshFilter>().mesh;
    //            //Vertices positions in local space
    //            Vector3[] vertices = objMesh.vertices;
    //            // Transform from world to local position, for get the distance
    //            Vector3 bbCenter_localGOPos = go.transform.InverseTransformPoint(bbCenter);
    //            //Get the Furthest position
    //            Vector3 currentGO_LocalPosition = SortDistances(vertices, bbCenter_localGOPos); // Sort vertices by distance to  Bounding Box center X axis projection
                
    //            //Get the closest one
    //            dist = (currentGO_LocalPosition - bbCenter_localGOPos).sqrMagnitude;
    //            if (fistTime)
    //            {
    //                minDistance = dist;
    //                surfaceGO = go;
    //                closestVertex = currentGO_LocalPosition;
    //                fistTime = false;
    //            }
    //            else if (dist < minDistance) {
    //                minDistance = dist;
    //                surfaceGO = go;
    //                closestVertex = currentGO_LocalPosition;
    //            }
    //        }
    //    }

    //    // Create label. Important to send the label position in the surface object local coordinates
    //    AddLabelToSinglePoint(labelID, surfaceGO, closestVertex, text, largeText);

    //}
        
   
    ///// <summary> Add a Label in a specific SinglePoint </summary>
    ///// <param name="obj"> gameObject where place the SinglePoint anchor</param>
    ///// <param name="position"> anchor position</param>
    ///// <param name="labelText"> Text to show in the label</param>

    //public void AddLabelToSinglePoint(string labelID, GameObject obj, Vector3 position, string labelText, bool largeText = false)

    //{
    //    //if (!this.GetComponent<Labels_Script>().LabelContains(labelID) && !this.GetComponent<Labels_Script>().LabelContains(this.GetComponent<ModelManagement_Script>().GetSpecialAncestorID_ByAreaID(labelID)))
    //    //{
    //        GameObject fatherGO = GameObject.FindGameObjectWithTag("go_father");
    //        this.GetComponent<LabelManager>().AddLabel(labelID, fatherGO, obj, position, labelText, largeText);

    //    //}
    //    //else {
    //        //Debug.Log ("There is already a label with the same ID");
    //    //}
    //}

    //Vector3 SortDistances(Vector3[] positions, Vector3 origin)
    //{
    //    float[] distances = new float[positions.Length];
    //    for (int i = 0; i < positions.Length; i++)
    //    {
    //        distances[i] = (positions[i] - origin).sqrMagnitude;
    //    }
    //    System.Array.Sort(distances, positions);
    //    //return positions[positions.Length-1];
    //    return positions[0];
    //}
    
}//END CLASS SinglePointManagement_Script

