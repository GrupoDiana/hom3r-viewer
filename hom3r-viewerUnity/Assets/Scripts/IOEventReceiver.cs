using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IOEventReceiver : MonoBehaviour {

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
                    hom3r.coreLink.Do(new CIOCommand(TIOCommands.SendHom3rStateToWebApp, true));    //Send Message to WebApp Hom3rReady                        
                    break;

                case TCoreEvent.Selection_ConfirmationOnFinished:
                    this.GetComponent<IOManager>().ToWebApp_SelectedPart(_event.data.textList);
                    break;

                case TCoreEvent.Selection_ConfirmationOffFinished:
                    this.GetComponent<IOManager>().ToWebApp_DeselectPart(_event.data.textList);                    
                    break;

                case TCoreEvent.RemovedPart_Activate:
                    this.GetComponent<IOManager>().ToWebApp_RemovedPartActivated(_event.data.textList);
                    break;
                case TCoreEvent.RemovedPart_Deactivated:
                    this.GetComponent<IOManager>().ToWebApp_RemovedPartDeactivated(_event.data.textList);
                    break;
                case TCoreEvent._3DFileManager_ShowMessageConsole:
                    hom3r.coreLink.Do(new CIOCommand(TIOCommands.DebugConsoleWeb, _event.data.text), Constants.undoNotAllowed);
                    break;

                case TCoreEvent.ModelManagement_ProductModelLoadSuccess:
                    //hom3r.coreLink.Do(new CIOCommand(TIOCommands.SendProductLoadedResultToWebApp, true));
                    hom3r.coreLink.GetComponent<IOManager>().ToWebApp_ProductLoadedResult(true);
                    break;

                case TCoreEvent.ModelManagement_ModelLoadError:
                    //hom3r.coreLink.Do(new CIOCommand(TIOCommands.SendProductLoadedResultToWebApp, false));
                    //hom3r.coreLink.Do(new CIOCommand(TIOCommands.ReportError, _event.data.text));
                    hom3r.coreLink.GetComponent<IOManager>().ToWebApp_ProductLoadedResult(false);                    
                    hom3r.coreLink.GetComponent<IOManager>().ToWebApp_ReportError(_event.data.text);
                    break;
                
                case TCoreEvent.ModelManagement_ProductModelEditOk:
                    hom3r.coreLink.Do(new CIOCommand(TIOCommands.SendProductModelEditionResultToWebApp, true));
                    break;

                case TCoreEvent.ModelManagement_ProductModelEditError:
                    hom3r.coreLink.Do(new CIOCommand(TIOCommands.SendProductModelEditionResultToWebApp, false));
                    hom3r.coreLink.Do(new CIOCommand(TIOCommands.ReportError, _event.data.text));
                    break;

                
                case TCoreEvent.ModelManagement_ModelReset_Success:
                    //hom3r.coreLink.Do(new CIOCommand(TIOCommands.SendProductLoadedResultToWebApp, false));
                    //TODO send to WEBAPP that there are no model loaded
                    break;
                case TCoreEvent.ModelManagement_3DListLoadSuccess:
                    hom3r.coreLink.GetComponent<IOManager>().ToWebApp_3DModelLoadResult(true);
                    break;
                case TCoreEvent.LabelManager_LabelRemoved:
                    hom3r.coreLink.Do(new CIOCommand(TIOCommands.SendLabelRemovedToWebApp, _event.data.text));
                    break;

                case TCoreEvent.LabelManager_LabelDataUpdated:
                    hom3r.coreLink.GetComponent<IOManager>().ToWebApp_LabelUpdatedData(_event.data.labelId, _event.data.areaId , _event.data.labelType, _event.data.boardPosition, _event.data.boardRotation, _event.data.anchorPosition, _event.data.scaleFactor);
                    break;

                case TCoreEvent.ModelManagement_3DLoadError:
                    hom3r.coreLink.GetComponent<IOManager>().ToWebApp_ReportError(_event.data.text);
                    break;
                case TCoreEvent.Occlusion_SmartTransparency_Enabled:
                    if (_event.data.origin == THom3rCommandOrigin.ui) {
                        hom3r.coreLink.GetComponent<IOManager>().ToWebApp_SmartTranspacencyState(true);
                    }
                    break;
                case TCoreEvent.Occlusion_SmartTransparency_Disabled:
                    if (_event.data.origin == THom3rCommandOrigin.ui) {
                        hom3r.coreLink.GetComponent<IOManager>().ToWebApp_SmartTranspacencyState(false);
                    }
                    break;
                case TCoreEvent.UI_SmartTransparency_AlphaLevelUpdated:
                        hom3r.coreLink.GetComponent<IOManager>().ToWebApp_UpdateTranspacencyAlphaLevel(_event.data.value1);
                    break;
                default:
                    break;
            }
        }
    }
}
