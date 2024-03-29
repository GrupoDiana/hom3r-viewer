using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEventReceiver : MonoBehaviour {

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
                case TCoreEvent.Core_ModeChanged:
                    hom3r.quickLinks.uiObject.GetComponent<UIManager>().UpdateUIState();
                    break; 
                case TCoreEvent.ConfigurationManager_ConfigurationUpdated:
                    hom3r.quickLinks.uiObject.GetComponent<UIManager>().UpdateUIState();
                    break;
                case TCoreEvent.Occlusion_SmartTransparency_Enabled:                      
                    hom3r.quickLinks.uiObject.GetComponent<UIManager>().SetSmartTransparencyButtonState(true);                
                    break;
                case TCoreEvent.Occlusion_SmartTransparency_Disabled:
                    hom3r.quickLinks.uiObject.GetComponent<UIManager>().SetSmartTransparencyButtonState(false);                                        
                    break;
                case TCoreEvent.Occlusion_SmartTransparency_AlphaLevelUpdated:
                    hom3r.quickLinks.uiObject.GetComponent<UIManager>().SetTransparencySliderValue();
                    break;
                case TCoreEvent.Occlusion_ExplosionGlobalON:                    
                    hom3r.quickLinks.uiObject.GetComponent<UIManager>().DisableExplodelayoutButton(true);
                    break;
                case TCoreEvent.Occlusion_ExplosionGlobalOFF:
                    hom3r.quickLinks.uiObject.GetComponent<UIManager>().DisableExplodelayoutButton(false);                    
                    break;
                case TCoreEvent.Occlusion_ExplosionChangedMode:
                    bool control = (hom3r.state.currentExplosionMode == THom3rExplosionMode.EXPLODE);
                    hom3r.quickLinks.uiObject.GetComponent<UIManager>().EmplodeImplode_ButtonManager(control);
                    break;                
                case TCoreEvent.ModelManagement_ShowMessage:
                    this.GetComponent<Core>().Do(new CUICommand(TUICommands.ShowAlertText, _event.data.text, _event.data.value1), Constants.undoNotAllowed);
                    break;
                case TCoreEvent._3DFileManager_ShowMessage:
                    this.GetComponent<Core>().Do(new CUICommand(TUICommands.ShowAlertText, _event.data.text, _event.data.value1), Constants.undoNotAllowed);
                    break;
                case TCoreEvent.ModelManagement_FileDownloadBegin:
                    hom3r.coreLink.Do(new CUICommand(TUICommands.BlockCanvasRayCasting, true), Constants.undoNotAllowed);    //Block UI canvas                        
                    break;
                case TCoreEvent.ModelManagement_FileDownloadEnd:
                    hom3r.coreLink.Do(new CUICommand(TUICommands.BlockCanvasRayCasting, false), Constants.undoNotAllowed);   //UnBlock UI canvas and selection
                    break;
                case TCoreEvent.ModelManagement_ProductModelLoadSuccess:
                    hom3r.quickLinks.uiObject.GetComponent<UIManager>().ProcessAfterProductModelLoaded();
                    break;
                case TCoreEvent.Selection_IndicationOnFinished:
                    hom3r.coreLink.Do(new CUICommand(TUICommands.ShowComponentName, _event.data.obj, true));                    
                    break;
                case TCoreEvent.Selection_IndicationOffFinished:
                    hom3r.coreLink.Do(new CUICommand(TUICommands.ShowComponentName, null, false), Constants.undoNotAllowed);
                    break;
                case TCoreEvent.Selection_ShowMessage:
                    hom3r.coreLink.Do(new CUICommand(TUICommands.ShowAlertText, _event.data.text, _event.data.value1), Constants.undoNotAllowed);
                    break;
                case TCoreEvent.LabelManager_ShowMessage:
                    hom3r.coreLink.Do(new CUICommand(TUICommands.ShowAlertText, _event.data.text, _event.data.value1), Constants.undoNotAllowed);
                    break;
                //case TCoreEvent.PointOnSurface_PointCaptureBegin:
                    //hom3r.quickLinks.uiObject.GetComponent<UIManager>().UpdateMouseIconPointOnSurface(true);
                //    break;
                //case TCoreEvent.PointOnSurface_PointCaptureEnd:
                    //hom3r.quickLinks.uiObject.GetComponent<UIManager>().UpdateMouseIconPointOnSurface(false);
                //    break;
               
                case TCoreEvent.Navigation_PanNavigation_Enabled:
                    //hom3r.quickLinks.uiObject.GetComponent<UIManager>().SetMouseIconMode(TMouseIconMode.pan_navigation);
                    hom3r.quickLinks.uiObject.GetComponent<UIManager>().UpdateUIState();
                    break;
                case TCoreEvent.Navigation_PanNavigation_Disabled:
                    //hom3r.quickLinks.uiObject.GetComponent<UIManager>().SetMouseIconMode(TMouseIconMode.iddle);
                    hom3r.quickLinks.uiObject.GetComponent<UIManager>().UpdateUIState();
                    break;                
                default:
                    break;
            }
        }
    }
}
