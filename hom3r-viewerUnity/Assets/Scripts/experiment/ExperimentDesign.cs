using System.Collections;
using System.Collections.Generic;

//[System.Serializable]
//public class CPlotLabelPosition
//{
//    public float x;
//    public float y;
//    public float z;

//    public CPlotLabelPosition(float _x, float _y, float _z)
//    {
//        this.x = _x;
//        this.y = _y;
//        this.z = _z;
//    }
//}

//[System.Serializable]
//public class CPlotSelectedAreaData
//{
//    public string areaID;
//    public string colour;
//    public CPlotSelectedAreaData() { }
//    public CPlotSelectedAreaData(string _areaID, string _colour)
//    {
//        this.areaID = _areaID;
//        this.colour = _colour;
//    }
//}

//[System.Serializable]
//public class CPlotHiddenAreaData
//{
//    public string id;
//    public CPlotHiddenAreaData() { }
//    public CPlotHiddenAreaData(string _areaID)
//    {
//        this.id = _areaID;
//    }
//}

//[System.Serializable]
//public class CPlotLabelData
//{
//    public string areaID;
//    public string text;
//    public string language;
//    public CPlotLabelPosition position;

//    public CPlotLabelData() { }

//    public CPlotLabelData(string _areaID, string _text, string _language, CPlotLabelPosition _position)
//    {
//        this.areaID = _areaID;
//        this.text = _text;
//        this.language = _language;
//        this.position = _position;
//    }
//}

[System.Serializable]
public class CTrialViewfinderData
{
    public int id;
    public string targetID;    
    public CPlotLabelPosition position;
    public float radious;

    public CTrialViewfinderData() { }

    public CTrialViewfinderData(int _id, string _targetId, CPlotLabelPosition _position, float _radious)
    {
        this.targetID = _targetId;
        this.id = _id;             
        this.position = _position;
        this.radious = _radious;
    }    
    public CTrialViewfinderData(int _id, string _targetId, float _positionX, float _positionY, float _positionZ, float _radious)
    {
        this.targetID = _targetId;
        this.id = _id;
        position = new CPlotLabelPosition(_positionX, _positionY, _positionZ);        
        this.radious = _radious;
    }
}

[System.Serializable]
public class CTrial
{
    public int trialNumber;
    public List<CPlotSelectedAreaData> selectedAreaList;
    public List<CPlotHiddenAreaData> hiddenAreaList;    
}

[System.Serializable]
public class CViewfinders
{
    public string areaID;
    public List<CTrialViewfinderData> viewfinderList;
}

[System.Serializable]
public class CZoomSteps
{
    public float minimum;
    public float maximum;
}

[System.Serializable]
public class CBlock
{
    public int blockNumber;
    public int repetitionNumber;
    public string productModelUrl;
    public string navigationSystem;
    public bool panNavigation;
    public bool zoomMinimumLimited;
    public bool zoomMaximumLimited;
    public CZoomSteps zoomLimits;
    public List<CTrial> trials;
    public List<CViewfinders> viewfinders;
    public string trialOrderId;

    public CBlock()
    {
        trials = new List<CTrial>();
        viewfinders = new List<CViewfinders>();
    }

    public CBlock (CBlock _block)
    {
        blockNumber = _block.blockNumber;
        repetitionNumber = _block.repetitionNumber;
        productModelUrl = _block.productModelUrl;
        navigationSystem = _block.navigationSystem;
        panNavigation = _block.panNavigation;
        zoomMinimumLimited = _block.zoomMinimumLimited;
        zoomMaximumLimited = _block.zoomMaximumLimited;
        zoomLimits = _block.zoomLimits;
        trials = new List<CTrial>(_block.trials);
        viewfinders = new List<CViewfinders>(_block.viewfinders);
    }
}

[System.Serializable]
public class COrders
{
    public string id;
    public List<CTrial> trials;    

    public COrders()
    {
        trials = new List<CTrial>();
    }
}

[System.Serializable]
public class CSession
{
    public int sessionNumber;
    public List<CBlock> blocks;
  
    //public List<CViewfinders> viewfinders;

    public CSession() {
        sessionNumber = 0;
        blocks = new List<CBlock>();
        //viewfinders = new List<CViewfinders>();
    }
}

[System.Serializable]
public class CParticipantList
{    
    public int participantId;
//    public string plotUrl;    
    public List<CSession> sessions;
    //public List<CPlotSelectedAreaData> selectedAreaList;
    //public List<CPlotHiddenAreaData> hiddenAreaList;
    //public List<CPlotLabelData> labelList;
    //public List<CPlotViewfinderData> viewfinderList;   
}

[System.Serializable]
public class CExperimentDesign
{
    public string experimentTitle;
    public string experimentDescription;
    public string dateTime;    
    public List<CParticipantList> participantList;
    public List<COrders> trialOrders;
    public List<CViewfinders> viewfinders;

    public CExperimentDesign() {
        participantList = new List<CParticipantList>();
        trialOrders = new List<COrders>();
    }
    
    public void Clear()
    {
        participantList.Clear();
        experimentTitle = null;
        experimentDescription = null;
        dateTime = null;
    }
}
