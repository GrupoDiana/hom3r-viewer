using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandaloneManager : MonoBehaviour
{
    GameObject standalonePanelGO;

    private void Awake()
    {
        //standalonePanelGO = GameObject.FindGameObjectWithTag("standalone_panel");
    }

    private void Start()
    {         
        if (hom3r.state.platform != THom3rPlatform.WebGL || hom3r.state.platform == THom3rPlatform.Editor)
        {            
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveUI, true));
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveUISelection, true));
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveUIAutomaticSelection, true));
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveNavigation, true));
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActivateUISettingsPanel, true));

            if (hom3r.state.platform == THom3rPlatform.Android || hom3r.state.platform == THom3rPlatform.IOS)
            {
                hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveTouchInteration, true));
                hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveMouseInteration, false));
            }
            else
            {
                hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveTouchInteration, false));
                hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveMouseInteration, true));
            }
        }        
    }



    public void StartStandaloneManager()
    {    
        if (hom3r.state.platform == THom3rPlatform.Android || hom3r.state.platform == THom3rPlatform.IOS || hom3r.state.platform == THom3rPlatform.Editor || hom3r.state.platform == THom3rPlatform.Windows)
        {
            hom3r.coreLink.Do(new CUICommand(TUICommands.LoadStandalonePanel));                                     // load standanlone panel
            hom3r.coreLink.Do(new CUICommand(TUICommands.BlockCanvasRayCasting, false), Constants.undoNotAllowed);   //UnBlock UI canvas and selection
        }
        //else
        //{
        //    SetActivatedStandAlonePanel(false);
        //}
     }

    public void LoadOBJFile(string fileName, string fileUrl)
    {

        C3DFileUrlIO tempList = new C3DFileUrlIO();
        tempList.data = new List<CIO3DFileData>();
        string filesPath = Application.dataPath.Substring(0, Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/")).LastIndexOf("/"));
        string basepath = "file:///" + filesPath;

        // OBJ
        CIO3DFileData item1 = new CIO3DFileData();
        item1.fileName = fileName;
        item1.fileURL = fileUrl;
        item1.fileType = "obj";
        tempList.data.Add(item1);

        // MTL
        CIO3DFileData item2 = new CIO3DFileData();
        item2.fileName = fileName.Substring(0, fileName.Length - 3) + "mtl";
        item2.fileURL = fileUrl.Substring(0, fileUrl.Length - 3) + "mtl";
        item2.fileType = "mtl";
        tempList.data.Add(item2);

        //// Tetris
        //IOLoad3DFile_editor_help(tempList, "iter_centralsolenoid.mtl", basepath + "/experiment/navigation/scenarios/models/iter_centralsolenoid.mtl", "mtl");
        //IOLoad3DFile_editor_help(tempList, "iter_centralsolenoid.obj", basepath + "/experiment/navigation/scenarios/models/iter_centralsolenoid.obj", "obj");

        string json_string = JsonUtility.ToJson(tempList);        
        hom3r.quickLinks.scriptsObject.GetComponent<IOManager>().IOLoad3DFiles(json_string);
    }

    public void LoadModelFromUI(string productModel)
    {
        hom3r.coreLink.Do(new CUICommand(TUICommands.BlockCanvasRayCasting, true), Constants.undoNotAllowed);    //Block UI canvas 
        hom3r.coreLink.Do(new CUICommand(TUICommands.ActivatedStandAlonePanel, false), Constants.undoNotAllowed);      
        string explosionModel;      
        explosionModel = "";        
        LoadModel(productModel, explosionModel);
        Debug.Log(productModel);
    }
    public void LoadModelFromUI(string productModel, string explosionModel)
    {
        hom3r.coreLink.Do(new CUICommand(TUICommands.BlockCanvasRayCasting, true), Constants.undoNotAllowed);    //Block UI canvas 
        hom3r.coreLink.Do(new CUICommand(TUICommands.ActivatedStandAlonePanel, false), Constants.undoNotAllowed);
        //string explosionModel;
        //explosionModel = "";
        LoadModel(productModel, explosionModel);
        Debug.Log(productModel);
    }

    private void LoadModel(string productModel, string explosionModel)
    {
        CIOProductModelUrls temp = new CIOProductModelUrls();
        temp.productModelURL = productModel;
        temp.explosionModelURL = explosionModel;
        string json_string = JsonUtility.ToJson(temp);

        hom3r.quickLinks.scriptsObject.GetComponent<IOManager>().IOLoadProductModel(json_string);
    }

    public void LoadExperimentCanvas()
    {

    }
 
}
