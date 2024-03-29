using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public enum TMouseIconMode { iddle, pan_navigation, remove, explosion, single_point_capture, single_point_remove }

public class UIManager : MonoBehaviour {

    // Canvas
    private GameObject canvas_GO;
    private GameObject gizmo_GO;
    // Panels
    private GameObject occlusionPanelGO;
    private GameObject hierarchyPanelGO;
    private GameObject navigationPanelGO;

    GameObject alertTextHandle;                         //Store a pointer to the loading text object    

    //--Navigation
    GameObject coordinates_icon_gameObject;                 //Store a pointer to 
    public Sprite ellipsoid_sprite;
    public Sprite cylinder_sprite;
    Image coordinates_img;

    //--Explosion
    // Image explode_img;
	//GameObject explode_icon_gameObject;
    //GameObject explodeLayout_icon_gameObject;
	public Sprite explode_sprite;
	public Sprite implode_sprite;
    
    //--Remove
    GameObject removeButtonGO;

    //--Smart Transparency
    GameObject smartTransparencyButtonGO;
    GameObject smartTransparencySliderGO;        
    bool blockOnChangeTransparencySliderEvent;

    public Color iconModeActive_color;
    public Color confirmColor;

    //--Isolate    
    GameObject isolateButtonGO;
    
    //--Show all
    GameObject showAllButtonGO;

    //-- Hide
    // GameObject remove_icon_GO;

    //--Single Point Location
    GameObject SPCaptureRemove_icon_GO;
    GameObject SPSave_icon_GO;
    GameObject SPClose_popUpPanel_GO;
    GameObject SPPanelMode_icon_GO;
    Image SPcapture_img;
    public Sprite SPcapture_sprite;
    public Sprite SPdelete_sprite;
    Image SPsave_img;
    public Sprite SPsaved_sprite;
    public Sprite SPunsaved_sprite;
    GameObject SPClose_icon_GO;

    //--Hierarchy
    //GameObject hierarchy_panel_GO;
    GameObject hierarchyIconGO;
    GameObject showPartNameLabel_icon_GO;          
    public Sprite component_sprite;
    public Sprite area_sprite;
    // Image hierarchy_img;
    bool firstTimeInside, firstTimeOutside;

    //--Selection
    public Text indicatedArea_text, indicatedComponent_text, indicatedArea_title, indicatedComponent_title;
    public Image indicatedNamePanel;
    public GameObject indicatedPanelGO;

    //-- Alert Text
    public GameObject alertTextPanelGO;
    bool showingAlertText;



    //Mouse Cursor
    private TMouseIconMode mouseIconMode;       // This is ours
    private CursorMode mouseCursorMode;         // This is a Unity thing 
    public Vector2 mouseCursorHotSpot;

    public Texture2D cursorTextureDeleteSP, cursorTextureCaptureSP, cursorTextureSimpleHide, cursorTextureLayoutExplosion;
    public Texture2D cursorTexturePanNav;

    // settings Panel
    GameObject openCloseSettingsButtonGO;
    GameObject settingsPanelGO;
    GameObject settingsPanelNavigationGO;
    GameObject settingsPanelCloseButtonGO;
    GameObject settingsPanelResetButtonGO;
    GameObject settingsPanelLatidudeCorrectionDropdownGO;
    GameObject settingsPanelLongitudeCorrectionDropdownGO;
    GameObject settingsPanelOblateCorrectionGO;
    GameObject settingsPanelNavigationSystemDropdownGO;
    GameObject settingsPanelExternaCameraToggleGO;
    GameObject settingsPanelChangeModelGO;
    GameObject settingsPanelAxisDropdownGO;
    GameObject settingsPanelObjectTypeTextGO;

    // Secundary camera
    GameObject secundaryCameraGO;
    
    

    void Update()
    {
      
    }

