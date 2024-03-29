using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePointCommandReceiver : MonoBehaviour {

    private void OnEnable()
    {        
        hom3r.coreLink.SubscribeCommandObserver(DoSinglePointCommand, UndoSinglePointCommand);  //Subscribe a method to the event delegate
        hom3r.coreLink.SubscribeEventObserver(DoInternalEventCommand);  //Subscribe a method to the event delegate
    }

    private void OnDisable()
    {        
        hom3r.coreLink.UnsubscribeCommandObserver(DoSinglePointCommand, UndoSinglePointCommand);    //Unsubscribe a method to the event delegate
        hom3r.coreLink.UnsubscribeEventObserver(DoInternalEventCommand);                            //Unsubscribe a method to the event delegate
    }

    private void DoSinglePointCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CSinglePointCommand)) { command.Do(this); }
        else { /* Do nothing */ }
    }

    private void UndoSinglePointCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CSinglePointCommand)) { command.Undo(this); }
        else { /* Do nothing */ }
    }

    private void DoInternalEventCommand(CCoreEvent _event)
    {
        if (_event.GetType() == typeof(CCoreEvent)) { ExecuteInternalEvents(_event); }
        else { /* Error - Do nothing */ }
    }

   
    /// <summary>Execute internal events commands</summary>
    /// <param name="_event">command to be executed</param>
    private void ExecuteInternalEvents(CCoreEvent _event)
    {
        if (_event.data != null)
        {
            switch (_event.data.commandEvent)
            {
                case TCoreEvent.Navigation_NavigationToFocusEnd:                    
                    this.GetComponent<SinglePointManager>().UpdateSinglePointScale();     //Single Point scale update
                    break;
                case TCoreEvent.Navigation_ApproximationEnd:                                                                                  
                    this.GetComponent<SinglePointManager>().UpdateSinglePointScale();     //Single Point scale update
                    break;
                case TCoreEvent.ObjectState_AreaRemoved:                    
                    //RemoveSinglePointsGroup                        
                    this.GetComponent<SinglePointManager>().DeleteASinglePointGroup_byAreaGO(_event.data.obj);
                    //INFORM Web APPP ABOUT WHICH LABEL AND SINGLEPOINT GROUP HAS BEEN DELETED
                    break;
                default:
                    break;
            }
        }
    }
}


/// <summary>SinglePoint commands</summary>
public enum TSinglePointCommands
{
    VisualizeSinglePoint, RemoveSinglePoint, RemoveAllSinglePoints, EditSinglePointGroup,
    SinglePointMode_Start, SinglePointMode_Save, SinglePointMode_Exit, SinglePointCaptureRemove, SinglePointUndo
}

/// <summary>SinglePoint data</summary>
public class CSinglePointCommandData
{
    public TSinglePointCommands command;

    public int singlePointGroupID { get; set; }
    public CSinglePointIO listOfSinglePoints { get; set; }

    public CSinglePointCommandData(TSinglePointCommands _command)
    {
        this.command = _command;
    }
}

/// <summary>A 'ConcreteCommand' class</summary>
public class CSinglePointCommand : CCoreCommand
{
    public CSinglePointCommandData data;

    //////////////////
    // Constructors //
    //////////////////
    public CSinglePointCommand(TSinglePointCommands _command)
    {
        data = new CSinglePointCommandData(_command);
    }

    public CSinglePointCommand(TSinglePointCommands _command, int _singlePointGroupID)
    {
        data = new CSinglePointCommandData(_command);
        data.singlePointGroupID = _singlePointGroupID;
    }

    public CSinglePointCommand(TSinglePointCommands _command, CSinglePointIO _listOfSinglePoints)
    {
        data = new CSinglePointCommandData(_command);
        data.listOfSinglePoints = _listOfSinglePoints;
    }

