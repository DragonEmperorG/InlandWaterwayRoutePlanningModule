using IWRPM;
using Shrulik.NKDBush;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;



namespace IWRPM
{
    /// <summary>
    /// WaterwayGraph 的摘要说明
    /// 航道拓扑网对象
    /// 负责存储从ShapeFile中读取到的所有的信息
    /// LoadWaterwayNetworkDatasets()
    /// |
    /// LoadWaterwayNodeDatasets()
    /// |
    /// LoadWaterwayLinkDatasets()
    /// |
    /// ConstructSpatialIndex()
    /// </summary>
    public class WaterwayGraph
    {
        //readonly string _shpfileDatasetsPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Datasets");
        public readonly string _shpfileDatasetsPath = "C:\\iis\\Datasets\\DatasetsUesdForTest\\WaterwayNetworkDatasets20181218";
        //readonly string _shpfileDatasetsPath = "D:\\GuangDongENCProject\\Datasets\\Issue02";
        #region WaterwayGraph Members
        
        // 标识航道拓扑网数据是否加载
        public bool isWaterwayNetworkDatasetsLoaded { get; set; } = false;
        // 存储航道拓扑点的字典
        public Dictionary<string, WaterwayTopoNode> m_dicWaterwayNode = new Dictionary<string, WaterwayTopoNode>();
        // 存储航道拓扑线的字典
        public Dictionary<string, WaterwayTopoLink> m_dicWaterwayLink = new Dictionary<string, WaterwayTopoLink>();
        // 存储航道拓扑点的空间索引
        public KDBush<WaterwayTopoNode> waterwayNodeSpatialIndex;

        #endregion

        public WaterwayGraph()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        public WaterwayGraph(WaterwayGraph _waterwayGraph)
        {
            this.isWaterwayNetworkDatasetsLoaded = _waterwayGraph.isWaterwayNetworkDatasetsLoaded;
            this.m_dicWaterwayNode = _waterwayGraph.m_dicWaterwayNode;
            this.m_dicWaterwayLink = _waterwayGraph.m_dicWaterwayLink;
            this.waterwayNodeSpatialIndex = _waterwayGraph.waterwayNodeSpatialIndex;
        }


        /// <summary>
        /// 自定义的航道拓扑网深拷贝的方法
        /// </summary>
        /// <param name="_waterwayGraph"></param>
        /// <returns></returns>
        public static WaterwayGraph ConstructNewInstanceFromExistClass(WaterwayGraph _waterwayGraph)
        {
            var waterwayGraphNew = new WaterwayGraph();
            bool isWaterwayNetworkDatasetsLoadedNew = new bool();
            Dictionary<string, WaterwayTopoNode> m_dicWaterwayNodeNew = new Dictionary<string, WaterwayTopoNode>();
            Dictionary<string, WaterwayTopoLink> m_dicWaterwayLinkNew = new Dictionary<string, WaterwayTopoLink>();
            KDBush<WaterwayTopoNode> waterwayNodeSpatialIndexNew;

            isWaterwayNetworkDatasetsLoadedNew = _waterwayGraph.isWaterwayNetworkDatasetsLoaded;
            foreach (var item in _waterwayGraph.m_dicWaterwayNode)
            {
                var waterwayTopoNodeNew = new WaterwayTopoNode();
                waterwayTopoNodeNew = WaterwayTopoNode.ConstructNewInstanceFromExistClass(item.Value);
                m_dicWaterwayNodeNew.Add(waterwayTopoNodeNew.waterNodeID, waterwayTopoNodeNew);
            }
            foreach (var item in _waterwayGraph.m_dicWaterwayLink)
            {
                var waterwayTopoLinkNew = new WaterwayTopoLink();
                waterwayTopoLinkNew = item.Value;
                //if (waterwayTopoLinkNew.waterLinkID == "GZGHD-0027-GZGHD-0026")
                //{
                //    Console.WriteLine(waterwayTopoLinkNew.waterLinkID);
                //}
                m_dicWaterwayLinkNew.Add(waterwayTopoLinkNew.waterLinkID, waterwayTopoLinkNew);
            }
            waterwayNodeSpatialIndexNew = new KDBush<WaterwayTopoNode>(m_dicWaterwayNodeNew.Values.ToArray(), p => p.waterNodeCoordinate[0], p => p.waterNodeCoordinate[1], nodeSize: 10);

            waterwayGraphNew.isWaterwayNetworkDatasetsLoaded = isWaterwayNetworkDatasetsLoadedNew;
            waterwayGraphNew.m_dicWaterwayNode = m_dicWaterwayNodeNew;
            waterwayGraphNew.m_dicWaterwayLink = m_dicWaterwayLinkNew;
            waterwayGraphNew.waterwayNodeSpatialIndex = waterwayNodeSpatialIndexNew;

            return waterwayGraphNew;
        }

