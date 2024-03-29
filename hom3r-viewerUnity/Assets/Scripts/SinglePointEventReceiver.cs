using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePointEventReceiver : MonoBehaviour {

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
                case TCoreEvent.MouseManager_LeftButtonUp:
                    if (hom3r.state.currentMode == THom3rMode.capturing_surface_point)
                    {
                        if (hom3r.quickLinks.scriptsObject.GetComponent<SinglePointManager>().GetSinglePoint_SubMode() == SinglePointSubmode_Type.CaptureSPMode && !hom3r.state.captureSinglePointBlocked)
                        {
                            //Capture Single Points
                            hom3r.quickLinks.scriptsObject.GetComponent<SinglePointManager>().CaptureSinglePoint(_event.data.mousePosition);
                        }
                        else if (hom3r.quickLinks.scriptsObject.GetComponent<SinglePointManager>().GetSinglePoint_SubMode() == SinglePointSubmode_Type.RemoveSPMode && !hom3r.state.captureSinglePointBlocked)
                        {
                            //Delete Single Points
                            hom3r.quickLinks.scriptsObject.GetComponent<SinglePointManager>().DeleteSinglePoint(_event.data.mousePosition);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
