using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CExperimentClickEvent
{
    public DateTime begin;
    public DateTime end;
    public string trialNumber;
    public string areaID;
    public string description;
    public double timeTaken;
    public double timeDocking;
    public double timeSearching;

    public CExperimentClickEvent(string _trialNumber)
    {        
        begin = DateTime.Now;
        trialNumber = _trialNumber;
        timeSearching = 0f;
        timeDocking = 0f;
    }
    public void SaveData(string _areadID, string _description, double _timeTaken, double _dockingTime)
    {
        end = DateTime.Now;
        areaID = _areadID;
        description = _description;
        timeTaken = _timeTaken;
        AddDockingTrialTime(_dockingTime);
    }
    public void AddSearchingTrialTime(double _timeSearching) {
        timeSearching += _timeSearching;
    }
    public void AddDockingTrialTime(double _dockingTime)
    {
        timeDocking += _dockingTime;
    }

}

public class CExperimentDragGesture
{
    public DateTime dateBegin;
    public DateTime dateEnd;
    public double timeTaken;
    public float latitudeTotal;
    public float latitudeEffective;
    public float longitudeTotal;
    public float longitudeEffective;

    public CExperimentDragGesture() {
        this.dateBegin = DateTime.Now;
    }  
}

public class CExperimentMovementGesture
{
    public DateTime dateBegin;
    public DateTime dateEnd;
    public double timeTaken;
    public float radioTotal;
    public float radioEffective;


    public CExperimentMovementGesture() {
        this.dateBegin = DateTime.Now;
    }    
}

public class CBlockDataToSave
{
    public string sessionNumber;
    public string blockNumber;
    public string repetitionNumber;
    public string productModel;
    public string navigationSystem;
    public string navigationAxis;
    public DateTime startDate;
    public DateTime finishDate;
    public double timeTaken;

    public List<int> workloadQuestionnaire;
    public List<int> susQuestionnaire;

    public List<CExperimentClickEvent> clickEventsList;
    //public List<CExperimentDragGesture> dragGestureList;
    //public List<CExperimentMovementGesture> movementGestureList;

    public CBlockDataToSave()
    {        
        clickEventsList = new List<CExperimentClickEvent>();
        //dragGestureList = new List<CExperimentDragGesture>();
        //movementGestureList = new List<CExperimentMovementGesture>();
        workloadQuestionnaire = new List<int>();
        susQuestionnaire = new List<int>();
    }   
}

public class CExperimentDataCollection
{
    public string participantID;
    public string sessionNumber;
    public string device;
    public string description;
    public string title;
    public DateTime startDate;
    public DateTime finishDate;
    public double timeTaken;
    public List<CBlockDataToSave> blocksDataList;
    
    public CExperimentDataCollection()
    {
        startDate = DateTime.Now;
        timeTaken = 0.0d;
        participantID = "";
        description = "";
        blocksDataList = new List<CBlockDataToSave>();
    }    
}

public class CTargetVertexData
{
    //public int id;
    public string areaID;
    public int viewfinderID;
    public Vector3 centerPosition;
    public Vector3 maxPosition;
    public Vector3 minPosition;


    public CTargetVertexData(CTargetVertexData _v)
    {
        areaID = _v.areaID;
        centerPosition = _v.centerPosition;
        maxPosition = _v.maxPosition;
        minPosition = _v.minPosition;
    }

    public CTargetVertexData(string _areaID, Vector3 _centerPosition, Vector3 _maxPosition, Vector3 _minPosition)
    {
        areaID = _areaID;
        
        centerPosition = _centerPosition;
        maxPosition = _maxPosition;
        minPosition = _minPosition;
    }

    public void SetViewfinderID(int _viewfinderID)
    {
        viewfinderID = _viewfinderID;
    }

}
public class CTargetData
{
    public string targetLeafID;
    public Vector3 targetCenterPosition;
    public List<CTargetVertexData> targetVeticesList;
    public int smallViewfinderID;
    public int bigViewfinderID;

    public CTargetData()
    {
        targetVeticesList = new List<CTargetVertexData>();
    }
}

public enum TExperimentState { iddle, getParticipantSessionData, startSession, startBlock, waitingProductModel, showStartBlockPanel, blockTraining, trialsExecution, endBlockTrialsPanel, workLoadQuestionnaire, SUSQuestionnaire, endBlock, endSession, captureTarget}

public class ExperimentManager : MonoBehaviour
{

    private CExperimentDesign experimentPlot;           // Store the exhibition plot    
    //private CPlot experimentPlot;                     // Store the exhibition plot    
    private bool experimentSessionStarted;              // Indicates if the exhibition is been showing or not
    private int currentTrialIndex;                      // Indicates which exhibition point are we showing now
    TNavigationRadialPositions currentZoomPosition;

    private TExperimentState currentExperimentState;
    private bool waitingKeyToStartTrial;

    private int currentParticipantIndex;
    private int currentSessionIndex;
    private int currentBlockIndex;
    private int nextTrialIndex;

    private bool experimentInitiated;

    System.Diagnostics.Stopwatch trialTimer;           //
    System.Diagnostics.Stopwatch searchingTimer;           //
    System.Diagnostics.Stopwatch dockingTimer;           //

    System.Diagnostics.Stopwatch dragTimer;             //
    System.Diagnostics.Stopwatch movementTimer;         //
    System.Diagnostics.Stopwatch blockTimer;         //
    System.Diagnostics.Stopwatch fullExperimentTimer;   //

    CExperimentDataCollection dataCollection;
    CBlockDataToSave currentBlockDataToSave;
    CExperimentClickEvent currenTrialDataToSave;
    CExperimentDragGesture tempDragGesture;
    CExperimentMovementGesture tempMovementGesture;
    bool capturingDragGesture;
    bool capturingMovementGesture;
    
    bool error;
    int questionnaireNumber;
    
    CTargetData targetData;
    List<CViewfinders> capturedViewfinders;

