using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandaloneEventReceiver : MonoBehaviour
{
    private void OnEnable()
    {
        hom3r.coreLink.SubscribeEventObserver(DoInternalEventCommand);      //Subscribe a method to the event delegate
    }

    private void OnDisable()
    {
        hom3r.coreLink.UnsubscribeEventObserver(DoInternalEventCommand);    //Unsubscribe a method to the event delegate
    }

    private void DoInternalEventCommand(CCoreEvent _event)
    {
        if (_event.GetType() == typeof(CCoreEvent)) { ExecuteCoreEvents(_event); }
        else { /* Do nothing */ }
    }

    /// <summary>Execute internal events commands</summary>
    /// <param name="_event">command to be executed</param>
    private void ExecuteCoreEvents(CCoreEvent _event)
    {
       
        if (_event.data != null)
        {
            switch (_event.data.commandEvent)
            {
                case TCoreEvent.Core_Hom3rReadyToStart:
                    hom3r.quickLinks.scriptsObject.GetComponent<StandaloneManager>().StartStandaloneManager();
                    break;                
                default:
                    break;
            }
        }
                
    }
}