        /// <summary>
        /// Load WaterwayNode Datasets
        /// 加载航道拓扑点ShapeFile数据的方法
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

            while (_readerWaterwayNodeDatasets.Read())
            {
                WaterwayTopoNode waterwayTopoNodeTemp = new WaterwayTopoNode();

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

                //if (waterwayTopoNodeTemp.waterLinkInNumber != waterwayTopoNodeTemp.waterLinkInList.Length)
                //{
                //    waterwayTopoNodeTemp.waterLinkInNumber = waterwayTopoNodeTemp.waterLinkInList.Length;
                //    Console.WriteLine("航道拓扑点 {0} NUMWLI属性填写错误！！！", waterwayTopoNodeTemp.waterNodeID);
                //}

                //if (waterwayTopoNodeTemp.waterLinkOutNumber != waterwayTopoNodeTemp.waterLinkOutList.Length)
                //{
                //    waterwayTopoNodeTemp.waterLinkOutNumber = waterwayTopoNodeTemp.waterLinkOutList.Length;
                //    Console.WriteLine("航道拓扑点 {0} NUMWLO属性填写错误！！！", waterwayTopoNodeTemp.waterNodeID);
                //}

                //if (waterwayTopoNodeTemp.waterNodeID == "XJ2DZZ-0037")
                //{
                //    Console.WriteLine(waterwayTopoNodeTemp.waterNodeID);
                //}
                _dicWaterwayNode.Add(waterwayTopoNodeTemp.waterNodeID, waterwayTopoNodeTemp);
                Console.WriteLine("{0} WaterwayNode has been loaded({1}/{2})!", waterwayTopoNodeTemp.waterNodeID, _countNumRecordWaterwayNodeDatasets, _numRecordsWaterwayNodeDatasets);
            }
            _readerWaterwayNodeDatasets.Close();
            return _dicWaterwayNode;
        }


        /// <summary>
        /// Load WaterwayLink Dataset
        /// 加载航道拓扑线ShapeFile数据的方法
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

            while (_readerWaterwayLinkDatasets.Read())
            {
                WaterwayTopoLink waterwayTopoLinkTemp = new WaterwayTopoLink();

                for (int i = 0; i < _headerWaterwayLinkDatasets.NumFields; i++)
                {
                    switch (_headerWaterwayLinkDatasets.Fields[i].Name)
                    {
                        case "WLIKID":
                            waterwayTopoLinkTemp.waterLinkID = _readerWaterwayLinkDatasets.GetString(i + 1);
                            waterwayTopoLinkTemp.channelGeometry = _readerWaterwayLinkDatasets.Geometry.Coordinates;
                            if (waterwayTopoLinkTemp.waterLinkID == "GZGHD1-0029")
                                Console.WriteLine(waterwayTopoLinkTemp.waterLinkID);
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
                            waterwayTopoLinkTemp.trafficDirection = _readerWaterwayLinkDatasets.GetInt32(i + 1);
                            break;
                        case "WLKGRA":
                            waterwayTopoLinkTemp.waterLinkClass = _readerWaterwayLinkDatasets.GetInt32(i + 1);
                            break;
                        case "CODWLK":
                            waterwayTopoLinkTemp.waterLinkType = _readerWaterwayLinkDatasets.GetInt32(i + 1);
                            break;
                        case "BLGCOD":
                            waterwayTopoLinkTemp.bridgeReference = _readerWaterwayLinkDatasets.GetString(i + 1);
                            break;
                        case "BLGNAM":
                            waterwayTopoLinkTemp.channelName = _readerWaterwayLinkDatasets.GetString(i + 1);
                            break;
                        case "CODWAW":
                            waterwayTopoLinkTemp.channelType = _readerWaterwayLinkDatasets.GetInt32(i + 1);
                            break;
                        case "LENWLK":
                            waterwayTopoLinkTemp.channelLength = _readerWaterwayLinkDatasets.GetDouble(i + 1);
                            break;
                        case "ATTWLK":
                            waterwayTopoLinkTemp.channelAverageTravelTime = _readerWaterwayLinkDatasets.GetDouble(i + 1);
                            break;
                        case "MAINTG":
                            waterwayTopoLinkTemp.channelClass = _readerWaterwayLinkDatasets.GetInt32(i + 1);
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
                            waterwayTopoLinkTemp.bridgeNumber = _readerWaterwayLinkDatasets.GetInt32(i + 1);
                            break;
                        case "BRGCOD":
                            waterwayTopoLinkTemp.bridgeReference = _readerWaterwayLinkDatasets.GetString(i + 1);
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
                            waterwayTopoLinkTemp.lockNumber = _readerWaterwayLinkDatasets.GetInt32(i + 1);
                            break;
                        case "LOBCOD":
                            waterwayTopoLinkTemp.lockReference = _readerWaterwayLinkDatasets.GetString(i + 1);
                            break;
                        case "LOBLVL":
                            waterwayTopoLinkTemp.lockClass = _readerWaterwayLinkDatasets.GetInt32(i + 1);
                            break;
                        case "LOBLIN":
                            waterwayTopoLinkTemp.lockLineNumber = _readerWaterwayLinkDatasets.GetInt32(i + 1);
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
                            waterwayTopoLinkTemp.lockStatus = _readerWaterwayLinkDatasets.GetInt32(i + 1);
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
                        //case "OBJECTID":
                        //    waterwayTopoLinkTemp.channelGeometry = _readerWaterwayLinkDatasets.Geometry.Coordinates;
                        //    break;
                    }
                }
                _countNumRecordWaterwayLinkDatasets += 1;
                _dicWaterwayLink.Add(waterwayTopoLinkTemp.waterLinkID, waterwayTopoLinkTemp);
                Console.WriteLine("{0} WaterwayLink has been loaded({1}/{2})!", waterwayTopoLinkTemp.waterLinkID, _countNumRecordWaterwayLinkDatasets, _numRecordsWaterwayLinkDatasets);
            }