    private void Awake()
    {
        experimentInitiated = false;
        experimentPlot = null;
        experimentSessionStarted = false;
        currentParticipantIndex = -1;
        currentSessionIndex = -1;        
        currentBlockIndex = -1;
        currentTrialIndex = -1;
        currentZoomPosition = TNavigationRadialPositions.initial;

        currentExperimentState = TExperimentState.iddle;
        waitingKeyToStartTrial = false;

        trialTimer = new System.Diagnostics.Stopwatch();
        searchingTimer = new System.Diagnostics.Stopwatch();
        dockingTimer = new System.Diagnostics.Stopwatch(); ;
        dragTimer = new System.Diagnostics.Stopwatch();
        movementTimer = new System.Diagnostics.Stopwatch();
        blockTimer = new System.Diagnostics.Stopwatch();
        fullExperimentTimer = new System.Diagnostics.Stopwatch();

        dataCollection = new CExperimentDataCollection();
        tempMovementGesture = new CExperimentMovementGesture();
        tempDragGesture = new CExperimentDragGesture();
        questionnaireNumber = 0;

        capturingDragGesture = false;
        capturingMovementGesture = false;
        //scenarioStarted = false;
        error = false;

        // To calculate selected object position
        //targetWaitingClick = false;
        //targetWaitingClickCenter = Vector3.zero;
        //targetWaitingClickGO = null;

        CTargetData targetsData = new CTargetData();         
    }
    private void Start()
    {
        // Configure hom3r to the experiment
        hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveUI, false));
        hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveUIGizmo, false));
        hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveUISelection, false));
        hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveNavigation, false));
        hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveMouseWheelInteraction, true));
        hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.SetDurationTransparencyAnimation, 0.2f));
        hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.SetDurationRemoveAnimation, 0.5f));
        hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActivePanNavigation, true));
        
        //if (hom3r.state.platform == THom3rPlatform.Editor) { hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveUI, true)); }

        // Instatiate experiment classes
        hom3r.quickLinks.scriptsObject.AddComponent<ExperimentDesignLoader>();
        hom3r.quickLinks.scriptsObject.AddComponent<ExperimentEventReceiver>();
        hom3r.quickLinks.scriptsObject.AddComponent<ExperimentCommandReceiver>();
        //hom3r.quickLinks.scriptsObject.AddComponent<MultipleExperimentManager>();
        //hom3r.quickLinks.scriptsObject.AddComponent<MultipleExperimentCommandReceiver>();

        //Prepare Experiment Design json URL
        string filesPath = Application.dataPath.Substring(0, Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/")).LastIndexOf("/"));
        string experimentBaseUrl = "file:///" + filesPath + "/experiment/navigation/";
                      
        if (hom3r.state.platform != THom3rPlatform.Editor)
        {
            experimentBaseUrl = "https://proyectos.diana.uma.es/hom3r/experiment/navigation/";
        }
        // string fileName = "8.nav_experiment_ring.json";
        // string fileName = "7.nav_experiment_watermill.json";
        // string fileName = "6.nav_experiment_doubleHexagonalPyramid.json";
        // string fileName = "5.nav_experiment_gas_turbine.json";
        // string fileName = "4.nav_experiment_hexagonalpyramid.json"; 
        // string fileName = "4.nav_experiment_steppedpyramid.json";         
        // string fileName = "3.nav_experiment_jeep.json";
        // string fileName = "2.nav_experiment_hexagonalprism.json";
        // string fileName = "1.nav_experiment_iter.json";
        string fileName = "nav_experiment_pilot_nov2022_v1.json";
        string experimentModel = experimentBaseUrl + fileName;        
        
        InitExperimentManager(experimentModel);        
    }

    private void Update()
    {

    }

    ///////////////////////////////////////////
    ////// Manage Exhibition Plot Methods /////
    ///////////////////////////////////////////

    public void InitExperimentManager(string experimentURL = "")
    {
        if (!experimentInitiated)
        {
            experimentInitiated = true;
            InitAudioObject();
            InitExperimentUI();                             // Initialize experiment UI
            InitPlotLoading(experimentURL);                 // Start experiment plot loading          
        }
    }

    private void InitExperimentUI()
    {
        // Instantiate the Experiment UI Canvas
        GameObject canvasExperiment = (GameObject)Resources.Load("prefabs/NavExperiment/Canvas_Experiment", typeof(GameObject));
        //GameObject canvasExperimentI = Instantiate(canvasExperiment, new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 0f));
        GameObject canvasExperimentI = Instantiate(canvasExperiment, new Vector3(0f, 0f, 0f), Quaternion.identity, hom3r.quickLinks.uiObject.transform);

        canvasExperimentI.transform.name = "Canvas_Experiment";
        //canvasExperimentI.transform.parent = hom3r.quickLinks.uiObject.transform;   // Make child of the UI Object        
        hom3r.quickLinks.uiObject.AddComponent<UIExperimentManager>();  // Attach the UIExperimentManager script                       
    }

    private void InitPlotLoading(string experimentURL = "")
    {        
        //if (experimentURL == "")
        //{
        //    if (hom3r.state.platform == THom3rPlatform.Editor)
        //    {
        //        string filesPath = Application.dataPath.Substring(0, Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/")).LastIndexOf("/"));
        //        experimentURL = "file:///" + filesPath + "/experiment/Plots/experiment_2/" + "experiment_2_pilot_bolas.json";
        //        //experimentURL = "http://150.214.59.100:8890/hom3rexperiment/experiment_2/plots/experiment_2_pilot_bolas.json";
        //        //experimentURL = "http://150.214.59.100:8890/hom3rexperiment/experiment_2/plots/experimentPlot_X.json";                
        //    }
        //    else
        //    {
        //        Debug.Log("THom3rPlatform.Other");
        //        //experimentURL = "http://150.214.59.100:8890/hom3rexperiment/experiment_2/plots/experimentPlot_X.json";
        //        experimentURL = "http://150.214.59.100:8890/hom3rexperiment/experiment_2/plots/experiment_2_pilot_bolas.json";
        //    }
        //}        
        hom3r.quickLinks.scriptsObject.GetComponent<ExperimentDesignLoader>().LoadExperimentPlot(experimentURL);

    }

    private void InitAudioObject()
    {
        GameObject audioObject = new GameObject();
        Instantiate(audioObject, new Vector3(0f, 0f, 0f), Quaternion.identity);
        audioObject.transform.name = "AudioSource";

        AudioSource audioSource = audioObject.AddComponent<AudioSource>();        
        audioSource.clip = (AudioClip)Resources.Load("NavExperiment/109663__grunz__success-low", typeof(AudioClip));
    }


    ///////////////////////////////////////////
    ////// Manage Exhibition Plot Methods /////
    ///////////////////////////////////////////
    public bool IsExperimentInitiated()
    {
        return experimentSessionStarted;
    }

    public void SaveUserData(string _userID, string _session)
    {
        dataCollection.participantID = _userID;
        dataCollection.sessionNumber = _session;
        //userID = _userID;
        //session = _session;
    }
    /// <summary>Start and Exhibition</summary>
    public bool StartExperimentSession()
    {
        if (experimentPlot != null || !experimentSessionStarted)
        {            
            int participantID = Int32.Parse(hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().GetParticipantId());
            int sessionNumber = Int32.Parse(hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().GetSessionNumber());
            int blockNumber = Int32.Parse(hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().GetBlockNumber());

            currentParticipantIndex = experimentPlot.participantList.FindIndex(r => r.participantId == participantID);
            if (currentParticipantIndex ==-1) {
                goToExperimentState(TExperimentState.getParticipantSessionData);
                return false;
            }
            currentSessionIndex = experimentPlot.participantList[currentParticipantIndex].sessions.FindIndex(r=>r.sessionNumber== sessionNumber);
            if (currentSessionIndex == -1) {
                goToExperimentState(TExperimentState.getParticipantSessionData);
                return false;
            }
            currentBlockIndex = experimentPlot.participantList[currentParticipantIndex].sessions[currentSessionIndex].blocks.FindIndex(r => r.blockNumber == blockNumber);
            //InitExperimentDesignBlocks();
            currentBlockIndex--;            //TODO chapuza

            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().HideUserDataPanel();
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ShowIntroSessionPanel();
            
            InitExperimentSessionDataCollection();
            StartMainTimer();
            experimentSessionStarted = true;
            return true;
        }
        else
        {
            //TODO Reset exhibition? Ignore?
            return false;
        }
    }

    private void InitExperimentDesignBlocks()
    {
        CBlock _currenBlock = GetCurrentBlock();
        if (_currenBlock.trials.Count==0)
        {            
            //experimentPlot.participantList[currentParticipantIndex].sessions[currentSessionIndex].blocks[currentBlockIndex].trials = new List<CTrial>(experimentPlot.trialOrders.Find(r => r.id == _currenBlock.trialOrderId).trials);
            _currenBlock.trials = experimentPlot.trialOrders.Find(r => r.id == _currenBlock.trialOrderId).trials;
        }

        if (_currenBlock.viewfinders.Count == 0)
        {
            _currenBlock.viewfinders = experimentPlot.viewfinders;
        }
        //experimentPlot.participantList[currentParticipantIndex].sessions[currentSessionIndex].blocks;
    }

    private void InitExperimentSessionDataCollection()
    {
        dataCollection = new CExperimentDataCollection();

        this.dataCollection.participantID = experimentPlot.participantList[currentParticipantIndex].participantId.ToString();
        this.dataCollection.sessionNumber = experimentPlot.participantList[currentParticipantIndex].sessions[currentSessionIndex].sessionNumber.ToString();
        this.dataCollection.title = experimentPlot.experimentTitle;
        this.dataCollection.description = experimentPlot.experimentDescription;
        this.dataCollection.startDate = DateTime.Now;

        if (hom3r.state.platform == THom3rPlatform.Android)
        {
            this.dataCollection.device = "tactile";
        }
        else if (hom3r.state.platform == THom3rPlatform.Windows)
        {
            this.dataCollection.device = "mouse";
        }
        else
        {
            this.dataCollection.device = "editor";
        }
    }

    private void StartNewBlock()
    {
        // clear all
        currentTrialIndex = 0;
        
        //Go to Next Block
        int previosBlockIndex = currentBlockIndex;
        currentBlockIndex++;
        if (currentBlockIndex == experimentPlot.participantList[currentParticipantIndex].sessions[currentSessionIndex].blocks.Count)
        {
            //End Session
            goToExperimentState(TExperimentState.endSession);        
            return;
        }
        
        //Check if next exhibition plot has different 3D model or not
        //if (HaveSame3DModel(previosBlockIndex, currentBlockIndex))
        //{
        //    //Same Product model --> just select new areas and show new labels            
        //    //this.ApplyExperimentPointActions();            
        //    goToExperimentState(TExperimentState.showStartBlockPanel);
        //}
        //else
        {
            //Different product model--> Remove the current product model loaded and load a new one
            hom3r.coreLink.Do(new CModelCommand(TModelCommands.ResetProductModel));
            LoadProductModel();
            goToExperimentState(TExperimentState.waitingProductModel);
        }
        
    }
   

    


    ///////////////////////////
    ////// Timer Methods //////
    ///////////////////////////

    public void StartEventsDataCollection()
    {
        if (experimentSessionStarted && !error)
        {
            capturingDragGesture = false;
            //scenarioStarted = false;
            dataCollection = new CExperimentDataCollection();
        }
    }

    public void StopSession()
    {
        if (!error)
        {
            StopMainTimer();
            dataCollection.finishDate = DateTime.Now;
            SaveUserFile();
            //SaveClickEventsSummaryFile();
            //SaveWorkloadQuestionnaireSummaryFile();
            //SaveSUSQuestionnaireSummaryFile();
            //SaveDragGesturesSummaryFile();
            //SaveMovementGesturesToFile();

            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ShowEndText();
        }
    }

    private void StartMainTimer()
    {
        double timeTaken = GetRestartTimer(fullExperimentTimer);
    }

    private void StopMainTimer()
    {
        dataCollection.timeTaken = GetRestartTimer(fullExperimentTimer);
    }

    ///////////////////
    // DATA CAPTURE  //
    ///////////////////

    private void StartBlockDataCapture()
    {
        currentBlockDataToSave = new CBlockDataToSave();

        currentBlockDataToSave.sessionNumber = experimentPlot.participantList[currentParticipantIndex].sessions[currentSessionIndex].sessionNumber.ToString();
        currentBlockDataToSave.blockNumber = GetCurrentBlock().blockNumber.ToString();
        currentBlockDataToSave.repetitionNumber = GetCurrentBlock().repetitionNumber.ToString();
        string _productModel = GetCurrentBlock().productModelUrl;
        currentBlockDataToSave.productModel = _productModel.Substring(_productModel.LastIndexOf("/") + 1);
        currentBlockDataToSave.navigationSystem = GetCurrentBlock().navigationSystem;

        string navigation_axis = hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().GetNavigationAxis();
       
        currentBlockDataToSave.navigationAxis = navigation_axis;
      
        currentBlockDataToSave.startDate = DateTime.Now;
        StartBlockTimer();
        //StartTrialTimers();
        StopTimer(dragTimer);
        StopTimer(movementTimer);
    }

    

    private void StartBlockTimer()
    {
        double timeTaken = GetRestartTimer(blockTimer);
    }
    private void StopAndSaveBlockTimer()
    {
        currentBlockDataToSave.timeTaken = GetRestartTimer(blockTimer);
    }

    private void StartTrialTimers()
    {
        double timeTaken = GetRestartTimer(trialTimer);        
    }
    private void StartSearchingTrialTimer()
    {        
        double timeTaken = GetRestartTimer(searchingTimer);
    }

    private void StartDockingTrialTimer()
    {
        double timeTaken = GetRestartTimer(dockingTimer);
    }

    private void StartTrialDataCapture()
    {
        string trialNumber = GetCurrentBlock().trials[currentTrialIndex].trialNumber.ToString();
        //string _areaID = GetCurrentBlock().trials[currentTrialIndex].selectedAreaList[0].areaID;
        //string _areaDescription = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetLeafDescriptionByLeafID(_areaID);
        //Debug.Log(_areaDescription);
        currenTrialDataToSave = new CExperimentClickEvent(trialNumber);
        StartTrialTimers();
        //StartSearchingTrialTimer();
    }

    private void SaveClickEventData(string areaSelectedID)
    {
        double timeTaken = GetRestartTimer(trialTimer);
        double accumulatedDockingTime = 0;//GetRestartTimer(dockingTimer);
        if (timeTaken != 0.0f)
        {
            string areaDescription = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetLeafDescriptionByLeafID(areaSelectedID);
            currenTrialDataToSave.SaveData(areaSelectedID, areaDescription, timeTaken, accumulatedDockingTime);
            //CExperimentClickEvent clickEvent = new CExperimentClickEvent(trialNumber.ToString(), _text, timeTaken);
            currentBlockDataToSave.clickEventsList.Add(currenTrialDataToSave);
        }
    }

    private void SaveAccumulatedSearchingTime()
    {
        if (currentExperimentState != TExperimentState.trialsExecution) { return; }
        double accumulatedSearchingTime = GetRestartTimer(searchingTimer);
        currenTrialDataToSave.AddSearchingTrialTime(accumulatedSearchingTime);
    }
    private void SaveAccumulatedDokingTime()
    {
        if (currentExperimentState != TExperimentState.trialsExecution) { return; }
        double accumulatedDockingTime = GetRestartTimer(dockingTimer);
        currenTrialDataToSave.AddDockingTrialTime(accumulatedDockingTime);
    }

    private void SaveUserFile()
    {
        FileStream file;
        StreamWriter writer;

        string fileName = "Exp_User_" + dataCollection.participantID + "_Session_" + dataCollection.sessionNumber + "_Data";
        CreateFile(fileName, out file, out writer);

        WriteUserFileHeader(writer);            // Write Header 
        writeFileLine(writer, "---------------------------------------------------;");        
        WriteUserClickEventsData(writer);
        writeFileLine(writer, "---------------------------------------------------;");
        WriteUserWorkloadQuestionnaire(writer);
        writeFileLine(writer, "---------------------------------------------------;");
        WriteUserSUSQuestionnaire(writer);
        //writeFileLine(writer, "---------------------------------------------------;");
        //WriteUserMovementGestureData(writer);
        //writeFileLine(writer, "---------------------------------------------------;");
        //WriteUserDragGestureData(writer);
        writer.Close();
    }

    private void WriteUserFileHeader(StreamWriter writer)
    {
        //Write header
        writeFileLine(writer, "Experiment;" + dataCollection.title);
        writeFileLine(writer, "Description;" + dataCollection.description);
        writeFileLine(writer, "Participant;" + dataCollection.participantID);
        writeFileLine(writer, "Session;" + dataCollection.sessionNumber);
        writeFileLine(writer, "Platform;" + dataCollection.device);

        writeFileLine(writer, "Begin;" + dataCollection.startDate);
        writeFileLine(writer, "End;" + dataCollection.finishDate);
        writeFileLine(writer, "Time taken;" + dataCollection.timeTaken.ToString().Replace('.', ','));
        writeFileLine(writer, "");
        writeFileLine(writer, "");
    }
    private void WriteUserFileBlocksSummary(StreamWriter writer)
    {
        writeFileLine(writer, "Scenario Summary");
        writeFileLine(writer, "Scenario;Begin;End;Time taken");
        foreach (CBlockDataToSave blockData in dataCollection.blocksDataList)
        {
            string temp = blockData.blockNumber + ";" + blockData.startDate + ";"
                + blockData.finishDate + ";" + blockData.timeTaken.ToString().Replace('.', ',') + ";";
            writeFileLine(writer, temp);
        }
    }

    private void WriteUserClickEventsData(StreamWriter writer)
    {
        writeFileLine(writer, "Click Events");
        writeFileLine(writer, "BlockNumber;RepetitionNumber;Model;Navigation System;Trial Number;Trial Area; Trial Description;Docked area;Begin;End;Time searching (sec);Time docking (sec);Total Time taken (sec);");
        //Write Click Events Body
        foreach (CBlockDataToSave blockData in dataCollection.blocksDataList)
        {
            foreach (CExperimentClickEvent item in blockData.clickEventsList)
            {
                string temp = blockData.blockNumber + ";" + blockData.repetitionNumber + ";" + blockData.productModel + ";" + blockData.navigationSystem + ";";
                temp = temp +item.trialNumber + ";" + item.areaID +";" + item.description + ";" + item.begin +   ";" + item.end + ";" + item.timeSearching.ToString().Replace('.', ',') + ";" + item.timeDocking.ToString().Replace('.', ',') + ";" + item.timeTaken.ToString().Replace('.', ',') + ";";
                writer.WriteLine(temp);
            }
        }
    }

    //private void WriteUserMovementGestureData(StreamWriter writer)
    //{
    //    writeFileLine(writer, "Movement Gesture Events");        
    //    writeFileLine(writer, "Scenario;Begin;End;Time Taken(sec);Radio Effective;Radio Total");
    //    //Write Click Events Body
    //    foreach (CBlockDataToSave scenario in dataCollection.blocksDataList)
    //    {
    //        foreach (CExperimentMovementGesture item in scenario.movementGestureList)
    //        {
    //            //string temp = scenario.description + ";" + item.date + ";" + item.description + ";" + item.timeTaken.ToString().Replace('.', ',');
    //            string temp = scenario.description + ";" + item.dateBegin.ToString() + ";" + item.dateEnd.ToString() + ";" + DoubleToStringExcel(item.timeTaken) + ";" + DoubleToStringExcel(item.radioEffective) + ";" + DoubleToStringExcel(item.radioTotal);

    //            writer.WriteLine(temp);
    //        }
    //    }
    //}

    //private void WriteUserDragGestureData(StreamWriter writer)
    //{
    //    writeFileLine(writer, "Drag Gesture Events");        
    //    writeFileLine(writer, "Scenario;Begin;End;Time Taken(sec);Latitude Effective;Latitude Total;Longitude Effective;Longitude Total");
    //    //Write Click Events Body
    //    foreach (CBlockDataToSave scenario in dataCollection.blocksDataList)
    //    {
    //        foreach (CExperimentDragGesture item in scenario.dragGestureList)
    //        {                                
    //            string temp = scenario.description + ";" + item.dateBegin.ToString() + ";" + item.dateEnd.ToString() + ";" + DoubleToStringExcel(item.timeTaken) + ";" + DoubleToStringExcel(item.latitudeEffective) + ";" + DoubleToStringExcel(item.latitudeTotal) + ";" + DoubleToStringExcel(item.longitudeEffective) + ";" + DoubleToStringExcel(item.longitudeTotal);
    //            writer.WriteLine(temp);
    //        }
    //    }
    //}


    private void WriteUserWorkloadQuestionnaire(StreamWriter writer)
    {
        writeFileLine(writer, "Workload Questionnaire");
        writeFileLine(writer, "Block Number; Repetition Number;Model;Navigation System;Question1;Question2;Question3;Question4;Question5;Question6");
        foreach (CBlockDataToSave blockData in dataCollection.blocksDataList)
        {
            string temp = blockData.blockNumber + ";" + blockData.repetitionNumber + ";" + blockData.productModel + ";" + blockData.navigationSystem;            
            foreach (int value in blockData.workloadQuestionnaire)
            {                
                temp = temp + ";" + value;
            }
            writeFileLine(writer, temp);
        }
    }

    private void WriteUserSUSQuestionnaire(StreamWriter writer)
    {
        // SUS Questionnaire
        writeFileLine(writer, "SUS Questionnaire");
        writeFileLine(writer, "Block Number;Repetition Number;Model;Navigation System;Question1;Question2;Question3;Question4;Question5;Question6;Question7;Question8;Question9;Question10");
        foreach (CBlockDataToSave blockData in dataCollection.blocksDataList)
        {
            string temp = blockData.blockNumber + ";" + blockData.repetitionNumber + ";" + blockData.productModel + ";" + blockData.navigationSystem;            
            foreach (int value in blockData.susQuestionnaire)
            {              
                temp = temp + ";" + value; 
            }
            writeFileLine(writer, temp);
        }
    }

    /////////////////////////////
    // Click Events Summary File
    /////////////////////////////
    private void SaveClickEventsSummaryFile()
    {
        FileStream file;
        StreamWriter writer;
        string fileName = "Exp_Summary_" + dataCollection.device + "_ClickEvents";
        if (FileExists(fileName + ".csv"))
        {
            OpenFile(fileName, out file, out writer);
        }
        else
        {
            CreateFile(fileName, out file, out writer);
            WriteClickEventsSummaryFileHeader(writer);      //Write header     
        }
        WriteClickEventsSummaryFileData(writer);
        writer.Close();
    }

    private void WriteClickEventsSummaryFileHeader(StreamWriter writer)
    {
        //Write header
        writeFileLine(writer, "ParticipantID;Session;Platform;Block Number;Repetition Number;Model;Navigation System;Trial Number;Area ID;Area Description;Begin;End;Time searching (sec);Time docking (sec);Total Time taken (sec);");
    }

    private void WriteClickEventsSummaryFileData(StreamWriter writer)
    {
        string line;
        line =  dataCollection.participantID+ ";";
        line += currentBlockDataToSave.sessionNumber + ";";
        line += dataCollection.device + ";";
        line += currentBlockDataToSave.blockNumber + ";";
        line += currentBlockDataToSave.repetitionNumber + ";";
        line += currentBlockDataToSave.productModel + ";";
        line += currentBlockDataToSave.navigationSystem + ";";
                
        foreach (CExperimentClickEvent trialData in currentBlockDataToSave.clickEventsList)
        {         
            string line2 = line + trialData.trialNumber + ";" + trialData.areaID + ";" + trialData.description + ";" + trialData.begin + ";" + trialData.end + ";" + trialData.timeSearching.ToString().Replace('.', ',') + ";" + trialData.timeDocking.ToString().Replace('.', ',') + ";" + trialData.timeTaken.ToString().Replace('.', ',') + ";";
            writeFileLine(writer, line2);
        }
    }

    //////////////////////////////////////////
    // Workload Questionnaire Summary File
    //////////////////////////////////////////
    private void SaveWorkloadQuestionnaireSummaryFile()
    {
        FileStream file;
        StreamWriter writer;
        string fileName = "Exp_Summary_" + dataCollection.device + "_WorkloadQuestionnaire";
        if (FileExists(fileName + ".csv"))
        {
            OpenFile(fileName, out file, out writer);
        }
        else
        {
            CreateFile(fileName, out file, out writer);
            WriteWorkloadQuestionnaireSummaryFileHeader(writer);      //Write header     
        }
        WriteWorkloadQuestionnaireSummaryFileData(writer);
        writer.Close();
    }

    private void WriteWorkloadQuestionnaireSummaryFileHeader(StreamWriter writer)
    {
        //Write header
        string headerLine = "ParticipantID;Session;Platform;Block Number;Repetition Number;Model;Navigation System;Begin;End;";
        string headerSecondLine="";
        headerSecondLine += "Question1;Question2;Question3;Question4;Question5;Question6;";        
        writeFileLine(writer, headerLine + headerSecondLine);
    }

    private void WriteWorkloadQuestionnaireSummaryFileData(StreamWriter writer)
    {

        string line;
        line = dataCollection.participantID + ";";
        line += currentBlockDataToSave.sessionNumber + ";";
        line += dataCollection.device + ";";
        line += currentBlockDataToSave.blockNumber + ";";
        line += currentBlockDataToSave.repetitionNumber + ";";
        line += currentBlockDataToSave.productModel + ";";
        line += currentBlockDataToSave.navigationSystem + ";";
        line += currentBlockDataToSave.startDate.ToString().Replace('.', ',') + ";";
        line += currentBlockDataToSave.finishDate.ToString().Replace('.', ',') + ";";
       
        string answersStr = "";
        foreach (int answer in currentBlockDataToSave.workloadQuestionnaire)
        {
            if (answersStr == "") { answersStr = answer.ToString(); }
            else { answersStr += ";" + answer.ToString(); }

        }
        line = line + answersStr;
        writeFileLine(writer, line);
        
    }

    //////////////////////////////////////////
    // SUS Questionnaire Summary File
    //////////////////////////////////////////
    private void SaveSUSQuestionnaireSummaryFile()
    {
        FileStream file;
        StreamWriter writer;
        string fileName = "Exp_Summary_" + dataCollection.device + "_SUSQuestionnaire";
        if (FileExists(fileName + ".csv"))
        {
            OpenFile(fileName, out file, out writer);
        }
        else
        {
            CreateFile(fileName, out file, out writer);
            WriteSUSQuestionnaireSummaryFileHeader(writer);      //Write header     
        }
        WriteSUSQuestionnaireSummaryFileData(writer);
        writer.Close();
    }

    private void WriteSUSQuestionnaireSummaryFileHeader(StreamWriter writer)
    {
        //Write header
        string headerLine = "ParticipantID;Session;Platform;Block Number;Repetition Number;Model;Navigation System;Begin;End;";
        string headerSecondLine = "";
        headerSecondLine += "Question1;Question2;Question3;Question4;Question5;Question6;Question7;Question8;Question9;Question10;";
        writeFileLine(writer, headerLine + headerSecondLine);       
    }

    private void WriteSUSQuestionnaireSummaryFileData(StreamWriter writer)
    {
        string line;
        line = dataCollection.participantID + ";";
        line += currentBlockDataToSave.sessionNumber + ";";
        line += dataCollection.device + ";";
        line += currentBlockDataToSave.blockNumber + ";";
        line += currentBlockDataToSave.repetitionNumber + ";";
        line += currentBlockDataToSave.productModel + ";";
        line += currentBlockDataToSave.navigationSystem + ";";
        line += currentBlockDataToSave.startDate.ToString().Replace('.', ',') + ";";
        line += currentBlockDataToSave.finishDate.ToString().Replace('.', ',') + ";";
        
        string answersStr = "";
        foreach (int answer in currentBlockDataToSave.susQuestionnaire)
        {
            if (answersStr == "") { answersStr = answer.ToString(); }
            else { answersStr += ";" + answer.ToString(); }

        }
        line = line + answersStr;
        writeFileLine(writer, line);       
    }


    /////////////////////////////
    // Click Events Summary File
    /////////////////////////////
   
    private string GetScenarioType(string navigationAxis, string modelRotation)
    {
        if (navigationAxis == "vertical" && modelRotation == "vertical")
        {
            return "A";
        }
        else if (navigationAxis == "vertical" && modelRotation == "horizontal")
        {
            return "B";
        }
        else if (navigationAxis == "horizontal" && modelRotation == "vertical")
        {
            return "C";
        }
        else if (navigationAxis == "horizontal" && modelRotation == "horizontal")
        {
            return "D";
        }
        return null;
    }


    ///////////////////
    // DRAG GESTURE  //
    ///////////////////
    public void RegisterStartDragGesture()
    {        
        if (currentExperimentState == TExperimentState.trialsExecution)
        {
             capturingDragGesture = true;
             if (GetRestartTimer(dragTimer) != 0.0d) {
                 Debug.LogError("Error: Timer should be stopped at this time");
             }else
             {
                 tempDragGesture = new CExperimentDragGesture();
             }
         }
    }

    public void RegisterStopDragGesture()
    {
        if (capturingDragGesture)
        {
            tempDragGesture.dateEnd = DateTime.Now;
            tempDragGesture.timeTaken = GetRestartTimer(dragTimer);
            //currentBlockData.dragGestureList.Add(tempDragGesture);                        
            StopTimer(dragTimer);
            capturingDragGesture = false;
        }
    }

    public void RegisterDragGestureLatitude(float latitude)
    {
        if (capturingDragGesture)
        {
            tempDragGesture.latitudeEffective += latitude;
            tempDragGesture.latitudeTotal += Mathf.Abs(latitude);
        }
    }

    public void RegisterDragGestureLongitude(float longitude)
    {
        if (capturingDragGesture)
        {
            tempDragGesture.longitudeEffective += longitude;
            tempDragGesture.longitudeTotal += Mathf.Abs(longitude);            
        }
    }
    
    public void RegisterStartMovementGesture()
    {
        if (currentExperimentState == TExperimentState.trialsExecution)
         {
             capturingMovementGesture = true;
             if (GetRestartTimer(movementTimer) != 0.0d)
             {
                 Debug.LogError("Error: Timer should be stopped at this time");
             }
             else
             {                                
                tempMovementGesture = new CExperimentMovementGesture();                
             }
         }
    }
    public void RegisterMovementGestureRadio(float pseudoRadio)
    {
        if (capturingMovementGesture)
        {
            tempMovementGesture.radioEffective += pseudoRadio;
            tempMovementGesture.radioTotal += Mathf.Abs(pseudoRadio);            
        }
    }
    public void RegisterStopMovementGesture()
    {
        if (experimentSessionStarted && capturingMovementGesture)
        {            
            tempMovementGesture.dateEnd = DateTime.Now;
            tempMovementGesture.timeTaken = GetRestartTimer(movementTimer);
            //currentBlockData.movementGestureList.Add(tempMovementGesture);
            StopTimer(movementTimer);
            capturingMovementGesture = false;
        }
    }

    //private void SaveMovementGesturesToFile()
    //{        
    //    FileStream file;
    //    StreamWriter writer;
    //    CreateFile("Exp_MovementGestures", out file, out writer);

    //    string navigation_axis = hom3r.quickLinks.orbitPlane.GetComponent<NavigationManager>().GetNavigationAxis();
    //    string model_Rotation = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetGeometryFileRotation(0);
    //    if (model_Rotation == "(0,0,0,0)") { model_Rotation = "vertical"; }
    //    else if (model_Rotation == "(1,0,0,1)") { model_Rotation = "horizontal"; }
    //    //Write header
    //    writeFileLine(writer, "Experiment;" + multiExperimentDescription);
    //    writeFileLine(writer, "Part;" + dataCollection.description);
    //    writeFileLine(writer, "Begin;" + dataCollection.startDate);
    //    writeFileLine(writer, "End;" + dataCollection.finishDate);
    //    writeFileLine(writer, "Time taken;" + dataCollection.timeTaken.ToString().Replace('.', ','));
    //    writeFileLine(writer, "User Id;" + dataCollection.userID);
    //    writeFileLine(writer, "Question 1;" + dataCollection.question1);
    //    writeFileLine(writer, "Question 2;" + dataCollection.question2);
    //    writeFileLine(writer, "");
    //    writeFileLine(writer, "");
    //    writeFileLine(writer, "Date;Time Taken(sec);Radio Effective;Radio Total");
    //    //Write Body
    //    foreach (CExperimentMovementGesture item in dataCollection.movementGestureList)
    //    {
    //        string temp = item.date.ToString() + ";" + DoubleToStringExcel(item.timeTaken) + ";" + DoubleToStringExcel(item.radioEffective) + ";" + DoubleToStringExcel(item.radioTotal) + ";" + navigation_axis + ";" + model_Rotation;
    //        writer.WriteLine(temp);
    //    }
    //    writer.Close();
    //}

    ////////////
    // TIMER  //
    ////////////
    private double GetRestartTimer(System.Diagnostics.Stopwatch _timer)
    {
        double totalTime = 0.0f;
        if (_timer.IsRunning)
        {
            _timer.Stop();
            totalTime = _timer.Elapsed.TotalSeconds;
        }
        _timer.Reset();
        _timer.Start();
        return totalTime;
    }


    private void StopTimer(System.Diagnostics.Stopwatch _timer)
    {
        if (_timer.IsRunning)
        {
            _timer.Stop();
            _timer.Reset();
        }
    }

    ////////////////////
    /// FILE CREATION
    ////////////////////
    private string DoubleToStringExcel(double value)
    {
        return value.ToString().Replace('.', ',');
    }

    private void CreateFile(string fileName, out FileStream _file, out StreamWriter _writer)
    {
        string basePath = "";
        if (hom3r.state.platform == THom3rPlatform.Android) { basePath = Application.persistentDataPath + "/"; }
        int i = 1;
        while (File.Exists(basePath + fileName + "_" + i.ToString() + ".csv"))
        {
            i++;
        }
        // If repeated we add a number at the end
        string temp = "";
        if (i > 1) { temp = "_" + i.ToString(); }
        // create file
        FileStream file = File.Open(basePath + fileName + temp + ".csv", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        StreamWriter writer = new StreamWriter(file);

        _file = file;
        _writer = writer;
    }

    private void writeFileLine(StreamWriter writer, string line)
    {
        writer.WriteLine(line);
    }


    private void OpenFile(string fileName, out FileStream _file, out StreamWriter _writer)
    {
        string basePath = "";
        if (hom3r.state.platform == THom3rPlatform.Android) { basePath = Application.persistentDataPath + "/"; }
        //int i = 1;
        //while (File.Exists(fileName + "_" + i.ToString() + ".csv"))
        //{
        //    i++;
        //}
        FileStream file = File.Open(basePath + fileName + ".csv", FileMode.Append, FileAccess.Write);
        StreamWriter writer = new StreamWriter(file);

        _file = file;
        _writer = writer;
    }

    private bool FileExists(string fileName)
    {
        string basePath = "";
        if (hom3r.state.platform == THom3rPlatform.Android) { basePath = Application.persistentDataPath + "/"; }        
        return File.Exists(basePath + fileName);      
    }
    ////////////////////
    /// QUESTION
    ////////////////////
    //private void MakeQuestion1()
    //{
    //    hom3r.quickLinks.uiObject.GetComponent<UIManager>().ShowExperimentPopUpPanel("El sistema de rotación es fácil de usar");//TODO use command or events
    //}

   /* public void StartQuestionnaire(int questionnaireNumber)
    {

        if (questionnaireNumber == 1)
        {
            this.questionnaireNumber = 1;
            //hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ShowExperimentQuestionPanel("El sistema de rotación es fácil de usar");//TODO use command or events            
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ShowWorkLoadQuestionnaire();
        }
        else if (questionnaireNumber == 2)
        {
            this.questionnaireNumber = 2;
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ShowSUSQuestioner();
            //hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ShowExperimentQuestionPanel("Los movimientos corresponden a lo que yo espero por mis acciones");//TODO use command or events
        }
    }*/
    public void SaveQuestionnaireAnswer(List<int> questionnaireAnswers)
    {
        if (questionnaireNumber == 1)
        {
            currentBlockDataToSave.workloadQuestionnaire = new List<int>(questionnaireAnswers);
            //dataCollection.workloadQuestionnaire = new List<int>(questionnaireAnswers);
            //StartQuestionnaire(2);
        }
        else if (questionnaireNumber == 2)
        {            
            currentBlockDataToSave.susQuestionnaire = new List<int>(questionnaireAnswers);
            //StopScenario();            
        }
    }

    ////////////////////
    /// Training
    ////////////////////
   


    ////////////////////////////////////////////
    /// Detect Target inside of Viewfinder
    ////////////////////////////
    
    

    
    public CTargetData GetTargetData()
    {
        return targetData;
    }

    public void SetDockingMade()
    {      
        FinishTrial(targetData.targetLeafID);
    }

    ////////////////////        
    // State machine
    ////////////////////

    public void goToNextExperimentState()
    {
        currentExperimentState++;
        applyExperimentState();
    }
    public void goToExperimentState(TExperimentState _state) {
        currentExperimentState = _state;
        applyExperimentState();
    }



    private void applyExperimentState()
    {
        if (currentExperimentState == TExperimentState.getParticipantSessionData)
        {
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ShowGetUserDataPanel();
        }
        else if (currentExperimentState == TExperimentState.startSession)
        {

            bool error = !StartExperimentSession();
            if (error)
            {                
                hom3r.coreLink.Do(new CUICommand(TUICommands.ShowAlertText, "Error in participant or session number"));
            }
        }
        else if (currentExperimentState == TExperimentState.startBlock)
        {
            // Start New Block
            StartNewBlock();
        }
        else if (currentExperimentState == TExperimentState.showStartBlockPanel)
        {
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().HideIntroPanel();            
            int totalBlocks = experimentPlot.participantList[currentParticipantIndex].sessions[currentSessionIndex].blocks.Count;           
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ShowStartBlockPanel(GetCurrentBlock().blockNumber, totalBlocks);
            InitExperimentDesignBlocks();

        }
        else if (currentExperimentState == TExperimentState.blockTraining)
        {        
            InitBlockTraining();        
        }
        else if (currentExperimentState == TExperimentState.captureTarget)
        {
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().SetCounterText("0");
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ShowCounterText();
        }
        else if (currentExperimentState == TExperimentState.trialsExecution)
        {
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().SetTrialPanelButtonColor(Color.white);
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ChangeViewFinderColor(new Color32(0xFF, 0x4C, 0x4C, 0xFF));
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().HideTrainingPanel();
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().SetCounterText("10");
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ShowCounterText();
            ShowStartBlockTrialsText();
            InitBlockTrials();
        }
        else if (currentExperimentState == TExperimentState.endBlockTrialsPanel)
        {
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveNavigation, false));
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveUISelection, false));
            hom3r.coreLink.Do(new CModelCommand(TModelCommands.ResetProductModel));
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().SetCounterText("0");
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().HideCounterText();
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().HideExperimentViewfinder();
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ShowEndScenarioPanel();

        }
        else if (currentExperimentState == TExperimentState.workLoadQuestionnaire)
        {
            questionnaireNumber = 1;
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().HideEndScenarioPanel();
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ShowWorkLoadQuestionnaire();
        }
        else if (currentExperimentState == TExperimentState.SUSQuestionnaire)
        {
            questionnaireNumber = 2;
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().HideWorkLoadQuestioner();
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ShowSUSQuestioner();
        }
        else if (currentExperimentState == TExperimentState.endBlock)
        {
            questionnaireNumber = 0;
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().HideSUSQuestioner();
            SaveBlockData();
            SaveClickEventsSummaryFile();
            SaveWorkloadQuestionnaireSummaryFile();
            SaveSUSQuestionnaireSummaryFile();
            Debug.Log("Experiment Scenario Finished");
            hom3r.coreLink.Do(new CUICommand(TUICommands.ShowAlertText, "Experiment Scenario Finished"));
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ActivatedAutomaticInsideViewFinderChecking(false);
            //hom3r.coreLink.Do(new CMultipleExperimentCommand(TMultipleExperimentCommands.nextMultipleExperimentPoint));
            StartNewBlock();            
        }
        else if (currentExperimentState == TExperimentState.endSession)
        {
            // END Experiment
            Debug.Log("Experiment Session Finished");
            hom3r.coreLink.Do(new CUICommand(TUICommands.ShowAlertText, "Experiment Session Finished"));
            StopSession();
            this.GetComponent<IOManager>().IOResetProductModel();

        }
    }

    
    private void SaveBlockData()
    {
        StopAndSaveBlockTimer();
        currentBlockDataToSave.finishDate = DateTime.Now;
        dataCollection.blocksDataList.Add(currentBlockDataToSave);              
    }


    // Target capture methods
    public bool IsDockDetectionMode()
    {
        if (currentExperimentState == TExperimentState.blockTraining || currentExperimentState == TExperimentState.trialsExecution) { return true; }
        return false;
    }

    public bool IsCaptureTargetMode()
    {
        if (currentExperimentState == TExperimentState.captureTarget) {return true; }
        return false;
    }

    public bool ChangeToCaptureTargetMode(bool editFromFirstTrial = true)
    {
        if (currentExperimentState != TExperimentState.blockTraining) { return false; }
        Debug.Log("Start Capture Mode");

        currentExperimentState = TExperimentState.captureTarget;
        //capturedTrialList = new List<CTrial>();
        capturedViewfinders = new List<CViewfinders>();
        applyExperimentState();
        if (editFromFirstTrial) {
            GoToTrial(0);
        } else
        {
            Debug.Log("Editting trial " + GetCurrentBlock().trials[currentTrialIndex].trialNumber);
        }
        
        return true;
    }
    public void SaveCaptureTargetViewfinder(List<CTrialViewfinderData> newViewfinderData)
    {
        if (currentExperimentState == TExperimentState.captureTarget) {
                               
            CViewfinders newViewfinder = new CViewfinders();
            newViewfinder.areaID = GetCurrentBlock().trials[currentTrialIndex].selectedAreaList[0].areaID;
            newViewfinder.viewfinderList = newViewfinderData;

            int index = capturedViewfinders.FindIndex(r => r.areaID == newViewfinder.areaID);
            if (index!=-1) { capturedViewfinders.RemoveAt(index); }
            capturedViewfinders.Add(newViewfinder);
        }
    }

    public void GetCaptureTrialList()
    {
        CBlock a = new CBlock();
        a.viewfinders = capturedViewfinders;

        string to_json = JsonUtility.ToJson(a);
        Debug.Log(to_json);

        a.blockNumber = GetCurrentBlock().blockNumber;
        a.repetitionNumber = GetCurrentBlock().repetitionNumber;
        a.navigationSystem = GetCurrentBlock().navigationSystem;
        a.productModelUrl = GetCurrentBlock().productModelUrl;
        a.trials = GetCurrentBlock().trials;
        
        to_json = JsonUtility.ToJson(a);
        Debug.Log(to_json);        
    }

    public void ResizeViewFindersCaptureMode(float percentage)
    {
        string _areaID;
        List<CTrialViewfinderData> _viewfinderList;
        if (capturedViewfinders.Count != 0)
        {
            _areaID = capturedViewfinders[capturedViewfinders.Count - 1].areaID;
            _viewfinderList = capturedViewfinders[capturedViewfinders.Count - 1].viewfinderList;
        }
        else
        {
            _areaID = GetCurrentBlock().trials[currentTrialIndex].selectedAreaList[0].areaID;
            int _indexViewfinder = GetCurrentBlock().viewfinders.FindIndex(r => r.areaID == _areaID);
            if (_indexViewfinder == -1) { return; }
            _viewfinderList = GetCurrentBlock().viewfinders[_indexViewfinder].viewfinderList;
        }
                      
        foreach (CTrialViewfinderData viewFinder in _viewfinderList)
        {
            viewFinder.radious *= percentage;

            Vector3 position = new Vector3(viewFinder.position.x, viewFinder.position.y, viewFinder.position.z);
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().SetViewfinderOnScreen(viewFinder.id, position, viewFinder.radious);           
        }

        CBlock a = new CBlock();
        a.viewfinders = GetCurrentBlock().viewfinders;
        string to_json = JsonUtility.ToJson(a);
        Debug.Log("ResizeViewFinder, area " + _areaID);
        Debug.Log("viewfinders : " + to_json);
    }

    private CBlock GetCurrentBlock()
    {
        return experimentPlot.participantList[currentParticipantIndex].sessions[currentSessionIndex].blocks[currentBlockIndex];
    }


    private void ZoomJumpToClosest()
    {
        hom3r.coreLink.Do(new CUICommand(TUICommands.ShowAlertText, "Zoom in"));
        hom3r.coreLink.Do(new CNavigationCommand(TNavigationCommands.ZoomToClosest, GetCurrentBlock().zoomLimits.maximum));
        currentZoomPosition = TNavigationRadialPositions.closest;
    }

    private void ZoomJumpToInitial()
    {
        hom3r.coreLink.Do(new CUICommand(TUICommands.ShowAlertText, "Zoom out"));
        hom3r.coreLink.Do(new CNavigationCommand(TNavigationCommands.ZoomToInitial, GetCurrentBlock().zoomLimits.minimum));
        currentZoomPosition = TNavigationRadialPositions.initial;
    }

    ///
    //////////////////////
    // TRIALS
    //////////////////////

    
    private void InitBlockTraining()
    {
        if (experimentPlot != null || currentExperimentState == TExperimentState.blockTraining)
        {
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().HideStartScenarioPanel();
            //hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveUISelection, true));
           
            currentTrialIndex = -1;
            questionnaireNumber = 0;
            capturingDragGesture = false;

            GoToNextRandomTrial();                        
            ShowTrainingPanel();
        }
    }

    public void FinishTrainning()
    {
        if (currentExperimentState == TExperimentState.blockTraining)
        {
            //ZoomJumpOut();
            hom3r.coreLink.Do(new CNavigationCommand(TNavigationCommands.InitializeNavigation));
            //ShowPressSpaceText();            
            goToExperimentState(TExperimentState.trialsExecution);
        }
    }
    private void GoToNextRandomTrial()
    {
        if (experimentSessionStarted)
        {
            System.Random random = new System.Random();
            int nextTrial = random.Next(GetCurrentBlock().trials.Count);
            GoToTrial(nextTrial);            
        }
    }

    private void GoToTrial(int _nextTrialIndex)
    {
        nextTrialIndex = _nextTrialIndex;
        //HiddeAllAreas(nextTrialIndex);
        waitingKeyToStartTrial = true;

        CleanTrialScenario(nextTrialIndex);
        ShowPressSpaceText();

        UpdateTrialCounter(_nextTrialIndex);
        Debug.Log("Waiting for the space key to be pressed");
        // TODO SHOW Waiting SPACE KEY IN UI
    }

    public void AfterSpaceKeyPressed()
    {
        if (waitingKeyToStartTrial)
        {            
            waitingKeyToStartTrial = false;
            HidePressSpaceText();
            HideBlockTrialsText();
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveNavigation, true));
            StartTrial(nextTrialIndex);
            ShowViewfinder();
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ActivatedAutomaticInsideViewFinderChecking(true);
        }
        //else
        //{
        //    if (currentZoomPosition == TNavigationRadialPositions.closest)
        //    {
        //        //It was docking and going back to search
        //        SaveAccumulatedDokingTime();
        //        StartSearchingTrialTimer();
        //        ZoomJumpToInitial();
        //        //currentZoomPosition = TNavigationRadialPositions.initial;
        //    }
        //    else if (currentZoomPosition == TNavigationRadialPositions.initial)
        //    {
        //        //It was searching and going to docking
        //        SaveAccumulatedSearchingTime();
        //        StartDockingTrialTimer();
        //        ZoomJumpToClosest();
        //        currentZoomPosition = TNavigationRadialPositions.closest;
        //    }
        //}                    
    }

    private void ZoomJump(TNavigationRadialPositions newPosition )
    {

    }

    public void InitBlockTrials()
    {
        if (experimentPlot != null || currentExperimentState == TExperimentState.trialsExecution)
        {            
            currentTrialIndex = -1;
            questionnaireNumber = 0;            

            capturingDragGesture = false;
            StartBlockDataCapture();

            currentZoomPosition = TNavigationRadialPositions.initial;
            GoToTrial(0);
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ActivatedAutomaticInsideViewFinderChecking(true);
        }
        else
        {
            //TODO Reset exhibition? Ignore?
        }
    }



    /// <summary>Update the scene to the next trial</summary>
    public void FinishTrial(string areaSelectedID)
    {
        if (!experimentSessionStarted) { return; }

        if (currentExperimentState == TExperimentState.blockTraining)
        {            
            this.GoToNextRandomTrial();
        }
        else if (currentExperimentState == TExperimentState.trialsExecution)
        {            
            CBlock currentBlock = experimentPlot.participantList[currentParticipantIndex].sessions[currentSessionIndex].blocks[currentBlockIndex];
            if (areaSelectedID == currentBlock.trials[currentTrialIndex].selectedAreaList[0].areaID)
            {
                // save the data                               
                SaveClickEventData(areaSelectedID);    // Data collection                  
                //Move to next point
                int nextTrial = currentTrialIndex + 1;
                if (nextTrial < currentBlock.trials.Count)
                {
                    this.GoToTrial(nextTrial);
                }
                else
                {
                    goToNextExperimentState();
                }
            }
        }
        else if (currentExperimentState == TExperimentState.captureTarget)
        {
            //CBlock currentBlock = experimentPlot.participantList[currentParticipantIndex].sessions[currentSessionIndex].blocks[currentBlockIndex];            
            int nextTrial = currentTrialIndex + 1;
            if (nextTrial < GetCurrentBlock().trials.Count)
            {
                this.GoToTrial(nextTrial);
            }
            else
            {
                GetCaptureTrialList();
            }
        }
    }
    
    public void GoBackTrial()
    {
        //if (currentExperimentState == TExperimentState.captureTarget)
        {            
            int nextTrial = currentTrialIndex -1;
            if (nextTrial < GetCurrentBlock().trials.Count)
            {
                this.GoToTrial(nextTrial);
            }
            else
            {
                GetCaptureTrialList();
            }
        }
    }


    private void CleanTrialScenario(int trialIndex)
    {
        hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveNavigation, false));        
        HideViewfinder();        
        HiddeAllTrialAreas(trialIndex);
        ZoomJumpToInitial();
        hom3r.coreLink.Do(new CNavigationCommand(TNavigationCommands.ResetPanNavigation));
    }
    
    /// <summary>Update the 3D scene to next trial following which is indicated into de experiment design</summary>
    /// <param name="nextTrialIndex"></param>
    private void StartTrial(int nextTrialIndex)
    {        
        currentTrialIndex = nextTrialIndex;

        HideAreasFromTheTrialList(currentTrialIndex);
        PrepareSelectedTrialAreas(currentTrialIndex);               
        PrepareTrialViewfinders(currentTrialIndex);

        hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().RestartViewFinderControl();
        //UpdateTrialCounter();

        StartTrialDataCapture();
    }



    //////////////////////////////
    /// CONFIGURE SCENARIO
    //////////////////////////////
    ///

    private void SetScenarioNavigationSystem(string navigationSystem)
    {
        hom3r.coreLink.Do(new CNavigationCommand(TNavigationCommands.InitializeNavigation));

        TNavigationSystemMode _TNavigationSystemMode = TNavigationSystemMode.Ellipsoid;
        TInteractionMappingCorrectionMode _navigationIteractionCorrection = TInteractionMappingCorrectionMode.distance;
        if (navigationSystem == "Ellipsoid")
        {
            _TNavigationSystemMode = TNavigationSystemMode.Ellipsoid;            
        }
        else if (navigationSystem == "Spheroid")
        {
            _TNavigationSystemMode = TNavigationSystemMode.Spheroid;
        }
        else if (navigationSystem == "Spherical")
        {
            _TNavigationSystemMode = TNavigationSystemMode.Spherical;
            _navigationIteractionCorrection = TInteractionMappingCorrectionMode.ellipsePerimeter;
        }

        hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.SetNavigationSystemMode, _TNavigationSystemMode));
        hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.SetNavigationLatitudeCorrectioFactorMode, _navigationIteractionCorrection));
    }

    private void SetScenarioPanNavigation(bool _activated)
    {
        hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActivePanNavigation, _activated));
    }

    private void SetZoomLimits(bool _zoomMinimumLimitedActivated, float _minimumLimit, bool _zoomMaximumLimitedActivated, float _maximumLimit)
    {
        hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveNavigationZoomMinimumLimit, _zoomMinimumLimitedActivated));
        hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.SetNavigationZoomMinimumLimit, _minimumLimit));

        hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveNavigationZoomMaximumLimit, _zoomMaximumLimitedActivated));
        hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.SetNavigationZoomMaximumLimit, _maximumLimit));
    }

    private void PrepareTrialViewfinders(int trialIndex)
    {

        string _areaID = GetCurrentBlock().trials[trialIndex].selectedAreaList[0].areaID;
        int _indexViewfinder = GetCurrentBlock().viewfinders.FindIndex(r => r.areaID == _areaID);
        if (_indexViewfinder == -1) { return; }
        float targetRadious= 0;

        foreach (CTrialViewfinderData viewFinder in GetCurrentBlock().viewfinders[_indexViewfinder].viewfinderList)
        {
            if (viewFinder.id==0) {
                targetRadious = viewFinder.radious;
                continue;
            }
            Vector3 position = new Vector3(viewFinder.position.x, viewFinder.position.y, viewFinder.position.z);
            hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().SetViewfinderOnScreen(viewFinder.id, position, viewFinder.radious);

            //Store the link between viewfinder and targets
            //int index = targetData.targetVeticesList.FindIndex(r => r.areaID == viewFinder.targetID && r.viewfinderID==0);
            //if (index != -1)
            //{
            //    targetData.targetVeticesList[index].SetViewfinderID(viewFinder.id);
            //}
            if (viewFinder.radious > targetRadious) { targetData.bigViewfinderID = viewFinder.id; }
            if (viewFinder.radious < targetRadious) { targetData.smallViewfinderID = viewFinder.id; }
            //if (targetData.smallViewfinderID == 0)
            //{
            //    targetData.smallViewfinderID = viewFinder.id;
            //} else if (targetData.bigViewfinderID == 0)
            //{
            //    targetData.bigViewfinderID = viewFinder.id;
            //} else { }                
        }
    }

    private void HideAreasFromTheTrialList(int trialIndex)
    {
        hom3r.state.selectionBlocked = false;

        CBlock _block = GetCurrentBlock();

        foreach (CPlotHiddenAreaData item in GetCurrentBlock().trials[trialIndex].hiddenAreaList)
        {
            if (item != null)
            {
                if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsLeaf(item.id))
                {
                    //HiddeLeafAreas(item.id);
                    hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.RemoveArea, item.id, THom3rCommandOrigin.io), Constants.undoAllowed);
                }
                else if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsArea(item.id))
                {
                    //GameObject obj = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(item.id);
                    //HiddeAreaGO(obj);

                    hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.RemoveArea, item.id, THom3rCommandOrigin.io), Constants.undoAllowed);
                }
            }

        }
        hom3r.state.selectionBlocked = true;
    }
    private void HiddeSelectableAreas(int trialIndex)
    {
        hom3r.state.selectionBlocked = false;
        foreach (CPlotSelectedAreaData item in GetCurrentBlock().trials[trialIndex].selectedAreaList)
        {
            if (item != null)
            {
                if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsLeaf(item.areaID))
                {
                    //HiddeLeafAreas(item.id);
                    hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.RemoveArea, item.areaID, THom3rCommandOrigin.io), Constants.undoAllowed);
                }
                else if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsArea(item.areaID))
                {
                    //GameObject obj = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(item.id);
                    //HiddeAreaGO(obj);

                    hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.RemoveArea, item.areaID, THom3rCommandOrigin.io), Constants.undoAllowed);
                }
            }

        }
        hom3r.state.selectionBlocked = true;
    }

    private void HiddeAllTrialAreas(int trialIndex)
    {
        HideAreasFromTheTrialList(trialIndex);
        HiddeSelectableAreas(trialIndex);
    }


    private void HiddeLeafAreas(string leafId)
    {
        List<GameObject> areasGOList = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObjectList_ByLeafID(leafId);
        foreach (GameObject obj in areasGOList)
        {
            HiddeAreaGO(obj);
        }
    }

    private void HiddeAreaGO(GameObject obj)
    {
        if (obj != null)
        {
            hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.RemoveGameObject, obj), Constants.undoAllowed);
        }
    }



    private void PrepareSelectedTrialAreas(int trialIndex)
    {        
        foreach (CPlotSelectedAreaData item in GetCurrentBlock().trials[trialIndex].selectedAreaList)
        {        
            SetTargetsParts(item.areaID, item.colour);
            Debug.Log("Find the area " + item.areaID);
        }
    }

    private void SetTargetsParts(string leafID, string selectionColor)
    {
        targetData = new CTargetData();
        List<string> areasIDList = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaList_ByLeafID(leafID);

        foreach (string _id in areasIDList)
        {
            string areaDescription = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaDescriptionByAreaID(_id);

            if (areaDescription.StartsWith("sphere_"))
            {             
                targetData.targetLeafID = leafID;
                hom3r.coreLink.Do(new CSelectionCommand(TSelectionCommands.SelectPart, _id, selectionColor));
                
                GameObject vertexGO = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(_id);
                Bounds vertexBounds = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().CalculateExtern3DModelBoundingBox(vertexGO);
                CTargetVertexData vertexData = new CTargetVertexData(_id, vertexBounds.center, vertexBounds.max, vertexBounds.min);
                targetData.targetVeticesList.Add(vertexData);



                //}
                //else if (areaDescription.StartsWith("star_sphere_"))
                //{
                //    hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.ShowRemovedArea, _id, THom3rCommandOrigin.io));
                //    GameObject vertexGO = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(_id);
                //    Bounds vertexBounds = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().CalculateExtern3DModelBoundingBox(vertexGO);
                //    CTargetVertexData vertexData = new CTargetVertexData(_id, vertexBounds.center, vertexBounds.max, vertexBounds.min);
                //    targetData.targetVeticesList.Add(vertexData);
            }
        }
    }



    //////////////////////////////
    /////// 3D Models
    //////////////////////////////
    ///
    /// <summary>Return if two exhibitions points have or not the same 3D Model</summary>
    /// <param name="previousBlockIndex">first exhibition point ID</param>
    /// <param name="currentBlockIndex">second exhibition point ID</param>
    /// <returns>true if the have the same 3D model</returns>
    private bool HaveSame3DModel(int previousBlockIndex, int currentBlockIndex)
    {
        if (previousBlockIndex == -1 || currentBlockIndex == -1)
        {
            return false;
        }
        else
        {
            string previousProductModel = experimentPlot.participantList[currentParticipantIndex].sessions[currentSessionIndex].blocks[previousBlockIndex].productModelUrl;
            string currentProductModel = GetCurrentBlock().productModelUrl;
            return previousProductModel == currentProductModel;
        }
    }

    /// <summary>Loads the product model indicated into an exhibition plot</summary>
    /// <param name="blockIndex"></param>
    private void LoadProductModel()
    {
        if (experimentSessionStarted)
        {
            string productModelUrl = GetCurrentBlock().productModelUrl;            
            hom3r.coreLink.Do(new CModelCommand(TModelCommands.LoadProductModel, productModelUrl, ""), Constants.undoNotAllowed);
        }
    }
    
    public void AfterLoad3DModel()
    {
        if (currentExperimentState == TExperimentState.waitingProductModel)
        {
            SetScenarioNavigationSystem(GetCurrentBlock().navigationSystem);            
            SetScenarioPanNavigation(GetCurrentBlock().panNavigation);
            SetZoomLimits(GetCurrentBlock().zoomMinimumLimited, GetCurrentBlock().zoomLimits.minimum, GetCurrentBlock().zoomMaximumLimited, GetCurrentBlock().zoomLimits.maximum);            
            goToExperimentState(TExperimentState.showStartBlockPanel);
        }
    }

    //////////////////////////////////////////
    ////// LOAD Experiment design       //////
    //////////////////////////////////////////

    /// <summary>Store exhibition plot</summary>
    /// <param name="_exhibitionDesign">Exhibition plot class</param>
    public void ExperimentDesignLoadSuccess(CExperimentDesign _exhibitionDesign)
    {
        this.experimentPlot = _exhibitionDesign;
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ExhibitionManager_ExhibitionPlotLoadSuccess));  
        if (!experimentSessionStarted)
        {
            goToExperimentState(TExperimentState.getParticipantSessionData);               // First state if the experiment state machine
        }
        else
        {
            goToExperimentState(TExperimentState.showStartBlockPanel);
        }
    }

    /// <summary>
    /// Emit and error event when the load of the exhibition plot has failed
    /// </summary>
    /// <param name="errorMessage"></param>
    public void ExperimentDesignLoadError(string errorMessage)
    {
        Debug.Log(errorMessage);        
        experimentSessionStarted = false;
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ExhibitionManager_ExhibitionPlotLoadError, errorMessage));
    }

    /////////////////////////////
    /// COMUNICATION WITH UI
    /////////////////////////////
    ///
    private void ShowViewfinder()
    {
        hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ShowExperimentViewfinder(); //TODO use command or events
    }
    private void HideViewfinder()
    {
        hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().HideExperimentViewfinder();
    }

    private void ShowTrainingPanel()
    {
        hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ShowTrainingPanel(); //TODO use command or events
    }
    private void HideTrainingPanel()
    {
        hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().HideTrainingPanel();//TODO use command or events
    }
    private void HideEndScenarioPanel()
    {
        hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().HideEndScenarioPanel();//TODO use command or events
    }

    private void UpdateTrialCounter(int _currentTrialIndex)
    {
        int remainingPoints = GetCurrentBlock().trials.Count - _currentTrialIndex;
        hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().SetCounterText(remainingPoints.ToString());
    }

    private void ShowPressSpaceText()
    {
        hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ShowPressSpaceText();
    }
    private void HidePressSpaceText()
    {
        hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().HidePressSpaceText();
    }
    private void ShowStartBlockTrialsText()
    {
        hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().ShowStartBlockTrialsText();
    }
    private void HideBlockTrialsText()
    {
        hom3r.quickLinks.uiObject.GetComponent<UIExperimentManager>().HideBlockTrialsText();
    }

    /////////////////////////////
    /// COMUNICATION WITH UI
    /////////////////////////////
    ///
    public void GenerateNewParticipant()
    {

        List<CParticipantList> participantList = new List<CParticipantList>();
               
        for (int participantId = 0; participantId < 33; participantId++)
        {
            CParticipantList newParticipant = new CParticipantList();
            CSession newSession = new CSession();
            newSession.sessionNumber = 1;

            List<int> order = GetBlockOrder(participantId);
            int blockNumber = 1;
            foreach (int i in order)
            {
                CBlock newBlock = new CBlock(experimentPlot.participantList[0].sessions[0].blocks[i - 1]);
                newBlock.blockNumber = blockNumber++;
                newBlock.trialOrderId = GetTrialsOrder(participantId, blockNumber-1);
                newSession.blocks.Add(newBlock);                
            }
            newParticipant.participantId = participantId;
            newParticipant.sessions = new List<CSession>();
            newParticipant.sessions.Add(newSession);

            participantList.Add(newParticipant);
            Debug.Log("Participant "+ participantId + " created");
        }
        CExperimentDesign temp = new CExperimentDesign();
        temp.experimentTitle = experimentPlot.experimentTitle;
        temp.experimentDescription = experimentPlot.experimentDescription;
        temp.dateTime = experimentPlot.dateTime;
        temp.trialOrders = experimentPlot.trialOrders;
        temp.viewfinders = experimentPlot.viewfinders;
        temp.participantList = participantList;


        string filesPath = Application.dataPath.Substring(0, Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/")).LastIndexOf("/"));
        string experimentBaseUrl = "file:///" + filesPath + "/experiment/navigation/";
        string jsonSavePath = "../experiment/navigation/AUTOGeneratedExperimetnFile.json";
        string jsonData = JsonUtility.ToJson(temp, false);
        File.WriteAllText(jsonSavePath, jsonData);
    }

    List<int> GetBlockOrder(int participantId)
    {
        List<int> blocks;

        switch (participantId)
        {
            case 0: blocks = new List<int>() { 1, 2, 1, 2 }; break;
            case 1: blocks = new List<int>() { 2, 1, 2, 1 }; break;
            case 2: blocks = new List<int>() { 1, 2, 1, 2 }; break;
            case 3: blocks = new List<int>() { 2, 1, 2, 1 }; break;
            case 4: blocks = new List<int>() { 2, 1, 2, 1 }; break;
            case 5: blocks = new List<int>() { 2, 1, 2, 1 }; break;
            case 6: blocks = new List<int>() { 1, 2, 1, 2 }; break;
            case 7: blocks = new List<int>() { 1, 2, 1, 2 }; break;
            case 8: blocks = new List<int>() { 1, 2, 1, 2 }; break;
            case 9: blocks = new List<int>() { 1, 2, 1, 2 }; break;
            case 10: blocks = new List<int>() { 2, 1, 2, 1 }; break;
            case 11: blocks = new List<int>() { 1, 2, 1, 2 }; break;
            case 12: blocks = new List<int>() { 1, 2, 1, 2 }; break;
            case 13: blocks = new List<int>() { 1, 2, 1, 2 }; break;
            case 14: blocks = new List<int>() { 2, 1, 2, 1 }; break;
            case 15: blocks = new List<int>() { 2, 1, 2, 1 }; break;
            case 16: blocks = new List<int>() { 2, 1, 2, 1 }; break;
            case 17: blocks = new List<int>() { 2, 1, 2, 1 }; break;
            case 18: blocks = new List<int>() { 2, 1, 2, 1 }; break;
            case 19: blocks = new List<int>() { 1, 2, 1, 2 }; break;
            case 20: blocks = new List<int>() { 2, 1, 2, 1 }; break;
            case 21: blocks = new List<int>() { 1, 2, 1, 2 }; break;
            case 22: blocks = new List<int>() { 2, 1, 2, 1 }; break;
            case 23: blocks = new List<int>() { 1, 2, 1, 2 }; break;
            case 24: blocks = new List<int>() { 1, 2, 1, 2 }; break;
            case 25: blocks = new List<int>() { 1, 2, 1, 2 }; break;
            case 26: blocks = new List<int>() { 1, 2, 1, 2 }; break;
            case 27: blocks = new List<int>() { 2, 1, 2, 1 }; break;
            case 28: blocks = new List<int>() { 1, 2, 1, 2 }; break;
            case 29: blocks = new List<int>() { 2, 1, 2, 1 }; break;
            case 30: blocks = new List<int>() { 2, 1, 2, 1 }; break;
            case 31: blocks = new List<int>() { 1, 2, 1, 2 }; break;
            case 32: blocks = new List<int>() { 2, 1, 2, 1 }; break;
            default:
                blocks = new List<int>() { 1, 2, 1, 2 };
                break;
        }
        return blocks;
    }
    string GetTrialsOrder(int participantId, int blockId)
    {
        List<string> trialOrders;
        switch (participantId)
        {
            case 0:
                trialOrders = new List<string>() { "a", "b", "c", "d" };
                break;
            case 1: trialOrders = new List<string>() { "b", "c", "d", "a" }; break;
            case 2: trialOrders = new List<string>() { "d", "a", "b", "c" }; break;
            case 3: trialOrders = new List<string>() { "c", "d", "a", "b" }; break;
            case 4: trialOrders = new List<string>() { "d", "a", "b", "c" }; break;
            case 5: trialOrders = new List<string>() { "a", "b", "c", "d" }; break;
            case 6: trialOrders = new List<string>() { "a", "b", "c", "d" }; break;
            case 7: trialOrders = new List<string>() { "c", "d", "a", "b" }; break;
            case 8: trialOrders = new List<string>() { "b", "c", "d", "a" }; break;
            case 9: trialOrders = new List<string>() { "d", "a", "b", "c" }; break;
            case 10: trialOrders = new List<string>() { "b", "c", "d", "a" }; break;
            case 11: trialOrders = new List<string>() { "a", "b", "c", "d" }; break;
            case 12: trialOrders = new List<string>() { "b", "c", "d", "a" }; break;
            case 13: trialOrders = new List<string>() { "c", "d", "a", "b" }; break;
            case 14: trialOrders = new List<string>() { "d", "a", "b", "c" }; break;
            case 15: trialOrders = new List<string>() { "c", "d", "a", "b" }; break;
            case 16: trialOrders = new List<string>() { "a", "b", "c", "d" }; break;
            case 17: trialOrders = new List<string>() { "d", "a", "b", "c" }; break;
            case 18: trialOrders = new List<string>() { "a", "b", "c", "d" }; break;
            case 19: trialOrders = new List<string>() { "c", "d", "a", "b" }; break;
            case 20: trialOrders = new List<string>() { "c", "d", "a", "b" }; break;
            case 21: trialOrders = new List<string>() { "b", "c", "d", "a" }; break;
            case 22: trialOrders = new List<string>() { "b", "c", "d", "a" }; break;
            case 23: trialOrders = new List<string>() { "a", "b", "c", "d" }; break;
            case 24: trialOrders = new List<string>() { "d", "a", "b", "c" }; break;
            case 25: trialOrders = new List<string>() { "c", "d", "a", "b" }; break;
            case 26: trialOrders = new List<string>() { "b", "c", "d", "a" }; break;
            case 27: trialOrders = new List<string>() { "a", "b", "c", "d" }; break;
            case 28: trialOrders = new List<string>() { "a", "b", "c", "d" }; break;
            case 29: trialOrders = new List<string>() { "b", "c", "d", "a" }; break;
            case 30: trialOrders = new List<string>() { "c", "d", "a", "b" }; break;
            case 31: trialOrders = new List<string>() { "d", "a", "b", "c" }; break;
            case 32: trialOrders = new List<string>() { "d", "a", "b", "c" }; break;
            default:
                trialOrders = new List<string>() { "a", "b", "c", "d" };
                break;
        }
        return trialOrders[blockId-1];
    }
}
