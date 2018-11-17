using IWRPM;
using NetTopologySuite.Geometries;
using Shrulik.NKDBush;
using Shrulik.NGeoKDBush;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Priority_Queue;


namespace IWRPM
{
    /// <summary>
    /// WaterwayRoutePlanner 最优航线规划类
    /// </summary>
    public class WaterwayRoutePlanner
    {
        public static readonly double EARTH_RADIUS = 6371004.0;
        public static readonly double Latitude_RADIUS = 11100.0;

        public bool isFindOptimalRoute = false;

        public WaterwayGraph waterwayRoutePlannerGraph = new WaterwayGraph();
        public Dictionary<string, string[]> cameFrom = new Dictionary<string, string[]>();
        public Dictionary<string, double> costSoFar = new Dictionary<string, double>();
        public GeoKDBush<WaterwayTopoNode> waterwayGeoKdBush = new GeoKDBush<WaterwayTopoNode>();
        public string StartWaterwayNodeID;
        public string GoalWaterwayNodeID;

        public double Rad(double d)
        {
            return d * Math.PI / 180.0;
        }

        public double GetDistanceByCoordinate(double[] _startCoordinate, double[] _endCoordinate)
        {
            var startLongitudeRad = Rad(_startCoordinate[0]);
            var startLatitudeRad = Rad(_startCoordinate[1]);
            var endLongitudeRad = Rad(_endCoordinate[0]);
            var endLatitudeRad = Rad(_endCoordinate[1]);
            var distance = Math.Acos(Math.Sin(startLatitudeRad) * Math.Sin(endLatitudeRad) + Math.Cos(startLatitudeRad) * Math.Cos(endLatitudeRad) * Math.Cos(startLongitudeRad - endLongitudeRad));
            distance = distance * EARTH_RADIUS;
            return distance;
        }

        // Note: a generic version of A* would abstract over Location and
        // also Heuristic
        public double ManhattanDistanceHeuristicWithDirection(WaterwayTopoNode _start, WaterwayTopoNode _goal, WaterwayTopoNode _current)
        {
            double alpha = 0.001;
            var dx_current2goal = Math.Abs(_current.waterNodeCoordinate[0] - _goal.waterNodeCoordinate[0]) * Latitude_RADIUS * Math.Cos(Rad(_goal.waterNodeCoordinate[1]));
            var dy_current2goal = Math.Abs(_current.waterNodeCoordinate[1] - _goal.waterNodeCoordinate[1]) * Latitude_RADIUS;
            var dx_start2goal = Math.Abs(_start.waterNodeCoordinate[0] - _goal.waterNodeCoordinate[0]) * Latitude_RADIUS * Math.Cos(Rad(_goal.waterNodeCoordinate[1])); ;
            var dy_start2goal = Math.Abs(_start.waterNodeCoordinate[1] - _goal.waterNodeCoordinate[1]) * Latitude_RADIUS;
            var cross = Math.Abs(dx_current2goal * dy_start2goal - dx_start2goal * dy_current2goal);
            var heuristic = Math.Abs(dx_current2goal) + Math.Abs(dy_current2goal) + cross * alpha;
            return heuristic;
        }

        public double Cost(double originalCost)
        {
            double aplha = 1.0;
            return (1 + aplha * (originalCost - 1));
        }

        public string GetNearestWaterwayNodeByCoorninate(WaterwayGraph _graph, double[] _currentCoordinate)
        {
            var nearestWaterwayNode = waterwayGeoKdBush.Around(_graph.waterwayNodeSpatialIndex, _currentCoordinate[0], _currentCoordinate[1], 1);
            return nearestWaterwayNode[0].waterNodeID;
        }

        public class ShapeNodeWithIndex
        {
            public int index;
            public double[] coordinate;

            public ShapeNodeWithIndex(int _index, double[] _coordinate)
            {
                this.index = _index;
                this.coordinate = _coordinate;
            }
        }

        public class ShapeLinkWithDistance
        {
            public string affiliatedTopoLinkID;
            public ShapeNodeWithIndex[] shapeNodeWithIndex;
            public double distanceToPoint;

            public ShapeLinkWithDistance(string _affiliatedTopoLinkID, ShapeNodeWithIndex[] _shapeNodeWithIndex, double _distanceToPoint)
            {
                this.affiliatedTopoLinkID = _affiliatedTopoLinkID;
                this.shapeNodeWithIndex = _shapeNodeWithIndex;
                this.distanceToPoint = _distanceToPoint;
            }
        }

