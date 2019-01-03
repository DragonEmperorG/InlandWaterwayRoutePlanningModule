using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace IWRPM
{
    /// <summary>
    /// WaterwayTopoNode 航道拓扑点用于保存航道网络拓扑点数据
    /// 属于基础的数据结构
    /// </summary>
    public class WaterwayTopoNode
    {
        //航道拓扑点标识/要素代码
        public string waterNodeID;
        //航道拓扑点坐标
        public double[] waterNodeCoordinate = new double[2];
        //航道拓扑点名称
        public string waterNodeName;
        //航道拓扑点信息
        public string waterNodeInformation;
        //航道拓扑点等级
        public int waterNodeClass;
        //航道拓扑点类型
        public int waterNodeType;
        //驶入航道拓扑线数量
        public int waterLinkInNumber;
        //驶出航道拓扑线数量
        public int waterLinkOutNumber;
        //驶入航道拓扑线标识列表
        public string[] waterLinkInList;
        //驶出航道拓扑线标识列表
        public string[] waterLinkOutList;

        public WaterwayTopoNode()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        /// <summary>
        /// 该构造方法在最终程序中未使用
        /// </summary>
        /// <param name="_waterNodeID"></param>
        /// <param name="_waterNodeCoordinate"></param>
        /// <param name="_waterNodeName"></param>
        /// <param name="_waterNodeType"></param>
        /// <param name="_waterLinkInNumber"></param>
        /// <param name="_waterLinkOutNumber"></param>
        /// <param name="_waterLinkInList"></param>
        /// <param name="_waterLinkOutList"></param>
        /// <param name="_waterNodeClass"></param>
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

        /// <summary>
        /// 需要一个自定义一个深拷贝的方法
        /// </summary>
        /// <param name="_waterwayTopoNode"></param>
        /// <returns></returns>
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