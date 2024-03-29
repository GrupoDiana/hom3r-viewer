using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CViewfinderData
{
    public int id;
    public GameObject viewfinderGO;
    public Vector3 position;
    public float radious;


    public CViewfinderData(int _id, GameObject viewfinderParentGO)
    {
        this.id = _id;
        this.viewfinderGO = viewfinderParentGO.transform.Find("Viewfinder_" + _id.ToString()).gameObject;
        this.viewfinderGO.SetActive(false);
    }
    public void SetViewfinder(Vector3 _screenPosition, float _radious)
    {
        this.position = _screenPosition;
        this.radious = _radious;
        this.viewfinderGO.SetActive(true);                                                          // Activate        
        this.viewfinderGO.transform.position = _screenPosition;                                      //Place viewfinder                    
        this.viewfinderGO.GetComponent<RectTransform>().sizeDelta = new Vector2(_radious*2,_radious*2);   // Change Size
    }
    public void ChangeViewfinderColor(Color _newColor)
    {
        this.viewfinderGO.GetComponent<Image>().color = _newColor;
    }

}


public class UIExperimentManager : MonoBehaviour
{
    private GameObject canvasExperiment;


    private GameObject trainingPanel;
    private GameObject startBlockPanel;
    private GameObject endScenarioPanel;
    
    private GameObject viewfinderGO;
    private List<CViewfinderData> viewfinderList;
    //private GameObject viewfinderCentralGO;

    private GameObject questionsPanelGO;
    private GameObject workLoadQuestionnaireGO;
    private GameObject susQuestionnaireGO;
    private GameObject userDataPanelGO;
    private GameObject endTextGO;    
    private GameObject counterTextGO;
    private GameObject introSessionPanelGO;
    private GameObject pressSpaceText;
    private GameObject startBlockTrialsMessage;
    private GameObject audioSource;
    

    private bool targetInsideViewFinder;
    private bool targetInsideViewFinderMessage;
    private bool targetInsideRutineWorking;
    private bool automaticInsideViewFinderCheckingActivated;

    private string userID;
    private string sessionNumber;
    private string blockNumber;

    //TNavigationRadialPositions zoomPosition;
    //Targets
    //private CTargetData targetDataList;


    private void Awake()
    {
        canvasExperiment = hom3r.quickLinks.uiObject.transform.Find("Canvas_Experiment").gameObject;
        
        trainingPanel = canvasExperiment.transform.Find("TrainingPhasePanel").gameObject;
                  
        viewfinderGO = canvasExperiment.transform.Find("Viewfinder").gameObject;
        InitViewfinderList(viewfinderGO);
        
        userDataPanelGO = canvasExperiment.transform.Find("UserPanel").gameObject;
        endTextGO = canvasExperiment.transform.Find("TextEnd").gameObject;
        counterTextGO = canvasExperiment.transform.Find("CounterText").gameObject;
        introSessionPanelGO = canvasExperiment.transform.Find("IntroSessionPanel").gameObject;
        pressSpaceText = canvasExperiment.transform.Find("PressSpaceText").gameObject;
        startBlockTrialsMessage = canvasExperiment.transform.Find("StartBlockTrials").gameObject;

        endScenarioPanel = canvasExperiment.transform.Find("EndBlockPanel").gameObject;
        startBlockPanel = canvasExperiment.transform.Find("StartBlocksPanel").gameObject;
        questionsPanelGO = canvasExperiment.transform.Find("QuestionsPanel").gameObject;
        workLoadQuestionnaireGO = questionsPanelGO.transform.Find("Workload").gameObject;
        susQuestionnaireGO = questionsPanelGO.transform.Find("SUS").gameObject;
        audioSource = GameObject.Find("AudioSource").gameObject;


        AddListeners();

        targetInsideViewFinder = false;
        targetInsideViewFinderMessage = false;
        targetInsideRutineWorking = false;
        automaticInsideViewFinderCheckingActivated = false;

        //zoomPosition = TNavigationRadialPositions.closest;
    }

