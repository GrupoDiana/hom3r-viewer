using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/////////////////////////////////////////////////////////////////////////////////
//                              IO AUXILIAR CLASSES
/////////////////////////////////////////////////////////////////////////////////

//////////////////
//Load Product
//////////////////
[System.Serializable]
public class CIOProductModelUrls
{
    public string productModelURL;
    public string explosionModelURL;    
}

//////////////////
// Load 3D File
//////////////////
[System.Serializable]
public class C3DFileUrlIO
{
    public List<CIO3DFileData> data;

    public C3DFileUrlIO() { data = new List<CIO3DFileData>(); }
    public C3DFileUrlIO(List<CIO3DFileData> _data) { this.data = _data; }
}

///////////////////////
// Selection
///////////////////////
[System.Serializable]
public class CAreasIO
{
    public List<CAreasIOData> data;

    public CAreasIO() { data = new List<CAreasIOData>(); }
    public CAreasIO(List<CAreasIOData> _data){this.data = _data;}
}

[System.Serializable]
public class CAreasIOData
{
    public string id;
    public string colour;

    public CAreasIOData() { }
    public CAreasIOData(string _id, string _colour)
    {
        this.id = _id;
        this.colour = _colour;
    }
}

//Single Point
[System.Serializable]
public class CSinglePointCaptureIO
{
    public int singlepointgroupID;
    public string areaID;
}
//Single Point
[System.Serializable]
public class CSinglePointIO
{
    public int singlepointgroupID;
    public string areaID;
    public List<CIOPositionVector> positions;

    public CSinglePointIO() {positions = new List<CIOPositionVector>();}

    public CSinglePointIO(int _singlepointgroupID, string _areaID, List<CIOPositionVector> _positions)
    {
        this.singlepointgroupID = _singlepointgroupID;
        this.areaID = _areaID;
        this.positions = _positions;
    }
}

//Single Point, Labels
[System.Serializable]
public class CIOPositionVector
{
    public double x;
    public double y;
    public double z;

    public CIOPositionVector() { }
    public CIOPositionVector(double _x, double _y, double _z)
    {
        this.x = _x;
        this.y = _y;
        this.z = _z;
    }
}

[System.Serializable]
public class CIORotationVector: CIOPositionVector
{
    public double w;

    public CIORotationVector() { }
    public CIORotationVector(double _x, double _y, double _z, double _w)
    {
        this.x = _x;
        this.y = _y;
        this.z = _z;
        this.w = _w;
    }
}

///////////////
// Labels
///////////////
[System.Serializable]
public class CIOLabelList
{
    public List<CIOLabelData> data;

    public CIOLabelList() { data = new List<CIOLabelData>(); }
    public CIOLabelList(List<CIOLabelData> _data) { this.data = _data; }
}

[System.Serializable]
public class CIOLabelData
{
    public string id;                           // Always
    public string type;                         // Always
    public string areaId;                       // Only anchored label
    public string text;                         // Always
    public CIOPositionVector position;          // Always
    public CIORotationVector rotation;          // Only board label
    public CIOPositionVector anchorPosition;    // Only anchored label
    public double scaleFactor;                  // Always

    public CIOLabelData() {
        
        position = new CIOPositionVector();
        rotation = new CIORotationVector();
        anchorPosition = new CIOPositionVector();
    }        
}

[System.Serializable]
public class CIOBoardLabelDataSend
{
    public string id;                           // Always
    public string type;                         // Always        
    public CIOPositionVector position;          // Always
    public CIORotationVector rotation;          // Only board label    
    public double scaleFactor;                  // Always

    public CIOBoardLabelDataSend()
    {
        position = new CIOPositionVector();
        rotation = new CIORotationVector();        
    }
}

[System.Serializable]
public class CIOAnchoredLabelDataSend
{
    public string id;                           // Always
    public string type;                         // Always        
    public string areaId;                       // Only anchored label
    public CIOPositionVector position;          // Always
    public CIOPositionVector anchorPosition;    // Only anchored label
    public double scaleFactor;                  // Always

    public CIOAnchoredLabelDataSend()
    {
        position = new CIOPositionVector();
        anchorPosition = new CIOPositionVector();
    }
}

///////////////
// Remove
///////////////
[System.Serializable]
public class CRemoveAreasIO
{
    public List<CRemoveAreasIOData> data;

    public CRemoveAreasIO() { data = new List<CRemoveAreasIOData>(); }
    public CRemoveAreasIO(List<CRemoveAreasIOData> _data) { this.data = _data; }
}

[System.Serializable]
public class CRemoveAreasIOData
{
    public string id;    

    public CRemoveAreasIOData() { }
    public CRemoveAreasIOData(string _id) { this.id = _id; }
}


//////////////////
// CONFIGURATION
//////////////////
[System.Serializable]
public class CIOConfigurationData
{
    public string parameter;
    public bool value;

    public CIOConfigurationData() { }    
}

//////////////////
// Transparency
//////////////////
[System.Serializable]
public class CIOSmartTransparencyData
{
    public bool enabled;
    public float alphaValue;
}


///////////////////////////
//  IO CLASS
///////////////////////////
public class IOManager : MonoBehaviour {

#if !UNITY_EDITOR && UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void SendToApp(string id, string message, string value);     // Define in Plug-ins/hom3rAPI.jslib
    [DllImport("__Internal")]
    private static extern void SendToConsole(string message);               // Define in Plug-ins/hom3rAPI.jslib
#else
    private static void SendToApp(string id, string message, string value)
    {
        Debug.Log("Sending to WebAPP: " + id + ", "+  message + ", " + value);
    }

    private static void SendToConsole(string message)
    {
        Debug.Log("Sending to WebConsole: " + message);
    }
#endif


    private string instanceId;

    private void Awake()
    {
        instanceId = "";
    }

