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

        public static WaterwayTopoNode ConstructNewInstanceFromExistClass(WaterwayTopoNode _waterwayTopoNode)
        {
            var waterwayTopoNodeNew = new WaterwayTopoNode();
            waterwayTopoNodeNew.waterNodeID = new string(_waterwayTopoNode.waterNodeID.ToCharArray());
            waterwayTopoNodeNew.waterNodeCoordinate = new double[2] { _waterwayTopoNode.waterNodeCoordinate[0], _waterwayTopoNode.waterNodeCoordinate[1] };
            waterwayTopoNodeNew.waterNodeName = new string(_waterwayTopoNode.waterNodeName.ToCharArray()); ;
            waterwayTopoNodeNew.waterNodeType = _waterwayTopoNode.waterNodeType;
            waterwayTopoNodeNew.waterLinkInNumber = _waterwayTopoNode.waterLinkInNumber;
            waterwayTopoNodeNew.waterLinkOutNumber = _waterwayTopoNode.waterLinkOutNumber;
            var waterLinkInListNew = new List<string>();
            for (var i = 0; i < _waterwayTopoNode.waterLinkInNumber; i++)
            {
                waterLinkInListNew.Add(_waterwayTopoNode.waterLinkInList[i]);
            }
            waterwayTopoNodeNew.waterLinkInList = waterLinkInListNew.ToArray();
            var waterLinkOutListNew = new List<string>();
            for (var i = 0; i < _waterwayTopoNode.waterLinkOutNumber; i++)
            {
                waterLinkOutListNew.Add(_waterwayTopoNode.waterLinkOutList[i]);
            }
            waterwayTopoNodeNew.waterLinkOutList = waterLinkOutListNew.ToArray();
            waterwayTopoNodeNew.waterNodeClass = _waterwayTopoNode.waterNodeClass;

            return waterwayTopoNodeNew;
        }
    }
}