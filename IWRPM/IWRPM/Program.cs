using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
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

    /// <summary>
    /// WaterwayTopoLink 航道拓扑线用于保存航道网络拓扑线数据
    /// </summary>
    class WaterwayTopoLink
    {
        public string waterLinkID;
        public string upStreamWaterNodeID;
        public string downStreamWaterNodeID;
        public bool oneWay;
        public byte trafficDirection;
        public byte waterLinkClass;
        public string channelName;
        public byte waterLinkType;
        public byte channelClass;
        public byte channelType;
        public double channelLength;
        public double channelAverageTravelTime;
        public float channelDepth;
        public float channelWidth;
        public float channelRadius;
        public byte bridgeNumber;
        public string bridgeName;
        public string bridgeInfomation;
        public float verticalClearanceHeight;
        public float horizontalClearanceWidth;
        public float verticalBysideHeight;
        public float horizontalUpsideWidth;
        public byte lockNumber;
        public string lockName;
        public string lockInformation;
        public byte lockLineNumber;
        public float lockClass;
        public float lockEffectiveLength;
        public float lockEffectiveWidth;
        public float lockSignificantDepth;
        public string lockNavigationRules;
        public string lockPipingStatus;
        public byte lockStatus;
        public float navigableWaterLevel;
        public string channelConditionsEventIDList;
        public Coordinate[] channelGeometry;

        public WaterwayTopoLink()
        {
        }

        public WaterwayTopoLink(
            string _waterLinkID,
            string _upStreamWaterNodeID,
            string _downStreamWaterNodeID,
            bool _oneWay,
            byte _trafficDirection,
            byte _waterLinkClass,
            string _channelName,
            byte _waterLinkType,
            byte _channelClass,
            double _channelLength,
            double _channelAverageTravelTime,
            float _channelDepth,
            float _channelWidth,
            byte _bridgeNumber,
            float _verticalClearanceHeight,
            float _horizontalClearanceWidth,
            byte _lockNumber,
            byte _lockClass,
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
    }

    class WaterwayGraph
    {
        string _shpfileDatasetsPath = @"D:\GuangDongENCProject\Datasets\WaterwayNetworkDatasets";

        #region WaterwayGraph Members

        Dictionary<string, WaterwayTopoNode> m_dicWaterwayNode = new Dictionary<string, WaterwayTopoNode>();
        Dictionary<string, WaterwayTopoLink> m_dicWaterwayLink = new Dictionary<string, WaterwayTopoLink>();

        #endregion


        /// <summary>
        /// Load WaterwayNode Datasets
        /// </summary>
        /// <param name="_shpfileWaterwayNodeDatasetsPath"></param>
        /// <returns></returns>
        public Dictionary<string, WaterwayTopoNode> LoadWaterwayNodeDatasets(string _shpfileWaterwayNodeDatasetsPath)
        {
            Dictionary<string, WaterwayTopoNode> _dicWaterwayNode = new Dictionary<string, WaterwayTopoNode>();

            ShapefileDataReader _readerWaterwayNodeDatasets = new ShapefileDataReader(_shpfileWaterwayNodeDatasetsPath, GeometryFactory.Default);

            DbaseFileHeader _headerWaterwayNodeDatasets = _readerWaterwayNodeDatasets.DbaseHeader;

            long _numRecordsWaterwayNodeDatasets = _headerWaterwayNodeDatasets.NumRecords;

            long _countNumRecordWaterwayNodeDatasets = 0;

            WaterwayTopoNode waterwayTopoNodeTemp = new WaterwayTopoNode();

            while (_readerWaterwayNodeDatasets.Read())
            {
                for (int i = 0; i < _headerWaterwayNodeDatasets.NumFields; i++)
                {
                    switch (_headerWaterwayNodeDatasets.Fields[i].Name)
                    {
                        case "WNODID":
                            waterwayTopoNodeTemp.waterNodeID = _readerWaterwayNodeDatasets.GetString(i + 1);
                            break;
                        case "WNDGRA":
                            waterwayTopoNodeTemp.waterNodeClass = _readerWaterwayNodeDatasets.GetInt32(i + 1);
                            break;
                        case "TPNGRA":
                            waterwayTopoNodeTemp.waterNodeType = _readerWaterwayNodeDatasets.GetInt32(i + 1);
                            break;
                        case "XCOORD":
                            waterwayTopoNodeTemp.waterNodeCoordinate[0] = _readerWaterwayNodeDatasets.Geometry.Coordinate.X;
                            break;
                        case "YCOORD":
                            waterwayTopoNodeTemp.waterNodeCoordinate[1] = _readerWaterwayNodeDatasets.Geometry.Coordinate.Y;
                            break;
                        case "NINFOM":
                            waterwayTopoNodeTemp.waterNodeInformation = _readerWaterwayNodeDatasets.GetString(i + 1);
                            break;
                        case "NOBJNM":
                            waterwayTopoNodeTemp.waterNodeName = _readerWaterwayNodeDatasets.GetString(i + 1);
                            break;
                        case "NUMWLI":
                            waterwayTopoNodeTemp.waterLinkInNumber = _readerWaterwayNodeDatasets.GetInt32(i + 1);
                            break;
                        case "NUMWLO":
                            waterwayTopoNodeTemp.waterLinkOutNumber = _readerWaterwayNodeDatasets.GetInt32(i + 1);
                            break;
                        case "LITWLI":
                            waterwayTopoNodeTemp.waterLinkInList = _readerWaterwayNodeDatasets.GetString(i + 1).Split(',');
                            break;
                        case "LITWLO":
                            waterwayTopoNodeTemp.waterLinkOutList = _readerWaterwayNodeDatasets.GetString(i + 1).Split(',');
                            break;
                    }
                }
                _countNumRecordWaterwayNodeDatasets += 1;
                _dicWaterwayNode.Add(waterwayTopoNodeTemp.waterNodeID, waterwayTopoNodeTemp);
                Console.WriteLine("{0} WaterwayNode has been loaded({1}/{2})!", waterwayTopoNodeTemp.waterNodeID, _countNumRecordWaterwayNodeDatasets, _numRecordsWaterwayNodeDatasets);
            }

            return _dicWaterwayNode;
        }


        /// <summary>
        /// Load WaterwayLink Dataset
        /// </summary>
        /// <param name="_shpfileWaterwayLinkDatasetsPath"></param>
        /// <returns></returns>
        public Dictionary<string, WaterwayTopoLink> LoadWaterwayLinkDatasets(string _shpfileWaterwayLinkDatasetsPath)
        {
            Dictionary<string, WaterwayTopoLink> _dicWaterwayLink = new Dictionary<string, WaterwayTopoLink>();

            ShapefileDataReader _readerWaterwayLinkDatasets = new ShapefileDataReader(_shpfileWaterwayLinkDatasetsPath, GeometryFactory.Default);

            DbaseFileHeader _headerWaterwayLinkDatasets = _readerWaterwayLinkDatasets.DbaseHeader;

            long _numRecordsWaterwayLinkDatasets = _headerWaterwayLinkDatasets.NumRecords;

            long _countNumRecordWaterwayLinkDatasets = 0;

            WaterwayTopoLink waterwayTopoLinkTemp = new WaterwayTopoLink();

            while (_readerWaterwayLinkDatasets.Read())
            {
                for (int i = 0; i < _headerWaterwayLinkDatasets.NumFields; i++)
                {
                    switch (_headerWaterwayLinkDatasets.Fields[i].Name)
                    {
                        case "WLIKID":
                            waterwayTopoLinkTemp.waterLinkID = _readerWaterwayLinkDatasets.GetString(i + 1);
                            break;
                        case "UWNCOD":
                            waterwayTopoLinkTemp.upStreamWaterNodeID = _readerWaterwayLinkDatasets.GetString(i + 1);
                            break;
                        case "DWNCOD":
                            waterwayTopoLinkTemp.downStreamWaterNodeID = _readerWaterwayLinkDatasets.GetString(i + 1);
                            break;
                        case "ONEWAY":
                            waterwayTopoLinkTemp.oneWay = (_readerWaterwayLinkDatasets.GetInt32(i + 1) == 0) ? false : true;
                            break;
                        case "TRFWAW":
                            waterwayTopoLinkTemp.trafficDirection = _readerWaterwayLinkDatasets.GetByte(i + 1);
                            break;
                        case "WLKGRA":
                            waterwayTopoLinkTemp.waterLinkClass = _readerWaterwayLinkDatasets.GetByte(i + 1);
                            break;
                        case "CODWLK":
                            waterwayTopoLinkTemp.waterLinkType = _readerWaterwayLinkDatasets.GetByte(i + 1);
                            break;
                        case "BLGCOD":
                            break;
                        case "BLGNAM":
                            waterwayTopoLinkTemp.channelName = _readerWaterwayLinkDatasets.GetString(i + 1);
                            break;
                        case "CODWAW":
                            waterwayTopoLinkTemp.channelType = _readerWaterwayLinkDatasets.GetByte(i + 1);
                            break;
                        case "LENWLK":
                            waterwayTopoLinkTemp.channelLength = _readerWaterwayLinkDatasets.Geometry.Length;
                            break;
                        case "ATTWLK":
                            waterwayTopoLinkTemp.channelAverageTravelTime = _readerWaterwayLinkDatasets.GetDouble(i + 1);
                            break;
                        case "MAINTG":
                            waterwayTopoLinkTemp.channelClass = _readerWaterwayLinkDatasets.GetByte(i + 1);
                            break;
                        case "DEPWAW":
                            waterwayTopoLinkTemp.channelDepth = _readerWaterwayLinkDatasets.GetFloat(i + 1);
                            break;
                        case "WIDWAW":
                            waterwayTopoLinkTemp.channelWidth = _readerWaterwayLinkDatasets.GetFloat(i + 1);
                            break;
                        case "CRDWAW":
                            waterwayTopoLinkTemp.channelRadius = _readerWaterwayLinkDatasets.GetFloat(i + 1);
                            break;
                        case "BRGNUM":
                            waterwayTopoLinkTemp.bridgeNumber = _readerWaterwayLinkDatasets.GetByte(i + 1);
                            break;
                        case "BRGCOD":                            
                            break;
                        case "INFBRG":
                            waterwayTopoLinkTemp.bridgeInfomation = _readerWaterwayLinkDatasets.GetString(i + 1);
                            break;
                        case "NAMBRG":
                            waterwayTopoLinkTemp.bridgeName = _readerWaterwayLinkDatasets.GetString(i + 1);
                            break;
                        case "NCWBNO":
                            waterwayTopoLinkTemp.horizontalClearanceWidth = _readerWaterwayLinkDatasets.GetFloat(i + 1);
                            break;
                        case "NCHBNO":
                            waterwayTopoLinkTemp.verticalClearanceHeight = _readerWaterwayLinkDatasets.GetFloat(i + 1);
                            break;
                        case "NUWBNO":
                            waterwayTopoLinkTemp.horizontalUpsideWidth = _readerWaterwayLinkDatasets.GetFloat(i + 1);
                            break;
                        case "NBHBNO":
                            waterwayTopoLinkTemp.verticalBysideHeight = _readerWaterwayLinkDatasets.GetFloat(i + 1);
                            break;
                        case "LOBNUM":
                            waterwayTopoLinkTemp.lockNumber = _readerWaterwayLinkDatasets.GetByte(i + 1);
                            break;
                        case "LOBCOD":
                            break;
                        case "LOBLVL":
                            waterwayTopoLinkTemp.lockClass = _readerWaterwayLinkDatasets.GetByte(i + 1);
                            break;
                        case "LOBLIN":
                            waterwayTopoLinkTemp.lockLineNumber = _readerWaterwayLinkDatasets.GetByte(i + 1);
                            break;
                        case "EFLLOB":
                            waterwayTopoLinkTemp.lockEffectiveLength = _readerWaterwayLinkDatasets.GetFloat(i + 1);
                            break;
                        case "EFWLOB":
                            waterwayTopoLinkTemp.lockEffectiveWidth = _readerWaterwayLinkDatasets.GetFloat(i + 1);
                            break;
                        case "MWDASI":
                            waterwayTopoLinkTemp.lockSignificantDepth = _readerWaterwayLinkDatasets.GetFloat(i + 1);
                            break;
                        case "NARLOB":
                            waterwayTopoLinkTemp.lockNavigationRules = _readerWaterwayLinkDatasets.GetString(i + 1);
                            break;
                        case "MASLOB":
                            waterwayTopoLinkTemp.lockPipingStatus = _readerWaterwayLinkDatasets.GetString(i + 1);
                            break;
                        case "STALOB":
                            waterwayTopoLinkTemp.lockStatus = _readerWaterwayLinkDatasets.GetByte(i + 1);
                            break;
                        case "INFLOB":
                            waterwayTopoLinkTemp.lockInformation = _readerWaterwayLinkDatasets.GetString(i + 1);
                            break;
                        case "NAMLOB":
                            waterwayTopoLinkTemp.lockName = _readerWaterwayLinkDatasets.GetString(i + 1);
                            break;
                        case "NAVWAL":
                            waterwayTopoLinkTemp.navigableWaterLevel = _readerWaterwayLinkDatasets.GetFloat(i + 1);
                            break;
                        case "CCEIDL":
                            waterwayTopoLinkTemp.channelConditionsEventIDList = _readerWaterwayLinkDatasets.GetString(i + 1);
                            break;
                        case "WLKGEO":
                            waterwayTopoLinkTemp.channelGeometry = _readerWaterwayLinkDatasets.Geometry.Coordinates;
                            break;
                    }
                }
                _countNumRecordWaterwayLinkDatasets += 1;
                _dicWaterwayLink.Add(waterwayTopoLinkTemp.waterLinkID, waterwayTopoLinkTemp);
                Console.WriteLine("{0} WaterwayLink has been loaded({1}/{2})!", waterwayTopoLinkTemp.waterLinkID, _countNumRecordWaterwayLinkDatasets, _numRecordsWaterwayLinkDatasets);
            }

            return _dicWaterwayLink;
        }

        public void LoadWaterwayNetworkDatasets()
        {
            m_dicWaterwayNode = LoadWaterwayNodeDatasets(Path.Combine(_shpfileDatasetsPath, "WaterwayNode.shp"));
            m_dicWaterwayLink = LoadWaterwayLinkDatasets(Path.Combine(_shpfileDatasetsPath, "WaterwayLink.shp"));

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
