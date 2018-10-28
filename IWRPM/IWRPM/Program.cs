using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.IO;

/// <summary>
/// 最优航线规划模型原型程序
/// QIAN LONG 2018/10/28
/// </summary>
namespace IWRPM
{
    /// <summary>
    /// WaterwayTopoNode 航道拓扑点用于保存航道网络拓扑点数据
    /// </summary>
    class WaterwayTopoNode
    {
        private string waterNodeID;
        private string waterNodeCoordinate;
        private string waterNodeName;
        private string waterNodeType;
        private string waterLinkInNumber;
        private string waterLinkOutNumber;
        private string waterLinkInList;
        private string waterLinkOutList;
        private string waterNodeClass;

        public WaterwayTopoNode(
            string _waterNodeID,
            string _waterNodeCoordinate,
            string _waterNodeName,
            string _waterNodeType,
            string _waterLinkInNumber,
            string _waterLinkOutNumber,
            string _waterLinkInList,
            string _waterLinkOutList,
            string _waterNodeClass
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

    /// <summary>
    /// WaterwayTopoLink 航道拓扑点用于保存航道网络拓扑线数据
    /// </summary>
    class WaterwayTopoLink
    {
        private string waterLinkID;
        private string upStreamWaterNodeID;
        private string downStreamWaterNodeID;
        private string oneWay;
        private string waterDirection;
        private string waterLinkClass;
        private string channelName;
        private string waterLinkType;
        private string channelClass;
        private string channelLength;
        private string channelAverageTravelTime;
        private string channelDepth;
        private string channelWidth;
        private string bridgeNumber;
        private string verticalClearanceHeight;
        private string horizontalClearanceWidth;
        private string lockNumber;
        private string lockClassification;
        private string lockEffectiveLength;
        private string lockEffectiveWidth;
        private string lockSignificantDepth;
        private string navigableWaterLevel;
        private string channelConditionsEventIDList;
        private string channelGeometry;

        public WaterwayTopoLink(
            string _waterLinkID,
            string _upStreamWaterNodeID,
            string _downStreamWaterNodeID,
            string _oneWay,
            string _waterDirection,
            string _waterLinkClass,
            string _channelName,
            string _waterLinkType,
            string _channelClass,
            string _channelLength,
            string _channelAverageTravelTime,
            string _channelDepth,
            string _channelWidth,
            string _bridgeNumber,
            string _verticalClearanceHeight,
            string _horizontalClearanceWidth,
            string _lockNumber,
            string _lockClassification,
            string _lockEffectiveLength,
            string _lockEffectiveWidth,
            string _lockSignificantDepth,
            string _navigableWaterLevel,
            string _channelConditionsEventIDList,
            string _channelGeometry
        )
        {
            this.waterLinkID = _waterLinkID;
            this.upStreamWaterNodeID = _upStreamWaterNodeID;
            this.downStreamWaterNodeID = _downStreamWaterNodeID;
            this.oneWay = _oneWay;
            this.waterDirection = _waterDirection;
            this.waterLinkClass = _waterLinkClass;
            this.channelName = _channelName;
            this.waterLinkType = _waterLinkType;
            this.channelClass = _channelClass;
            this.channelLength = _channelLength;
            this.channelAverageTravelTime = _channelAverageTravelTime;
            this.channelDepth = _channelDepth;
            this.channelWidth = _channelWidth;
            this.bridgeNumber = _bridgeNumber;
            this.verticalClearanceHeight = _verticalClearanceHeight;
            this.horizontalClearanceWidth = _horizontalClearanceWidth;
            this.lockNumber = _lockNumber;
            this.lockClassification = _lockClassification;
            this.lockEffectiveLength = _lockEffectiveLength;
            this.lockEffectiveWidth = _lockEffectiveWidth;
            this.lockSignificantDepth = _lockSignificantDepth;
            this.navigableWaterLevel = _navigableWaterLevel;
            this.channelConditionsEventIDList = _channelConditionsEventIDList;
            this.channelGeometry = _channelGeometry;
        }
    }

    class WaterwayGraph
    {
        GDBReader 

        public void LoadWaterwayNetworkDatasets()
        {
            Console.WriteLine("Waterway Network Datasets have been loaded!");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var guangDongWaterwayGraph = new WaterwayGraph();
            guangDongWaterwayGraph.LoadWaterwayNetworkDatasets();

            Console.ReadLine();
        }
    }
}
