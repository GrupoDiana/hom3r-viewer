using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStandaloneManager : MonoBehaviour
{
    GameObject standalonePanelGO;
    GameObject standaloneInitialPanelGO;
    GameObject standaloneLoadExperimentButtonGO;
    GameObject standaloneFreeDemoButtonGO;

    GameObject standaloneFreeDemoPanelGO;
    GameObject standaloneTestObjectsDropdownGO;
    GameObject standaloneTestObjectsAxisDropdownGO;
    GameObject standaloneLoadTestObjectsButtonGO;
    GameObject standaloneOtherObjectsDropdownGO;
    GameObject standaloneLoadOtherObjectsButtonGO;
    GameObject standaloneFileURLInputGO;
    //GameObject standaloneLoadFileURLButtonGO;


    private void Awake()
    {
        standalonePanelGO = GameObject.FindGameObjectWithTag("standalone_panel");
        /////////////////////////
        /// StandAlone INITIALPanel
        //////////////////////////
        standaloneInitialPanelGO = GameObject.FindGameObjectWithTag("standaloneInitialPanel");
        standaloneFreeDemoButtonGO = standaloneInitialPanelGO.transform.Find("Button_FreeDemo").gameObject;
        standaloneFreeDemoButtonGO.GetComponent<Button>().onClick.AddListener(OnClickStandaloneInitialPanelFreeDemoButton);

        standaloneLoadExperimentButtonGO = standaloneInitialPanelGO.transform.Find("Button_Experiment").gameObject;
        standaloneLoadExperimentButtonGO.GetComponent<Button>().onClick.AddListener(OnClickStandaloneInitialPanelLoadExperimentButton);


        /////////////////////////
        /// StandAlone FreeDEMO Panel
        //////////////////////////
        standaloneFreeDemoPanelGO = GameObject.FindGameObjectWithTag("standaloneFreeDemoPanel");

        standaloneTestObjectsDropdownGO = standaloneFreeDemoPanelGO.transform.Find("Dropdown_TestObjects").gameObject;        
        standaloneTestObjectsAxisDropdownGO = standaloneFreeDemoPanelGO.transform.Find("Dropdown_TestObjAxis").gameObject;

        standaloneLoadTestObjectsButtonGO = standaloneFreeDemoPanelGO.transform.Find("Button_LoadTestObject").gameObject;
        standaloneLoadTestObjectsButtonGO.GetComponent<Button>().onClick.AddListener(OnClickStandalonePanelLoadTestObjectsButton);

        standaloneOtherObjectsDropdownGO = standaloneFreeDemoPanelGO.transform.Find("Dropdown_OtherObjects").gameObject;
        standaloneLoadOtherObjectsButtonGO = standaloneFreeDemoPanelGO.transform.Find("Button_LoadOtherObject").gameObject;
        standaloneLoadOtherObjectsButtonGO.GetComponent<Button>().onClick.AddListener(OnClickStandalonePanelLoadRealObjectsButton);

        standaloneFileURLInputGO = standaloneFreeDemoPanelGO.transform.Find("InputField_URL").gameObject;
        standaloneFileURLInputGO.GetComponent<InputField>().onEndEdit.AddListener(OnEndEditStandaloneFreeDemoPanelFileURL);

        InitPanelsVisualStatus();
        
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.F1))
        {
            standaloneInitialPanelGO.transform.Find("Button_Experiment").gameObject.GetComponent<Button>().interactable = true;
        }
    }

    public void SetActivatedStandAlonePanel(bool activate)
    {
        standalonePanelGO.SetActive(activate);
        InitPanelsVisualStatus();
    }

    private void InitPanelsVisualStatus()
    {
        standaloneFreeDemoPanelGO.SetActive(false);
        standaloneInitialPanelGO.SetActive(true);
    }

    private void OnClickStandalonePanelLoadTestObjectsButton()
    {
        string basePath = "https://proyectos.diana.uma.es/hom3r/models/test/";

        int valueModelName = standaloneTestObjectsDropdownGO.GetComponent<Dropdown>().value;
        List<Dropdown.OptionData> dataModelName = standaloneTestObjectsDropdownGO.GetComponent<Dropdown>().options;

        int valueAxis = standaloneTestObjectsAxisDropdownGO.GetComponent<Dropdown>().value;
        List<Dropdown.OptionData> dataAxis = standaloneTestObjectsAxisDropdownGO.GetComponent<Dropdown>().options;

        string modelToLoadStandalonePanel = basePath + dataModelName[valueModelName].text+ GetModelToLoadAxis(dataAxis[valueAxis].text) + ".json";
        
        hom3r.quickLinks.scriptsObject.GetComponent<StandaloneManager>().LoadModelFromUI(modelToLoadStandalonePanel);        
    }

    private string GetModelToLoadAxis(string axis)
    {
        if (axis == "Vertical")
        {
            return "_v";
        }
        else return "_h";
    }

    private void OnClickStandalonePanelLoadRealObjectsButton()
    {
        //string basePath = "https://proyectos.diana.uma.es/hom3r/models/real/";

        int value = standaloneOtherObjectsDropdownGO.GetComponent<Dropdown>().value;
        List<Dropdown.OptionData> data = standaloneOtherObjectsDropdownGO.GetComponent<Dropdown>().options;
        string modelToLoadStandalonePanel = GetRealObjectURL(data[value].text);
        string explosionModeLURL = GetRealObjectExplosionModelURL(data[value].text);

        hom3r.quickLinks.scriptsObject.GetComponent<StandaloneManager>().LoadModelFromUI(modelToLoadStandalonePanel, explosionModeLURL);
    }

    private string GetRealObjectURL(string _model)
    {
        string basePath = "https://proyectos.diana.uma.es/hom3r/models/real/";
        //string experimentBasePath = "https://proyectos.diana.uma.es/hom3r/experiment/navigation/models/";

        if (_model =="Saturn5_horizontal")
        {
            return basePath + "saturn5/saturn5_horizontal_v3.json";
        }
        else if (_model == "Saturn5_vertical")
        {
            return basePath + "saturn5/saturn5_vertical_v2.json";
        }
        else if (_model == "Automovil_Leonardo")
        {
            return basePath + "leonardo/automovil_leonardo_v2.json";
        }
        else if (_model == "Gas_turbine")
        {
            //return experimentBasePath + "5.gas_turbine.json";
            return basePath + "gasturbine/gas_turbine.json";
        }
        else if (_model == "Bus")
        {            
            return basePath + "BusIndonesian/celcius.json";
        }
        else if (_model == "Jeep")
        {
            return basePath + "jeep/jeep_rotated.json";            
        }       
        else if (_model == "Water_Wheel")
        {
            return basePath + "waterwheel/waterwheel.json";
        }       
        else
        {
            return null;
        }
    }

    private string GetRealObjectExplosionModelURL(string _model)
    {
        string basePath = "https://proyectos.diana.uma.es/hom3r/models/real/";        

        if (_model == "Saturn5_horizontal")
        {
            return basePath + "saturn5/saturn5_Explosion_horizontal.xml";                              
        }
        else if (_model == "Saturn5_vertical")
        {
            return basePath + "saturn5/saturn5_Explosion_vertical.xml";
        }
        else
        {
            return null;
        }
    }

    private void OnEndEditStandaloneFreeDemoPanelFileURL(string fileUrl)
    {
        if (!fileUrl.StartsWith("http") & !fileUrl.StartsWith("file:///"))
        {
            // Local file
            string filesPath = Application.dataPath.Substring(0, Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/")).LastIndexOf("/"));
            string basepath = "file:///" + filesPath;
            fileUrl = basepath + "/experiment/navigation/scenarios/models/" + fileUrl;
        }

        Debug.Log(fileUrl);
        if (fileUrl.EndsWith(".json"))
        {
            hom3r.quickLinks.scriptsObject.GetComponent<StandaloneManager>().LoadModelFromUI(fileUrl);
            SetActivatedStandAlonePanel(false);
        }
        else if(fileUrl.EndsWith(".obj"))
        {            
            string fileName = fileUrl.Substring(fileUrl.LastIndexOfAny(new char[] { '/', '\\' }) + 1);                        
            hom3r.quickLinks.scriptsObject.GetComponent<StandaloneManager>().LoadOBJFile(fileName, fileUrl);
            SetActivatedStandAlonePanel(false);
        } else
        {
            // DO nothing
            hom3r.coreLink.Do(new CUICommand(TUICommands.ShowAlertText, "Error: Bad file extension."));
        }
        
    }

    private void OnClickStandaloneInitialPanelLoadExperimentButton()
    {
        Debug.Log("Load Experiment");
        SetActivatedStandAlonePanel(false);
        hom3r.quickLinks.scriptsObject.AddComponent<ExperimentManager>();  // Attach the script object
        
    }


    private void OnClickStandaloneInitialPanelFreeDemoButton()
    {
        standaloneFreeDemoPanelGO.SetActive(true);
        standaloneInitialPanelGO.SetActive(false);        
    }


}
