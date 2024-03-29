using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppWebGLManager : MonoBehaviour
{

    private void Start()
    {
        if (hom3r.state.platform == THom3rPlatform.WebGL)
        {
            // UI
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveUI, true));
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActivateUIHierarchyPanel, false));

            // Selection
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveUISelection, true));
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveUIAutomaticSelection, true));

            // Navigation
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveNavigation, true));

            // Label
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveLabelEdition, true));

            // Interaction
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveTouchInteration, false));
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveMouseInteration, true));
        }        
    }
}
