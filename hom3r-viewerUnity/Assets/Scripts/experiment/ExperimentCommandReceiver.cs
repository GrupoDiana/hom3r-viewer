using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentCommandReceiver : MonoBehaviour {

    private void OnEnable()
    {
        hom3r.coreLink.SubscribeCommandObserver(DoExhibitionCommand, UndoExhibitionCommand);  //Subscribe a method to the event delegate
    }

    private void OnDisable()
    {
        hom3r.coreLink.UnsubscribeCommandObserver(DoExhibitionCommand, UndoExhibitionCommand);
    }

    private void DoExhibitionCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CExperimentCommand)) { command.Do(this); }
        else { /* Error - Do nothing */ }
    }

    private void UndoExhibitionCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CExperimentCommand)) { command.Undo(this); }
        else { /* Error - Do nothing */ }
    }
}


/// <summary>Exhibition Command</summary>
public enum TExperimentCommands
{    
    LoadExperimentPlot, StartExperiment,
    NextExperimentPoint, PreviousExperimentPoint,
    SetExperimentQuestionAnswer    
};

/// <summary>Exhibition data</summary>
public class CExperimentCommandData
{
    public TExperimentCommands commandEvent;
    public string url { get; set; }
    public string text { get; set; }
    public int value;

    public CExperimentCommandData(TExperimentCommands _commandEvent) { this.commandEvent = _commandEvent; }
}

/// <summary>A 'ConcreteCommand' class</summary>
public class CExperimentCommand : CCoreCommand
{
    public CExperimentCommandData data;

    //////////////////
    // Constructors //
    //////////////////
    public CExperimentCommand(TExperimentCommands _command)
    {
        data = new CExperimentCommandData(_command);
    }
    public CExperimentCommand(TExperimentCommands _command, string _url)
    {
        data = new CExperimentCommandData(_command);
        data.url = _url;
    }

    public CExperimentCommand(TExperimentCommands _command, string _url, string _text)
    {
        data = new CExperimentCommandData(_command);
        data.url = _url;
        data.text = _text;
    }

    public CExperimentCommand(TExperimentCommands _command, int _value)
    {
        data = new CExperimentCommandData(_command);
        data.value = _value;
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
                case TExperimentCommands.LoadExperimentPlot:
                    //hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().LoadExperimentPlot(data.url, data.text);
                    break;
                case TExperimentCommands.StartExperiment:
                    //hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().StartExperiment();
                    break;
                case TExperimentCommands.NextExperimentPoint:
                    //.quickLinks.scriptsObject.GetComponent<ExperimentManager>().NextExperimentPoint();
                    break;

                case TExperimentCommands.PreviousExperimentPoint:
                    //hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().PreviousExperimentPoint();
                    break;
                case TExperimentCommands.SetExperimentQuestionAnswer:
                    //hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().SetQuestionAnswer(data.value);
                    break;
                default:
                    Debug.LogError("Error: This command " + data.commandEvent + " is not valid.");
                    break;
            }
        }
        else
        {
            Debug.LogError("Error: Has been called a Selecction command without command.");
        }
    }
    public void Undo(MonoBehaviour m)
    {
        throw new System.NotImplementedException();

    }
   
}
