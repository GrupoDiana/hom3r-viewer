using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IOCommandReceiver : MonoBehaviour {

    private void OnEnable()
    {
        hom3r.coreLink.SubscribeCommandObserver(DoIOCommand, UndoIOCommand);    //Subscribe a method to the event delegate
    }

    private void OnDisable()
    {
        hom3r.coreLink.UnsubscribeCommandObserver(DoIOCommand, UndoIOCommand);  //Unsubscribe a method to the event delegate        
    }

    private void DoIOCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CIOCommand)) { command.Do(this); }
        else { /* Do nothing */ }
    }

    private void UndoIOCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CIOCommand)) { command.Undo(this); }
        else { /* Do nothing */ }
    }
}


/// <summary>IO commands</summary>
public enum TIOCommands
{
    SendHom3rStateToWebApp,
    SendProductLoadedResultToWebApp, SendProductModelEditionResultToWebApp,
    SendLabelRemovedToWebApp,
    SendSinglePointToWebApp,
    ReportError,
    DebugConsoleWeb
};

/// <summary>IO data</summary>
public class CIOCommandData
{
    public TIOCommands commandEvent;

    public bool status { get; set; }
    public string text { get; set; }
    public CSinglePointIO listOfSinglePoints { get; set; }

    public CIOCommandData(TIOCommands _commandEvent)
    {
        this.commandEvent = _commandEvent;
    }
}

/// <summary>A 'ConcreteCommand' class</summary>
public class CIOCommand : CCoreCommand
{
    public CIOCommandData data;
    //////////////////
    // Constructors //
    //////////////////
    public CIOCommand(TIOCommands _command)
    {
        data = new CIOCommandData(_command);
    }

    public CIOCommand(TIOCommands _command, string _labelID)
    {
        data = new CIOCommandData(_command);

        data.text = _labelID;
    }

    public CIOCommand(TIOCommands _command, bool _status)
    {
        data = new CIOCommandData(_command);
        data.status = _status;
    }

    public CIOCommand(TIOCommands _command, CSinglePointIO _listOfSinglePoints)
    {
        data = new CIOCommandData(_command);
        data.listOfSinglePoints = _listOfSinglePoints;
    }
    //////////////////
    //   Execute    //
    //////////////////
    public void Do(MonoBehaviour m)
    {
        if (data != null)
        {
            switch (data.commandEvent)
            {

                case TIOCommands.SendHom3rStateToWebApp:                    
                    hom3r.coreLink.GetComponent<IOManager>().ToWebApp_Hom3rState(data.status);     //Send result to IO
                    break;

                case TIOCommands.SendProductLoadedResultToWebApp:                            
                    hom3r.coreLink.GetComponent<IOManager>().ToWebApp_ProductLoadedResult(data.status); // Send result to IO
                    break;
                case TIOCommands.SendProductModelEditionResultToWebApp:
                    hom3r.coreLink.GetComponent<IOManager>().ToWebApp_ProductModelEditionResult(data.status); // Send result to IO
                    break;

                case TIOCommands.SendLabelRemovedToWebApp:
                    hom3r.coreLink.GetComponent<IOManager>().ToWebApp_RemoveLabel(data.text);
                    break;

                case TIOCommands.SendSinglePointToWebApp:
                    //hom3r.coreLink.GetComponent<IOManager>().ToWebApp_NewSinglePoint(data.listOfSinglePoints);
                    break;

                case TIOCommands.DebugConsoleWeb:
                    hom3r.coreLink.GetComponent<IOManager>().ToWebApp_ConsoleWeb(data.text);
                    break;

                case TIOCommands.ReportError:
                    hom3r.coreLink.GetComponent<IOManager>().ToWebApp_ReportError(data.text);
                    break;

                default:
                    Debug.LogError("Error: This command " + data.commandEvent + " is not valid.");
                    break;
            }
        }
        else
        {
            Debug.LogError("Error: Has been called a IO command without command.");
        }


    }

    public void Undo(MonoBehaviour b)
    {
        throw new System.NotImplementedException();
    }
}