    private void Awake()
    {
        // Register in hom3r class
        hom3r.quickLinks.uiObject = GameObject.FindGameObjectWithTag("uiManager_script");

        //////////////////////
        // Init PANEL objets
        //////////////////////
        canvas_GO = GameObject.FindGameObjectWithTag("canvas_tag");
        navigationPanelGO = canvas_GO.transform.Find("Panel_Navigation").gameObject;
        occlusionPanelGO = canvas_GO.transform.Find("Panel_OcclusionManager").gameObject;
        hierarchyPanelGO = canvas_GO.transform.Find("Panel_Hierarchy").gameObject;
        settingsPanelGO = canvas_GO.transform.Find("SettingsPanel").gameObject;
        settingsPanelNavigationGO = settingsPanelGO.transform.Find("Panel_Navigation").gameObject;
        secundaryCameraGO = GameObject.FindGameObjectWithTag("secundarycamera");
        gizmo_GO = GameObject.FindGameObjectWithTag("gizmoModel_tag");

        /////////////////////////////////////////////////
        // Add pointer ENTER and EXIT EVENTS to panels
        ///////////////////////////////////////////////////
        this.AddEventsManagersToPanel(navigationPanelGO);
        this.AddEventsManagersToPanel(occlusionPanelGO);
        this.AddEventsManagersToPanel(hierarchyPanelGO);
        this.AddEventsManagersToPanel(settingsPanelGO);

        //////////////////////
        /// NAVIGATION Panel
        //////////////////////        
        navigationPanelGO.transform.Find("Image_home").gameObject.GetComponent<Button>().onClick.AddListener(OnClickHomeButton);
        navigationPanelGO.transform.Find("Image_coordinates").gameObject.GetComponent<Button>().onClick.AddListener(OnClickNavigationCoordinatesButton);

        //////////////////////
        /// Occlusion PANEL
        //////////////////////

        // REMOVE
        removeButtonGO = occlusionPanelGO.transform.Find("Image_remove").gameObject;
        removeButtonGO.GetComponent<Button>().onClick.AddListener(OnClickRemoveModeButton);
        
        // SMART-TRANSPARENCY      
        smartTransparencyButtonGO = occlusionPanelGO.transform.Find("Image_SmartTransparency").gameObject;
        smartTransparencyButtonGO.GetComponent<Button>().onClick.AddListener(OnClickSmartTransparencyUIButton);

        smartTransparencySliderGO = occlusionPanelGO.transform.Find("Slider_SmartTransparency").gameObject;
        this.InitSmartTransparencyAlphaValue();
        this.SetInteractableSmartTransparencySlider(false);
        smartTransparencySliderGO.GetComponent<Slider>().onValueChanged.AddListener(OnChangeTransparencySliderValue);
        blockOnChangeTransparencySliderEvent = false;


        // Explosion
        //occlusionPanelGO.transform.Find("Image_explodeLayout_implodeLayout").gameObject.GetComponent<Image>().sprite = explode_sprite;

        // Isolate Focus
        isolateButtonGO = occlusionPanelGO.transform.Find("Image_focus").gameObject;
        isolateButtonGO.GetComponent<Button>().onClick.AddListener(OnClickFocusButton);
        // Show all
        showAllButtonGO = occlusionPanelGO.transform.Find("Image_showAll").gameObject;
        showAllButtonGO.GetComponent<Button>().onClick.AddListener(OnClickShowAllButton);

        //////////////////////
        /// Hierarchy PANEL
        //////////////////////        
        //hierarchy_panel_GO = GameObject.FindGameObjectWithTag("panel_Hierarchy_tag");
        hierarchyIconGO = hierarchyPanelGO.transform.Find("Image_hierarchy").gameObject; // .FindGameObjectWithTag("hierarchy_icon_tag");
        //hierarchy_img = hierarchyIconGO.GetComponent<Image>();
        Hierarchy_ButtonManager(hom3r.state.currentSelectionMode == THom3rSelectionMode.AREA);
        // firstTimeInside = true;
        // firstTimeOutside = false;
        //showPartNameLabel_icon_GO = GameObject.FindGameObjectWithTag("showPartNameLabel_icon_tag");
        showPartNameLabel_icon_GO = hierarchyPanelGO.transform.Find("Image_showPartNameLabel").gameObject;

        // MOUSE
        mouseIconMode = TMouseIconMode.iddle;        
        mouseCursorMode = CursorMode.Auto; //CursorMode.ForceSoftware; 
        mouseCursorHotSpot = Vector2.zero;

        //////////////////////
        /// Settings PANEL        
        ////////////////////// 
        //Open Button
        openCloseSettingsButtonGO = canvas_GO.transform.Find("Image_OpenCloseSettings").gameObject;
        openCloseSettingsButtonGO.GetComponent<Button>().onClick.AddListener(OnClickOpenSettingsPanelButton);                
        //Close button
        settingsPanelCloseButtonGO = settingsPanelGO.transform.Find("Image_CloseSettings").gameObject;
        settingsPanelCloseButtonGO.GetComponent<Button>().onClick.AddListener(OnClickCloseSettingsPanelButton);
        //Nav system
        settingsPanelNavigationSystemDropdownGO = settingsPanelNavigationGO.transform.Find("Dropdown_navigation").gameObject;
        settingsPanelNavigationSystemDropdownGO.GetComponent<Dropdown>().onValueChanged.AddListener(OnChangeAuxPanelNavigationSystemDropdown);
        // Nav Axis
        settingsPanelAxisDropdownGO = settingsPanelNavigationGO.transform.Find("Dropdown_Axis").gameObject;
        settingsPanelAxisDropdownGO.GetComponent<Dropdown>().onValueChanged.AddListener(OnChangeSettingsPanelNavigationAxisDropdown);

        // Mapping corrections
        settingsPanelLatidudeCorrectionDropdownGO = settingsPanelNavigationGO.transform.Find("Dropdown_LatitudeCorrection").gameObject;
        settingsPanelLatidudeCorrectionDropdownGO.GetComponent<Dropdown>().onValueChanged.AddListener(OnChangeSettingsPanelLatitudeDropdown);

        settingsPanelLongitudeCorrectionDropdownGO = settingsPanelNavigationGO.transform.Find("Dropdown_LongitudeCorrection").gameObject;
        settingsPanelLongitudeCorrectionDropdownGO.GetComponent<Dropdown>().onValueChanged.AddListener(OnChangeSettingsPanelLongitudeDropdown);
        //Reset button
        settingsPanelResetButtonGO = settingsPanelNavigationGO.transform.Find("Button_Reset").gameObject;
        settingsPanelResetButtonGO.GetComponent<Button>().onClick.AddListener(OnClickAuxPanelResetButton);
        // Reset model button
        settingsPanelChangeModelGO = settingsPanelGO.transform.Find("Button_Change3D").gameObject;
        settingsPanelChangeModelGO.GetComponent<Button>().onClick.AddListener(OnClickSettingsPanelChange3DButton);

        //Technicalview togle
        settingsPanelExternaCameraToggleGO = settingsPanelGO.transform.Find("Toggle_TechnicalView").gameObject;
        settingsPanelExternaCameraToggleGO.GetComponent<Toggle>().onValueChanged.AddListener(OnChangeSettingsPanelTechnicalviewToggle);

        // Corrections
        settingsPanelOblateCorrectionGO = settingsPanelNavigationGO.transform.Find("Dropdown_OblatesCorrection").gameObject;
        settingsPanelOblateCorrectionGO.GetComponent<Dropdown>().onValueChanged.AddListener(OnChangesSettingsPanelOblateCorrection);
        // Hide settings menu
        settingsPanelGO.SetActive(false);
        // Object type text
        settingsPanelObjectTypeTextGO = settingsPanelNavigationGO.transform.Find("InputField_ObjectType").gameObject;

        //
        if (hom3r.state.platform == THom3rPlatform.WebGL)
        {
            settingsPanelChangeModelGO.GetComponent<Button>().interactable = false;
        }


        /////////////////////////
        /// SecundaryCamera
        //////////////////////////
        secundaryCameraGO.GetComponent<Camera>().enabled = false;

        ///////////////////////////
        ///// StandAlone Panel
        ////////////////////////////
        //standaloneTestObjectsDropdownGO = standalonePanelGO.transform.Find("Dropdown_TestObjects").gameObject;
        ////standaloneTestObjectsDropdownGO.GetComponent<Dropdown>().onValueChanged.AddListener(OnChangeStandalonePanelTestObjectsDropdown);
        
        //standaloneLoadTestObjectsButtonGO = standalonePanelGO.transform.Find("Button_LoadTestObject").gameObject;
        //standaloneLoadTestObjectsButtonGO.GetComponent<Button>().onClick.AddListener(OnClickStandalonePanelLoadTestObjectsButton);

        //standaloneOtherObjectsDropdownGO = standalonePanelGO.transform.Find("Dropdown_OtherObjects").gameObject;
        //standaloneLoadOtherObjectsButtonGO = standalonePanelGO.transform.Find("Button_LoadOtherObject").gameObject;
        //standaloneLoadOtherObjectsButtonGO.GetComponent<Button>().onClick.AddListener(OnClickStandalonePanelLoadOtherObjectsButton);
    }