    //////////////////
    //   Execute    //
    //////////////////
    public void Do(MonoBehaviour m)
    {
        if (data != null)
        {
            switch (data.command)
            {
                case TSinglePointCommands.SinglePointMode_Start:
                    //Leave transparency mode if is activated
                    if (hom3r.state.smartTransparencyModeActive)
                    {
                        m.GetComponent<Core>().Do(new COcclusionCommand(TOcclusionCommands.DisableSmartTransparency), Constants.undoNotAllowed);
                    }
                    hom3r.state.currentMode = THom3rMode.capturing_surface_point;
                    m.GetComponent<SinglePointManager>().SetSinglePoint_SubMode(SinglePointSubmode_Type.CaptureSPMode);
                    //Change mouse cursor image
                    //hom3r.quickLinks.scriptsObject.GetComponent<MouseKeyboardManager>().ChangeMouseIcon();
                    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Core_ModeChanged));
                    //Mode icons show and reset
                    hom3r.coreLink.Do(new CUICommand(TUICommands.EnableSinglePointIcons), Constants.undoNotAllowed);
                    hom3r.coreLink.Do(new CUICommand(TUICommands.ResetSinglePointIcons), Constants.undoNotAllowed);
                    //Reset ID
                    hom3r.quickLinks.scriptsObject.GetComponent<SinglePointManager>().SetSinglePointGroupID(data.singlePointGroupID);
                    break;

                case TSinglePointCommands.SinglePointMode_Save:
                    //Change SinglePoint Mode status
                    hom3r.quickLinks.scriptsObject.GetComponent<SinglePointManager>().SinglePointMode_statusManager(SinglePointsActions_Type.SaveData);
                    //Prepare and send data
                    hom3r.quickLinks.scriptsObject.GetComponent<SinglePointManager>().SendSinglePointsList();
                    break;

                case TSinglePointCommands.SinglePointMode_Exit:
                    SinglePointsStatus_Type currentSPStatus = m.GetComponent<SinglePointManager>().GetSinglePointMode_Status();
                    int singlepointID = hom3r.quickLinks.scriptsObject.GetComponent<SinglePointManager>().GetSinglePointGroupID();
                    //If  the user has NOT introduce any data in the database we have to delete de singlePoint group field previously created
                    if (currentSPStatus == SinglePointsStatus_Type.EmptyMode || currentSPStatus == SinglePointsStatus_Type.FirstChanges_unsaved)
                    {
                        hom3r.quickLinks.scriptsObject.GetComponent<IOManager>().ToWebApp_DeleteSinglePoint(singlepointID);
                    }
                    else
                    {
                        //Indicate to web-app that the SP mode has finished
                        hom3r.quickLinks.scriptsObject.GetComponent<IOManager>().ToWebApp_EndSinglePoint(singlepointID);
                    }
                    //Delete SinglePoints
                    hom3r.quickLinks.scriptsObject.GetComponent<SinglePointManager>().DeleteSinglePointGroups();
                    //Deselect Object
                    hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().ConfirmALLGameObjectOFF();
                    //ResetValues                                                            
                    hom3r.quickLinks.scriptsObject.GetComponent<SinglePointManager>().SinglePointMode_statusManager(SinglePointsActions_Type.ExitMode);
                    //Change the HOM3R mode
                    hom3r.state.currentMode = THom3rMode.idle;
                    

                    //Change mouse cursor image
                    //hom3r.quickLinks.scriptsObject.GetComponent<MouseKeyboardManager>().ChangeMouseIcon();
                    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Core_ModeChanged));
                    //Mode icons                    
                    hom3r.coreLink.Do(new CUICommand(TUICommands.DisableSinglePointIcons), Constants.undoNotAllowed);
                    break;

                case TSinglePointCommands.SinglePointCaptureRemove:
                    //Move from RemoveSP SinglePointMode type to CaptureSP SinglePointMode type
                    if (m.GetComponent<SinglePointManager>().GetSinglePoint_SubMode() == SinglePointSubmode_Type.RemoveSPMode)
                    {
                        //Change modeType in SinglePoint Mode
                        m.GetComponent<SinglePointManager>().SetSinglePoint_SubMode(SinglePointSubmode_Type.CaptureSPMode);
                        //Change mouse cursor image          
                        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Core_ModeChanged));                  
                        //hom3r.coreLink.Do(new UICoreCommand(TUICommands.ChangeMouseIcon), Constants.undoNotAllowed);
                    }
                    //Move from CaptureSP SinglePointMode type to RemoveSP SinglePointMode type
                    else
                    {
                        //Change modeType in SinglePoint Mode
                        hom3r.quickLinks.scriptsObject.GetComponent<SinglePointManager>().SetSinglePoint_SubMode(SinglePointSubmode_Type.RemoveSPMode);
                        //Change mouse cursor image                            
                        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Core_ModeChanged));
                        //hom3r.coreLink.Do(new UICoreCommand(TUICommands.ChangeMouseIcon), Constants.undoNotAllowed);
                    }
                    break;

                case TSinglePointCommands.SinglePointUndo:
                    m.GetComponent<SinglePointManager>().Undo_SinglePointAction();
                    break;

                case TSinglePointCommands.VisualizeSinglePoint:
                    //Check if the areaID exist
                    GameObject areaGO = m.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(data.listOfSinglePoints.areaID);
                    //Send info to SP class
                    if (areaGO != null)
                    {
                        List<Vector3> positions = new List<Vector3>();
                        foreach (var item in data.listOfSinglePoints.positions)
                        {
                            positions.Add(new Vector3((float)item.x, (float)item.y, (float)(item.z)));
                        }
                        m.GetComponent<SinglePointManager>().ReceiveSinglePoints_ToAdd(data.listOfSinglePoints.singlepointgroupID, areaGO, positions);
                    }
                    break;

                case TSinglePointCommands.RemoveSinglePoint:
                    m.GetComponent<SinglePointManager>().ReceiveSinglePoints_ToHide(data.singlePointGroupID);
                    break;

                case TSinglePointCommands.RemoveAllSinglePoints:
                    m.GetComponent<SinglePointManager>().RemoveAllSinglePoints();
                    break;

                case TSinglePointCommands.EditSinglePointGroup:
                    GameObject areaObj = m.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(data.listOfSinglePoints.areaID);
                    if (areaObj != null)
                    {
                        //1. Activate The singlePoint Mode
                        //this.GetComponent<Core_Script>().Do(new IOCoreCommand(TIOEvent.SinglePointModeStart), Constants.undoNotAllowed);
                        m.GetComponent<Core>().Do(new CSinglePointCommand(TSinglePointCommands.SinglePointMode_Start), Constants.undoNotAllowed);
                        //2. Add new SinglePoints                     
                        List<Vector3> positions = new List<Vector3>();
                        foreach (var item in data.listOfSinglePoints.positions)
                        {
                            positions.Add(new Vector3((float)item.x, (float)item.y, (float)(item.z)));
                        }
                        m.GetComponent<SinglePointManager>().ReceiveSinglePoints_ToEdit(data.listOfSinglePoints.singlepointgroupID, areaObj, positions);
                    }
                    break;

                default:
                    Debug.LogError("Error: This command " + data.command + " is not valid.");
                    break;
            }
        }
        else
        {
            Debug.LogError("Error: Has been called a Single Point command without a valid command");
        }
    }
    public void Undo(MonoBehaviour m)
    {
        throw new System.NotImplementedException();
    }
}
