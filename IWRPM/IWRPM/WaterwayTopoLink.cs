using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GeoAPI.Geometries;


namespace IWRPM
{
    /// <summary>
    /// WaterwayTopoLink 航道拓扑线用于保存航道网络拓扑线数据
    /// WaterwayTopoLink定义了与航道拓扑线表ShapeFile属性名称对应的字段物理存储结构
    /// </summary>
    public class WaterwayTopoLink
    {
        //航道拓扑线标识
        public string waterLinkID;
        //航道上游拓扑点标识
        public string upStreamWaterNodeID;
        //航道下游拓扑点标识
        public string downStreamWaterNodeID;
        //是否单行
        public bool oneWay = true;
        //航道内交通流/通航方向
        public int trafficDirection = 0;
        //航道拓扑线等级
        public int waterLinkClass;
        //所在航道名称
        public string channelName;
        //航道拓扑线类型
        public int waterLinkType;
        //航道维护等级
        public int channelClass;
        //航道属性
        public int channelType;
        //航道长度/里程（对应该段拓扑线的几何长度）
        public double channelLength;
        //航道平均通航时间
        public double channelAverageTravelTime;
        //航道维护水深
        public float channelDepth = 1000;
        //航道维护宽度
        public float channelWidth = 1000;
        //航道维护弯曲半径
        public float channelRadius = 1000;
        //通过桥梁数量
        public int bridgeNumber = 0;
        //通过桥梁要素索引
        public string bridgeReference = "";
        //桥梁中文名称
        public string bridgeName = "";
        //桥梁中文信息
        public string bridgeInfomation = "";
        //桥梁通航净空高度
        public float verticalClearanceHeight = 1000;
        //桥梁通航净空宽度
        public float horizontalClearanceWidth = 1000;
        //桥梁通航侧高
        public float verticalBysideHeight = 1000;
        //桥梁通航上底宽
        public float horizontalUpsideWidth = 1000;
        //通过船闸数量
        public int lockNumber = 0;
        //通过船闸要素索引
        public string lockReference = "";
        //船闸中文名称
        public string lockName = "";
        //船闸中文信息
        public string lockInformation = "";
        //船闸线数
        public int lockLineNumber;
        //通过船闸级别
        public float lockClass;
        //船闸有效长度
        public float lockEffectiveLength;
        //船闸有效宽度
        public float lockEffectiveWidth;
        //船闸门槛水深
        public float lockSignificantDepth;
        //船闸通航规则
        public string lockNavigationRules;
        //船闸管道设标情况
        public string lockPipingStatus;
        //船闸状况
        public int lockStatus;
        //通航水位
        public float navigableWaterLevel;
        //实时航道状况事件标识列表
        public string channelConditionsEventIDList;
        //航道几何
        public Coordinate[] channelGeometry;

        public WaterwayTopoLink()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        /// <summary>
        /// 该构造方法没有使用
        /// </summary>
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

        /// <summary>
        /// 需要一个参数为本类的构造方法
        /// </summary>
        /// <param name="_waterwayTopoLink"></param>
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
            this.bridgeReference = _waterwayTopoLink.bridgeReference;
            this.bridgeName = _waterwayTopoLink.bridgeName;
            this.bridgeInfomation = _waterwayTopoLink.bridgeInfomation;
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