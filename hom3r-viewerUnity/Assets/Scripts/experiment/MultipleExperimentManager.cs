using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleExperimentManager : MonoBehaviour {

    CPlot multipleExperimentPlot;       // Store the exhibition Plot
    bool experimentInitiated;           // 
    int currentExperimentPointID;       // 

    private void Awake()
    {
        multipleExperimentPlot = null;        
        experimentInitiated = false;
        currentExperimentPointID = -1;
    }

    /////////////////////////////////////////////////
    ////// Manage Multi Exhibition Plot Methods /////
    /////////////////////////////////////////////////
    public void StartMultiExperiment()
    {
        if (multipleExperimentPlot != null)
        {
            experimentInitiated = true;
            currentExperimentPointID = -1;
            NextPlotPoint();
        }
        else
        {
            //TODO Reset exhibition? Ignore?
        }
    }

    /// <summary>Update the 3D Scene to the next exhibition point</summary>
    public void NextPlotPoint()
    {
        if (experimentInitiated)
        {
            int nextExhibitionPointID = currentExperimentPointID + 1;
            if (nextExhibitionPointID < multipleExperimentPlot.plotPointList.Count)
            {
                this.GoToExperimentPoint(nextExhibitionPointID);
            }
            else
            {
                Debug.Log("Experiment Session Finished");
                hom3r.coreLink.Do(new CUICommand(TUICommands.ShowAlertText, "Experiment Session Finished"));
                hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().StopSession();
                this.GetComponent<IOManager>().IOResetProductModel();
            }
        }
    }

    /// <summary>Update the 3D scene to next exhibition point following which is indicated into de exhibition plot</summary>
    /// <param name="nextExperimentPointID"></param>
    private void GoToExperimentPoint(int nextExperimentPointID)
    {
        currentExperimentPointID = nextExperimentPointID;        
        //hom3r.coreLink.Do(new CExperimentCommand(TExperimentCommands.LoadExperimentPlot, multipleExperimentPlot.plotPointList[currentExperimentPointID].plotUrl, multipleExperimentPlot.description));
        hom3r.quickLinks.scriptsObject.GetComponent<ExperimentDesignLoader>().LoadExperimentPlot(multipleExperimentPlot.plotPointList[currentExperimentPointID].plotUrl);
    }

    public string GetMultiExperimentTitle()
    {
        return multipleExperimentPlot.title;
    }

    public string GetMultiExperimentDescription()
    {
        return multipleExperimentPlot.description;
    }
    //////////////////////////////////////////
    ////// LOAD Exhibition Plot Methods //////
    //////////////////////////////////////////


    /// <summary>Store Multiple exhibition plot</summary>
    /// <param name="_exhibitionPlot">Multi Exhibition plot class</param>
    public void MultipleExperimentPlotLoadSuccess(CPlot _exhibitionPlot)
    {
        this.multipleExperimentPlot = _exhibitionPlot;
        //hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ExhibitionManager_ExhibitionPlotLoadSuccess));

        this.StartMultiExperiment();
    }

    public int GetTotalOfMultipleExperimentPoints() { return multipleExperimentPlot.plotPointList.Count; }
    public int GetCurrentMultipleExperimentPoint() { return currentExperimentPointID;  }
}