    //////////////////////////
    // From WebAPP to Hom3r //
    //////////////////////////
    public void IOConfiguration(string json_string) {

        if (Application.isEditor)
        {
            
            //json_string = @"{""parameter"": ""UIEnabled"", ""value"": false}";
            json_string = @"{""parameter"": ""UISelectionEnabled"", ""value"": false}";
            //json_string = @"{""parameter"": ""LabelEditionEnabled"", ""value"": false}";
        }

        CIOConfigurationData dataReceived = JsonUtility.FromJson<CIOConfigurationData>(json_string);
        Debug.Log(dataReceived.parameter);

        if (dataReceived.parameter == "UIEnabled") {            
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveUI, dataReceived.value));
        }
        else if (dataReceived.parameter == "UIHierarchyPanelEnabled")
        {
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActivateUIHierarchyPanel, dataReceived.value));
        }
        else if (dataReceived.parameter == "UIOcclusionPanelEnabled")
        {
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActivateUIOcclusionPanel, dataReceived.value));
        }
        else if (dataReceived.parameter == "UISelectionEnabled")
        {
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveUISelection, dataReceived.value));

        } else if (dataReceived.parameter == "LabelEditionEnabled") {            
            hom3r.coreLink.Do(new CConfigurationCommand(TConfigurationCommands.ActiveLabelEdition, dataReceived.value));
        }

    }
    
    public void IOSetID(string id)
    {
        this.instanceId = id;
    }

    public void IOLoadProductModel(string json_string)
    {
        //if (Application.isEditor)
        //{
        //    //AssetBundle
        //    //string productModel = "ProductModelHom3r.json";
        //    //string explosionModel = "turbineexplosion.xml";
        //    //string productModel     = "saturn5/saturn5.json";            
        //    //string productModel = "saturn5/saturn5_horizontal.json";
        //    //string explosionModel = "saturn5/saturn5_Explosion.xml";
        //    //string productModel = "parthenon/parthenon.json";
        //    //string explosionModel = "";            
        //    //string productModel = "http://150.214.59.100:8890/developmentModels/saturn5/saturn5.json";
        //    //string explosionModel = "http://150.214.59.100:8890/developmentModels/saturn5/saturn5_Explosion.xml";
        //    //string productModel = "silversmithing/Pistol.json";
        //    //string explosionModel = "";
        //    //string productModel = "silversmithing/cutlery.json";
        //    //string explosionModel = "";
        //    //string productModel = "rock1/rock0.json";
        //    //string productModel = "http://150.214.59.100:8890/developmentModels/rock1/rock0.json";
        //    //string explosionModel = "";

        //    //OBJ            
        //    string productModel = "nav.ellipsoidal/caja_type_VI.json";
        //    //string productModel = "nav.ellipsoidal/ellipsoid_type_I.json";
        //    //string productModel = "nav.ellipsoidal/pencil.json";
        //    string explosionModel = "";
        //    //string productModel = "saturn5OBJ/saturn5.json";
        //    //string explosionModel = "";
        //    //string productModel = "parthenon/obj/parthenonOBJ.json";
        //    //string explosionModel = "";
        //    //string productModel = "rock1/rock0.json";            
        //    //string explosionModel = "";
        //    //string productModel = "http://150.214.59.100:8890/developmentModels/rock1/Rock1.json";
        //    //string productModel = "esfera_dodecaedro/esfera_dodecaedro2.json";
        //    //string explosionModel = "";
        //    //string productModel = "https://develop.pluggy.eu/api/v1/assets/5c484d47561cb4292a398dbd/media/5c484d85561cb4292a398ddb";            
        //    //string explosionModel = "";
        //    //string productModel = "Leonardo/automovil_leonardo.json";
        //    //string explosionModel = "";

        //    string modelsPath = Application.dataPath.Substring(0, Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/")).LastIndexOf("/"));
        //    CIOProductModelUrls temp = new CIOProductModelUrls();
        //    if (productModel != "")
        //    {
        //        temp.productModelURL = productModel;
        //        if (productModel.Substring(0, 4) != "http") { temp.productModelURL = "file:///" + modelsPath + "/developmentModels/" + temp.productModelURL; }
        //    }
        //    if (explosionModel != "")
        //    {
        //        temp.explosionModelURL = explosionModel;
        //        if (temp.explosionModelURL.Substring(0, 4) != "http") { temp.explosionModelURL = "file:///" + modelsPath + "/developmentModels/" + temp.explosionModelURL; }
        //    }

        //    json_string = JsonUtility.ToJson(temp);
        //}
        Debug.Log(json_string);
        // Parse JSON to and object
        CIOProductModelUrls dataReceived   = JsonUtility.FromJson<CIOProductModelUrls>(json_string);
        string productModelURL      = dataReceived.productModelURL;
        string explosionModelURL    = dataReceived.explosionModelURL;
        
        //Send to the Core                
        CCoreCommand command = new CModelCommand(TModelCommands.LoadProductModel, productModelURL, explosionModelURL);
        this.GetComponent<Core>().Do(command, Constants.undoNotAllowed);
    }

    public void IOEditProductModel(string json_string)
    {
        if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            if (json_string == "")
            {
                //CIOEditProductModel dataToSend = new CIOEditProductModel();
                //dataToSend.command = "create_node";
                //dataToSend.name = "new group";
                //dataToSend.parentId = "2";
                //dataToSend.childrenIdList = new List<string>((new string[] {"3","5"}));

                CIOEditProductModel dataToSend = new CIOEditProductModel();
                dataToSend.command = "remove_part";                
                dataToSend.id = "2";                

                json_string = JsonUtility.ToJson(dataToSend);
                
            }            
        }        
        // Parse JSON to and object
        CIOEditProductModel dataReceived = JsonUtility.FromJson<CIOEditProductModel>(json_string);        
        
        //Send to the Core                
        CCoreCommand corecommand = new CModelCommand(TModelCommands.ModifyProductModel, dataReceived);
        hom3r.coreLink.Do(corecommand, Constants.undoNotAllowed);
    }

    public void IOGetProductModel()
    {
        this.ToWebApp_GetProducModel();
    }

    public void IOSetNavigationAxis(string _navigationAxis)
    {
        if ((hom3r.state.platform == THom3rPlatform.Editor) && (_navigationAxis == ""))
        {
            string navigation_axis = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetModelNavigationAxis();
            if (navigation_axis == "horizontal") {
                _navigationAxis = "vertical";
            } else
            {
                _navigationAxis = "horizontal";
            }
            
        }
        hom3r.coreLink.Do(new CModelCommand(TModelCommands.SetNavigationAxis, _navigationAxis));
    }

    public void IOGetRotation()
    {
        Quaternion cameraRotation = Camera.main.transform.rotation;
        Quaternion cameraInverseRotation = Quaternion.Inverse(cameraRotation);

        // TODO apply to current 3Dmodel Rotation
        //Quaternion newRotation = cameraInverseRotation * new Quaternion(0f, -1f, 0f, 0f);

        Debug.Log(cameraRotation);
        Debug.Log(cameraInverseRotation);
        //Debug.Log(newRotation);

        // TODO return to webAPP
    }

    public void IOResetProductModel()
    {
        // Remove all labels
        hom3r.coreLink.Do(new CLabelManager2Command(TLabelManager2Commands.RemoveAllLabel), Constants.undoNotAllowed);
        //Send to the Core               
        this.GetComponent<Core>().Do(new CModelCommand(TModelCommands.ResetProductModel), Constants.undoNotAllowed);
    }
 
    public void IOLoad3DFiles(string json_string)
    {
        if (Application.isEditor && json_string == "")
        {
            C3DFileUrlIO tempList = new C3DFileUrlIO();
            tempList.data = new List<CIO3DFileData>();            
            string filesPath = Application.dataPath.Substring(0, Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/")).LastIndexOf("/"));
            string basepath = "file:///" + filesPath;

            // Tetris
            IOLoad3DFile_editor_help(tempList, "iter_centralsolenoid.mtl", basepath + "/experiment/navigation/scenarios/models/iter_centralsolenoid.mtl", "mtl");
            IOLoad3DFile_editor_help(tempList, "iter_centralsolenoid.obj", basepath + "/experiment/navigation/scenarios/models/iter_centralsolenoid.obj", "obj");
            // Cilinder
            //IOLoad3DFile_editor_help(tempList, "cilinder.mtl", basepath + "/experiment/navigation/scenarios/models/cilinder.mtl", "mtl");
            //IOLoad3DFile_editor_help(tempList, "cilinder.obj", basepath + "/experiment/navigation/scenarios/models/cilinder.obj", "obj");

            // Leonardo
            //IOLoad3DFile_editor_help(tempList, "Automovil_Leonardo_centrado.mtl", basepath + "/developmentModels/Leonardo/Automovil_Leonardo_centrado.mtl", "mtl");
            //IOLoad3DFile_editor_help(tempList, "Automovil_Leonardo_centrado.obj", basepath + "/developmentModels/Leonardo/Automovil_Leonardo_centrado.obj", "obj");

            // FILE CAJA            
            //IOLoad3DFile_editor_help(tempList, "caja.mtl", basepath + "/developmentModels/caja/caja.mtl", "mtl");
            //IOLoad3DFile_editor_help(tempList, "caja.obj", basepath + "/developmentModels/caja/caja.obj", "obj");

            ////File  stadium OK    
            //IOLoad3DFile_editor_help(tempList, "wheel.obj", basepath + "/developmentModels/wheel/wheel.obj", "obj");

            //IOLoad3DFile_editor_help(tempList, "rock0.mtl", basepath + "/developmentModels/rock1/rock0.mtl", "mtl");
            //IOLoad3DFile_editor_help(tempList, "rock0.obj", basepath + "/developmentModels/rock1/rock0.obj", "obj");
            //IOLoad3DFile_editor_help(tempList, "Rock-Texture-Surface.jpg", basepath + "/developmentModels/rock1/Rock-Texture-Surface.jpg", "jpg");


            //IOLoad3DFile_editor_help(tempList, "20190327 3 floors section_v1.obj", basepath + "/developmentModels/AnaPujol/20190327 3 floors section_v1.obj", "obj");

            //IOLoad3DFile_editor_help(tempList, "20190327 3 floors section_v2.obj", basepath + "/developmentModels/AnaPujol/20190327 3 floors section_v2.obj", "obj");

            //IOLoad3DFile_editor_help(tempList, "20190327 3 floors section_v3.obj", basepath + "/developmentModels/AnaPujol/20190327 3 floors section_v3.obj", "obj");
            //IOLoad3DFile_editor_help(tempList, "20190327 3 floors section_v3.mtl", basepath + "/developmentModels/AnaPujol/20190327 3 floors section_v3.mtl", "mtl");

            //IOLoad3DFile_editor_help(tempList, "Automovil_Leonardo_centrado.mtl", basepath + "/developmentModels/Leonardo/Automovil_Leonardo_centrado.mtl", "mtl");
            //IOLoad3DFile_editor_help(tempList, "Automovil_Leonardo_centrado.obj", basepath + "/developmentModels/Leonardo/Automovil_Leonardo_centrado.obj", "obj");

            // IOANNINA PISTOL
            //IOLoad3DFile_editor_help(tempList, "Pistol_basecolor.png", basepath + "/developmentModels/silversmithing/test/Pistol_basecolor.png", "png");
            //IOLoad3DFile_editor_help(tempList, "Pistol_metallic_smoothness.png", basepath + "/developmentModels/silversmithing/test/Pistol_metallic_smoothness.png", "png");
            //IOLoad3DFile_editor_help(tempList, "Pistol_normal.png", basepath + "/developmentModels/silversmithing/test/Pistol_normal.png", "png");
            //IOLoad3DFile_editor_help(tempList, "Pistol_Phong.mtl", basepath + "/developmentModels/silversmithing/test/Pistol_Phong.mtl", "mtl");
            //IOLoad3DFile_editor_help(tempList, "Pistol_Phong.obj", basepath + "/developmentModels/silversmithing/test/Pistol_Phong.obj", "obj");


            /* IOANNINA MEGILLA 
            IOLoad3DFile_editor_help(tempList, "Megillah_AlbedoTransparency.png", basepath + "/developmentModels/silversmithing/test/Megillah_AlbedoTransparency.png", "png");
            IOLoad3DFile_editor_help(tempList, "Megillah_MetallicSmoothness.png", basepath + "/developmentModels/silversmithing/test/Megillah_MetallicSmoothness.png", "png");
            IOLoad3DFile_editor_help(tempList, "Megillah_Normal.png", basepath + "/developmentModels/silversmithing/test/Megillah_Normal.png", "png");
            IOLoad3DFile_editor_help(tempList, "Megillah_Phong.mtl", basepath + "/developmentModels/silversmithing/test/Megillah_Phong.mtl", "mtl");
            IOLoad3DFile_editor_help(tempList, "Megillah_Phong.obj", basepath + "/developmentModels/silversmithing/test/Megillah_Phong.obj", "obj");
            */


            json_string = JsonUtility.ToJson(tempList);
        }        
        C3DFileUrlIO dataReceived = JsonUtility.FromJson<C3DFileUrlIO>(json_string);
                
        //Send to the Core                    
        this.GetComponent<Core>().Do(new CModelCommand(TModelCommands.Load3DFiles, dataReceived.data), Constants.undoNotAllowed);

    }

    public void IOLoad3DFile_editor_help(C3DFileUrlIO tempList, string fileName, string fileURL, string fileType)
    {
        CIO3DFileData item1 = new CIO3DFileData();
        item1.fileName = fileName;
        item1.fileURL = fileURL;
        item1.fileType = fileType;
        tempList.data.Add(item1);
    }

    public void IOSelectPart(string json_string)
    {        
        if (Application.isEditor)
        {
            json_string = @"
            {        
            ""data""      : [
                {""id"": ""4"", ""colour"": ""#E02010""}]
            }";
        }        
        // Parse JSON to and object        
        CAreasIO HighlightAreaDataReceived = JsonUtility.FromJson<CAreasIO>(json_string);

        //Send to the Core
        foreach (var item in HighlightAreaDataReceived.data)
        {            
            this.GetComponent<Core>().Do(new CSelectionCommand(TSelectionCommands.SelectPart, item.id, item.colour), Constants.undoNotAllowed);
        }      
    }

    public void IOUnselectPart(string json_string)
    {
        if (Application.isEditor)
        {
            json_string = @"
            {        
            ""data""      : [                
                {""id"": ""4"", ""colour"": """"}]
            }";
        }

        // Parse JSON to and object
        //Areas_packet HighlightAreaDataReceived;
        //HighlightAreaDataReceived = JsonMapper.ToObject<Areas_packet>(json_string);
        CAreasIO HighlightAreaDataReceived = JsonUtility.FromJson<CAreasIO>(json_string);
        //Send to the Core
        foreach (var item in HighlightAreaDataReceived.data)
        {
            //this.GetComponent<Core_Script>().Do(new IOCoreCommand(TIOEvent.DeselectPart, item.areaID, ""), Constants.undoNotAllowed);
            this.GetComponent<Core>().Do(new CSelectionCommand(TSelectionCommands.DeselectPart, item.id, ""), Constants.undoNotAllowed);
        }   
    }

    public void IOUnselectAllParts()
    {        
        this.GetComponent<Core>().Do(new CSelectionCommand(TSelectionCommands.DeselectAllParts, ""), Constants.undoNotAllowed);
    }

    public void IOResetSelectionColours()
    {        
        this.GetComponent<Core>().Do(new CSelectionCommand(TSelectionCommands.ClearSelectionColour, ""), Constants.undoNotAllowed);
    }

    public void SetMainNavigationAxis(string mainAxis)
    {

    }//END SetMainNavigationAxis

    /// <summary>Turn ON/OFF intelligent transparency, depending on the parameter received</summary>
    /// <param name="status">string parameter that indicates the new status of the smart transparency.Valid values "on" or "off".</param>
    public void IOSmartTransparency(string json_string)
    {
        if (Application.isEditor)
        {
            json_string = @"{""enabled"": false, ""alphaValue"": 0.7}";
        }
        // Parse JSON to and object        
        CIOSmartTransparencyData smartTransparencyData = JsonUtility.FromJson<CIOSmartTransparencyData>(json_string);
          
        if (smartTransparencyData.enabled) {            
            hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.SetSmartTransparencyAlphaLevel, smartTransparencyData.alphaValue, THom3rCommandOrigin.io));            
            hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.EnableSmartTransparency, THom3rCommandOrigin.io));
        }
        else
        {
            hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.DisableSmartTransparency, THom3rCommandOrigin.io));
        }
        
    }

    public void Explosion (string activate, string type)
    {
        //bool tempStatus;
        //if (status == "on")
        //{
        //    tempStatus = true;            
        //    CoreCommand command = new IOCoreCommand("IO/Explosion", tempStatus, type);      //Create command
        //    this.GetComponent<Core_Script>().Do(command, Constants.undoNotAllowed);        //Send to core
        //}
        //else if (status == "off")
        //{
        //    tempStatus = false;
        //    CoreCommand command = new IOCoreCommand("IO/Explosion", tempStatus, type);      //Create command
        //    this.GetComponent<Core_Script>().Do(command, Constants.undoNotAllowed);        //Send to core            
        //}
        //else
        //{
        //    //Do nothing
        //}
    }//END explosion

    //////////////////////////////
    //// SINGLE POINT
    //////////////////////////////
    /*
    public void CaptureSinglePoint(string json_string)
    {
        if (Application.isEditor)
        {
            json_string = @"{ ""singlepointgroupID"":1,""areaID"":""1.1""}";
        }
        
        
        CSinglePointCaptureIO dataReceived = JsonUtility.FromJson<CSinglePointCaptureIO>(json_string);//Parse JSON to and object        
        string areaNodeId = dataReceived.areaID;
        int singlepointgroupID = dataReceived.singlepointgroupID;


        string colour = "";                               
        this.GetComponent<Core>().Do(new CSelectionCommand(TSelectionCommands.DeselectAllParts, ""), Constants.undoNotAllowed); //Un-select all parts                                                                                                                                
        this.GetComponent<Core>().Do(new CSelectionCommand(TSelectionCommands.SelectPart, areaNodeId, colour), Constants.undoNotAllowed);       //Activate the Single point capture mode        
        this.GetComponent<Core>().Do(new CSinglePointCommand(TSinglePointCommands.SinglePointMode_Start, singlepointgroupID), Constants.undoNotAllowed);
    }

    public void VisualizeSinglePoint(string json_string)
    {
        if (Application.isEditor)
        {           
            json_string = @"{""singlepointgroupID"":1,""areaID"":""1.2.3.1"",""positions"":[{""x"":3.20707678794861,""y"":1.51604390144348,""z"":3.87232637405396},{""x"":3.99228501319885,""y"":1.60840725898743,""z"":3.04904127120972},{""x"":4.97847652435303,""y"":1.74071383476257,""z"":0.670296430587769},{""x"":5.00705623626709,""y"":1.74127423763275,""z"":-0.446723699569702}]}";
        }                
        CSinglePointIO singlePointsReceived = JsonUtility.FromJson<CSinglePointIO>(json_string);    //Parse JSON to and object

        //this.GetComponent<Core_Script>().Do(new IOCoreCommand(TIOEvent.VisualizeSinglePoint, singlePointsReceived), Constants.undoNotAllowed);
        this.GetComponent<Core>().Do(new CSinglePointCommand(TSinglePointCommands.VisualizeSinglePoint, singlePointsReceived), Constants.undoNotAllowed);
    }

    /// <summary>
    /// Method to process the information of the SinglePoint group to edit
    /// </summary>
    /// <param name="json_string"></param>
    public void EditSinglePoint(string json_string)
    {
        if (Application.isEditor)
        {
            json_string = @"{""singlepointgroupID"":1,""areaID"":""1.2.3.1"",""positions"":[{""x"":3.20707678794861,""y"":1.51604390144348,""z"":3.87232637405396},{""x"":3.99228501319885,""y"":1.60840725898743,""z"":3.04904127120972},{""x"":4.97847652435303,""y"":1.74071383476257,""z"":0.670296430587769},{""x"":5.00705623626709,""y"":1.74127423763275,""z"":-0.446723699569702}]}";
        }        
        //Parse JSON to and object
        //SinglePoint_packet singlePointsReceived;
        //singlePointsReceived = JsonMapper.ToObject<SinglePoint_packet>(json_string);
        CSinglePointIO singlePointsReceived = JsonUtility.FromJson<CSinglePointIO>(json_string);

        string colour = "";
        //Highlight off all the areas
        //this.GetComponent<Core_Script>().Do(new IOCoreCommand("IO/HighlightAreaOff", singlePointsReceived.areaID), Constants.undoNotAllowed);
        //Select the indicated area
        //this.GetComponent<Core_Script>().Do(new IOCoreCommand(TIOEvent.SelectPart, singlePointsReceived.areaID, colour), Constants.undoNotAllowed);
        this.GetComponent<Core>().Do(new CSelectionCommand(TSelectionCommands.SelectPart, singlePointsReceived.areaID, colour), Constants.undoNotAllowed);
        //Activate the SinglePoint Editing mode
        //this.GetComponent<Core_Script>().Do(new IOCoreCommand(TIOEvent.EditSinglePointGroup, singlePointsReceived), Constants.undoNotAllowed);
        this.GetComponent<Core>().Do(new CSinglePointCommand(TSinglePointCommands.EditSinglePointGroup, singlePointsReceived), Constants.undoNotAllowed);
    }
       
    /// <summary> Method to process the information of the SinglePoint group to hide</summary>
    /// <param name="json_string"></param>
    public void RemoveSinglePoint(int singlepointgroupID)
    {
        //this.GetComponent<Core_Script>().Do(new IOCoreCommand(TIOEvent.RemoveSinglePoint, singlepointgroupID), Constants.undoNotAllowed);
        this.GetComponent<Core>().Do(new CSinglePointCommand(TSinglePointCommands.RemoveSinglePoint, singlepointgroupID), Constants.undoNotAllowed);
    }//End SinglePointVisualize

    public void RemoveAllSinglePoint()
    {
        //this.GetComponent<Core_Script>().Do(new IOCoreCommand(TIOEvent.RemoveAllSinglePoints,""), Constants.undoNotAllowed);
        this.GetComponent<Core>().Do(new CSinglePointCommand(TSinglePointCommands.RemoveAllSinglePoints), Constants.undoNotAllowed);
    }
    */

    //////////////////////////////
    //// LABEL
    //////////////////////////////
         
   public void IOAddLabel(string json_string)
    {
        if (Application.isEditor) {
            //json_string = @"{""id"": ""1"", ""type"": ""boardLabel"", ""text"": ""Εμπροσθογεμής μονόκανη ροκα / πιστόλα,, vitae eu montes natoque non pulvinar accumsan. Tempor ante magnis commodo penatibus et id vehicula orci risus, nascetur interdum facilisis eu neque tempus molestie fusce vel consequat, lacinia torquent eros vitae in est litora vestibulum.""}";
            //gun
            //json_string = @"{""id"": ""1"", ""type"": ""anchoredLabel"",""areaId"": ""4"", ""text"": ""HANDLE & POMMEL""}";
            //saturn
            json_string = @"{""id"": ""1"", ""type"": ""anchoredLabel"",""areaId"": ""1.3.1.1"", ""text"": ""part""}";
            //rock
            //json_string = @"{""id"": ""1"", ""type"": ""anchoredLabel"",""areaId"": ""1.1.1"", ""text"": ""part""}";
        }                
        CIOLabelData label = JsonUtility.FromJson<CIOLabelData>(json_string);     //Parse JSON to and object        
        if (label.type == "boardLabel")
        {
            this.AddBoardLabel(label.id, label.text);
        } else if (label.type == "anchoredLabel")
        {
            this.AddAnchoredLabel(label.id, label.areaId, label.text);
        }

    }

    private void AddBoardLabel(string labelId, string text)
    {                     
        hom3r.coreLink.Do(new CLabelManager2Command(TLabelManager2Commands.AddBoardLabel, labelId, text));        
    }

    private void AddAnchoredLabel(string labelId, string areaId, string text)
    {        
        string colour = "#E02010";
        hom3r.coreLink.Do(new CLabelManager2Command(TLabelManager2Commands.AddAnchoredLabel, labelId, areaId, text, colour));
    }

    /// <summary>
    /// Show label list 
    /// </summary>
    /// <param name="json_string"></param>
    public void IOShowLabelList(string json_string)
    {
        if (Application.isEditor && json_string == "")
        {
            /*json_string = @"
            {        
            ""data""      : [                
                {""id"": ""2"", ""type"": ""anchoredLabel"" , ""areaId"": ""4"", ""text"": ""HANDLE & POMMEL"", ""position"":{""x"": -17.455366134643556 ,""y"": 1.9230165481567383,""z"": -3.3861117362976076}, ""anchorPosition"":{""x"": -14.972845077514649 ,""y"":-0.49157431721687319,""z"": -0.9096667170524597}, ""scaleFactor"": 2.0}                
                ]}";*/
            json_string = @"
            {        
            ""data""      : [
                {""id"": ""1"", ""type"": ""boardLabel"",                              ""text"": ""Εμπροσθογεμής μονόκανη ροκα / πιστόλα adipiscing elit ad, vitae eu montes natoque non pulvinar accumsan.Tempor ante magnis commodo penatibus et id vehicula orci risus, nascetur interdum facilisis eu neque tempus molestie fusce vel consequat, lacinia torquent eros vitae in est litora vestibulum."", ""position"":{""x"": 11.39673900604248,""y"":4.028753757476807,""z"":1.1818979978561402}, ""rotation"":{""x"": 0 ,""y"":0,""z"": 0, ""w"": 0}, ""scaleFactor"": 1.0 },                                
                {""id"": ""2"", ""type"": ""anchoredLabel"" , ""areaId"": ""4"", ""text"": ""HANDLE & POMMEL"", ""position"":{""x"": -17.455366134643556 ,""y"": 1.9230165481567383,""z"": -3.3861117362976076}, ""anchorPosition"":{""x"": -14.972845077514649 ,""y"":-0.49157431721687319,""z"": -0.9096667170524597}, ""scaleFactor"": 2.0}                
                ]}";

            //json_string = @"
            //{        
            //""data""      : [
            //    {""id"": ""1"", ""type"": ""boardLabel"",                              ""text"": ""Εμπροσθογεμής μονόκανη ροκα / πιστόλα."", ""position"":{""x"": 11.39673900604248,""y"":4.028753757476807,""z"":1.1818979978561402}, ""rotation"":{""x"": 0 ,""y"":0,""z"": 0, ""w"": 0}, ""scaleFactor"": 1.0 },                                
            //    {""id"": ""2"", ""type"": ""anchoredLabel"" , ""areaId"": ""4"", ""text"": ""HANDLE & POMMEL"", ""position"":{""x"": -17.455366134643556 ,""y"": 1.9230165481567383,""z"": -3.3861117362976076}, ""anchorPosition"":{""x"": -14.972845077514649 ,""y"":-0.49157431721687319,""z"": -0.9096667170524597}, ""scaleFactor"": 2.0}                
            //    ]}";
            //json_string = @"{ ""data"":[{""id"":""description_en"",""type"":""boardLabel"",""text"":""Muzzle-loading single-barrel flintlock roka style pistol, silver gilt with savati engraving. Part of a pair of pistols belonging to Edward John Trelawny (1792-1881), Lord Byron’s companion during his trips.\\n\\nEpirus, early 19th century."",""position"":{""x"":0.7310714721679688,""y"":0.04012061282992363,""z"":-0.055859409272670746},""rotation"":{""x"":0,""y"":0,""z"":0,""w"":1},""scaleFactor"":2}]}";

            //json_string = @"{ ""data"":[{""id"":""description_en"",""type"":""boardLabel"",""text"":""Muzzle-loading single-barrel flintlock roka style pistol, silver gilt with savati engraving. Part of a pair of pistols belonging to Edward John Trelawny (1792-1881), Lord Byron’s companion during his trips.\\n\\nEpirus, early 19th century."",""position"":{""x"":0.7310714721679688,""y"":0.04012061282992363,""z"":-0.055859409272670746},""rotation"":{""x"":0,""y"":0,""z"":0,""w"":1},""scaleFactor"":2}]}";

        }
        CIOLabelList labelListReceived = JsonUtility.FromJson<CIOLabelList>(json_string);     //Parse JSON to and object

        //Send to the core the labels one by one
        foreach (var label in labelListReceived.data)
        {
            Vector3 position = new Vector3((float)label.position.x, (float)label.position.y, (float)label.position.z);
            if (label.type == "boardLabel")
            {
                Quaternion rotation = new Quaternion((float)label.rotation.x, (float)label.rotation.y, (float)label.rotation.z, (float)label.rotation.w);
                this.ShowBoardLabel(label.id, label.text, position, rotation, (float)label.scaleFactor);
            } else if (label.type == "anchoredLabel")
            {
                if (label.areaId == null) { return; }
                Vector3 anchorPosition = new Vector3((float)label.anchorPosition.x, (float)label.anchorPosition.y, (float)label.anchorPosition.z);
                this.ShowAnchoredLabel(label.id, label.areaId, label.text, position, anchorPosition, (float)label.scaleFactor);
            }
        }
    }


    public void testShowLabel(int label)
    {
        string json_string;

        if (label == 1)
        {
            json_string = @"
            {        
            ""data""      : [
                {""id"": ""1"", ""type"": ""boardLabel"",                              ""text"": ""Εμπροσθογεμής μονόκανη ροκα / πιστόλα adipiscing elit ad, vitae eu montes natoque non pulvinar accumsan.Tempor ante magnis commodo penatibus et id vehicula orci risus, nascetur interdum facilisis eu neque tempus molestie fusce vel consequat, lacinia torquent eros vitae in est litora vestibulum."", ""position"":{""x"": 11.39673900604248,""y"":4.028753757476807,""z"":1.1818979978561402}, ""rotation"":{""x"": 0 ,""y"":0,""z"": 0, ""w"": 0}, ""scaleFactor"": 1.0 },                                
                {""id"": ""2"", ""type"": ""anchoredLabel"" , ""areaId"": ""4"", ""text"": ""HANDLE & POMMEL"", ""position"":{""x"": -17.455366134643556 ,""y"": 1.9230165481567383,""z"": -3.3861117362976076}, ""anchorPosition"":{""x"": -14.972845077514649 ,""y"":-0.49157431721687319,""z"": -0.9096667170524597}, ""scaleFactor"": 2.0}                
                ]}";
        } else if (label == 2)
        {
            json_string = @"
            {        
            ""data""      : [                
                {""id"": ""2"", ""type"": ""anchoredLabel"" , ""areaId"": ""8"", ""text"": ""Third level"", ""position"":{""x"": 0.46960270404815674 ,""y"": 4.016263008117676,""z"": -3.6651687622070312}, ""anchorPosition"":{""x"": -0.26111871004104614 ,""y"":4.057229518890381,""z"": -1.8403557538986206}, ""scaleFactor"": 2.0}                
                ]}";
        } else
        {
            json_string = "";
        }

        this.IOShowLabelList(json_string);
    }

    private void ShowBoardLabel(string labelId, string text, Vector3 position, Quaternion rotation, float scaleFactor)
    {        
        hom3r.coreLink.Do(new CLabelManager2Command(TLabelManager2Commands.ShowBoardLabel, labelId, text, position, rotation, scaleFactor ));
    }

    private void ShowAnchoredLabel(string labelId, string areaId, string text, Vector3 position, Vector3 anchorPosition, float scaleFactor)
    {
        /*string labelId = "2";
        string text = "Lorem ipsum dolor";
        Vector3 position = new Vector3(-0.2502460479736328f, 0.20486906170845033f, -0.25028595328330996f);
        Vector3 anchorPosition = new Vector3(0.022657567635178567f, -0.03269069269299507f, 0.044847920536994937f);
        float scaleFactor = 2.0f;
        string areaId = "1.2.3.1";*/

        hom3r.coreLink.Do(new CLabelManager2Command(TLabelManager2Commands.ShowAnchoredLabel, labelId, areaId, text, position, anchorPosition, scaleFactor));
    }

    public void IOUpdateLabelText(string json_string)
    {
        if (Application.isEditor)
        {
            //json_string = @"{""id"": ""1"", ""text"": ""Dani""}";
            json_string = @"{""id"": ""2"", ""text"": ""Lorem ipsum dolor sit amet consectetur adipiscing elit ad, vitae eu montes natoque non pulvinar accumsan. Tempor ante magnis commodo penatibus et id vehicula orci risus, nascetur interdum facilisis eu neque tempus molestie fusce vel consequat, lacinia torquent eros vitae in est litora vestibulum.""}";
        }

        CIOLabelData label = JsonUtility.FromJson<CIOLabelData>(json_string);     //Parse JSON to and object 

        hom3r.coreLink.Do(new CLabelManager2Command(TLabelManager2Commands.UpdateLabelText, label.id, label.text));
    }

    /*public void IOEditLabel(string json_string)
    {
        Debug.Log("IOEditLabel");
        hom3r.coreLink.Do(new CLabelManager2Command(TLabelManager2Commands.EditLabel, "1"));
    }*/

    public void IORemoveLabel(string labelID)
    {
        //Send Message to Core        
        this.GetComponent<Core>().Do(new CLabelManager2Command(TLabelManager2Commands.RemoveLabel, labelID), Constants.undoNotAllowed);
    }

    public void IORemoveAllBoardLabels()
    {
        //this.GetComponent<Core_Script>().Do(new IOCoreCommand(TIOEvent.RemoveAllLabel), Constants.undoNotAllowed);
        this.GetComponent<Core>().Do(new CLabelManager2Command(TLabelManager2Commands.RemoveAllBoardLabel), Constants.undoNotAllowed);
    }

    public void IORemoveAllLabels()
    {
        //this.GetComponent<Core_Script>().Do(new IOCoreCommand(TIOEvent.RemoveAllLabel), Constants.undoNotAllowed);
        this.GetComponent<Core>().Do(new CLabelManager2Command(TLabelManager2Commands.RemoveAllLabel), Constants.undoNotAllowed);
    }

    //////////////////////////////
    //// REMOVE
    //////////////////////////////

    public void IORemovePart(string json_string)
    {        
        if (Application.isEditor)
        {
            //json_string = @"
            //{        
            //""data""      : [
            //    {""id"": ""238""}, {""id"": ""1.3.1.2""}, {""id"": ""1.3.1.3""}]
            //}";
            json_string = @"
            {        
            ""data""      : [
                {""id"": ""238""}]
            }";
        }

        // Parse JSON to and object        
        CRemoveAreasIO areasToRemove = JsonUtility.FromJson<CRemoveAreasIO>(json_string);
        //Send to the Core      
        foreach (CRemoveAreasIOData area in areasToRemove.data)
        {           
            hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.RemoveArea, area.id, THom3rCommandOrigin.io));
        }        
    }

    public void IOShowRemovedPart(string json_string)
    {
        if (Application.isEditor)
        {
            json_string = @"
            {        
            ""data""      : [
                {""id"": ""12""}, {""id"": ""1.3.1.2""}, {""id"": ""1.3.1.3""}]
            }";
        }

        // Parse JSON to and object        
        CRemoveAreasIO areasToShow = JsonUtility.FromJson<CRemoveAreasIO>(json_string);     
        foreach (CRemoveAreasIOData area in areasToShow.data)
        {
            hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.ShowRemovedArea, area.id, THom3rCommandOrigin.io));
        }
    }

    public void IOShowAllRemovePart()
    {
        // 0.Save the necessary command to execute the undo
        THom3rIsolationMode currentMode = hom3r.state.currentIsolateMode;
        List<GameObject> goRemovedList = new List<GameObject>(hom3r.quickLinks.scriptsObject.GetComponent<RemoveManager>().GetRemovedList());
        //1. Notify the Core        
        CCoreCommand command = new COcclusionCommand(TOcclusionCommands.ShowAll, currentMode, goRemovedList);

        hom3r.coreLink.Do(command, Constants.undoAllowed);
    }


    /// <summary>
    /// Deselect all parts, show all the removed areas, disable smarttransparency, reset selection colours to default one
    /// </summary>
    public void IOResetScene()
    {
        // Show all
        hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.ShowAll));
        // Unselect all parts        
        hom3r.coreLink.Do(new CSelectionCommand(TSelectionCommands.DeselectAllParts, ""), Constants.undoNotAllowed);
        // Reset selection colours        
        hom3r.coreLink.Do(new CSelectionCommand(TSelectionCommands.ClearSelectionColour, ""), Constants.undoNotAllowed);
        // Remove all labels
        hom3r.coreLink.Do(new CLabelManager2Command(TLabelManager2Commands.RemoveAllLabel), Constants.undoNotAllowed);        
    }



    //////////////////
    //  OTHERS      //
    //////////////////

    public void IOControlKeyPressed(string KeyControlPressed)
    {
        bool _KeyControlPressed;                
        bool.TryParse(KeyControlPressed, out _KeyControlPressed);
        this.GetComponent<MouseManager>().SetKeyControlPressed(_KeyControlPressed);
    }

    //public void ShowLabelsWithAreaComponentName(string activated)
    //{
    //    bool _activated;
    //    if (bool.TryParse(activated, out _activated))
    //    {            
    //        this.GetComponent<Core>().Do(new CLabelCommand(TLabelCommands.LabelModeManager, _activated), Constants.undoNotAllowed);
    //    }
    //}


    //////////////////////////////////
    //////////////////////////////////
    //                              //
    // From Unity to WebAPP Methods //
    //                              //
    //////////////////////////////////
    //////////////////////////////////
    public void ToWebApp_ReportError(string error)
    {
        ToWebApp_SendToApp("error", error);
    }

    /// <summary>Send to the web-application if hom3r has been started correctly or not</summary>
    /// <param name="status">true is everything ok</param>
    public void ToWebApp_Hom3rState(bool status)
    {
        if (status)
        {
            ToWebApp_SendToApp("hom3r", "success");
        }
        else
        {
            ToWebApp_SendToApp("hom3r", "error");
            ToWebApp_SendToApp("error", "An unknown error has happened while starting hom3r.");
        }
    }

    /// <summary>Send to the web-application that product model has been loaded correctly or not</summary>
    /// <param name="status">True if ok</param>
    public void ToWebApp_ProductLoadedResult(bool status)
    {
        if (status)
        {
            ToWebApp_SendToApp("load-product-model", "success");
        }
        else
        {
            ToWebApp_SendToApp("load-product-model", "error");
        }
    }

    /// <summary>Send to the web-application that product model edition has been done correctly or not</summary>
    /// <param name="status">True if ok</param>
    public void ToWebApp_ProductModelEditionResult(bool status)
    {
        if (status)
        {
            ToWebApp_SendToApp("edit-product-model", "success");
        }
        else
        {
            ToWebApp_SendToApp("edit-product-model", "error");
        }
    }
    
    public void ToWebApp_GetProducModel()
    {
        CFileProductModel toSend_model = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetProductModel();
        string toSend_string = JsonUtility.ToJson(toSend_model);

        ToWebApp_SendToApp("product-model", toSend_string);
    }
        
    public void ToWebApp_3DModelLoadResult(bool status) {
        if (status)
        {
            ToWebApp_SendToApp("load-3D-model", "success");
        }
        else
        {
            ToWebApp_SendToApp("load-3D-model", "error");
        }
    }


    public void ToWebApp_SelectedPart(List<string> listAreaID)
    {
        CAreasIO areasToBeSend = new CAreasIO();
        foreach (var item in listAreaID)
        {
            GameObject areaGO = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(item);
            string areaColour = "";
            if (areaGO != null) {
                areaColour = "#" + ObjectStateMaterialUtils.ColorToHex(areaGO.GetComponent<ObjectStateManager>().confirmedColour);
            }            
            areasToBeSend.data.Add(new CAreasIOData(item, areaColour));
        }
        //Create JSON string
        //string toSend_string = JsonMapper.ToJson(areasToBeSend);
        string toSend_string = JsonUtility.ToJson(areasToBeSend);
        //Send to web application
        ToWebApp_SendToApp("select-part", toSend_string);        
    }
       
    public void ToWebApp_DeselectPart(List<string> listAreaID)
    {
        if (listAreaID!=null && listAreaID.Count != 0)
        {
            CAreasIO areasToBeSend = new CAreasIO();
            foreach (var item in listAreaID)
            {
                areasToBeSend.data.Add(new CAreasIOData(item, ""));
            }
            //Create JSON string
            //string toSend_string = JsonMapper.ToJson(areasToBeSend);
            string toSend_string = JsonUtility.ToJson(areasToBeSend);
            //Call external jscript function
            ToWebApp_SendToApp("deselect-part", toSend_string);
        }        
    }//ToWebApp_DeselectPart

    public void ToWebApp_DeselectAllParts()
    {
        //Call external JavaScript function
        ToWebApp_SendToApp("deselect-all-parts", "");        
    }

    




    /// <summary>Send a list of single points, all belonging to the same area, to the Web app</summary>
    /// <param name="singlePointsSendList">A structure that contain de areaID and the list of single points</param>
    //public void ToWebApp_NewSinglePoint(CSinglePointIO singlePointsList_toSend)
    //{
        
    //    string toSend_string = JsonUtility.ToJson(singlePointsList_toSend); //Create JSON string                
    //    ToWebApp_SendToApp("SaveSinglePoint", toSend_string); //Check which method we have to call       

    //    //TODO Delete me
    //    string areaID = singlePointsList_toSend.areaID;
    //    foreach (CIOPositionVector point in singlePointsList_toSend.positions)
    //    {
    //        string text = "ENGRAVED DECORATING TECHNIQUE - SAVATI\nSilver gilt pistol. Engraved decoration with elaborate floral patterns, combined with rococo-style clams.\nThe elaborate decoration on the total surface of the weapon indicates that the weapon was not intended for military use but rather for duelling and / or prestige.\nMore information on the engraving decorative technique (embossing, chasing) and on savati can be found in Module 3 «How?», on the first level of the exhibition.";
    //        Vector3 p = new Vector3((float)point.x, (float)point.y, (float)point.z); 
    //        hom3r.coreLink.Do(new CLabelCommand(TLabelCommands.AddLabel, "", areaID, text, p));
    //    }

    //}

    public void ToWebApp_DeleteSinglePoint(int singlepointID)
    {
        //ToWebApp_SendToApp("RemoveSinglePoint", singlepointID);
    }

    public void ToWebApp_EndSinglePoint(int singlepointID)
    {
        //ToWebApp_SendToApp("EndSinglePoint", singlepointID);
    }

    /// <summary>
    /// Send label data after change its position, orientation or size.
    /// </summary>
    /// <param name="labelId"></param>
    /// <param name="_areaId"></param>
    /// <param name="_labelType"></param>
    /// <param name="_labelPosition"></param>
    /// <param name="_labelRotation"></param>
    /// <param name="_anchorPosition"></param>
    /// <param name="scaleFactor"></param>
    public void ToWebApp_LabelUpdatedData(string labelId, string _areaId, TLabelType _labelType, Vector3 _labelPosition, Quaternion _labelRotation, Vector3 _anchorPosition, float scaleFactor)
    {
        string toSend_string = null;
        if (_labelType == TLabelType.boardLabel)
        {
            CIOBoardLabelDataSend labelDataToSend = new CIOBoardLabelDataSend();                    
            labelDataToSend.id = labelId;
            labelDataToSend.type = "boardLabel";
            labelDataToSend.position = new CIOPositionVector(_labelPosition.x, _labelPosition.y, _labelPosition.z);
            labelDataToSend.scaleFactor = scaleFactor;            
            labelDataToSend.rotation = new CIORotationVector(_labelRotation.x, _labelRotation.y, _labelRotation.z, _labelRotation.w);

            toSend_string = JsonUtility.ToJson(labelDataToSend); //Create JSON string                
        } else if (_labelType == TLabelType.anchoredLabel)
        {
            CIOAnchoredLabelDataSend labelDataToSend = new CIOAnchoredLabelDataSend();                        
            labelDataToSend.id = labelId;
            labelDataToSend.areaId = _areaId;
            labelDataToSend.type = "anchoredLabel";
            labelDataToSend.position = new CIOPositionVector(_labelPosition.x, _labelPosition.y, _labelPosition.z);
            labelDataToSend.scaleFactor = scaleFactor;            
            labelDataToSend.anchorPosition = new CIOPositionVector(_anchorPosition.x, _anchorPosition.y, _anchorPosition.z);

            toSend_string = JsonUtility.ToJson(labelDataToSend); //Create JSON string                
        }
      
        if (toSend_string != null) { ToWebApp_SendToApp("get-label-data", toSend_string); }
    }

    public void ToWebApp_RemoveLabel(string labelID)
    {
        ToWebApp_SendToApp("removed-label", labelID);
    }


    public void ToWebApp_RemovedPartActivated(List<string> listAreaID)
    {
        CRemoveAreasIO areasToBeSend = new CRemoveAreasIO();
        foreach (var item in listAreaID)
        {
            areasToBeSend.data.Add(new CRemoveAreasIOData(item));
        }                
        string toSend_string = JsonUtility.ToJson(areasToBeSend);       //Create JSON string        
        ToWebApp_SendToApp("remove-part", toSend_string);    //Send to web application
    }

    public void ToWebApp_RemovedPartDeactivated(List<string> listAreaID)
    {
        if (listAreaID != null && listAreaID.Count != 0)
        {
            CRemoveAreasIO areasToBeSend = new CRemoveAreasIO();
            foreach (var item in listAreaID)
            {
                areasToBeSend.data.Add(new CRemoveAreasIOData(item));
            }
            
            string toSend_string = JsonUtility.ToJson(areasToBeSend);       // Create JSON string            
            ToWebApp_SendToApp("show-removed-part", toSend_string);  // Send
        }
    }

    //////////////////
    // OCLUSSION
    //////////////////
    
    /// <summary>Send through the js interface the smart transparency state</summary>
    /// <param name="_enabled">true if it's enabled</param>
    public void ToWebApp_SmartTranspacencyState(bool _enabled)
    {
        CIOSmartTransparencyData dataToBeSend = new CIOSmartTransparencyData();

        dataToBeSend.enabled = _enabled;
        if (_enabled)
        {
            //dataToBeSend.alphaValue = hom3r.quickLinks.scriptsObject.GetComponent<UIMaterialTransparency>().AlphaLevel;
            dataToBeSend.alphaValue = hom3r.state.smartTransparencyAlphaLevel;
        }
        string toSend_string = JsonUtility.ToJson(dataToBeSend);
        ToWebApp_SendToApp("smart-transparency-state", toSend_string);
    }

    /// <summary>Send Smart-Transparency state to through the js interface after alpha value change</summary>
    public void ToWebApp_UpdateTranspacencyAlphaLevel(float value)
    {
        CIOSmartTransparencyData dataToBeSend = new CIOSmartTransparencyData();

        dataToBeSend.enabled = hom3r.state.smartTransparencyModeActive;
        if (hom3r.state.smartTransparencyModeActive)
        {         
            dataToBeSend.alphaValue = value;
        }
        string toSend_string = JsonUtility.ToJson(dataToBeSend);
        ToWebApp_SendToApp("smart-transparency-state", toSend_string);

    }
    
    //////////////////
    // OTHER
    //////////////////
    /// <summary>DebugConsoleWeb show the message in the browser console</summary>
    /// <param name="message">message to be show in the navigator console</param>
    public void ToWebApp_ConsoleWeb(string message)
    {
        SendToConsole(message);
    }

    private void ToWebApp_SendToApp(string message, string value)
    {
        SendToApp(this.instanceId, message, value);
    }
    
}
