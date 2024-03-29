using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICommandReceiver : MonoBehaviour {

    private void OnEnable()
    {        
        hom3r.coreLink.SubscribeCommandObserver(DoUICommand, UndoUICommand);    //Subscribe a method to the event delegate
    }

    private void OnDisable()
    {        
        hom3r.coreLink.UnsubscribeCommandObserver(DoUICommand, UndoUICommand);  //Unsubscribe a method to the event delegate 
    }

    private void DoUICommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CUICommand)) { command.Do(this); }
        else { /* Do nothing */ }
    }

    private void UndoUICommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CUICommand)) { command.Undo(this); }
        else { /*  Do nothing */ }
    }    
}


/// <summary>UI Commands</summary>
public enum TUICommands
{    
    HomeButtonPressed, UpdateCoordinateSystemIndicator,       
    EnableSinglePointIcons, DisableSinglePointIcons, ResetSinglePointIcons,   
    BlockCanvasRayCasting,
    ShowAlertText, ShowComponentName,
    LoadStandalonePanel, ActivatedStandAlonePanel
};

/// <summary>UI data</summary>
public class CUICommandData
{
    public TUICommands commandEvent;

    public string message { get; set; }
    public float timeShown { get; set; }
    public bool show { get; set; }
    public bool block { get; set; }
    //public NavigationManager.TCoordinateSystem coordinateSystem { get; set; }       //FIXME after Navigation Refactoring
    public GameObject obj { get; set; }
    public float alphaValue { get; set; }

    public CUICommandData(TUICommands _commandEvent) { this.commandEvent = _commandEvent; }
}

/// <summary>A 'ConcreteCommand' class</summary>
public class CUICommand : CCoreCommand
{
    public CUICommandData UIData;

    //////////////////
    // Constructors //
    //////////////////
    public CUICommand(TUICommands _command)
    {
        UIData = new CUICommandData(_command);
    }
    public CUICommand(TUICommands _command, float _alphaValue)
    {
        UIData = new CUICommandData(_command);
        UIData.alphaValue = _alphaValue;
    }
    public CUICommand(TUICommands _command, string _message, float _timeShown = 5.0f)
    {
        UIData = new CUICommandData(_command);
        UIData.message = _message;
        UIData.timeShown = _timeShown;
    }
    //FIXME after Navigation Refactoring
    //public UICoreCommand(TUICommands _command, NavigationManager.TCoordinateSystem _coordinateSystem)
    //{
    //    UIData = new CUICommandData(_command);
    //    UIData.coordinateSystem = _coordinateSystem;
    //}
    public CUICommand(TUICommands _command, GameObject _object, bool show)
    {
        UIData = new CUICommandData(_command);
        UIData.obj = _object;
        UIData.show = show;
    }
    public CUICommand(TUICommands _command, bool block)
    {
        UIData = new CUICommandData(_command);
        UIData.show = block;
    }
    
    //////////////////
    //   Execute    //
    //////////////////
    public void Do(MonoBehaviour m)
    {
        if (UIData != null)
        {
            switch (UIData.commandEvent)
            {
                //----------------------------------------------------------------------------------
                // NAVIGATION PANEL
                //----------------------------------------------------------------------------------
                case TUICommands.HomeButtonPressed:
                    if ((hom3r.state.currentIsolateMode == THom3rIsolationMode.ISOLATE || hom3r.state.currentIsolateMode == THom3rIsolationMode.WITH_REMOVEDNODES)/* && (this.GetComponent<Remove_Script>().GetRemovedList().Count != 0)*/)
                    {
                        m.GetComponent<IsolateManager>().ReFocusIsolatedGO_withRotation(); //FIXME UI cannot call Isolate method directly, have to use a command
                    }
                    else
                    {
                        //FIXME after Navigation Refactoring
                        //hom3r.quickLinks.orbitPlane.GetComponent<NavigationManager>().ResetView_BothNavSystems();//FIXME UI cannot call Navigation method directly, have to use a command
                    }
                    break;
                case TUICommands.UpdateCoordinateSystemIndicator:
                    //Set Coordinate System in the UI
                    //FIXME after Navigation Refactoring
                    //hom3r.quickLinks.uiObject.GetComponent<UIManager>().ChangeCoordinatesButtonImage(UIData.coordinateSystem);
                    break;                

                case TUICommands.EnableSinglePointIcons:

                    hom3r.quickLinks.uiObject.GetComponent<UIManager>().SP_ShowIcons(true);

                    break;
                case TUICommands.DisableSinglePointIcons:

                    hom3r.quickLinks.uiObject.GetComponent<UIManager>().SP_ShowIcons(false);

                    break;
                case TUICommands.ResetSinglePointIcons:

                    hom3r.quickLinks.uiObject.GetComponent<UIManager>().Reset_SPMode_icons();

                    break;


                case TUICommands.BlockCanvasRayCasting:

                    hom3r.quickLinks.uiObject.GetComponent<UIManager>().BlockCanvasRayCasting(UIData.block);

                    break;
                //-----------------------------------------------------------------------------------
                // MOUSE
                //----------------------------------------------------------------------------------
                /*case TUICommands.ChangeMouseIcon:

                    hom3r.quickLinks.scriptsObject.GetComponent<MouseKeyboardManager>().ChangeMouseIcon();

                    break;*/

                //----------------------------------------------------------------------------------
                // UI Message
                //----------------------------------------------------------------------------------
                case TUICommands.ShowAlertText:

                    hom3r.quickLinks.uiObject.GetComponent<UIManager>().SetActiveAlertText(UIData.message, UIData.timeShown);

                    break;
                case TUICommands.ShowComponentName:

                    hom3r.quickLinks.uiObject.GetComponent<UIManager>().ManageIndicatedGOnameTextBox(UIData.obj, UIData.show);

                    break;
                case TUICommands.LoadStandalonePanel:
                    hom3r.quickLinks.uiObject.GetComponent<UIManager>().LoadStandalonePanel();
                    break;
                case TUICommands.ActivatedStandAlonePanel:
                    hom3r.quickLinks.uiObject.GetComponent<UIStandaloneManager>().SetActivatedStandAlonePanel(UIData.show);
                    break;
                default:
                    Debug.LogError("Error: This command " + UIData.commandEvent + " is not valid.");
                    break;
            }
        }
        else
        {
            Debug.LogError("Error: Has been called a UI command without command.");
        }
    }
    public void Undo(MonoBehaviour b)
    {
        throw new System.NotImplementedException();

    }
}