    private void InitViewfinderList(GameObject viewfinderParentGO)
    {
        viewfinderList = new List<CViewfinderData>();
        for (int i = 0; i<3; i++)
        {
            CViewfinderData oneviewfinder = new CViewfinderData(i, viewfinderParentGO);
            viewfinderList.Add(oneviewfinder);
        }        
    }

    private void Update()
    {
        if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            //Keyboard capture
            if (Input.GetKeyUp(KeyCode.C))
            {
                StartCaptureMode();
            }
            else if (Input.GetKeyUp(KeyCode.X))
            {
                StartCaptureMode(false);
            }
            else if (Input.GetKeyUp(KeyCode.Return))
            {
                //Delete view targets
                hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().SetDockingMade();
            }
            else if (Input.GetKeyUp(KeyCode.KeypadEnter))
            {
                hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().GoBackTrial();
            }
        }

        // Rest of the operative
        if (hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().IsDockDetectionMode())
        {
            DockDetection();
        }
        else if (hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().IsCaptureTargetMode())
        {
            TargetCapture();        
        }


        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveKeyboard())
            {
                hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().AfterSpaceKeyPressed();
            }            
        }
        else if (Input.GetKeyUp(KeyCode.Return))
        {
            //Delete view targets
            hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().SetDockingMade();
        }
    }


    private void AddListeners()
    {
        workLoadQuestionnaireGO.transform.Find("Next_Button").gameObject.GetComponent<Button>().onClick.AddListener(OnClickWorkloadQuestionerNextButton);
        susQuestionnaireGO.transform.Find("End_Button").gameObject.GetComponent<Button>().onClick.AddListener(OnClickSUSQuestionerEndButton);
        userDataPanelGO.transform.Find("Start_Button").gameObject.GetComponent<Button>().onClick.AddListener(OnClickUserPanelEndButton);

        startBlockPanel.transform.Find("Button").gameObject.GetComponent<Button>().onClick.AddListener(OnClickStartBlockPanelButton);
        trainingPanel.transform.Find("Button").gameObject.GetComponent<Button>().onClick.AddListener(OnClickFinishTrainingButton);        
        introSessionPanelGO.transform.Find("Button").gameObject.GetComponent<Button>().onClick.AddListener(OnClickIntroSessionPanelButton);
        endScenarioPanel.transform.Find("Button").gameObject.GetComponent<Button>().onClick.AddListener(OnClickEndScenarioButton);        
    }

    private void DockDetection()
    {                
        // Check If it is inside viewfinder

        CTargetData targetDataList = hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().GetTargetData();

        if (targetDataList== null) { return; }
        bool control = true;
        //foreach(CTargetVertexData vertex in targetDataList.targetVeticesList) {
        //    control = control && IsTargetInsideViewfinder(vertex);            
        //}       
        control = IsTargetInsideViewfinderRing(targetDataList.targetVeticesList[0], targetDataList.bigViewfinderID, targetDataList.smallViewfinderID);

        //if (control) {
        //    if (!targetInsideViewFinder)
        //    {
        //        Debug.Log("Valido");
        //        targetInsideViewFinder = true;
        //    }
        //} else {
        //    if (targetInsideViewFinder)
        //    {
        //        Debug.Log("Invalido");
        //        targetInsideViewFinder = false;
        //    }            
        //}
        //return;        
        //control = false;
        if (control)
        {
            if (!targetInsideRutineWorking)
            {
                //Debug.Log("AMARILLO");
                targetInsideViewFinder = true;
                ChangeViewFinderColor(new Color32(0xFF, 0xFF, 0x00, 0xFF));
                if (automaticInsideViewFinderCheckingActivated)
                {
                    //Debug.Log("LANZA RUTINA");                        
                    targetInsideRutineWorking = true;
                    StartCoroutine(CheckTimeInsideViewFinder(1f));
                }
            }


            if (!targetInsideViewFinderMessage) { Debug.Log("DENTRO"); }            // To show the message only once
            targetInsideViewFinderMessage = true;        
        }
        else
        {
            if (targetInsideViewFinder)
            {
                targetInsideViewFinder = false;
                //ChangeViewFinderColor(Color.white);
                ChangeViewFinderColor(new Color32(0xFF, 0x4C, 0x4C, 0xFF));
            }

            if (targetInsideViewFinderMessage) { Debug.Log("FUERA"); }              // To show the message only once
            targetInsideViewFinderMessage = false;            
        }    
    }
    private void StartCaptureMode(bool editFromFirstTrial=true)
    {
        if (hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().ChangeToCaptureTargetMode(editFromFirstTrial))
        {
            if (editFromFirstTrial)
            {
                //for (int index = 1; index <= 5; index++)
                //{
                //    GameObject viewfinderNGO = viewfinderGO.transform.Find("Viewfinder_" + index).gameObject;    // Get object            
                //    viewfinderNGO.SetActive(false);                                                              // Desactivate                
                //}
                trainingPanel.transform.Find("Text").gameObject.GetComponent<Text>().text = "Capture target";
            }
        }
    }

    private void TargetCapture()
    {
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {            
            CTargetData targetData = hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().GetTargetData();  // Get targets list

            List<CTrialViewfinderData> viewfinderList = new List<CTrialViewfinderData>();
            int index = 0;
            foreach (CTargetVertexData vertex in targetData.targetVeticesList)
            {
                ////Get data
                //float radious = CalculateTargetRadious(vertex.centerPosition, vertex.maxPosition, vertex.minPosition);
                //Vector3 centerPositionOnScreen = GetPositionOnScreen(vertex.centerPosition);                                 
                ////Save data
                //Vector3 centerPositionOnScreenNormalize = GetNormalizeViewFinderPosition(centerPositionOnScreen);
                //float normalizedRadious = GetNormalizeRadious(radious);
                //CTrialViewfinderData tempViewfinder = new CTrialViewfinderData(index, vertex.areaID, centerPositionOnScreenNormalize.x, centerPositionOnScreenNormalize.y, centerPositionOnScreenNormalize.z, normalizedRadious);
                //viewfinderList.Add(tempViewfinder);

                ////Show viewfinder on screen
                //SetViewfinderOnScreen(index, centerPositionOnScreenNormalize, normalizedRadious);

                viewfinderList.Add(CaptureOneViewfinder(vertex, 1f, index));
                index++;
                viewfinderList.Add(CaptureOneViewfinder(vertex, 0.5f, index)); ;
                index++;
                viewfinderList.Add(CaptureOneViewfinder(vertex, 1.5f, index)); ;
                index++;
            }
            hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().SaveCaptureTargetViewfinder(viewfinderList);      //Save Data
        }
        else if (Input.GetKeyUp(KeyCode.P))
        {
            hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().GetCaptureTrialList();
        }
        else if (Input.GetKeyUp(KeyCode.KeypadPlus))
        {
            hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().ResizeViewFindersCaptureMode(1.05f);
        }
        else if (Input.GetKeyUp(KeyCode.KeypadMinus))
        {
            hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().ResizeViewFindersCaptureMode(0.95f);
        }
    }
    private CTrialViewfinderData CaptureOneViewfinder(CTargetVertexData vertex, float radiousMultiplier, int index)
    {
        //Get data
        float radious = CalculateTargetRadious(vertex.centerPosition, vertex.maxPosition, vertex.minPosition) * radiousMultiplier;
        //radious *= 1.1f;
        Vector3 centerPositionOnScreen = GetPositionOnScreen(vertex.centerPosition);
        //Save data
        Vector3 centerPositionOnScreenNormalize = GetNormalizeViewFinderPosition(centerPositionOnScreen);
        float normalizedRadious = GetNormalizeRadious(radious);
        CTrialViewfinderData tempViewfinder = new CTrialViewfinderData(index, vertex.areaID, centerPositionOnScreenNormalize.x, centerPositionOnScreenNormalize.y, centerPositionOnScreenNormalize.z, normalizedRadious);
        
        //Show viewfinder on screen
        SetViewfinderOnScreen(index, centerPositionOnScreenNormalize, normalizedRadious);

        return tempViewfinder;
    }
    ////////////////////////////////////////////
    /// Detect Target inside of Viewfinder
    ////////////////////////////////////////////
    ///
   

    private float CalculateTargetRadious(Vector3 centerPosition, Vector3 maxPosition, Vector3 minPosition)
    {
        float radious;

        Vector3 centerOnscreen = Camera.main.WorldToScreenPoint(centerPosition);
        Vector3 maxPositionOnscreen = Camera.main.WorldToScreenPoint(maxPosition);
        Vector3 minPositionOnscreen = Camera.main.WorldToScreenPoint(minPosition);
       
        //float radiousX = Mathf.Max(maxPositionOnscreen.x - centerOnscreen.x, centerOnscreen.x - minPositionOnscreen.x);
        //float radiousY = Mathf.Max(maxPositionOnscreen.y - centerOnscreen.y, centerOnscreen.y - minPositionOnscreen.y);

        float radiousX = 0.5f * Mathf.Abs(maxPositionOnscreen.x - minPositionOnscreen.x);
        float radiousY = 0.5f * Mathf.Abs(maxPositionOnscreen.y - minPositionOnscreen.y);

        radious = Mathf.Max(Mathf.Abs(radiousX), Mathf.Abs(radiousY));
        //radious *= 2f; // Always I have a too small radious, this is a magic number

        //int screenWidth = Camera.main.pixelWidth;

        return radious;
    }

    public bool IsTargetInsideViewfinderRing(CTargetVertexData target, int bigViewfinderId, int smallViewfinderId)
    {
        // Check if the target is in the front view of the sphere
        Vector3 _3DModelCenter = Get3DModelCenter();
        if (target.centerPosition.z > _3DModelCenter.z) { return false; }
        
        // Get target data
        Vector3 targetCenterPositionOnScreen = GetPositionOnScreen(target.centerPosition);
        float targetRadious = CalculateTargetRadious(target.centerPosition, target.maxPosition, target.minPosition);
        // Get viewfinders
        Vector3 bigViewfinderCenterPosition;
        float bigViewfinderRadious;
        GetViewFinderData(out bigViewfinderCenterPosition, out bigViewfinderRadious, bigViewfinderId);
        Vector3 smallViewfinderCenterPosition;
        float smallViewfinderRadious;
        GetViewFinderData(out smallViewfinderCenterPosition, out smallViewfinderRadious, smallViewfinderId);
        //Check if it is inside the viewfinder

        if ((bigViewfinderRadious < targetRadious) || (targetRadious < smallViewfinderRadious)){ 
            return false;
        }
        
        return IsCircleInsideOfOther(targetCenterPositionOnScreen, targetRadious, smallViewfinderCenterPosition, smallViewfinderRadious);
    }

   


    private void GetViewFinderData(out Vector3 viewfinderCenterPosition, out float viewfinderRadious, int viewfinderID)
    {
        viewfinderCenterPosition = Vector3.zero;
        viewfinderRadious = 0f;
        int index = viewfinderList.FindIndex(r => r.id == viewfinderID);
        if (index != -1)
        {
            viewfinderCenterPosition = viewfinderList[index].position;
            viewfinderRadious = 1f * viewfinderList[index].radious;
        }
    }


    public bool IsTargetInsideViewfinder(CTargetVertexData target)
    {
        // Check if the target is in the front view of the sphere
        Vector3 _3DModelCenter = Get3DModelCenter();
        if (target.centerPosition.z > _3DModelCenter.z) { return false; }

        // Check if it is inside the viewfinder

        Vector3 targetCenterPositionOnScreen = GetPositionOnScreen(target.centerPosition);
        float targetRadious = CalculateTargetRadious(target.centerPosition, target.maxPosition, target.minPosition);

        Vector3 viewfinderCenterPosition = Vector3.zero;
        float viewfinderRadious = 0f;
        int index = viewfinderList.FindIndex(r => r.id == target.viewfinderID);
        if (index != -1)
        {
            viewfinderCenterPosition = viewfinderList[index].position;
            viewfinderRadious = 1f * viewfinderList[index].radious;
        }

        return IsCircleInsideOfOther(viewfinderCenterPosition, viewfinderRadious, targetCenterPositionOnScreen, targetRadious);             
    }

    private bool IsCircleInsideOfOther(Vector3 bigCircleCenter, float bigCircleRadious, Vector3 smallCircleCenter, float smallCircleRadious)
    {
        if (bigCircleRadious < smallCircleRadious) { return false; }

        //float distance = Vector3.Distance(bigCircleCenter, smallCircleCenter);
        float distance = Vector2.Distance(new Vector2(bigCircleCenter.x, bigCircleCenter.y), new Vector2(smallCircleCenter.x, smallCircleCenter.y));

        if (bigCircleRadious > (distance + smallCircleRadious))
        {            
            return true;
        }        
        else
        {         
            return false;
        }
    }
    
    IEnumerator CheckTimeInsideViewFinder(float _waitingTime)
    {
        float waitingTime = _waitingTime * 0.25f;
        yield return new WaitForSeconds(_waitingTime);
        if (targetInsideViewFinder) {           
            hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().SetDockingMade();
            PlaySuccessSound();                
        }
        else
        {
            targetInsideRutineWorking = false;
        }        
    }


    public void RestartViewFinderControl()
    {
        targetInsideRutineWorking = false;        
    }


    public Vector3 Get3DModelCenter()
    {
        Vector3 modelCenter = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().Get3DModelBoundingBox().center;
        Vector3 modelCenterScreenProjection = Camera.main.WorldToScreenPoint(modelCenter);
        return modelCenterScreenProjection;
    }

    public void ActivatedAutomaticInsideViewFinderChecking(bool _activated)
    {
        automaticInsideViewFinderCheckingActivated = _activated;
    }

    //////////////////
    /// VIEWFINDER //
    //////////////////
    ///
    public void SetViewfinderOnScreen(int _id, Vector3 position, float radious)
    {                        
        // Find viewfinder 
        int index = viewfinderList.FindIndex(r => r.id == _id);
        if (index != -1) {
            //Prepare data
            Vector3 centerScreenPosition = GetDesNormalizeViewFinderPosition(position);
            float radiousOnScreen = /*1.8f * */GetDesNormalizeRadious(radious);
            
            viewfinderList[index].SetViewfinder(centerScreenPosition, radiousOnScreen);
        }
    }
   
    //private void SetViewfinderOnScreen(int id, Vector3 centerScreenPosition, float radious)
    //{
    //    GameObject viewfinderNGO = viewfinderGO.transform.Find("Viewfinder_" + id).gameObject;              // Get object            
    //    viewfinderNGO.SetActive(true);                                                                      // Activate
    //    viewfinderNGO.transform.position = centerScreenPosition;                                            // Placing
    //    viewfinderNGO.GetComponent<RectTransform>().sizeDelta = GetViewfinderRadiousOnScreen(radious);      // Change Size       
    //}

    private Vector3 GetPositionOnScreen(Vector3 worldPosition)
    {
        return Camera.main.WorldToScreenPoint(worldPosition);
    }

    private Vector3 GetNormalizeViewFinderPosition(Vector3 centerScreenPosition)
    {
        return new Vector3(centerScreenPosition.x / Camera.main.pixelWidth, centerScreenPosition.y / Camera.main.pixelHeight, centerScreenPosition.z);        
    }

    private Vector3 GetDesNormalizeViewFinderPosition(Vector3 centerScreenPosition)
    {
        return new Vector3(centerScreenPosition.x * Camera.main.pixelWidth, centerScreenPosition.y * Camera.main.pixelHeight, centerScreenPosition.z);
    }

    private float GetNormalizeRadious(float radious)
    {
        return radious;// / Camera.main.aspect;
    }

    private float GetDesNormalizeRadious(float radious)
    {
        return radious;// * Camera.main.aspect;
    }


    //private Vector2 GetViewfinderRadiousOnScreen(float radious)
    //{
    //    return new Vector2(1.8f * radious, 1.8f * radious);
    //}

    //////////////////
    /// METHODS  //
    //////////////////
    public void OnClickWorkloadQuestionerNextButton()
    {
        Debug.Log("clicked Next");

        List<int> questionnaireAnswers = new List<int>();
        for (int i=1; i<7; i++)
        {
            int questionValue = (int)workLoadQuestionnaireGO.transform.Find("Question" + i).gameObject.transform.Find("Slider").GetComponent<Slider>().value;
            questionnaireAnswers.Add(questionValue);            
        }
        
        hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().SaveQuestionnaireAnswer(questionnaireAnswers);
        hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().goToNextExperimentState();

        //Reset questionnaire
        for (int i = 1; i < 7; i++)
        {
            workLoadQuestionnaireGO.transform.Find("Question" + i).gameObject.transform.Find("Slider").GetComponent<Slider>().value = 10;
        }
    }

    public void OnClickSUSQuestionerEndButton()
    {
        Debug.Log("clicked End");

        List<int> questionerAnswers = new List<int>();
        for (int i = 1; i < 11; i++)
        {
            int questionValue = (int)susQuestionnaireGO.transform.Find("Question" + i).gameObject.transform.Find("Slider").GetComponent<Slider>().value;
            questionerAnswers.Add(questionValue);
        }
        hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().SaveQuestionnaireAnswer(questionerAnswers);
        hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().goToNextExperimentState();        

        //Reset questionnaire
        for (int i = 1; i < 11; i++)
        {
            susQuestionnaireGO.transform.Find("Question" + i).gameObject.transform.Find("Slider").GetComponent<Slider>().value = 3;            
        }

    }

    public void OnClickUserPanelEndButton()
    {
        Debug.Log("clicked end User Panel");
        userID = userDataPanelGO.transform.Find("UserID_InputField").gameObject.transform.Find("Text").GetComponent<Text>().text;        
        sessionNumber = userDataPanelGO.transform.Find("Sesion_InputField").gameObject.transform.Find("Text").GetComponent<Text>().text;
        blockNumber = userDataPanelGO.transform.Find("Block_InputField").gameObject.transform.Find("Text").GetComponent<Text>().text;        
        //hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().SetParticipantID(userID);
        //hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().SetSessionNumber(session);
        //hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().goToNextExperimentState();
        hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().goToExperimentState(TExperimentState.startSession);
    }

    public void OnClickIntroSessionPanelButton()
    {
        introSessionPanelGO.transform.Find("Button").gameObject.GetComponent<Button>().interactable = false;
        introSessionPanelGO.transform.Find("Button").gameObject.GetComponent<Button>().gameObject.transform.Find("Text").gameObject.GetComponent<Text>().text = "Loading...";
        hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().goToNextExperimentState();               
    }

    public void OnClickStartBlockPanelButton()
    {
        //hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().SaveUserData(userID, session);
        hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().goToNextExperimentState();
        //hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().StartExperimentSession(userID, session);                
    }

    public void OnClickFinishTrainingButton()
    {        
        hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().FinishTrainning();   
    }

    public void OnClickEndScenarioButton()
    {
        //HideEndScenarioPanel();
        //hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().StartQuestionnaire(1);
        hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().goToNextExperimentState();
    }

    private void HideAll()
    {
        this.HideQuestionsPanel();
        this.HideTrainingPanel();
        this.HideExperimentViewfinder();
    }

    public void ShowWorkLoadQuestionnaire()
    {
        ShowQuestionsPanel();
        workLoadQuestionnaireGO.SetActive(true);
    }

    public void HideWorkLoadQuestioner()
    {
        workLoadQuestionnaireGO.SetActive(false);
    }

    public void ShowSUSQuestioner()
    {
        ShowQuestionsPanel();
        susQuestionnaireGO.SetActive(true);        
    }

    public void HideSUSQuestioner()
    {
        susQuestionnaireGO.SetActive(false);
        HideQuestionsPanel();
    }
 
    public void ShowQuestionsPanel()
    {
        questionsPanelGO.SetActive(true);        
    }

    public void HideQuestionsPanel()
    {
        questionsPanelGO.SetActive(false);        
    }

    public void ShowGetUserDataPanel()
    {
        userDataPanelGO.SetActive(true);
    }

    public void HideUserDataPanel()
    {
        userDataPanelGO.SetActive(false);
    }

    public void ShowIntroSessionPanel()
    {
        introSessionPanelGO.SetActive(true);
        SetIntroSessionPanelInfoText("Participante " + this.userID + " / Sesión " + this.sessionNumber);
    }

    public void HideIntroPanel()
    {
        introSessionPanelGO.SetActive(false);
    }     

    public void ShowTrainingPanel()
    {
        trainingPanel.SetActive(true);        
    }

    public void HideTrainingPanel()
    {
        trainingPanel.SetActive(false);        
    }

    public void ShowStartBlockPanel(int currentScenario, int totaScenario)
    {
        startBlockPanel.SetActive(true);
        string temp = "Bloque " + currentScenario.ToString() + " de " + totaScenario.ToString();
        startBlockPanel.transform.Find("TitleText").gameObject.GetComponent<Text>().text = temp;
    }

    public void HideStartScenarioPanel()
    {
        startBlockPanel.SetActive(false);
    }

    public void ShowExperimentViewfinder()
    {
        viewfinderGO.SetActive(true);        
    }

    public void HideExperimentViewfinder()
    {
        viewfinderGO.SetActive(false);        
    }

    public void ShowEndText()
    {
        endTextGO.gameObject.SetActive(true);
    }

    public void ShowCounterText()
    {
        counterTextGO.gameObject.SetActive(true);
    }
    public void HideCounterText()
    {
        counterTextGO.gameObject.SetActive(false);
    }

    public void ShowEndScenarioPanel()
    {
        endScenarioPanel.SetActive(true);
    }

    public void HideEndScenarioPanel()
    {
        endScenarioPanel.SetActive(false);
    }

    public void SetCounterText(string _text)
    {        
        counterTextGO.GetComponent<Text>().text = _text;
    }
    
    public void ChangeViewFinderColor(Color _newColor)
    {
        foreach (CViewfinderData item in viewfinderList)
        {
            item.ChangeViewfinderColor(_newColor);
        }        
    }

    public void SetTrialPanelButtonColor(Color _newColor)
    {
        GameObject startScenarioButtonGO = trainingPanel.transform.Find("Button").gameObject;
        ColorBlock temp = startScenarioButtonGO.GetComponent<Button>().colors;
        temp.normalColor = _newColor;
        startScenarioButtonGO.GetComponent<Button>().colors = temp;
    }

    public void PlaySuccessSound()
    {
        audioSource.GetComponent<AudioSource>().Play();
    }

    public string GetParticipantId() { return userID; }
    public string GetSessionNumber() { return sessionNumber; }
    public string GetBlockNumber() { return blockNumber; }

    public void SetIntroSessionPanelInfoText(string _text)
    {
        introSessionPanelGO.transform.Find("TextInfoSession").gameObject.GetComponent<Text>().text = _text;
    }

    public void ShowPressSpaceText()
    {
        pressSpaceText.SetActive(true);
    }
    public void HidePressSpaceText()
    {
        pressSpaceText.SetActive(false);
    }
    public void ShowStartBlockTrialsText()
    {
        startBlockTrialsMessage.SetActive(true);
    }
    public void HideBlockTrialsText()
    {
        startBlockTrialsMessage.SetActive(false);
    }
}
