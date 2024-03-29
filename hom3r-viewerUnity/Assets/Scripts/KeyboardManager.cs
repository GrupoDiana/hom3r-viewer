#define kosicheChapuzza

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class KeyboardManager : MonoBehaviour {

    // bool isNavigatingMouse;     // Mouse is moving while right button or scroll wheel is pressed 
    //bool isDraggingMouse;       // Mouse is moving while left button is pressed
    //bool firstTime;
    //bool draggingObject;

    //GameObject current_rayCastedSinglePoint;         //Object ray-casting
    //bool startEditingSP;                            //To control when the user is 
    //bool addingSP;

    //Layers ID
    int productRootLayer;
    //int singlePointsLayer;
    //int SPConectorsLayer;

    Camera gizmoCamera;
    int gizmoLayer;
    int labelsUILayer;
    
    //Mouse Cursor
    public Texture2D cursorTextureDeleteSP, cursorTextureCaptureSP, cursorTextureSimpleHide, cursorTextureLayoutExplosion, cursorTexturePanNav;
    //CursorMode cursorMode;
    public Vector2 hotSpot;
    //bool blockedByMovingSP;
    //public bool SPCapture_restrictedByArea;
    bool keyControlPressed;                     //To know when the control key has been pressed in the web application

    int generateExperimentParticipants;
    //Mouse 
    //Vector3 previousMousePosition;              //Mouse position
    //Vector3 previousDragAndDropPosition;        // Mouse position during drag and drop actions

    private void Awake()
    {
        
        //Initialize variables           
        //isDraggingMouse = false;
        //firstTime = true;
        //cursorMode = CursorMode.Auto; //CursorMode.ForceSoftware; 
        //hotSpot = Vector2.zero;        
        keyControlPressed = false;
        //draggingObject = false;
        generateExperimentParticipants = 1;


        // Initialize the layer masks
        // NameToLayer() returns the layer index 
        //'1 << ...' converts that to a bit mask, turning on the bit associated with that layer
        productRootLayer        = 1 << LayerMask.NameToLayer(hom3r.state.productRootLayer);
        //singlePointsLayer   = 1 << LayerMask.NameToLayer("singlePoints_layer");
        //SPConectorsLayer    = 1 << LayerMask.NameToLayer("SPConectors_layer");
        gizmoLayer          = 1 << LayerMask.NameToLayer("gizmo_layer");        
        labelsUILayer       = 1 << LayerMask.NameToLayer(hom3r.state.labelsUILayer);


        gizmoCamera = GameObject.FindWithTag("GizmoCamera").GetComponent<Camera>();


        //Disable keyboard abduction by WebGL
#if !UNITY_EDITOR && UNITY_WEBGL
            UnityEngine.WebGLInput.captureAllKeyboardInput = false;
#endif
    }

    
    // Update is called once per frame
    void Update () {
        if (hom3r.state.platform != THom3rPlatform.Android && hom3r.state.platform!= THom3rPlatform.IOS)
        {
            if (hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveKeyboard())
            {
                Keyboard_Manager();
            }                       
        }                      
    }


    ////////////////////
    // Others Methods //
    ////////////////////
    public void SetKeyControlPressed(bool _keyControlPressed)
    {
        keyControlPressed = _keyControlPressed;
    }


    /// <summary>Keyboard Manager, this module manages the app keyboard interaction.</summary>
    private void Keyboard_Manager()
    {
        if (Input.anyKey)
        {
            /////////////////
            //UNDO ACTION
            /////////////////
            //Normally is activate pressing Control + Z. But Inside the editor is done by pressing only Z

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Z))
#else
            if ((keyControlPressed || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyUp(KeyCode.Z))
#endif
            {
                // CTRL + Z            
                this.GetComponent<Core>().Undo();
            }
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Y))
#else
            if ((keyControlPressed || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyUp(KeyCode.Y))
#endif
            {
                // CTRL + Z            
                this.GetComponent<Core>().Redo();
            }

            ////////////////
            //Navigation
            ////////////////

            ////Translation
            if (Input.GetKey(KeyCode.RightArrow))
            {
                //Create command and send to core
                
                hom3r.coreLink.Do(new CNavigationCommand(TNavigationCommands.MouseMovement, 0.01f, 0.0f, 0.0f));
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                //Create command and send to core
                hom3r.coreLink.Do(new CNavigationCommand(TNavigationCommands.MouseMovement, -0.01f, 0.0f, 0.0f));
            }
            //// Orbit
            if (Input.GetKey(KeyCode.UpArrow))
            {
                hom3r.coreLink.Do(new CNavigationCommand(TNavigationCommands.MouseMovement, 0.0f, 0.01f, 0.0f));
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                hom3r.coreLink.Do(new CNavigationCommand(TNavigationCommands.MouseMovement, 0.0f, -0.01f, 0.0f));
            }            
            
            if (Input.GetKeyDown(KeyCode.F12))
            {
                bool _value = hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveUI();
                hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveUI, !_value));

                //hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveUI, true));
            }          
            if (Application.isEditor)
            {
                if (Input.GetKeyDown(KeyCode.KeypadPlus))
                {
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationHelper>().SetBiggerLines();
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationHelper>().SetBiggerCamera();
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationHelper>().SetBiggerPointToLook();
                }
                if (Input.GetKeyDown(KeyCode.KeypadMinus))
                {
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationHelper>().SetSmallerLines();
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationHelper>().SetSmallerCamera();
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationHelper>().SetSmallerPointToLook();
                }               
                if (Input.GetKeyDown(KeyCode.E))
                {
                    this.GetComponent<IOManager>().IOSetNavigationAxis("");
                }
                if (Input.GetKeyDown(KeyCode.T))
                {
                    this.GetComponent<IOManager>().IOSmartTransparency("");
                    hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().GenerateNewParticipant();
                }
                if (Input.GetKeyDown(KeyCode.M))
                {
                    //this.GetComponent<IOManager>().IOGetProductModel();
                    //hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().Set3DModelScale(0.5f);
                }
                if (Input.GetKeyDown(KeyCode.N))
                {
                    // this.GetComponent<IOManager>().IOUnselectPart("");
                    //this.GetComponent<IOManager>().IOConfiguration("");
                    hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.DisableSmartTransparencyInstanlly));
                }
               
                if (Input.GetKeyDown(KeyCode.H))
                {                                     
                    this.GetComponent<IOManager>().IOLoadProductModel("");                 
                }
                if (Input.GetKeyDown(KeyCode.G))
                {
                    //C3DFileUrlIO tempList = new C3DFileUrlIO();
                    //tempList.data = new List<CIO3DFileData>();
                    //string filesPath = Application.dataPath.Substring(0, Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/")).LastIndexOf("/"));
                    //string basepath = "file:///" + filesPath;

                    //// IOANNINA PISTOL
                    //hom3r.quickLinks.scriptsObject.GetComponent<IOManager>().IOLoad3DFile_editor_help(tempList, "Pistol_basecolor.png", basepath + "/developmentModels/silversmithing/test/Pistol_basecolor.png", "png");
                    //hom3r.quickLinks.scriptsObject.GetComponent<IOManager>().IOLoad3DFile_editor_help(tempList, "Pistol_metallic_smoothness.png", basepath + "/developmentModels/silversmithing/test/Pistol_metallic_smoothness.png", "png");
                    //hom3r.quickLinks.scriptsObject.GetComponent<IOManager>().IOLoad3DFile_editor_help(tempList, "Pistol_normal.png", basepath + "/developmentModels/silversmithing/test/Pistol_normal.png", "png");
                    //hom3r.quickLinks.scriptsObject.GetComponent<IOManager>().IOLoad3DFile_editor_help(tempList, "Pistol_Phong.mtl", basepath + "/developmentModels/silversmithing/test/Pistol_Phong.mtl", "mtl");
                    //hom3r.quickLinks.scriptsObject.GetComponent<IOManager>().IOLoad3DFile_editor_help(tempList, "Pistol_Phong.obj", basepath + "/developmentModels/silversmithing/test/Pistol_Phong.obj", "obj");
                    //string json_string = JsonUtility.ToJson(tempList);

                    //this.GetComponent<IOManager>().IOLoad3DFiles(json_string);
                    this.GetComponent<IOManager>().IOLoad3DFiles("");
                }
                if (Input.GetKeyDown(KeyCode.F))
                {
                    //C3DFileUrlIO tempList = new C3DFileUrlIO();
                    //tempList.data = new List<CIO3DFileData>();
                    //string filesPath = Application.dataPath.Substring(0, Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/")).LastIndexOf("/"));
                    //string basepath = "file:///" + filesPath;


                    //hom3r.quickLinks.scriptsObject.GetComponent<IOManager>().IOLoad3DFile_editor_help(tempList, "rockmountain2.mtl", basepath + "/developmentModels/rock1/rockmountain2.mtl", "mtl");
                    //hom3r.quickLinks.scriptsObject.GetComponent<IOManager>().IOLoad3DFile_editor_help(tempList, "rockmountain2.obj", basepath + "/developmentModels/rock1/rockmountain2.obj", "obj");
                    //hom3r.quickLinks.scriptsObject.GetComponent<IOManager>().IOLoad3DFile_editor_help(tempList, "Rock-Texture-Surface.jpg", basepath + "/developmentModels/rock1/Rock-Texture-Surface.jpg", "jpg");



                    //string json_string = JsonUtility.ToJson(tempList);

                    //this.GetComponent<IOManager>().IOLoad3DFiles(json_string);
                    this.GetComponent<IOManager>().IOGetProductModel();
                }                
                if (Input.GetKeyDown(KeyCode.L))
                {                                        
                    //this.GetComponent<IOManager>().IOAddLabel("");
                    //this.GetComponent<IOManager>().IOAddLabel("");                 
                }
                if (Input.GetKeyDown(KeyCode.S))
                {
                    //this.GetComponent<IOManager>().IOAddLabel("");
                    //this.GetComponent<IOManager>().IOShowBoardLabel("");
                    //this.GetComponent<IOManager>().IOShowAnchoredLabel("");
                    this.GetComponent<IOManager>().IOShowLabelList("");
                    //this.GetComponent<IOManager>().IOSelectPart("");

                    //this.GetComponent<IOManager>().testShowLabel(1);
                }
                if (Input.GetKeyDown(KeyCode.A))
                {
                    this.GetComponent<IOManager>().testShowLabel(2);
                    //this.GetComponent<IOManager>().IOAddAnchoredLabel("");
                }
                if (Input.GetKeyDown(KeyCode.K))
                {                    
                    // this.GetComponent<IOManager>().IOSmartTransparency("");
                    hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.DisableSmartTransparencyInstanlly, THom3rCommandOrigin.io));
                    // this.GetComponent<IOManager>().IORemovePart("");
                }         
                if (Input.GetKeyDown(KeyCode.X))
                {
                    //this.GetComponent<Core_Script>().Do(new UICoreCommand("UI/ShowAll"), Constants.undoNotAllowed);
                    //this.GetComponent<IO_Script>().ShowLabelsWithAreaComponentName("false");
                    //this.GetComponent<IOManager>().SmartTransparency("true");
                    //hom3r.coreLink.Do(new CPointOnSurfaceCommand(TPointOnSurfaceCommands.StartPointCapture));
                }
                if (Input.GetKeyDown(KeyCode.D))
                {
                    //this.GetComponent<IOManager>().IOShowRemovedPart("");
                    this.GetComponent<IOManager>().IOShowAllRemovePart();
                }
                if (Input.GetKeyDown(KeyCode.R))
                {
                    //// this.GetComponent<IOManager>().IORemovePart("");
                    //// this.GetComponent<IOManager>().IORemoveLabel("1");
                    ////this.GetComponent<IOManager>().IORemoveAllLabels();
                    ////this.GetComponent<IOManager>().IORemoveAllBoardLabels();
                    ////this.GetComponent<IOManager>().IOResetSelectionColours();
                    //this.GetComponent<IOManager>().IOResetProductModel();

                    //C3DFileUrlIO tempList = new C3DFileUrlIO();
                    //tempList.data = new List<CIO3DFileData>();
                    //string filesPath = Application.dataPath.Substring(0, Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/")).LastIndexOf("/"));
                    //string basepath = "file:///" + filesPath;


                    //hom3r.quickLinks.scriptsObject.GetComponent<IOManager>().IOLoad3DFile_editor_help(tempList, "rockmountain2.mtl", basepath + "/developmentModels/rock1/rockmountain2.mtl", "mtl");
                    //hom3r.quickLinks.scriptsObject.GetComponent<IOManager>().IOLoad3DFile_editor_help(tempList, "rockmountain2.obj", basepath + "/developmentModels/rock1/rockmountain2.obj", "obj");
                    //hom3r.quickLinks.scriptsObject.GetComponent<IOManager>().IOLoad3DFile_editor_help(tempList, "Rock-Texture-Surface.jpg", basepath + "/developmentModels/rock1/Rock-Texture-Surface.jpg", "jpg");



                    //string json_string = JsonUtility.ToJson(tempList);

                    //this.GetComponent<IOManager>().IOLoad3DFiles(json_string);

                }

                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    //hom3r.coreLink.Do(new CExperimentCommand(TExperimentCommands.NextExperimentPoint));
                }
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    //hom3r.coreLink.Do(new CExperimentCommand(TExperimentCommands.PreviousExperimentPoint));
                }
            }//END if (Application.isEditor)           
        }
    }
}