        double DistanceToShapeLink(double[] _point, ShapeNodeWithIndex[] _link)
        {
            var from = _link[0].coordinate;
            var to = _link[1].coordinate;
            var k = (from[1] - to[1]) / (from[0] - to[0]);
            var A = k;
            var B = -1;
            var C = from[1] - k * from[0];
            var distance = Math.Abs(A * _point[0] + B * _point[1] + C) / Math.Sqrt(Math.Pow(A, 2) + Math.Pow(B, 2));
            return distance;
        }

        double[] VerticalInsertedPointToShapeLink(double[] _point, ShapeNodeWithIndex[] _link)
        {
            var verticalInsertedPointCoordinate = new double[2];
            var from = _link[0].coordinate;
            var to = _link[1].coordinate;
            var k = (from[1] - to[1]) / (from[0] - to[0]);

            if (k == 0)
            {
                verticalInsertedPointCoordinate[0] = _point[0];
                verticalInsertedPointCoordinate[1] = from[1];
            }
            else if (double.IsInfinity(k))
            {
                verticalInsertedPointCoordinate[0] = from[0];
                verticalInsertedPointCoordinate[1] = _point[1];
            }
            else
            {
                var A1 = k;
                var B1 = -1;
                var C1 = from[1] - k * from[0];
                var A2 = B1 / A1;
                var B2 = -1;
                var C2 = _point[1] - k * _point[0];
                var x = (B1 * C2 - C1 * B2) / (A1 * B2 - A2 * B1);
                var y = (A1 * C2 - A2 * C1) / (B1 * A2 - B2 * A1);
                verticalInsertedPointCoordinate[0] = x;
                verticalInsertedPointCoordinate[1] = y;
            }

            return verticalInsertedPointCoordinate;
        }

        private static int ShapeLinkWithDistanceCompare(ShapeLinkWithDistance _ShapeLinkWithDistance1, ShapeLinkWithDistance _ShapeLinkWithDistance2)
        {
            int res = 0;
            if ((_ShapeLinkWithDistance1 == null) && (_ShapeLinkWithDistance2 == null))
            {
                return 0;
            }
            else if ((_ShapeLinkWithDistance1 != null) && (_ShapeLinkWithDistance2 == null))
            {
                return 1;
            }
            else if ((_ShapeLinkWithDistance1 == null) && (_ShapeLinkWithDistance2 != null))
            {
                return -1;
            }
            if (_ShapeLinkWithDistance1.distanceToPoint > _ShapeLinkWithDistance2.distanceToPoint)
            {
                res = 1;
            }
            else if (_ShapeLinkWithDistance1.distanceToPoint < _ShapeLinkWithDistance2.distanceToPoint)
            {
                res = -1;
            }
            return res;
        }

