using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleExperimentCommandReceiver : MonoBehaviour {

    private void OnEnable()
    {
        hom3r.coreLink.SubscribeCommandObserver(DoMultiExperimentCommand, UndoMultiExperimentCommand);  //Subscribe a method to the event delegate
    }

    private void OnDisable()
    {
        hom3r.coreLink.UnsubscribeCommandObserver(DoMultiExperimentCommand, UndoMultiExperimentCommand);
    }

    private void DoMultiExperimentCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CMultipleExperimentCommand)) { command.Do(this); }
        else { /* Error - Do nothing */ }
    }

    private void UndoMultiExperimentCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CMultipleExperimentCommand)) { command.Undo(this); }
        else { /* Error - Do nothing */ }
    }
}

/// <summary>Exhibition Command</summary>
public enum TMultipleExperimentCommands
{
    nextMultipleExperimentPoint
};

/// <summary>Exhibition data</summary>
public class CMultipleExperimentCommandData
{
    public TMultipleExperimentCommands commandEvent;       

    public CMultipleExperimentCommandData(TMultipleExperimentCommands _commandEvent) { this.commandEvent = _commandEvent; }
}

/// <summary>A 'ConcreteCommand' class</summary>
public class CMultipleExperimentCommand : CCoreCommand
{
    public CMultipleExperimentCommandData data;

    //////////////////
    // Constructors //
    //////////////////
    public CMultipleExperimentCommand(TMultipleExperimentCommands _command)
    {
        data = new CMultipleExperimentCommandData(_command);
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
                case TMultipleExperimentCommands.nextMultipleExperimentPoint:
                    hom3r.quickLinks.scriptsObject.GetComponent<MultipleExperimentManager>().NextPlotPoint();
                    break;
               
                default:
                    Debug.LogError("Error: This command " + data.commandEvent + " is not valid.");
                    break;
            }
        }
        else
        {
            Debug.LogError("Error: Has been called a MultipleExperiment command without command.");
        }
    }
    public void Undo(MonoBehaviour m)
    {
        throw new System.NotImplementedException();

    }

}