            _readerWaterwayLinkDatasets.Close();
            return _dicWaterwayLink;
        }

        /// <summary>
        /// 构造航道拓扑点的空间索引
        /// </summary>
        /// <param name="_dicWaterwayNode"></param>
        /// <returns></returns>
        KDBush<WaterwayTopoNode> ConstructSpatialIndex(Dictionary<string, WaterwayTopoNode> _dicWaterwayNode)
        {
            return new KDBush<WaterwayTopoNode>(_dicWaterwayNode.Values.ToArray(), p => p.waterNodeCoordinate[0], p => p.waterNodeCoordinate[1], nodeSize: 10);
        }

        /// <summary>
        /// 在航线规划用用于判定船舶是否能穿过该段航道
        /// </summary>
        /// <param name="passingThroughWaterwayLinkID"></param>
        /// <param name="passingThroughVehicle"></param>
        /// <returns></returns>
        bool EstimatePassingThroughChannel(string passingThroughWaterwayLinkID, WaterwayVehicle passingThroughVehicle)
        {
            bool IsPassingThroughChannel = true;
            var passingThroughWaterwayLink = m_dicWaterwayLink[passingThroughWaterwayLinkID];

            if (passingThroughWaterwayLink.channelDepth < passingThroughVehicle.loadedDraft)
                IsPassingThroughChannel = false;

            if (passingThroughWaterwayLink.channelWidth < passingThroughVehicle.mouldedBreadth)
                IsPassingThroughChannel = false;

            if (passingThroughWaterwayLink.bridgeNumber != 0)
                if (passingThroughWaterwayLink.verticalClearanceHeight < passingThroughVehicle.freeboardHeight || passingThroughWaterwayLink.horizontalClearanceWidth < passingThroughVehicle.mouldedBreadth)
                    IsPassingThroughChannel = false;

            return IsPassingThroughChannel;
        }

        /// <summary>
        /// Get WaterwayNeighborNodeIDs
        /// 用于距离优先的航线规划，对于给定船只，用于获取从当前节点所能通行邻接的所有其他节点
        /// </summary>
        /// <param name="currentWaterwayNodeID"></param>
        /// <param name="currentVehicle"></param>
        /// <returns></returns>
        public IEnumerable<WaterwayNeighbor> Neighbors(string currentWaterwayNodeID, WaterwayVehicle currentVehicle)
        {
            string[] currentWaterwayLinkOutIDLists = m_dicWaterwayNode[currentWaterwayNodeID].waterLinkOutList;
            foreach (var currentWaterwayLinkOutID in currentWaterwayLinkOutIDLists)
            {
                if (!EstimatePassingThroughChannel(currentWaterwayLinkOutID, currentVehicle))
                    continue;
                string currentWaterwayLinkOutUpStreamWaterNodeID = m_dicWaterwayLink[currentWaterwayLinkOutID].upStreamWaterNodeID;
                string nextWaterwayNodeID = currentWaterwayNodeID != currentWaterwayLinkOutUpStreamWaterNodeID ? currentWaterwayLinkOutUpStreamWaterNodeID : m_dicWaterwayLink[currentWaterwayLinkOutID].downStreamWaterNodeID;

                if (!m_dicWaterwayNode.ContainsKey(nextWaterwayNodeID))
                    continue;

                WaterwayNeighbor nextWaterwayNeighbor = new WaterwayNeighbor(nextWaterwayNodeID, currentWaterwayLinkOutID, m_dicWaterwayLink[currentWaterwayLinkOutID].channelLength);
                yield return nextWaterwayNeighbor;
            }
        }

        /// <summary>
        /// Get WaterwayNeighborNodeIDs
        /// 该方法弃用了
        /// </summary>
        /// <param name="currentWaterwayNodeID"></param>
        /// <returns></returns>
        public List<string> Neighbors(string currentWaterwayNodeID)
        {
            List<string> currentWaterwayLinkConnectIDLists = new List<string>(m_dicWaterwayNode[currentWaterwayNodeID].waterLinkOutList);
            foreach (var currentWaterwayLinkInID in m_dicWaterwayNode[currentWaterwayNodeID].waterLinkInList)
            {
                if (!currentWaterwayLinkConnectIDLists.Contains(currentWaterwayLinkInID))
                    currentWaterwayLinkConnectIDLists.Add(currentWaterwayLinkInID);                 
            }
            return currentWaterwayLinkConnectIDLists;
        }

        /// <summary>
        /// 用于综合考虑的航线规划，对于给定船只，用于获取从当前节点所能通行邻接的所有其他节点
        /// </summary>
        /// <param name="currentWaterwayNodeID"></param>
        /// <param name="currentVehicle"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public IEnumerable<WaterwayNeighbor> Neighbors(string currentWaterwayNodeID, WaterwayVehicle currentVehicle, int method)
        {
            if (method == 2)
            {
                string[] currentWaterwayLinkOutIDLists = m_dicWaterwayNode[currentWaterwayNodeID].waterLinkOutList;
                foreach (var currentWaterwayLinkOutID in currentWaterwayLinkOutIDLists)
                {
                    if (!EstimatePassingThroughChannel(currentWaterwayLinkOutID, currentVehicle))
                        continue;
                    string currentWaterwayLinkOutUpStreamWaterNodeID = m_dicWaterwayLink[currentWaterwayLinkOutID].upStreamWaterNodeID;
                    string nextWaterwayNodeID = currentWaterwayNodeID != currentWaterwayLinkOutUpStreamWaterNodeID ? currentWaterwayLinkOutUpStreamWaterNodeID : m_dicWaterwayLink[currentWaterwayLinkOutID].downStreamWaterNodeID;

                    if (!m_dicWaterwayNode.ContainsKey(nextWaterwayNodeID))
                        continue;

                    var cost = m_dicWaterwayLink[currentWaterwayLinkOutID].channelLength * m_dicWaterwayLink[currentWaterwayLinkOutID].channelClass;

                    WaterwayNeighbor nextWaterwayNeighbor = new WaterwayNeighbor(nextWaterwayNodeID, currentWaterwayLinkOutID, cost);
                    yield return nextWaterwayNeighbor;
                }
            }                
        }


        /// <summary>
        /// Load WaterwayNetwork Datasets
        /// 整合的航道拓扑网络数据加载方法
        /// </summary>
        public void LoadWaterwayNetworkDatasets()
        {
            var pathTemp = Path.Combine(_shpfileDatasetsPath, "WaterwayNodeCGCS2000.shp");
            m_dicWaterwayNode = LoadWaterwayNodeDatasets(pathTemp);
            m_dicWaterwayLink = LoadWaterwayLinkDatasets(Path.Combine(_shpfileDatasetsPath, "WaterwayLinkCGCS2000.shp"));
            waterwayNodeSpatialIndex = ConstructSpatialIndex(m_dicWaterwayNode);
            isWaterwayNetworkDatasetsLoaded = true;
            Console.WriteLine("Waterway Network Datasets have been loaded!");
        }

    }
}