        public string InsertStartTopoFeatures(string _startWaterwayNodeID, double[] startCoordinate)
        {
            var availableWaterwayLinkList = waterwayRoutePlannerGraph.Neighbors(_startWaterwayNodeID);
            var candidateShapeLink = new List<ShapeLinkWithDistance>();
            foreach (var currentAvailableWaterwayLink in availableWaterwayLinkList)
            {
                var currentCandidateShapeLink = new List<ShapeLinkWithDistance>();
                var currentAvailablecandidateShapeLNodeList = new List<ShapeNodeWithIndex>();
                var currentAvailableWaterwayLinkCoordinates = waterwayRoutePlannerGraph.m_dicWaterwayLink[currentAvailableWaterwayLink].channelGeometry;
                var currentAvailableWaterwayLinkCoordinatesLength = currentAvailableWaterwayLinkCoordinates.Length;
                for (var i = 0; i < currentAvailableWaterwayLinkCoordinatesLength; i++)
                {
                    currentAvailablecandidateShapeLNodeList.Add(new ShapeNodeWithIndex(i, new double[2] { currentAvailableWaterwayLinkCoordinates[i].X, currentAvailableWaterwayLinkCoordinates[i].Y }));
                }
                var currentAvailablecandidateShapeLNodeIndex = new KDBush<ShapeNodeWithIndex>(currentAvailablecandidateShapeLNodeList.ToArray(), p => p.coordinate[0], p => p.coordinate[1], nodeSize: 10);
                var currentAvailablecandidateShapeLNodeKdBush = new GeoKDBush<ShapeNodeWithIndex>();
                var nearestShapeNode = currentAvailablecandidateShapeLNodeKdBush.Around(currentAvailablecandidateShapeLNodeIndex, startCoordinate[0], startCoordinate[1], 1);
                if (nearestShapeNode[0].index == 0)
                {
                    var shapeLinkTemp = new ShapeNodeWithIndex[] { currentAvailablecandidateShapeLNodeList[0], currentAvailablecandidateShapeLNodeList[1] };
                    currentCandidateShapeLink.Add(new ShapeLinkWithDistance(currentAvailableWaterwayLink, shapeLinkTemp, DistanceToShapeLink(startCoordinate, shapeLinkTemp)));
                }
                else if (nearestShapeNode[0].index == (currentAvailableWaterwayLinkCoordinatesLength - 1))
                {
                    var shapeLinkTemp = new ShapeNodeWithIndex[] { currentAvailablecandidateShapeLNodeList[currentAvailableWaterwayLinkCoordinatesLength - 2], currentAvailablecandidateShapeLNodeList[currentAvailableWaterwayLinkCoordinatesLength - 1] };
                    currentCandidateShapeLink.Add(new ShapeLinkWithDistance(currentAvailableWaterwayLink, shapeLinkTemp, DistanceToShapeLink(startCoordinate, shapeLinkTemp)));
                }
                else
                {
                    var shapeLinkTempUp = new ShapeNodeWithIndex[] { currentAvailablecandidateShapeLNodeList[nearestShapeNode[0].index - 1], currentAvailablecandidateShapeLNodeList[nearestShapeNode[0].index] };
                    var shapeLinkTempDown = new ShapeNodeWithIndex[] { currentAvailablecandidateShapeLNodeList[nearestShapeNode[0].index], currentAvailablecandidateShapeLNodeList[nearestShapeNode[0].index + 1] };
                    currentCandidateShapeLink.Add(new ShapeLinkWithDistance(currentAvailableWaterwayLink, shapeLinkTempUp, DistanceToShapeLink(startCoordinate, shapeLinkTempUp)));
                    currentCandidateShapeLink.Add(new ShapeLinkWithDistance(currentAvailableWaterwayLink, shapeLinkTempDown, DistanceToShapeLink(startCoordinate, shapeLinkTempDown)));
                }
                currentCandidateShapeLink.Sort(ShapeLinkWithDistanceCompare);
                candidateShapeLink.Add(currentCandidateShapeLink[0]);
            }
            candidateShapeLink.Sort(ShapeLinkWithDistanceCompare);
            var insertedShapeLink = candidateShapeLink[0];
            var insertedTopoLink = new WaterwayTopoLink();
            insertedTopoLink = waterwayRoutePlannerGraph.m_dicWaterwayLink[insertedShapeLink.affiliatedTopoLinkID];
            var insertedTopoNodeCoordinate = VerticalInsertedPointToShapeLink(startCoordinate, insertedShapeLink.shapeNodeWithIndex);
            var insertedWaterwayTopoNode = new WaterwayTopoNode();
            insertedWaterwayTopoNode.waterNodeID = "START-0000-START";
            insertedWaterwayTopoNode.waterNodeClass = 2;
            insertedWaterwayTopoNode.waterNodeType = 6;
            insertedWaterwayTopoNode.waterNodeCoordinate = insertedTopoNodeCoordinate;
            insertedWaterwayTopoNode.waterNodeInformation = "Start WaterwayTopoNode Mapped on the WaterwayGraph";
            insertedWaterwayTopoNode.waterNodeName = "出发点";

            var insertedWaterwayTopoLinkUp = new WaterwayTopoLink(insertedTopoLink);
            insertedWaterwayTopoLinkUp.waterLinkID = insertedTopoLink.upStreamWaterNodeID + '-' + insertedWaterwayTopoNode.waterNodeID;
            insertedWaterwayTopoLinkUp.downStreamWaterNodeID = insertedWaterwayTopoNode.waterNodeID;
            insertedWaterwayTopoLinkUp.channelLength = insertedTopoLink.channelLength * insertedShapeLink.shapeNodeWithIndex[0].index / (insertedTopoLink.channelGeometry.Length - 1);
            var insertedWaterwayTopoLinkUpGeometry = new CoordinateList();
            for (var i = 0; i <= insertedShapeLink.shapeNodeWithIndex[0].index; i++)
            {
                insertedWaterwayTopoLinkUpGeometry.Add(insertedTopoLink.channelGeometry[i]);
            }
            insertedWaterwayTopoLinkUpGeometry.Add(new GeoAPI.Geometries.Coordinate(insertedTopoNodeCoordinate[0], insertedTopoNodeCoordinate[1]));
            insertedWaterwayTopoLinkUp.channelGeometry = insertedWaterwayTopoLinkUpGeometry.ToArray();
            waterwayRoutePlannerGraph.m_dicWaterwayLink.Add(insertedWaterwayTopoLinkUp.waterLinkID, insertedWaterwayTopoLinkUp);

            var insertedWaterwayTopoLinkDown = new WaterwayTopoLink(insertedTopoLink);
            insertedWaterwayTopoLinkDown.waterLinkID = insertedWaterwayTopoNode.waterNodeID + insertedTopoLink.downStreamWaterNodeID;
            insertedWaterwayTopoLinkDown.upStreamWaterNodeID = insertedWaterwayTopoNode.waterNodeID;
            insertedWaterwayTopoLinkDown.channelLength = insertedTopoLink.channelLength - insertedWaterwayTopoLinkUp.channelLength;
            var insertedWaterwayTopoLinkDownGeometry = new CoordinateList();
            insertedWaterwayTopoLinkDownGeometry.Add(new GeoAPI.Geometries.Coordinate(insertedTopoNodeCoordinate[0], insertedTopoNodeCoordinate[1]));
            for (var i = insertedShapeLink.shapeNodeWithIndex[1].index; i < insertedTopoLink.channelGeometry.Length; i++)
            {
                insertedWaterwayTopoLinkUpGeometry.Add(insertedTopoLink.channelGeometry[i]);
            }
            insertedWaterwayTopoLinkDown.channelGeometry = insertedWaterwayTopoLinkDownGeometry.ToArray();
            waterwayRoutePlannerGraph.m_dicWaterwayLink.Add(insertedWaterwayTopoLinkDown.waterLinkID, insertedWaterwayTopoLinkDown);

            var upStreamWaterNodeIDTempFix = waterwayRoutePlannerGraph.m_dicWaterwayLink[insertedTopoLink.waterLinkID].upStreamWaterNodeID;
            var downStreamWaterNodeIDTempFix = waterwayRoutePlannerGraph.m_dicWaterwayLink[insertedTopoLink.waterLinkID].downStreamWaterNodeID;
            var upStreamWaterwayNodeTempFix = waterwayRoutePlannerGraph.m_dicWaterwayNode[upStreamWaterNodeIDTempFix];
            var downStreamWaterwayNodeTempFix = waterwayRoutePlannerGraph.m_dicWaterwayNode[downStreamWaterNodeIDTempFix];

            if (!insertedTopoLink.oneWay)
            {
                insertedWaterwayTopoNode.waterLinkInNumber = 2;
                insertedWaterwayTopoNode.waterLinkOutNumber = 2;
                insertedWaterwayTopoNode.waterLinkInList = new string[2] { insertedWaterwayTopoLinkUp.waterLinkID, insertedWaterwayTopoLinkDown.waterLinkID };
                insertedWaterwayTopoNode.waterLinkOutList = insertedWaterwayTopoNode.waterLinkInList;

                upStreamWaterwayNodeTempFix.waterLinkInList[Array.IndexOf(upStreamWaterwayNodeTempFix.waterLinkInList, insertedTopoLink.waterLinkID)] = insertedWaterwayTopoLinkUp.waterLinkID;
                upStreamWaterwayNodeTempFix.waterLinkOutList[Array.IndexOf(upStreamWaterwayNodeTempFix.waterLinkOutList, insertedTopoLink.waterLinkID)] = insertedWaterwayTopoLinkUp.waterLinkID;
                downStreamWaterwayNodeTempFix.waterLinkInList[Array.IndexOf(downStreamWaterwayNodeTempFix.waterLinkInList, insertedTopoLink.waterLinkID)] = insertedWaterwayTopoLinkDown.waterLinkID;
                downStreamWaterwayNodeTempFix.waterLinkOutList[Array.IndexOf(downStreamWaterwayNodeTempFix.waterLinkOutList, insertedTopoLink.waterLinkID)] = insertedWaterwayTopoLinkDown.waterLinkID;

            }
            else
            {
                insertedWaterwayTopoNode.waterLinkInNumber = 1;
                insertedWaterwayTopoNode.waterLinkOutNumber = 1;
                if (insertedTopoLink.trafficDirection == 1)
                {
                    insertedWaterwayTopoNode.waterLinkInList = new string[1] { insertedWaterwayTopoLinkUp.waterLinkID };
                    insertedWaterwayTopoNode.waterLinkOutList = new string[1] { insertedWaterwayTopoLinkDown.waterLinkID };

                    upStreamWaterwayNodeTempFix.waterLinkOutList[Array.IndexOf(upStreamWaterwayNodeTempFix.waterLinkOutList, insertedTopoLink.waterLinkID)] = insertedWaterwayTopoLinkUp.waterLinkID;
                    downStreamWaterwayNodeTempFix.waterLinkInList[Array.IndexOf(downStreamWaterwayNodeTempFix.waterLinkInList, insertedTopoLink.waterLinkID)] = insertedWaterwayTopoLinkDown.waterLinkID;
                }
                else if (insertedTopoLink.trafficDirection == -1)
                {
                    insertedWaterwayTopoNode.waterLinkInList = new string[1] { insertedWaterwayTopoLinkDown.waterLinkID };
                    insertedWaterwayTopoNode.waterLinkOutList = new string[1] { insertedWaterwayTopoLinkUp.waterLinkID };

                    upStreamWaterwayNodeTempFix.waterLinkInList[Array.IndexOf(upStreamWaterwayNodeTempFix.waterLinkInList, insertedTopoLink.waterLinkID)] = insertedWaterwayTopoLinkUp.waterLinkID;
                    downStreamWaterwayNodeTempFix.waterLinkOutList[Array.IndexOf(downStreamWaterwayNodeTempFix.waterLinkOutList, insertedTopoLink.waterLinkID)] = insertedWaterwayTopoLinkDown.waterLinkID;
                }
            }
            waterwayRoutePlannerGraph.m_dicWaterwayNode.Add(insertedWaterwayTopoNode.waterNodeID, insertedWaterwayTopoNode);
            waterwayRoutePlannerGraph.m_dicWaterwayLink.Remove(insertedTopoLink.waterLinkID);

            return insertedWaterwayTopoNode.waterNodeID;
        }