    private void AddEventsManagersToPanel(GameObject _panel)
    {
        // Add event to POINTER ENTER        
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { OnPointerEnterDelegate((PointerEventData)data); });
        _panel.GetComponent<EventTrigger>().triggers.Add(entry);
        // Add event to POINTER Exit
        EventTrigger.Entry entry2 = new EventTrigger.Entry();
        entry2.eventID = EventTriggerType.PointerExit;
        entry2.callback.AddListener((data) => { OnPointerExitDelegate((PointerEventData)data); });
        _panel.GetComponent<EventTrigger>().triggers.Add(entry2);
    }

    public void OnPointerEnterDelegate(PointerEventData data)
    {
        hom3r.state.navigationBlocked = true;
        hom3r.state.selectionBlocked = true;
        hom3r.coreLink.Do(new CSelectionCommand(TSelectionCommands.IndicateAllOff));        
    }

    public void OnPointerExitDelegate(PointerEventData data)
    {
        hom3r.state.navigationBlocked = false;
        hom3r.state.selectionBlocked = false;
    }


    private void InitSmartTransparencyAlphaValue()
    {
        smartTransparencySliderGO.GetComponent<Slider>().value = 0.3f;
        hom3r.state.smartTransparencyAlphaLevel = 0.3f;
    }
    

    // Use this for initialization
    void Start ()
    {        
        //--EXPLOSION
  //      explode_icon_gameObject = GameObject.FindGameObjectWithTag("explode_icon_tag");
  //      explodeLayout_icon_gameObject = GameObject.FindGameObjectWithTag("explodeLayout_icon_tag");
		//explode_img = explode_icon_gameObject.GetComponent<Image>();
		//explode_img.sprite = explode_sprite;

        
       
		//externSliderHandle  = GameObject.FindGameObjectsWithTag ("handle_slide_extern_area");
        alertTextHandle     = GameObject.FindGameObjectWithTag("ui_alertText");                      // Point to Loading Text object
        //Show the Transparency Slider at the beginning
        //ShowTransparecy_Slider(true);
        //smartTransparencySliderGO.GetComponent<Slider>().value = 0.4f;
        //smartTransparencySliderGO.GetComponent<Slider>().interactable = false;



        //--ISOLATE
        // isolate_icon_GO = GameObject.FindGameObjectWithTag("isolate_icon_tag");

        //--SinglePointLocation
        SPCaptureRemove_icon_GO = GameObject.FindGameObjectWithTag("SPD_icon_tag");
        SPcapture_img = SPCaptureRemove_icon_GO.GetComponent<Image>();
        SPcapture_img.sprite = SPdelete_sprite;
        SPSave_icon_GO = GameObject.FindGameObjectWithTag("SPSave_icon_tag");
        SPsave_img = SPSave_icon_GO.GetComponent<Image>();
        SPsave_img.sprite = SPsaved_sprite;
        SPClose_icon_GO = GameObject.FindGameObjectWithTag("SPClose_icon_tag");

        SPClose_popUpPanel_GO = GameObject.FindGameObjectWithTag("SPClose_popUpPanel_tag");
        SPClose_popUpPanel_GO.SetActive(false);

        SPPanelMode_icon_GO = GameObject.FindGameObjectWithTag("panel_SP_tag");
        SPPanelMode_icon_GO.SetActive(false);
  
        //--Remove
        // remove_icon_GO = GameObject.FindGameObjectWithTag("hide_icon_tag");

        ////--Hierarchy
        //hierarchy_panel_GO = GameObject.FindGameObjectWithTag("panel_Hierarchy_tag");
        //hierarchy_icon_GO = GameObject.FindGameObjectWithTag("hierarchy_icon_tag");
        //hierarchy_img = hierarchy_icon_GO.GetComponent<Image>();
        //Hierarchy_ButtonManager(hom3r.state.currentSelectionMode == THom3rSelectionMode.AREA);
        //firstTimeInside = true;
        //firstTimeOutside = false;
        //showPartNameLabel_icon_GO = GameObject.FindGameObjectWithTag("showPartNameLabel_icon_tag");

        //-- Dialog boxes
        //Indication name
        indicatedArea_title.enabled = false;
        indicatedComponent_title.enabled = false;
        indicatedPanelGO.SetActive(false);
        //Alert Text
        alertTextPanelGO.SetActive(true);
        showingAlertText = false;

        //--Canvas
        //canvas_GO = GameObject.FindGameObjectWithTag("canvas_tag");
        // canvas_GO.SetActive(hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveUI());
        this.UpdateUIState();
    }



    private void BindingCanvasButton()
    {
        

    }

    /********************************************************************************************************************************
	 * INTERFACE ICONS
	 * *****************************************************************************************************************************/
    public void BlockCanvasRayCasting(bool block)
    {
        if (canvas_GO == null) { canvas_GO = GameObject.FindGameObjectWithTag("canvas_tag"); }
        canvas_GO.GetComponent<CanvasGroup>().blocksRaycasts = !block;
    }
    
	/// HOME ICON ***********************************************************************************************************
	/// ---------------------------------------------------------------------------------------------------------------------
	/// <summary> Set camera to the inital position, removes all the transparencies and make visibles all objets.</summary>
	/// ---------------------------------------------------------------------------------------------------------------------
	public void OnClickHomeButton(){

        //1. Notify the Core
        //CCoreCommand command = new CUICommand(TUICommands.HomeButtonPressed);                         //Create command to send to core  
        //hom3r.coreLink.Do(command, Constants.undoNotAllowed);    //Send to the core

        hom3r.coreLink.Do(new CNavigationCommand(TNavigationCommands.InitializeNavigation));


        //if((hom3r.state.currentVisualizationMode == THom3rIsolationMode.ISOLATE || hom3r.state.currentVisualizationMode == THom3rIsolationMode.WITH_REMOVEDNODES)/* && (this.GetComponent<Remove_Script>().GetRemovedList().Count != 0)*/)
        //{
        //    //hom3r.quickLinks.scriptsObject.GetComponent<IsolateManager>().ReFocusIsolatedGO_withRotation(); //FIXME UI cannot call Isolate method directly, have to use a command
        //} else {
        //    //FIXME after Navigation Refactoring
        //    //hom3r.quickLinks.orbitPlane.GetComponent<NavigationManager>().ResetView_BothNavSystems();//FIXME UI cannot call Navigation method directly, have to use a command
        //}
    }

    /// NAVIGATION COORDINATES SYSTEM BUTTON ********************************************************************************
    /// ---------------------------------------------------------------------------------------------------------------------
    /// <summary> Change coordinate system for navigation</summary>
    /// ---------------------------------------------------------------------------------------------------------------------
    public void OnClickNavigationCoordinatesButton()
    {
        //FIXME after Navigation Refactoring
        //hom3r.coreLink.Do(new CNavigationCommand(TNavigationCommands.ChangeCoordinateSystem), Constants.undoNotAllowed);
    }

    //FIXME after Navigation Refactoring
    //public void ChangeCoordinatesButtonImage(NavigationManager.TCoordinateSystem newCoodinateSystem)
    //{
    //    if (newCoodinateSystem == NavigationManager.TCoordinateSystem.ELLIPSOIDAL)
    //    {
    //        coordinates_img.sprite = ellipsoid_sprite;
    //    }
    //    else
    //    {
    //        coordinates_img.sprite = cylinder_sprite;
    //    }
    //}


    // ********************************************************************************************************************************
    //EXPLODE & IMPLODE ICON ********************************************************************************************************
    // ********************************************************************************************************************************

    /// <summary>
    /// Set if the explosion buttons are activated or not.
    /// </summary>
    /// <param name="_enabled">true activate the explosion buttons</param>
    public void SetActiveExplosionButtons(bool _enabled)
    {        
        this.SetActiveExplodeButton(_enabled);
        this.SetActiveLayoutExplodeImplodeButton(_enabled);
    }

    /// <summary>
    /// Set if the main explosion button is activated or not.
    /// </summary>
    /// <param name="_enabled">true activate the explosion buttons</param>
    private void SetActiveExplodeButton(bool _enabled)
    {
        occlusionPanelGO.transform.Find("Image_explode_implode").gameObject.GetComponent<Button>().interactable = _enabled;
    }

    /// <summary>
    /// Set if the layout explosion button is activated or not.
    /// </summary>
    /// <param name="_enabled">true activate the explosion buttons</param>
    private void SetActiveLayoutExplodeImplodeButton(bool _enabled)
    {
        occlusionPanelGO.transform.Find("Image_explodeLayout_implodeLayout").gameObject.GetComponent<Button>().interactable = _enabled;
    }


    private void DisableExplosionButton(bool disable)
    {
        this.SetActiveExplodeButton(!disable);
        //if (disable)
        //{
        //    explode_icon_gameObject.GetComponent<Button>().interactable = false;
        //}
        //else {
        //    explode_icon_gameObject.GetComponent<Button>().interactable = true;
        //}
    }

    public void EmplodeImplodeButton()
    {
        //1. Notify the core           
        //CoreCommand command = new UICoreCommand(TUIEvent.Explosion, true, "global");
        CCoreCommand command = new COcclusionCommand(TOcclusionCommands.StartStopGlobalExplosion);
        hom3r.coreLink.Do(command, Constants.undoNotAllowed);        
        //2. Manage the button
        EmplodeImplode_ButtonManager(hom3r.state.currentExplosionMode == THom3rExplosionMode.EXPLODE);
    }
       
    public void EmplodeImplode_ButtonManager(bool modeActive)
    {
        //1. Update icon text
        Update_ExplosionIconText();
        //2. Update button icon (image)
        Image temp = occlusionPanelGO.transform.Find("Image_explode_implode").gameObject.GetComponent<Image>();
        if (modeActive)
        {
             temp.sprite = implode_sprite;    //change button image 
        }
        else
        {
            temp.sprite = explode_sprite;    //change button image 
        }
    }
   
    /// <summary> Update the icon text according to the Explosion submode </summary>
    void Update_ExplosionIconText()
    {
        if (hom3r.state.currentExplosionMode == THom3rExplosionMode.EXPLODE)
        {
            occlusionPanelGO.transform.Find("Image_explode_implode").gameObject.GetComponentInChildren<Text>().text = "Global Implosion";
            //explode_icon_gameObject.GetComponentInChildren<Text>().text = "Global Implosion";
        }
        else {
            occlusionPanelGO.transform.Find("Image_explode_implode").gameObject.GetComponentInChildren<Text>().text = "Global Explosion";
            //explode_icon_gameObject.GetComponentInChildren<Text>().text = "Global Explosion";
        }
    }

    /// ---------------------------------------------------------------------------------------------------------
    /// <summary> Local exploded-view </summary>
    /// ---------------------------------------------------------------------------------------------------------
    public void ExplodeLayout_UIButton()
    {
        //1. Notify the core
        //CoreCommand command = new UICoreCommand(TUIEvent.LayoutExplosionModeManager);
        CCoreCommand command = new COcclusionCommand(TOcclusionCommands.StartStopLayoutExplosionMode);
        hom3r.coreLink.Do(command, Constants.undoNotAllowed);

        // 2.Button manager
        Explodelayout_ButtonManager(hom3r.state.currentMode == THom3rMode.local_explosion);
    }

    private void Explodelayout_ButtonManager(bool modeActive)
    {
        Button b = occlusionPanelGO.transform.Find("Image_explodeLayout_implodeLayout").gameObject.GetComponent<Button>();
        //Button b = explodeLayout_icon_gameObject.GetComponent<Button>();
        ColorBlock cb = b.colors;

        if (modeActive) { cb.normalColor = iconModeActive_color;            
        } else { cb.normalColor = Color.white; }

        b.colors = cb;
    }

    public void DisableExplodelayoutButton(bool disable)
    {
        if (disable) {  this.SetActiveLayoutExplodeImplodeButton(false);}
        else {          this.SetActiveLayoutExplodeImplodeButton(true); }
    }


    // ********************************************************************************************************************
    // FOCUS ICON ********************************************************************************************************
    // ****************************************************************************************************************

    /// <summary> Call the Focus function </summary>
    public void OnClickFocusButton()
    {
        //0. Get List of confirmed objets                
        List<GameObject> listRemovedObjets = new List<GameObject>(hom3r.quickLinks.scriptsObject.GetComponent<RemoveManager>().GetRemovedList());

        //1. Notify the Core        
        CCoreCommand command = new COcclusionCommand(TOcclusionCommands.Isolate, listRemovedObjets);
        hom3r.coreLink.Do(command, Constants.undoAllowed);      
    }

   public void DisableIsolateButton(bool disable)
    {
        if (disable) {  isolateButtonGO.GetComponent<Button>().interactable = false; }
        else {          isolateButtonGO.GetComponent<Button>().interactable = true; }
    }


    // ********************************************************************************************************************************
    // REMOVE MODE ICON ********************************************************************************************************
    // ********************************************************************************************************************************

    /// ---------------------------------------------------------------------------------------------------------
    /// <summary> Hide the Gameobject confirmed. </summary>
    /// ---------------------------------------------------------------------------------------------------------
    public void OnClickRemoveModeButton()
    {
        //1. Notify the core     
        hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.StartStopRemoveMode), Constants.undoAllowed);

        // 2.Button manager
        Remove_ButtonManager(hom3r.state.currentMode == THom3rMode.remove);
    }

    private void Remove_ButtonManager(bool modeActive)
    {        
        ColorBlock cb = removeButtonGO.GetComponent<Button>().colors;
        if (modeActive) { cb.normalColor = iconModeActive_color; }
        else { cb.normalColor = Color.white; }
        removeButtonGO.GetComponent<Button>().colors = cb;
    }

    public void DisableRemoveButton(bool disable)
    {
        if (disable){   removeButtonGO.GetComponent<Button>().interactable = false;}
        else{           removeButtonGO.GetComponent<Button>().interactable = true; }
    }

    // ********************************************************************************************************************
    // SHOW ALL ICON ********************************************************************************************************
    // ****************************************************************************************************************

    /// <summary> Call the Focus function </summary>
    public void OnClickShowAllButton()
    {
        //0. Save the command necesary to execute the undo
        THom3rIsolationMode currentMode = hom3r.state.currentIsolateMode;        
        List<GameObject> goRemovedList =  new List<GameObject>(hom3r.quickLinks.scriptsObject.GetComponent<RemoveManager>().GetRemovedList());               //Copy the list
        
        //1. Notify the Core
        CCoreCommand command = new COcclusionCommand(TOcclusionCommands.ShowAll, currentMode, goRemovedList);

        hom3r.coreLink.Do(command, Constants.undoAllowed);     
    }


    // UNDO ICON ********************************************************************************************************
    /// ----------------------------------------------------------------------------------------------------------------------
    /// <summary> Call the undo function for Isolate and Remove component </summary>
    public void Undo_UIButton() {
        //Notify the core
        hom3r.coreLink.Undo();
    }


    //////////////////////////////
    // SMART-TRANSPARENCY 
    //////////////////////////////

    /// <summary> Use by SinglePointMode
    /// 
    /// </summary>
    /// <param name="disable"></param>
    private void DisableTransparencyButton(bool disable)
    {
        if (disable)
        {
            smartTransparencyButtonGO.GetComponent<Button>().interactable = false;
        }
        else
        {
            smartTransparencyButtonGO.GetComponent<Button>().interactable = true;
        }
    }

    public void OnClickSmartTransparencyUIButton()
    {
        if (hom3r.state.smartTransparencyModeActive)
        {
            //Stop Smart Transparency            
            hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.DisableSmartTransparency, THom3rCommandOrigin.ui));         
        } else {
            //Start Smart Transparency
            hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.EnableSmartTransparency, THom3rCommandOrigin.ui));            
        }

    }

    public void OnChangeTransparencySliderValue(float value)
    {
        if (blockOnChangeTransparencySliderEvent) {
            blockOnChangeTransparencySliderEvent = false;
            return;
        }        
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.UI_SmartTransparency_AlphaLevelUpdated, value));        
    }

    
    public void SetTransparencySliderValue()
    {
        blockOnChangeTransparencySliderEvent = true;
        smartTransparencySliderGO.GetComponent<Slider>().value = hom3r.state.smartTransparencyAlphaLevel;   
    }

    /// <summary>
    /// Update the SmartTransparecy state
    /// </summary>
    /// <param name="_enabled"></param>
    public void SetSmartTransparencyButtonState(bool _enabled)
    {
        if (_enabled)
        {         
            //Enable Transparency slider         
            this.SetInteractableSmartTransparencySlider(true);
            //Change colour button to red, to indicate that the Smart Transparency is activated
            Button b = smartTransparencyButtonGO.GetComponent<Button>();
            ColorBlock cb = b.colors;
            cb.normalColor = iconModeActive_color;
            b.colors = cb;
        }
        else
        {
            //Disable Transparency slider          
            this.SetInteractableSmartTransparencySlider(false);            

            //Change colour button
            Button b = smartTransparencyButtonGO.GetComponent<Button>();
            ColorBlock cb = b.colors;
            cb.normalColor = Color.white;
            b.colors = cb;
        }
    }

    private void SetInteractableSmartTransparencyButton(bool _enabled)
    {
        smartTransparencyButtonGO.GetComponent<Button>().interactable = _enabled;
    }
    private void SetInteractableSmartTransparencySlider(bool _enabled)
    {
        smartTransparencySliderGO.GetComponent<Slider>().interactable = _enabled;
    }
    private void SetValueSmartTransparencySlider(float _value)
    {        
        smartTransparencySliderGO.GetComponent<Slider>().value = _value;
    }

    
    // ********************************************************************************************************************************
    // SINGLE POINT MODE ICON ********************************************************************************************************
    // ********************************************************************************************************************************

    //*** Panel Methods *****************************************************

    public void SP_ShowIcons(bool show)
    {
        SPPanelMode_icon_GO.SetActive(show);    //SinglePoint panel show/hide
        UpdateDisableButtons_SinglePointMode(show);                // Another modes have to be disabled/enabled
    }

    /// <summary> Disable a set of modes that are not accesible from SinglePoint mode </summary>
    /// <param name="disable"> true if disable, false if enable</param>
    public void UpdateDisableButtons_SinglePointMode(bool disable)
    {
        DisableExplodelayoutButton(disable);
        DisableExplosionButton(disable);
        DisableHierarchyButton(disable);
        DisableTransparencyButton(disable);
        DisableRemoveButton(disable);
    }

   /// <summary>
   /// Icons to show when initializing the SinglePoint mode </summary>
    public void Reset_SPMode_icons() {
        CaptureRemove_IconManager();
        SPSave_IconManager();
    }

    /// <summary>
    /// UnBlock Modes due to the mouse has exit the panel (This has to be done in the script because the panel has been remove and the eventTrigger does not detect the mouse Point Exit)
    /// </summary>
    public void BlockModes(bool bBlock) {
        //Unblock selection when the user leave the mode
        //hom3r.coreLink.setSelectionBlocked(bBlock);
        hom3r.coreLink.Do(new CSelectionCommand(TSelectionCommands.BlockSelection, bBlock), Constants.undoNotAllowed);
        //Unblock CapturePoint 
        //hom3r.coreLink.setCaptureSinglePointBlocked(bBlock);
        this.BlockCaptureSinglePoint(bBlock);
        //Unblock Navigation 
        //hom3r.coreLink.setNavigationBlocked(bBlock);
        this.BlockNavigation(bBlock);
    }

    //*** Undo Button *****************************************************

    /// <summary>
    /// Active the undo process of the single point through the Core 
    /// </summary>
    public void SPUndo_UIButton() {
        //1. Buttons Notifications
        //goScripts.GetComponent<Core_Script>().Do(new UICoreCommand(TUIEvent.SinglePointUndo), Constants.undoNotAllowed);
        hom3r.coreLink.Do(new CSinglePointCommand(TSinglePointCommands.SinglePointUndo), Constants.undoNotAllowed);
    }
    
    //*** Save Button *****************************************************

    /// <summary> SinglePoint Save Button management</summary>
    public void SPSave_Button()
    {
        //1. Buttons Notifications
        //goScripts.GetComponent<Core_Script>().Do(new UICoreCommand(TUIEvent.SinglePointModeSave), Constants.undoNotAllowed);
        hom3r.coreLink.Do(new CSinglePointCommand(TSinglePointCommands.SinglePointMode_Save), Constants.undoNotAllowed);
        //2. Button Behaviour
        SPSave_IconManager();
    }
       
    /// <summary> Method to manage the save button image</summary>
    public void SPSave_IconManager()
    {
        SinglePointsStatus_Type currentStatus = hom3r.quickLinks.scriptsObject.GetComponent<SinglePointManager>().GetSinglePointMode_Status();
        if (currentStatus == SinglePointsStatus_Type.Changes_Saved || currentStatus == SinglePointsStatus_Type.EmptyMode)
        {
            //Show Save icon
            SPsave_img.sprite = SPsaved_sprite;
            //Button color
            Button b = SPSave_icon_GO.GetComponent<Button>();
            ColorBlock cb = b.colors;
            cb.normalColor = Color.white;
            b.colors = cb;
        }

        else if (currentStatus == SinglePointsStatus_Type.Changes_Unsaved || currentStatus == SinglePointsStatus_Type.FirstChanges_unsaved)
        {
            //Show Save Asterisk icon
            SPsave_img.sprite = SPunsaved_sprite;
            Button b = SPSave_icon_GO.GetComponent<Button>();
            ColorBlock cb = b.colors;
            cb.normalColor = iconModeActive_color;
            b.colors = cb;
        }

    }
    
    //** Capture/Remove Button *****************************************************************

    /// <summary> Activate/Deactivate the SinglePoint Deletion Mode </summary>
    public void SPCaptureRemove_Button()
    {
        //1. Notify the core
        //goScripts.GetComponent<Core_Script>().Do(new UICoreCommand(TUIEvent.SinglePointCaptureRemove), Constants.undoNotAllowed);
        hom3r.coreLink.Do(new CSinglePointCommand(TSinglePointCommands.SinglePointCaptureRemove), Constants.undoNotAllowed);
        //2. UI Buttons behavior
        CaptureRemove_IconManager();
        //3. Reset single point undo stack
        hom3r.quickLinks.scriptsObject.GetComponent<SinglePointManager>().ResetUndoStack();
    }

    /// <summary> Ask to the Core the status of the SinglePoint Deletion Mode and change the color of the button according to this </summary>
    void CaptureRemove_IconManager()
    {
        if (hom3r.quickLinks.scriptsObject.GetComponent<SinglePointManager>().GetSinglePoint_SubMode() == SinglePointSubmode_Type.RemoveSPMode)
        {
            SPcapture_img.sprite = SPcapture_sprite; //Change icon image
        }
        else if (hom3r.quickLinks.scriptsObject.GetComponent<SinglePointManager>().GetSinglePoint_SubMode() == SinglePointSubmode_Type.CaptureSPMode)
        {
            SPcapture_img.sprite = SPdelete_sprite;  //Change icon image
        }
        //Update button info text
        Update_CaptureRemoveIconText();
    }

    public void CaptureRemoveUI_textManager(bool pointerEnter) {
        if (pointerEnter)
        {
            //1. Update button info text
            Update_CaptureRemoveIconText();
            //2. Show text
            SPCaptureRemove_icon_GO.GetComponentInChildren<Text>().enabled = true;
        }
        else {
            SPCaptureRemove_icon_GO.GetComponentInChildren<Text>().enabled = false;
        }
    }

    /// <summary> Update the icon text according to the SP submode </summary>
    void Update_CaptureRemoveIconText()
    {
        if(hom3r.quickLinks.scriptsObject.GetComponent<SinglePointManager>().GetSinglePoint_SubMode() == SinglePointSubmode_Type.RemoveSPMode)
        {
            SPCaptureRemove_icon_GO.GetComponentInChildren<Text>().text = "Draw point";
        }
        else if (hom3r.quickLinks.scriptsObject.GetComponent<SinglePointManager>().GetSinglePoint_SubMode() == SinglePointSubmode_Type.CaptureSPMode)
        {
            SPCaptureRemove_icon_GO.GetComponentInChildren<Text>().text = "Remove point";
        }
    }
       
    //** Close Button ***************************************************************************

    /// <summary> Activate/Deactivate the SinglePoint Location Mode </summary>
    public void SPClose_Button()
    {
        //Make sure the user wants to exit the mode
        if (hom3r.quickLinks.scriptsObject.GetComponent<SinglePointManager>().GetSinglePointMode_Status() == SinglePointsStatus_Type.FirstChanges_unsaved || hom3r.quickLinks.scriptsObject.GetComponent<SinglePointManager>().GetSinglePointMode_Status() == SinglePointsStatus_Type.Changes_Unsaved)
        {
            //1. Show Panel
            SPClose_popUpPanel_GO.SetActive(true);
            //2. Change mouse icon
            //hom3r.quickLinks.scriptsObject.GetComponent<MouseKeyboardManager>().MouseInsidePanel(true);
            MouseInsidePanel(true);
            //3.Block Modes to disable everything behind the panel
            BlockModes(true);
        }
        else
        {
            //EXIT - the changes has been already saved
            //1. Notify the core
            //goScripts.GetComponent<Core_Script>().Do(new UICoreCommand(TUIEvent.SinglePointModeExit), Constants.undoNotAllowed);
            hom3r.coreLink.Do(new CSinglePointCommand(TSinglePointCommands.SinglePointMode_Exit), Constants.undoNotAllowed);
            //2. Remove SPMode PANEL
            RemoveSPModePanel();
        }
    }

    /// <summary> Methods to manage PopUp panel buttons click  </summary>
    /// <param name="bYes"> if true the button clicked has been the Yes button, if false, the No button</param>
    public void SPClosePopUpPanelButtons(bool bYes)
    {
        //Exit mode without saving
        if (bYes) {
            //1. Notify the core
            //goScripts.GetComponent<Core_Script>().Do(new UICoreCommand(TUIEvent.SinglePointModeExit), Constants.undoNotAllowed);
            hom3r.coreLink.Do(new CSinglePointCommand(TSinglePointCommands.SinglePointMode_Exit), Constants.undoNotAllowed);
            //2. Remove SPMode PANEL
            RemoveSPModePanel();
        }
        //Cancel Exit
        else {
            //Change mouse cursor image
            //hom3r.quickLinks.scriptsObject.GetComponent<MouseKeyboardManager>().ChangeMouseIcon();
            UpdateMouseIconMode();
        }

        //Close popUp Panel
        SPClose_popUpPanel_GO.SetActive(false);
        //Unblock modes previously blocked
        BlockModes(false);
    }

    /// <summary> Close SinglePoint Mode Panel </summary>
    void RemoveSPModePanel()
    {
        //Disable icon text
        SPClose_icon_GO.GetComponentInChildren<Text>().enabled = false;
        // No show buttons
        SP_ShowIcons(false);
        //Unblock modes previously blocked
        BlockModes(false);
        //Change mouse cursor image
        //hom3r.quickLinks.scriptsObject.GetComponent<MouseKeyboardManager>().ChangeMouseIcon();
        UpdateMouseIconMode();
    }

    
    // ********************************************************************************************************************************
    ///HIERARCHY ********************************************************************************************************
    // ********************************************************************************************************************************
    /// ---------------------------------------------------------------------------------------------------------------------
    /// <summary> Deactivate global turbine exploded view</summary>
    /// ---------------------------------------------------------------------------------------------------------------------
    public void DisableHierarchyButton(bool disable)
    {

        if (disable)
        {
            hierarchyIconGO.GetComponent<Button>().interactable = false;
        }
        else
        {
            hierarchyIconGO.GetComponent<Button>().interactable = true;
        }
    }

    public void Hierarchy_UIButton()
    {
        //1. Notify the core           
        //CoreCommand command = new UICoreCommand(TUIEvent.Hierarchy);
        CCoreCommand command = new CSelectionCommand(TSelectionCommands.ChangeHierarchyOfSelectionMode);
        hom3r.coreLink.Do(command, Constants.undoNotAllowed);
        //2. Manage the button
        Hierarchy_ButtonManager(hom3r.state.currentSelectionMode == THom3rSelectionMode.AREA);
    }

    public void Hierarchy_ButtonManager(bool modeActive)
    {
        if (modeActive)
        {
            hierarchyIconGO.GetComponent<Image>().sprite = area_sprite;
            //hierarchy_img.sprite = area_sprite;
            //Change colour button to red, to indicate that the Explode Mode is activated
            Button b = hierarchyIconGO.GetComponent<Button>();
            ColorBlock cb = b.colors;
            cb.normalColor = Color.white;
            b.colors = cb;
        }
        else
        {
            //change button image  
            hierarchyIconGO.GetComponent<Image>().sprite = component_sprite;
            //Change colour button to red, to indicate that the Explode Mode is active
            Button b = hierarchyIconGO.GetComponent<Button>();
            ColorBlock cb = b.colors;
            cb.normalColor = Color.white;
            b.colors = cb;
        }
    }

    //TODO fixme. It is disabled because the new label manager
    public void ShowPartNameLabel_UIButton()
    {
        /*//1. Notify the core           
        //CoreCommand command = new UICoreCommand(TUIEvent.ShowPartNameLabel);
        CCoreCommand command = new CLabelCommand(TLabelCommands.ShowPartNameLabel);
        hom3r.coreLink.Do(command, Constants.undoNotAllowed);
        //2. Manage the button
        ShowPartNameLabel_ButtonManager(hom3r.state.currentLabelMode == THom3rLabelMode.show);*/
    }

    public void ShowPartNameLabel_ButtonManager(bool modeActive)
    {
        if (modeActive)
        {
            //Change colour button to red, to indicate that the Explode Mode is activated
            Button b = showPartNameLabel_icon_GO.GetComponent<Button>();
            ColorBlock cb = b.colors;
            cb.normalColor = iconModeActive_color;
            b.colors = cb;
        }
        else
        {
            //Change colour button to red, to indicate that the Explode Mode is active
            Button b = showPartNameLabel_icon_GO.GetComponent<Button>();
            ColorBlock cb = b.colors;
            cb.normalColor = Color.white;
            b.colors = cb;
        }
    }

    // ********************************************************************************************************************************
    // ALERT TEXT ********************************************************************************************************
    // ********************************************************************************************************************************

    /// <summary>
    /// Make visible a ext message in the interface during 5 seconds.
    /// </summary>
    /// <param name="message">Message to show</param>
    public void SetActiveAlertText(string message, float timeShown)
    {        
        if (alertTextHandle == null) { alertTextHandle = GameObject.FindGameObjectWithTag("ui_alertText");}
        if (message == "")
        {
            //Switch off the message
            alertTextHandle.GetComponent<Text>().enabled = false;
            alertTextHandle.GetComponent<Text>().text = "";
            //Panel
            alertTextPanelGO.SetActive(false);
            showingAlertText = false;
        }
        else
        {
            ManageIndicatedGOnameTextBox(null, false);
            //Switch on the message
            alertTextHandle.GetComponent<Text>().enabled = true;
            alertTextHandle.GetComponent<Text>().text = message;
            //Panel
            alertTextPanelGO.SetActive(true);
            showingAlertText = true;
            //Wait x seconds before switch off the message, if is required
            if (timeShown != 0.0f) { StartCoroutine(WaitforSeconds(timeShown));}
            else { StartCoroutine(WaitforSeconds(5.0f)); }                        
        }        
    }//SetActiveAlertText

    public IEnumerator WaitforSeconds(float seconds)
    {                        
        yield return new WaitForSeconds(seconds);
        alertTextHandle.GetComponent<Text>().enabled = false;
        alertTextHandle.GetComponent<Text>().text = "";
        //Panel
        alertTextPanelGO.SetActive(false);
        showingAlertText = false;
    }


    public void ManageIndicatedGOnameTextBox(GameObject obj, bool show)
    {        
        if (!showingAlertText)
        {
            indicatedPanelGO.SetActive(show);
            if (show)
            {                                
                string areaID = obj.GetComponent<ObjectStateManager>().areaID;
                if ((hom3r.state.currentSelectionMode == THom3rSelectionMode.AREA) && (areaID != null))
                {                    
                    
                    string areaDescription = "";                    
                    areaDescription += hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaFullDescription1Line_ByAreaID(areaID);                    
                    indicatedArea_text.text = areaDescription;
                    indicatedArea_title.enabled = true;
                    indicatedArea_text.enabled = true;
                    indicatedComponent_title.enabled = false;
                    indicatedComponent_text.enabled = false;
                    
                }
                else if (hom3r.state.currentSelectionMode == THom3rSelectionMode.SPECIAL_NODE)
                {                    
                    if (areaID != null)
                    {
                        string areaAncestor = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetSpecialAncestorID_ByAreaID(areaID);
                        string areaDescription = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetProductNodeFullDescription1Line_ByNodeID(areaAncestor);
                        indicatedComponent_text.text = areaDescription;
                        indicatedArea_title.enabled = false;
                        indicatedArea_text.enabled = false;
                        indicatedComponent_title.enabled = true;
                        indicatedComponent_text.enabled = true;
                    }
                }
            }
        }
    }


        
    ///////////////////////////////////
    //Methods to block Modes         //
    ///////////////////////////////////

    /// <summary>
    /// Block Selection
    /// </summary>
    /// <param name="_selectionBlocked"></param>
    public void BlockSelection(bool _selectionBlocked)
    {
        // Debug.Log("BlockSelection: " + _selectionBlocked);
        hom3r.coreLink.Do(new CSelectionCommand(TSelectionCommands.BlockSelection, _selectionBlocked), Constants.undoNotAllowed); //Block Selection
    }

    /// <summary>
    /// Method to block/unblock the capture of singlePoints
    /// </summary>
    /// <param name="_captureSPBlocked"></param>
    public void BlockCaptureSinglePoint(bool _captureSPBlocked)
    {
        hom3r.state.captureSinglePointBlocked = _captureSPBlocked;

    }

    public void BlockNavigation(bool _navigationBlocked)
    {
        hom3r.state.navigationBlocked = _navigationBlocked;

    }//END setSelectionBlocked



    ///////////////////////
    // CORE STATE        //
    ///////////////////////
    //public void UpdateState()
    //{
    //    UpdateMouseIconMode();
    //    if (hom3r.coreLink.GetCurrentMode() == THom3rMode.IDLE)
    //    {
    //        DisableTransparencyButton(false);

    //        this.SetActiveExplodeButton(true);
    //        DisableExplosionButton(false);
    //        DisableExplodelayoutButton(false);
    //        DisableRemoveButton(false);
    //        DisableIsolateButton(false);

    //    }
    //    else if (hom3r.coreLink.GetCurrentMode() == THom3rMode.REMOVE)
    //    {
    //        DisableTransparencyButton(true);
    //        DisableExplosionButton(true);
    //        DisableExplodelayoutButton(true);            
    //    }
    //    else if (hom3r.coreLink.GetCurrentMode() == THom3rMode.REMOVE)
    //    {
    //        DisableTransparencyButton(true);
    //        DisableRemoveButton(true);
    //        DisableIsolateButton(true);
    //        DisableExplosionButton(true);
    //    }
    //    this.UpdateUIState();
    //}

    public void UpdateUIState()
    {
        canvas_GO.SetActive(hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveUI());
        gizmo_GO.SetActive(hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveUIGizmo());
        hierarchyPanelGO.SetActive(hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveUIHierarchyPanel());
        
        this.UpdateOcclusionPanelState();           // Update Occlusion Panel State
        this.UpdateMouseIconMode();                 // Update Mouse Icon
        this.UpdateSettingsPanelState();
    }

    private void UpdateOcclusionPanelState()
    {
        Remove_ButtonManager(hom3r.state.currentMode == THom3rMode.remove); // Update remove mode
        if ( hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveUIOcclusionPanel())
        {            
            occlusionPanelGO.SetActive(true);           // Occlusion Panel is visible                   
            this.UpdateOcclusionPanelButtonsState();    // UpdateOcclusionPanelButtons
        }
        else
        {            
            occlusionPanelGO.SetActive(false);          //  Occlusion Panel is hidden           
        }                                
    }

    private void UpdateOcclusionPanelButtonsState()
    {
        // Update Occlusion Panelbuttons according with hom3r current mode
        if (hom3r.state.currentMode == THom3rMode.idle)
        {
            // Activate Explosion buttons
            SetActiveExplosionButtons(hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveExplosion());

            // Activate Transparency button
            DisableTransparencyButton(false);
            DisableRemoveButton(false);
            DisableIsolateButton(false);

        }
        else if (hom3r.state.currentMode == THom3rMode.remove)
        {
            DisableTransparencyButton(true);
            if (hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveExplosion())
            {
                //DisableExplosionButton(true);
                //DisableExplodelayoutButton(true);
                this.SetActiveExplosionButtons(false);
            }            
        }
        else if (hom3r.state.currentMode == THom3rMode.capturing_surface_point)
        {
            DisableTransparencyButton(true);
            DisableRemoveButton(true);
            DisableIsolateButton(true);            
            if (hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveExplosion())
            {
                // DisableExplosionButton(true);
                this.SetActiveExplodeButton(false);
            }
        } 
    }

    private void UpdateSettingsPanelState()
    {
        bool activated = hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveUISettingsPanel();
        if (activated)
        {
            openCloseSettingsButtonGO.SetActive(true);            
        }
        else
        {            
            settingsPanelGO.SetActive(false);            
            openCloseSettingsButtonGO.SetActive(false);
            secundaryCameraGO.GetComponent<Camera>().enabled = false;
        }

        TNavigationSystemMode newNavigationSystemMode= hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveNavigationSystemMode();
        //TODO change Setting navigation option
        UpdateSettingsPanelNavigationSystem(newNavigationSystemMode);
        if (newNavigationSystemMode == TNavigationSystemMode.Spheroid)
        {
            settingsPanelOblateCorrectionGO.GetComponent<Dropdown>().interactable = true;
        }
        else
        {
            settingsPanelOblateCorrectionGO.GetComponent<Dropdown>().interactable = false;
        }


        string newNavigationObjectType = hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetNavigationObjectType();
        UpdateSettingsPanelObjectType(newNavigationObjectType);
    }

    // Update navigation axis in the settings panel
    private void UpdateSettingsPanelNavigationAxis(string _navigationAxis)
    {
        if (_navigationAxis == "vertical")
        {
            settingsPanelAxisDropdownGO.GetComponent<Dropdown>().SetValueWithoutNotify(0);
        }
        else if (_navigationAxis == "horizontal")
        {
            settingsPanelAxisDropdownGO.GetComponent<Dropdown>().SetValueWithoutNotify(1);
        }
    }
    
    // Update navigation system in the settings panel
    private void UpdateSettingsPanelNavigationSystem(TNavigationSystemMode _newNavigationSystemMode)
    {
        int _value = 0;
        if (_newNavigationSystemMode == TNavigationSystemMode.Ellipsoid)
        {
            _value = 0;
        }
        else if (_newNavigationSystemMode == TNavigationSystemMode.Spheroid) 
        {
            _value = 1;
        }
        else if (_newNavigationSystemMode == TNavigationSystemMode.Spherical) 
        {
            _value = 2;
        }
        else
        {
            //Nothing
        }
        settingsPanelNavigationSystemDropdownGO.GetComponent<Dropdown>().value = _value;        
    }

    private void UpdateSettingsPanelObjectType(string _objetType)
    {
        settingsPanelObjectTypeTextGO.GetComponent<InputField>().text = _objetType;
    }

    /// <summary>
    /// Method to update the mouse image accoding with hom3r events
    /// </summary>
    /// <param name="newMouseIconMode"></param>
    public void SetMouseIconMode(TMouseIconMode newMouseIconMode)
    {
        if (this.mouseIconMode == newMouseIconMode) { return; }

        this.mouseIconMode = newMouseIconMode;

        if (this.mouseIconMode == TMouseIconMode.iddle)
        {            
            SetMouseCursor(null);
        } else if (this.mouseIconMode == TMouseIconMode.pan_navigation)
        {            
            SetMouseCursor(cursorTexturePanNav);
        }
        else if (this.mouseIconMode == TMouseIconMode.explosion)
        {            
            SetMouseCursor(cursorTextureLayoutExplosion);
        }
        else if (this.mouseIconMode == TMouseIconMode.remove)
        {            
            SetMouseCursor(cursorTextureSimpleHide);
        }
        else if (this.mouseIconMode == TMouseIconMode.single_point_capture)
        {            
            SetMouseCursor(cursorTextureCaptureSP);
        }
        else if (this.mouseIconMode == TMouseIconMode.single_point_remove)
        {            
            SetMouseCursor(cursorTextureDeleteSP);
        }
    }
    /// <summary>
    /// Auxiliar method to change mouse cursor. Use SetMouseIconMode instead of this.
    /// </summary>
    /// <param name="_icon"> Texture2D Icon</param>
    private void SetMouseCursor(Texture2D _icon)
    {
        Cursor.SetCursor(_icon, mouseCursorHotSpot, mouseCursorMode);
    }
    /// <summary>Method to change mouse image according to HOM3R mode status</summary>
    private void UpdateMouseIconMode()
    {
        if (hom3r.state.navigationMode == THom3rNavigationMode.pan)
        {
            this.SetMouseIconMode(TMouseIconMode.pan_navigation);
        } else if (hom3r.state.currentMode == THom3rMode.local_explosion)
        {
            this.SetMouseIconMode(TMouseIconMode.explosion);
        }
        else if (hom3r.state.currentMode == THom3rMode.remove)
        {
            this.SetMouseIconMode(TMouseIconMode.remove);
        }
        else if (hom3r.state.currentMode == THom3rMode.capturing_surface_point)
        {
            this.SetMouseIconMode(TMouseIconMode.single_point_capture);
            /*if (this.GetComponent<SinglePointManager>().GetSinglePoint_SubMode() == SinglePointSubmode_Type.CaptureSPMode)
            {
                this.SetMouseIconMode(TMouseIconMode.single_point_capture);
            }
            else if (this.GetComponent<SinglePointManager>().GetSinglePoint_SubMode() == SinglePointSubmode_Type.RemoveSPMode)
            {              
                this.SetMouseIconMode(TMouseIconMode.single_point_remove);
            }*/       
        } else {            
            this.SetMouseIconMode(TMouseIconMode.iddle);
        }       
    }

    
    public void UpdateMouseIconPointOnSurface(bool activate)
    {
        if (activate)
        {
            Cursor.SetCursor(cursorTextureCaptureSP, mouseCursorHotSpot, mouseCursorMode);
        } else
        {
            Cursor.SetCursor(null, Vector2.zero, mouseCursorMode);
        }
        
    }

    public void MouseInsidePanel(bool inside)
    {
        if (inside)
        {
            Debug.Log("MouseInsidePanel");
            //ChangeMouseIcon(MouseInOutPanel_type.mouseInsidePanel);
            Cursor.SetCursor(null, Vector2.zero, mouseCursorMode);
        }
        else
        {
            UpdateMouseIconMode();
        }
    }



    ///////////////////////////////////////
    /// settings Panel
    /// ///////////////////////////////////
    
    private void OnClickOpenSettingsPanelButton()
    {        
        settingsPanelGO.SetActive(!settingsPanelGO.activeSelf);
        openCloseSettingsButtonGO.SetActive(false);
    }
        /// 
    private void OnClickCloseSettingsPanelButton()
    {
        settingsPanelGO.SetActive(false);
        openCloseSettingsButtonGO.SetActive(true);
        hom3r.state.navigationBlocked = false;
        hom3r.state.selectionBlocked = false;
    }
    private void OnChangeAuxPanelNavigationSystemDropdown(int value)
    {
        TNavigationSystemMode TNavigationSystemMode = TNavigationSystemMode.Ellipsoid;
        if (value == 0)
        {
            TNavigationSystemMode = TNavigationSystemMode.Ellipsoid;
        }
        else if (value == 1)
        {
            TNavigationSystemMode = TNavigationSystemMode.Spheroid;
        }
        else if (value == 2)
        {
            TNavigationSystemMode = TNavigationSystemMode.Spherical;
        }

        hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.SetNavigationSystemMode, TNavigationSystemMode));
    }

    private void OnChangeSettingsPanelNavigationAxisDropdown(int value)
    {
        string _navigationAxis="";
        if (value==0)
        {
            _navigationAxis = "vertical";
        }
        else
        {
            _navigationAxis = "horizontal";
        }        
        hom3r.coreLink.Do(new CModelCommand(TModelCommands.SetNavigationAxis, _navigationAxis));
    }


    private void OnChangeSettingsPanelLatitudeDropdown(int value)
    {
        TInteractionMappingCorrectionMode interactionMappingCorrection = TInteractionMappingCorrectionMode.distance;
        if (value == 0)
        {
            interactionMappingCorrection = TInteractionMappingCorrectionMode.none;
        }
        else if (value == 1)
        {
            interactionMappingCorrection = TInteractionMappingCorrectionMode.ellipsePerimeter;
        }
        else if (value == 2)
        {
            interactionMappingCorrection = TInteractionMappingCorrectionMode.distance;
        }
        hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.SetNavigationLatitudeCorrectioFactorMode, interactionMappingCorrection));
    }
    private void OnChangeSettingsPanelLongitudeDropdown(int value)
    {
        TInteractionMappingCorrectionMode interactionMappingCorrection = TInteractionMappingCorrectionMode.distance;
        if (value == 0)
        {
            interactionMappingCorrection = TInteractionMappingCorrectionMode.none;
        }
        else if (value == 1)
        {
            interactionMappingCorrection = TInteractionMappingCorrectionMode.ellipsePerimeter;
        }
        else if (value == 2)
        {
            interactionMappingCorrection = TInteractionMappingCorrectionMode.distance;
        }
        hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.SetNavigationLongitudeCorrectioFactorMode, interactionMappingCorrection));
    }
    private void OnClickAuxPanelResetButton()
    {
        hom3r.coreLink.Do(new CNavigationCommand(TNavigationCommands.InitializeNavigation));
    }

    private void OnClickSettingsPanelChange3DButton()
    {        
        hom3r.coreLink.Do(new CModelCommand(TModelCommands.ResetProductModel), Constants.undoNotAllowed);
        hom3r.quickLinks.uiObject.GetComponent<UIStandaloneManager>().SetActivatedStandAlonePanel(true);
        OnClickOpenSettingsPanelButton();
    }

    private void OnChangeSettingsPanelTechnicalviewToggle(bool value)
    {
        secundaryCameraGO.GetComponent<Camera>().enabled = value;
    }

    private void OnChangesSettingsPanelOblateCorrection(int value) {

        TOblateSpheroidCorrectionMode oblateSpheroidCorrectionMode = TOblateSpheroidCorrectionMode.none;
        if (value == 0)
        {
            oblateSpheroidCorrectionMode = TOblateSpheroidCorrectionMode.none;
        }
        else if (value == 1)
        {
            oblateSpheroidCorrectionMode = TOblateSpheroidCorrectionMode.ec_minimun;
        }
        else if (value == 2)
        {
            oblateSpheroidCorrectionMode = TOblateSpheroidCorrectionMode.ec_averaged;
        }
        else if (value == 3)
        {
            oblateSpheroidCorrectionMode = TOblateSpheroidCorrectionMode.cameraOrientationDepedentRotation;
        }

        hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveNavigationOblateOrientationCorrection, oblateSpheroidCorrectionMode));
    }
    
    public void LoadStandalonePanel()
    {
        Transform standalonePanelTransform = canvas_GO.transform.Find("Panel_Standalone");
        if (standalonePanelTransform != null) { return;  }
        // Instantiate the Experiment UI Canvas
        GameObject standalonePanel = (GameObject)Resources.Load("prefabs/UI/Panel_StandalonePrefab", typeof(GameObject));
        GameObject standalonePanelI = Instantiate(standalonePanel, new Vector3(0f, 0f, 0f), Quaternion.identity, canvas_GO.transform);
        
        standalonePanelI.transform.name = "Panel_Standalone";
        //standalonePanelI.transform.parent = canvas_GO.transform;   // Make child of the UI Object     

        standalonePanelI.transform.localPosition = Vector3.zero;

        hom3r.quickLinks.uiObject.AddComponent<UIStandaloneManager>();  // Attach the UIExperimentManager script
    }


    public void ProcessAfterProductModelLoaded()
    {
        string _navigationAxis = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetModelNavigationAxis();

        //if (_navigationAxis == "vertical")
        //{
        //    settingsPanelAxisDropdownGO.GetComponent<Dropdown>().SetValueWithoutNotify(0);
        //} 
        //else if (_navigationAxis == "horizontal")
        //{
        //    settingsPanelAxisDropdownGO.GetComponent<Dropdown>().SetValueWithoutNotify(1);
        //}

        UpdateSettingsPanelNavigationAxis(_navigationAxis);
    }
}   
