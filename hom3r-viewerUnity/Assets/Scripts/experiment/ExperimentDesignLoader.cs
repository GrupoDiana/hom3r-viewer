using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ExperimentDesignLoader : MonoBehaviour {

    /// <summary>Load Experiment design from an URL</summary>
    /// <param name="url"></param>
    public void LoadExperimentPlot(string url)
    {
        StartCoroutine(CoroutineLoadExperimentDesign(url));
    }


    /// <summary> Coroutine to load experiment design json</summary>
    /// <param name="url">Product model URL</param>    
    IEnumerator CoroutineLoadExperimentDesign(string url)
    {        
        UnityWebRequest fileWWW = UnityWebRequest.Get(url);
        fileWWW.SendWebRequest();
     
        //Show downloaded progress
        while (!fileWWW.isDone)
        {
            SendMessageToUI("Loading Experiment design from DataBase: " + Mathf.Round(fileWWW.downloadProgress * 100.0f).ToString() + "%", 0.0f);
            yield return new WaitForSeconds(0.1f);
        }

        //Has been download correctly?
        if (fileWWW.isNetworkError || fileWWW.isHttpError) {           
            string erroMessage = "Error: Experiment design JSON cannot be download from specified URL: " + fileWWW.error;
            hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().ExperimentDesignLoadError(erroMessage);
        }
        else
        {
            SendMessageToUI("Loading Experiment design from DataBase: 100%");
            CExperimentDesign experimentDesign = JsonUtility.FromJson<CExperimentDesign>(fileWWW.downloadHandler.text);
            //if (exhibitionPlot.type == "single")
            //{
            hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().ExperimentDesignLoadSuccess(experimentDesign);
            //}
            //else if (exhibitionPlot.type == "multiple")
            //{
            //    hom3r.quickLinks.scriptsObject.GetComponent<MultipleExperimentManager>().MultipleExperimentPlotLoadSuccess(exhibitionPlot);
            //}
            //else
            //{
            //    string erroMessage = "Error: Experiment design JSON has a no valid type";
            //    hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().ExperimentPlotLoadError(erroMessage);
            //}

            if ((experimentDesign == null))
            {
                //Send error to App and UI                
                string erroMessage = "Error: Experiment design JSON has bad format";
                hom3r.quickLinks.scriptsObject.GetComponent<ExperimentManager>().ExperimentDesignLoadError(erroMessage);
            }
        }
    }

    /////////////////////
    ////// Others  //////
    /////////////////////

    void SendMessageToUI(string _message)
    {
        SendMessageToUI(_message, 0.0f);
    }
    void SendMessageToUI(string _message, float _time)
    {
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_ShowMessage, _message, _time));
        Debug.Log(_message);
    }
}