        public string InsertGoalTopoFeatures(string _goalWaterwayNodeID, double[] goalCoordinate)
        {
            var availableWaterwayLinkList = waterwayRoutePlannerGraph.Neighbors(_goalWaterwayNodeID);
            var candidateShapeLink = new List<ShapeLinkWithDistance>();
            foreach (var currentAvailableWaterwayLink in availableWaterwayLinkList)
            {
                var currentCandidateShapeLink = new List<ShapeLinkWithDistance>();
                var currentAvailablecandidateShapeLNodeList = new List<ShapeNodeWithIndex>();
                var currentAvailableWaterwayLinkCoordinates = waterwayRoutePlannerGraph.m_dicWaterwayLink[currentAvailableWaterwayLink].channelGeometry;
                var currentAvailableWaterwayLinkCoordinatesLength = currentAvailableWaterwayLinkCoordinates.Length;
                for (var i = 0; i < currentAvailableWaterwayLinkCoordinatesLength; i++)
                {
                    currentAvailablecandidateShapeLNodeList.Add(new ShapeNodeWithIndex(i, new double[2] { currentAvailableWaterwayLinkCoordinates[i].X, currentAvailableWaterwayLinkCoordinates[i].Y }));
                }
                var currentAvailablecandidateShapeLNodeIndex = new KDBush<ShapeNodeWithIndex>(currentAvailablecandidateShapeLNodeList.ToArray(), p => p.coordinate[0], p => p.coordinate[1], nodeSize: 10);
                var currentAvailablecandidateShapeLNodeKdBush = new GeoKDBush<ShapeNodeWithIndex>();
                var nearestShapeNode = currentAvailablecandidateShapeLNodeKdBush.Around(currentAvailablecandidateShapeLNodeIndex, goalCoordinate[0], goalCoordinate[1], 1);
                if (nearestShapeNode[0].index == 0)
                {
                    var shapeLinkTemp = new ShapeNodeWithIndex[] { currentAvailablecandidateShapeLNodeList[0], currentAvailablecandidateShapeLNodeList[1] };
                    currentCandidateShapeLink.Add(new ShapeLinkWithDistance(currentAvailableWaterwayLink, shapeLinkTemp, DistanceToShapeLink(goalCoordinate, shapeLinkTemp)));
                }
                else if (nearestShapeNode[0].index == (currentAvailableWaterwayLinkCoordinatesLength - 1))
                {
                    var shapeLinkTemp = new ShapeNodeWithIndex[] { currentAvailablecandidateShapeLNodeList[currentAvailableWaterwayLinkCoordinatesLength - 2], currentAvailablecandidateShapeLNodeList[currentAvailableWaterwayLinkCoordinatesLength - 1] };
                    currentCandidateShapeLink.Add(new ShapeLinkWithDistance(currentAvailableWaterwayLink, shapeLinkTemp, DistanceToShapeLink(goalCoordinate, shapeLinkTemp)));
                }
                else
                {
                    var shapeLinkTempUp = new ShapeNodeWithIndex[] { currentAvailablecandidateShapeLNodeList[nearestShapeNode[0].index - 1], currentAvailablecandidateShapeLNodeList[nearestShapeNode[0].index] };
                    var shapeLinkTempDown = new ShapeNodeWithIndex[] { currentAvailablecandidateShapeLNodeList[nearestShapeNode[0].index], currentAvailablecandidateShapeLNodeList[nearestShapeNode[0].index + 1] };
                    currentCandidateShapeLink.Add(new ShapeLinkWithDistance(currentAvailableWaterwayLink, shapeLinkTempUp, DistanceToShapeLink(goalCoordinate, shapeLinkTempUp)));
                    currentCandidateShapeLink.Add(new ShapeLinkWithDistance(currentAvailableWaterwayLink, shapeLinkTempDown, DistanceToShapeLink(goalCoordinate, shapeLinkTempDown)));
                }
                currentCandidateShapeLink.Sort(ShapeLinkWithDistanceCompare);
                candidateShapeLink.Add(currentCandidateShapeLink[0]);
            }
            candidateShapeLink.Sort(ShapeLinkWithDistanceCompare);
            var insertedShapeLink = candidateShapeLink[0];
            var insertedTopoLink = waterwayRoutePlannerGraph.m_dicWaterwayLink[insertedShapeLink.affiliatedTopoLinkID];
            var insertedTopoNodeCoordinate = VerticalInsertedPointToShapeLink(goalCoordinate, insertedShapeLink.shapeNodeWithIndex);
            var insertedWaterwayTopoNode = new WaterwayTopoNode();
            insertedWaterwayTopoNode.waterNodeID = "GOAL-9999-GOAL";
            insertedWaterwayTopoNode.waterNodeClass = 2;
            insertedWaterwayTopoNode.waterNodeType = 6;
            insertedWaterwayTopoNode.waterNodeCoordinate = insertedTopoNodeCoordinate;
            insertedWaterwayTopoNode.waterNodeInformation = "Goal WaterwayTopoNode Mapped on the WaterwayGraph";
            insertedWaterwayTopoNode.waterNodeName = "目标点";

            var insertedWaterwayTopoLinkUp = new WaterwayTopoLink(insertedTopoLink);
            insertedWaterwayTopoLinkUp.waterLinkID = insertedTopoLink.upStreamWaterNodeID + '-' + insertedWaterwayTopoNode.waterNodeID;
            insertedWaterwayTopoLinkUp.downStreamWaterNodeID = insertedWaterwayTopoNode.waterNodeID;
            insertedWaterwayTopoLinkUp.channelLength = insertedTopoLink.channelLength * insertedShapeLink.shapeNodeWithIndex[0].index / (insertedTopoLink.channelGeometry.Length - 1);
            var insertedWaterwayTopoLinkUpGeometry = new CoordinateList();
            for (var i = 0; i <= insertedShapeLink.shapeNodeWithIndex[0].index; i++)
            {
                insertedWaterwayTopoLinkUpGeometry.Add(insertedTopoLink.channelGeometry[i]);
            }
            insertedWaterwayTopoLinkUpGeometry.Add(new GeoAPI.Geometries.Coordinate(insertedTopoNodeCoordinate[0], insertedTopoNodeCoordinate[1]));
            insertedWaterwayTopoLinkUp.channelGeometry = insertedWaterwayTopoLinkUpGeometry.ToArray();
            waterwayRoutePlannerGraph.m_dicWaterwayLink.Add(insertedWaterwayTopoLinkUp.waterLinkID, insertedWaterwayTopoLinkUp);

            var insertedWaterwayTopoLinkDown = new WaterwayTopoLink(insertedTopoLink);
            insertedWaterwayTopoLinkDown.waterLinkID = insertedWaterwayTopoNode.waterNodeID + insertedTopoLink.downStreamWaterNodeID;
            insertedWaterwayTopoLinkDown.upStreamWaterNodeID = insertedWaterwayTopoNode.waterNodeID;
            insertedWaterwayTopoLinkDown.channelLength = insertedTopoLink.channelLength - insertedWaterwayTopoLinkUp.channelLength;
            var insertedWaterwayTopoLinkDownGeometry = new CoordinateList();
            insertedWaterwayTopoLinkDownGeometry.Add(new GeoAPI.Geometries.Coordinate(insertedTopoNodeCoordinate[0], insertedTopoNodeCoordinate[1]));
            for (var i = insertedShapeLink.shapeNodeWithIndex[1].index; i < insertedTopoLink.channelGeometry.Length; i++)
            {
                insertedWaterwayTopoLinkUpGeometry.Add(insertedTopoLink.channelGeometry[i]);
            }
            insertedWaterwayTopoLinkDown.channelGeometry = insertedWaterwayTopoLinkDownGeometry.ToArray();
            waterwayRoutePlannerGraph.m_dicWaterwayLink.Add(insertedWaterwayTopoLinkDown.waterLinkID, insertedWaterwayTopoLinkDown);

            var upStreamWaterNodeIDTempFix = waterwayRoutePlannerGraph.m_dicWaterwayLink[insertedTopoLink.waterLinkID].upStreamWaterNodeID;
            var downStreamWaterNodeIDTempFix = waterwayRoutePlannerGraph.m_dicWaterwayLink[insertedTopoLink.waterLinkID].downStreamWaterNodeID;
            var upStreamWaterwayNodeTempFix = waterwayRoutePlannerGraph.m_dicWaterwayNode[upStreamWaterNodeIDTempFix];
            var downStreamWaterwayNodeTempFix = waterwayRoutePlannerGraph.m_dicWaterwayNode[downStreamWaterNodeIDTempFix];

            if (!insertedTopoLink.oneWay)
            {
                insertedWaterwayTopoNode.waterLinkInNumber = 2;
                insertedWaterwayTopoNode.waterLinkOutNumber = 2;
                insertedWaterwayTopoNode.waterLinkInList = new string[2] { insertedWaterwayTopoLinkUp.waterLinkID, insertedWaterwayTopoLinkDown.waterLinkID };
                insertedWaterwayTopoNode.waterLinkOutList = insertedWaterwayTopoNode.waterLinkInList;

                upStreamWaterwayNodeTempFix.waterLinkInList[Array.IndexOf(upStreamWaterwayNodeTempFix.waterLinkInList, insertedTopoLink.waterLinkID)] = insertedWaterwayTopoLinkUp.waterLinkID;
                upStreamWaterwayNodeTempFix.waterLinkOutList[Array.IndexOf(upStreamWaterwayNodeTempFix.waterLinkOutList, insertedTopoLink.waterLinkID)] = insertedWaterwayTopoLinkUp.waterLinkID;
                downStreamWaterwayNodeTempFix.waterLinkInList[Array.IndexOf(downStreamWaterwayNodeTempFix.waterLinkInList, insertedTopoLink.waterLinkID)] = insertedWaterwayTopoLinkDown.waterLinkID;
                downStreamWaterwayNodeTempFix.waterLinkOutList[Array.IndexOf(downStreamWaterwayNodeTempFix.waterLinkOutList, insertedTopoLink.waterLinkID)] = insertedWaterwayTopoLinkDown.waterLinkID;

            }
            else
            {
                insertedWaterwayTopoNode.waterLinkInNumber = 1;
                insertedWaterwayTopoNode.waterLinkOutNumber = 1;
                if (insertedTopoLink.trafficDirection == 1)
                {
                    insertedWaterwayTopoNode.waterLinkInList = new string[1] { insertedWaterwayTopoLinkUp.waterLinkID };
                    insertedWaterwayTopoNode.waterLinkOutList = new string[1] { insertedWaterwayTopoLinkDown.waterLinkID };

                    upStreamWaterwayNodeTempFix.waterLinkOutList[Array.IndexOf(upStreamWaterwayNodeTempFix.waterLinkOutList, insertedTopoLink.waterLinkID)] = insertedWaterwayTopoLinkUp.waterLinkID;
                    downStreamWaterwayNodeTempFix.waterLinkInList[Array.IndexOf(downStreamWaterwayNodeTempFix.waterLinkInList, insertedTopoLink.waterLinkID)] = insertedWaterwayTopoLinkDown.waterLinkID;
                }
                else if (insertedTopoLink.trafficDirection == -1)
                {
                    insertedWaterwayTopoNode.waterLinkInList = new string[1] { insertedWaterwayTopoLinkDown.waterLinkID };
                    insertedWaterwayTopoNode.waterLinkOutList = new string[1] { insertedWaterwayTopoLinkUp.waterLinkID };

                    upStreamWaterwayNodeTempFix.waterLinkInList[Array.IndexOf(upStreamWaterwayNodeTempFix.waterLinkInList, insertedTopoLink.waterLinkID)] = insertedWaterwayTopoLinkUp.waterLinkID;
                    downStreamWaterwayNodeTempFix.waterLinkOutList[Array.IndexOf(downStreamWaterwayNodeTempFix.waterLinkOutList, insertedTopoLink.waterLinkID)] = insertedWaterwayTopoLinkDown.waterLinkID;
                }
            }
            waterwayRoutePlannerGraph.m_dicWaterwayNode.Add(insertedWaterwayTopoNode.waterNodeID, insertedWaterwayTopoNode);
            waterwayRoutePlannerGraph.m_dicWaterwayLink.Remove(insertedTopoLink.waterLinkID);

            return insertedWaterwayTopoNode.waterNodeID;
        }

