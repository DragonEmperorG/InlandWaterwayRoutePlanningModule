using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GeoAPI.Geometries;


namespace IWRPM
{
    /// <summary>
    /// WaterwayTopoLink 航道拓扑线用于保存航道网络拓扑线数据
    /// </summary>
    public class WaterwayTopoLink
    {
        public string waterLinkID;
        public string upStreamWaterNodeID;
        public string downStreamWaterNodeID;
        public bool oneWay = true;
        public int trafficDirection = 0;
        public int waterLinkClass;
        public string channelName;
        public int waterLinkType;
        public int channelClass;
        public int channelType;
        public double channelLength;
        public double channelAverageTravelTime;
        public float channelDepth = 1000;
        public float channelWidth = 1000;
        public float channelRadius = 1000;
        public int bridgeNumber = 0;
        public string bridgeReference = "";
        public string bridgeName = "";
        public string bridgeInfomation = "";
        public float verticalClearanceHeight = 1000;
        public float horizontalClearanceWidth = 1000;
        public float verticalBysideHeight = 1000;
        public float horizontalUpsideWidth = 1000;
        public int lockNumber = 0;
        public string lockReference = "";
        public string lockName = "";
        public string lockInformation = "";
        public int lockLineNumber;
        public float lockClass;
        public float lockEffectiveLength;
        public float lockEffectiveWidth;
        public float lockSignificantDepth;
        public string lockNavigationRules;
        public string lockPipingStatus;
        public int lockStatus;
        public float navigableWaterLevel;
        public string channelConditionsEventIDList;
        public Coordinate[] channelGeometry;

        public WaterwayTopoLink()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        public WaterwayTopoLink(
                string _waterLinkID,
                string _upStreamWaterNodeID,
                string _downStreamWaterNodeID,
                bool _oneWay,
                int _trafficDirection,
                int _waterLinkClass,
                string _channelName,
                int _waterLinkType,
                int _channelClass,
                double _channelLength,
                double _channelAverageTravelTime,
                float _channelDepth,
                float _channelWidth,
                int _bridgeNumber,
                float _verticalClearanceHeight,
                float _horizontalClearanceWidth,
                int _lockNumber,
                int _lockClass,
                float _lockEffectiveLength,
                float _lockEffectiveWidth,
                float _lockSignificantDepth,
                float _navigableWaterLevel,
                string _channelConditionsEventIDList,
                Coordinate[] _channelGeometry
            )
        {
            this.waterLinkID = _waterLinkID;
            this.upStreamWaterNodeID = _upStreamWaterNodeID;
            this.downStreamWaterNodeID = _downStreamWaterNodeID;
            this.oneWay = _oneWay;
            this.trafficDirection = _trafficDirection;
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
            this.lockClass = _lockClass;
            this.lockEffectiveLength = _lockEffectiveLength;
            this.lockEffectiveWidth = _lockEffectiveWidth;
            this.lockSignificantDepth = _lockSignificantDepth;
            this.navigableWaterLevel = _navigableWaterLevel;
            this.channelConditionsEventIDList = _channelConditionsEventIDList;
            this.channelGeometry = _channelGeometry;
        }

        public WaterwayTopoLink(WaterwayTopoLink _waterwayTopoLink)
        {
            this.waterLinkID = _waterwayTopoLink.waterLinkID;
            this.upStreamWaterNodeID = _waterwayTopoLink.upStreamWaterNodeID;
            this.downStreamWaterNodeID = _waterwayTopoLink.downStreamWaterNodeID;
            this.oneWay = _waterwayTopoLink.oneWay;
            this.trafficDirection = _waterwayTopoLink.trafficDirection;
            this.waterLinkClass = _waterwayTopoLink.waterLinkClass;
            this.channelName = _waterwayTopoLink.channelName;
            this.waterLinkType = _waterwayTopoLink.waterLinkType;
            this.channelClass = _waterwayTopoLink.channelClass;
            this.channelLength = _waterwayTopoLink.channelLength;
            this.channelAverageTravelTime = _waterwayTopoLink.channelAverageTravelTime;
            this.channelDepth = _waterwayTopoLink.channelDepth;
            this.channelWidth = _waterwayTopoLink.channelWidth;
            this.bridgeNumber = _waterwayTopoLink.bridgeNumber;
            this.verticalClearanceHeight = _waterwayTopoLink.verticalClearanceHeight;
            this.horizontalClearanceWidth = _waterwayTopoLink.horizontalClearanceWidth;
            this.lockNumber = _waterwayTopoLink.lockNumber;
            this.lockClass = _waterwayTopoLink.lockClass;
            this.lockEffectiveLength = _waterwayTopoLink.lockEffectiveLength;
            this.lockEffectiveWidth = _waterwayTopoLink.lockEffectiveWidth;
            this.lockSignificantDepth = _waterwayTopoLink.lockSignificantDepth;
            this.navigableWaterLevel = _waterwayTopoLink.navigableWaterLevel;
            this.channelConditionsEventIDList = _waterwayTopoLink.channelConditionsEventIDList;
            this.channelGeometry = _waterwayTopoLink.channelGeometry;
        }
    }
}