using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace IWRPM
{
    /// <summary>
    /// WaterwayTopoNode 航道拓扑点用于保存航道网络拓扑点数据
    /// </summary>
    public class WaterwayTopoNode
    {
        public string waterNodeID;
        public double[] waterNodeCoordinate = new double[2];
        public string waterNodeName;
        public string waterNodeInformation;
        public int waterNodeClass;
        public int waterNodeType;
        public int waterLinkInNumber;
        public int waterLinkOutNumber;
        public string[] waterLinkInList;
        public string[] waterLinkOutList;

        public WaterwayTopoNode()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        public WaterwayTopoNode(
                string _waterNodeID,
                double[] _waterNodeCoordinate,
                string _waterNodeName,
                int _waterNodeType,
                int _waterLinkInNumber,
                int _waterLinkOutNumber,
                string[] _waterLinkInList,
                string[] _waterLinkOutList,
                int _waterNodeClass
            )
        {
            this.waterNodeID = _waterNodeID;
            this.waterNodeCoordinate = _waterNodeCoordinate;
            this.waterNodeName = _waterNodeName;
            this.waterNodeType = _waterNodeType;
            this.waterLinkInNumber = _waterLinkInNumber;
            this.waterLinkOutNumber = _waterLinkOutNumber;
            this.waterLinkInList = _waterLinkInList;
            this.waterLinkOutList = _waterLinkOutList;
            this.waterNodeClass = _waterNodeClass;

        }
    }
}