        public string[] InsertTemporaryTopoFeatures(string _startWaterwayNodeID, double[] startCoordinate, string _goalWaterwayNodeID, double[] goalCoordinat)
        {
            var _startWaterwayNodeIDOnNet = InsertStartTopoFeatures(_startWaterwayNodeID, startCoordinate);
            var _goalWaterwayNodeIDOnNet = InsertGoalTopoFeatures(_goalWaterwayNodeID, goalCoordinat);
            return new string[2] { _startWaterwayNodeIDOnNet, _goalWaterwayNodeIDOnNet };
        }

        public WaterwayRoutePlanner()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        public WaterwayRoutePlanner(WaterwayGraph graph, WaterwayVehicle vehicle, string start, string goal)
        {
            SimplePriorityQueue<string, double> frontier = new SimplePriorityQueue<string, double>();
            frontier.Enqueue(start, 0.0);

            cameFrom.Add(start, new string[] { start, "START" });
            costSoFar[start] = Cost(0.0);

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current.Equals(goal))
                {
                    isFindOptimalRoute = true;
                    break;
                }

                foreach (var next in graph.Neighbors(current, vehicle))
                {
                    double newCost = costSoFar[current] + Cost(next.toNextWaterwayNodeCost);
                    if (!costSoFar.ContainsKey(next.nextWaterwayNodeID) || newCost < costSoFar[next.nextWaterwayNodeID])
                    {
                        costSoFar[next.nextWaterwayNodeID] = newCost;
                        double priority = newCost + ManhattanDistanceHeuristicWithDirection(graph.m_dicWaterwayNode[start], graph.m_dicWaterwayNode[goal], graph.m_dicWaterwayNode[next.nextWaterwayNodeID]);
                        frontier.Enqueue(next.nextWaterwayNodeID, priority);
                        cameFrom[next.nextWaterwayNodeID] = new string[2] { current, next.toNextWaterwayNodeLinkID };
                    }
                }
            }
        }

        public WaterwayRoutePlanner(WaterwayGraph graph, WaterwayVehicle vehicle, double[] startCoordinate, double[] goalCoordinate)
        {
            waterwayRoutePlannerGraph = graph;
            
            StartWaterwayNodeID = GetNearestWaterwayNodeByCoorninate(graph, startCoordinate);
            GoalWaterwayNodeID = GetNearestWaterwayNodeByCoorninate(graph, goalCoordinate);

            var StartGoalWaterwayNodeIDOnNetArray = InsertTemporaryTopoFeatures(StartWaterwayNodeID, startCoordinate, GoalWaterwayNodeID, goalCoordinate);

            SimplePriorityQueue<string, double> frontier = new SimplePriorityQueue<string, double>();
            frontier.Enqueue(StartGoalWaterwayNodeIDOnNetArray[0], 0.0);

            cameFrom.Add(StartGoalWaterwayNodeIDOnNetArray[0], new string[] { StartWaterwayNodeID, "START" });
            costSoFar[StartGoalWaterwayNodeIDOnNetArray[0]] = Cost(0.0);

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current.Equals(StartGoalWaterwayNodeIDOnNetArray[1]))
                {
                    isFindOptimalRoute = true;
                    break;
                }

                foreach (var next in waterwayRoutePlannerGraph.Neighbors(current, vehicle))
                {
                    double newCost = costSoFar[current] + Cost(next.toNextWaterwayNodeCost);
                    if (!costSoFar.ContainsKey(next.nextWaterwayNodeID) || newCost < costSoFar[next.nextWaterwayNodeID])
                    {
                        costSoFar[next.nextWaterwayNodeID] = newCost;
                        double priority = newCost + ManhattanDistanceHeuristicWithDirection(waterwayRoutePlannerGraph.m_dicWaterwayNode[StartGoalWaterwayNodeIDOnNetArray[0]], waterwayRoutePlannerGraph.m_dicWaterwayNode[StartGoalWaterwayNodeIDOnNetArray[1]], waterwayRoutePlannerGraph.m_dicWaterwayNode[next.nextWaterwayNodeID]);
                        frontier.Enqueue(next.nextWaterwayNodeID, priority);
                        cameFrom[next.nextWaterwayNodeID] = new string[2] { current, next.toNextWaterwayNodeLinkID };
                    }
                }
            }
        }

    }
}