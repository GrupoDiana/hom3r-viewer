using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentEventReceiver : MonoBehaviour {

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
                case TCoreEvent.ModelManagement_FileDownloadBegin:
                    
                    break;
                case TCoreEvent.ModelManagement_FileDownloadEnd:
                    
                    break;
                                
                case TCoreEvent.ModelManagement_3DLoadSuccess:
                    //hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().ApplyExperimentPointActions();
                    hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().AfterLoad3DModel();
                    break;

                case TCoreEvent.Selection_ConfirmationOnFinished:
                    //Error, has been clicked wrong area
                    //hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().ResetExperiment();                    
                    break;
                case TCoreEvent.ObjectState_AreaConfirmationOff:
                    //string areaID = _event.data.obj.GetComponent<ObjectStateManager>().areaID;
                    //hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().NextExperimentPoint(areaID);
                    break;
                case TCoreEvent.Selection_ConfirmationOffFinished:                    
                    //hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().NextExperimentPoint(_event.data.textList);                    
                    break;
                case TCoreEvent.MouseManager_RightButtonUp:
                    hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().RegisterStopDragGesture();
                    hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().RegisterStartMovementGesture();
                    break;
                case TCoreEvent.MouseManager_RightButtonDown:
                    hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().RegisterStartDragGesture();
                    hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().RegisterStopMovementGesture();
                    break;
                case TCoreEvent.Navigation_PseudoLatitudeMovement:
                    hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().RegisterDragGestureLatitude(_event.data.value1);
                    break;
                case TCoreEvent.Navigation_PseudoLongitudeMovement:
                    hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().RegisterDragGestureLongitude(_event.data.value1);
                    break;
                case TCoreEvent.Navigation_PseudoRadioMovement:
                    hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().RegisterMovementGestureRadio(_event.data.value1);
                    break;
                //case TCoreEvent.TouchManager_DragMovementBegin:
                //    hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().RegisterStartDragGesture();
                //    break;
                //case TCoreEvent.TouchManager_DragMovementEnd:
                //    hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().RegisterStopDragGesture();
                //    break;
                //case TCoreEvent.TouchManager_PinchZoomBegin:
                //    hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().RegisterStartMovementGesture();
                //    break;
                //case TCoreEvent.TouchManager_PinchZoomEnd:
                //    hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().RegisterStopMovementGesture();
                //    break;
                default:
                    break;
            }
        }
    }
}
