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
        public static readonly double epsilon = 0.000001;

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
            var distance = 0.0;

            if ((_startCoordinate[0] != _endCoordinate[0]) || (_startCoordinate[1] != _endCoordinate[1]))
            {
                var startLongitudeRad = Rad(_startCoordinate[0]);
                var startLatitudeRad = Rad(_startCoordinate[1]);
                var endLongitudeRad = Rad(_endCoordinate[0]);
                var endLatitudeRad = Rad(_endCoordinate[1]);
                var midVarient = Math.Sin(startLatitudeRad) * Math.Sin(endLatitudeRad) + Math.Cos(startLatitudeRad) * Math.Cos(endLatitudeRad) * Math.Cos(startLongitudeRad - endLongitudeRad);
                distance = Math.Acos(Math.Min(midVarient, 1.0));
                distance = distance * EARTH_RADIUS;
            }

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

        public double GreatCircleDistanceHeuristicWithDirection(WaterwayTopoNode _start, WaterwayTopoNode _goal, WaterwayTopoNode _current)
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

        public double GreatCircleDistance(WaterwayTopoNode _start, WaterwayTopoNode _goal, WaterwayTopoNode _current)
        {
            var _currentCoordinate = _current.waterNodeCoordinate;
            var _goalCoordinate = _goal.waterNodeCoordinate;
            var _cosC = Math.Cos(Rad(90.0 - _goalCoordinate[1])) * Math.Cos(Rad(90.0 - _currentCoordinate[1])) + Math.Sin(Rad(90.0 - _goalCoordinate[1])) * Math.Sin(Rad(90.0 - _currentCoordinate[1])) * Math.Cos(Rad(_goalCoordinate[0]) - Rad(_currentCoordinate[0]));
            var _cRad = Math.Acos(_cosC);
            var distance = EARTH_RADIUS * _cRad;

            return distance;
        }

        public double GreatCircleDistance(double[] _goal, double[] _current)
        {
            var _currentCoordinate = _current;
            var _goalCoordinate = _goal;
            var _cosC = Math.Cos(Rad(90.0 - _goalCoordinate[1])) * Math.Cos(Rad(90.0 - _currentCoordinate[1])) + Math.Sin(Rad(90.0 - _goalCoordinate[1])) * Math.Sin(Rad(90.0 - _currentCoordinate[1])) * Math.Cos(Rad(_goalCoordinate[0]) - Rad(_currentCoordinate[0]));
            var _cRad = Math.Acos(Math.Min(_cosC, 1.0));
            var distance = EARTH_RADIUS * _cRad;

            return distance;
        }

        public double Cost(double originalCost)
        {
            double aplha = 1.0;
            return (1 + aplha * (originalCost - 1));
        }

        public string GetNearestWaterwayNodeByCoorninate(WaterwayGraph _graph, double[] _currentCoordinate)
        {
            var nearestWaterwayNode = waterwayGeoKdBush.Around(_graph.waterwayNodeSpatialIndex, _currentCoordinate[0], _currentCoordinate[1], 5);
            var nearestWaterwayNodeIDOnNet = nearestWaterwayNode[0].waterNodeID;
            var waterwayNodeCandidateNumber = Math.Min(nearestWaterwayNode.Count, 5);
            for (var i = 0; i < waterwayNodeCandidateNumber; i++)
            {
                if (nearestWaterwayNode[i].waterNodeType != 4)
                {
                    nearestWaterwayNodeIDOnNet = nearestWaterwayNode[i].waterNodeID;
                    break;
                }
            }

            return nearestWaterwayNodeIDOnNet;
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

        public class ShapeLinkWithShortestDistance
        {
            public string affiliatedTopoLinkID;
            public ShapeNodeWithIndex[] shapeNodeWithIndex;
            public double distanceToPoint;

            public ShapeLinkWithShortestDistance(string _affiliatedTopoLinkID, ShapeNodeWithIndex[] _shapeNodeWithIndex, double _distanceToPoint)
            {
                this.affiliatedTopoLinkID = _affiliatedTopoLinkID;
                this.shapeNodeWithIndex = _shapeNodeWithIndex;
                this.distanceToPoint = _distanceToPoint;
            }
        }

        static bool IsSameCoordinate(double[] _traceCoordinate, double[] _comparedCoordinate)
        {
            bool isSameCoordinate = false;
            double diffrence = Math.Abs((_traceCoordinate[0] - _comparedCoordinate[0]) + (_traceCoordinate[1] - _comparedCoordinate[1]));
            if (diffrence < epsilon)
                isSameCoordinate = true;

            return isSameCoordinate;
        }

        static bool IsInBoundingBox(double[] _traceCoordinate, double _minLongititude, double _minLaititutde, double _maxLongititude, double _maxLaititutde)
        {
            bool isInBoundingBox = false;
            if ((_traceCoordinate[0] >= _minLongititude) && (_traceCoordinate[0] <= _maxLongititude))
            {
                if ((_traceCoordinate[1] >= _minLaititutde) && (_traceCoordinate[1] <= _maxLaititutde))
                {
                    isInBoundingBox = true;
                }
            };

            return isInBoundingBox;
        }

        /// <summary>
        /// 求解输入坐标点距离形状线的距离
        /// </summary>
        /// <param name="_point"></param>
        /// <param name="_link"></param>
        /// <returns></returns>
        double ShortestDistanceToShapeLink(double[] _point, ShapeNodeWithIndex[] _link)
        {
            var shortestDistance = 0.0;
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
                var C2 = _point[1] - A2 * _point[0];
                var x = (B1 * C2 - C1 * B2) / (A1 * B2 - A2 * B1);
                var y = (A1 * C2 - A2 * C1) / (B1 * A2 - B2 * A1);
                verticalInsertedPointCoordinate[0] = x;
                verticalInsertedPointCoordinate[1] = y;
            }

            var vertical2pointDistance = GreatCircleDistance(_point, verticalInsertedPointCoordinate);
            var from2pointDistance = GreatCircleDistance(_point, from);
            var to2pointDistance = GreatCircleDistance(_point, to);

            var minLongitude = Math.Min(from[0], to[0]);
            var minLatitude = Math.Min(from[1], to[1]);
            var maxLongitude = Math.Max(from[0], to[0]);
            var maxLatitude = Math.Max(from[1], to[1]);

            if (from2pointDistance > to2pointDistance)
            {
                shortestDistance = to2pointDistance;
                if (to2pointDistance > vertical2pointDistance)
                {
                    if (IsInBoundingBox(verticalInsertedPointCoordinate, minLongitude, minLatitude, maxLongitude, maxLatitude))
                    {
                        shortestDistance = vertical2pointDistance;
                    }              
                }
            }
            else
            {
                shortestDistance = from2pointDistance;
                if (from2pointDistance > vertical2pointDistance)
                {
                    if (IsInBoundingBox(verticalInsertedPointCoordinate, minLongitude, minLatitude, maxLongitude, maxLatitude))
                    {
                        shortestDistance = vertical2pointDistance;
                    }
                }
            }

            return shortestDistance;
        }

        double[] ShortestInsertedPointToShapeLink(double[] _point, ShapeNodeWithIndex[] _link)
        {
            var shortestInsertedPointCoordinate = new double[2];
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
                var C2 = _point[1] - A2 * _point[0];
                var x = (B1 * C2 - C1 * B2) / (A1 * B2 - A2 * B1);
                var y = (A1 * C2 - A2 * C1) / (B1 * A2 - B2 * A1);
                verticalInsertedPointCoordinate[0] = x;
                verticalInsertedPointCoordinate[1] = y;
            }

            var vertical2pointDistance = GreatCircleDistance(_point, verticalInsertedPointCoordinate);
            var from2pointDistance = GreatCircleDistance(_point, from);
            var to2pointDistance = GreatCircleDistance(_point, to);

            var minLongitude = Math.Min(from[0], to[0]);
            var minLatitude = Math.Min(from[1], to[1]);
            var maxLongitude = Math.Max(from[0], to[0]);
            var maxLatitude = Math.Max(from[1], to[1]);

            if (from2pointDistance > to2pointDistance)
            {
                shortestInsertedPointCoordinate = to;
                if (to2pointDistance > vertical2pointDistance)
                {
                    if (IsInBoundingBox(verticalInsertedPointCoordinate, minLongitude, minLatitude, maxLongitude, maxLatitude))
                    {
                        shortestInsertedPointCoordinate = verticalInsertedPointCoordinate;
                    }
                }
            }
            else
            {
                shortestInsertedPointCoordinate = from;
                if (from2pointDistance > vertical2pointDistance)
                {
                    if (IsInBoundingBox(verticalInsertedPointCoordinate, minLongitude, minLatitude, maxLongitude, maxLatitude))
                    {
                        shortestInsertedPointCoordinate = verticalInsertedPointCoordinate;
                    }
                }
            }

            return shortestInsertedPointCoordinate;
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
                var C2 = _point[1] - A2 * _point[0];
                var x = (B1 * C2 - C1 * B2) / (A1 * B2 - A2 * B1);
                var y = (A1 * C2 - A2 * C1) / (B1 * A2 - B2 * A1);
                verticalInsertedPointCoordinate[0] = x;
                verticalInsertedPointCoordinate[1] = y;
            }

            return verticalInsertedPointCoordinate;
        }

        private static int ShapeLinkWithDistanceCompare(ShapeLinkWithShortestDistance _ShapeLinkWithDistance1, ShapeLinkWithShortestDistance _ShapeLinkWithDistance2)
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
            var candidateShapeLink = new List<ShapeLinkWithShortestDistance>();
            foreach (var currentAvailableWaterwayLink in availableWaterwayLinkList)
            {
                var currentCandidateShapeLink = new List<ShapeLinkWithShortestDistance>();
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
                    currentCandidateShapeLink.Add(new ShapeLinkWithShortestDistance(currentAvailableWaterwayLink, shapeLinkTemp, ShortestDistanceToShapeLink(startCoordinate, shapeLinkTemp)));
                }
                else if (nearestShapeNode[0].index == (currentAvailableWaterwayLinkCoordinatesLength - 1))
                {
                    var shapeLinkTemp = new ShapeNodeWithIndex[] { currentAvailablecandidateShapeLNodeList[currentAvailableWaterwayLinkCoordinatesLength - 2], currentAvailablecandidateShapeLNodeList[currentAvailableWaterwayLinkCoordinatesLength - 1] };
                    currentCandidateShapeLink.Add(new ShapeLinkWithShortestDistance(currentAvailableWaterwayLink, shapeLinkTemp, ShortestDistanceToShapeLink(startCoordinate, shapeLinkTemp)));
                }
                else
                {
                    var shapeLinkTempUp = new ShapeNodeWithIndex[] { currentAvailablecandidateShapeLNodeList[nearestShapeNode[0].index - 1], currentAvailablecandidateShapeLNodeList[nearestShapeNode[0].index] };
                    var shapeLinkTempDown = new ShapeNodeWithIndex[] { currentAvailablecandidateShapeLNodeList[nearestShapeNode[0].index], currentAvailablecandidateShapeLNodeList[nearestShapeNode[0].index + 1] };
                    currentCandidateShapeLink.Add(new ShapeLinkWithShortestDistance(currentAvailableWaterwayLink, shapeLinkTempUp, ShortestDistanceToShapeLink(startCoordinate, shapeLinkTempUp)));
                    currentCandidateShapeLink.Add(new ShapeLinkWithShortestDistance(currentAvailableWaterwayLink, shapeLinkTempDown, ShortestDistanceToShapeLink(startCoordinate, shapeLinkTempDown)));
                }
                currentCandidateShapeLink.Sort(ShapeLinkWithDistanceCompare);
                candidateShapeLink.Add(currentCandidateShapeLink[0]);
            }
            candidateShapeLink.Sort(ShapeLinkWithDistanceCompare);
            var insertedShapeLink = candidateShapeLink[0];
            var insertedTopoLink = new WaterwayTopoLink();
            insertedTopoLink = waterwayRoutePlannerGraph.m_dicWaterwayLink[insertedShapeLink.affiliatedTopoLinkID];

            var upStreamWaterNodeIDTempFix = waterwayRoutePlannerGraph.m_dicWaterwayLink[insertedTopoLink.waterLinkID].upStreamWaterNodeID;
            var downStreamWaterNodeIDTempFix = waterwayRoutePlannerGraph.m_dicWaterwayLink[insertedTopoLink.waterLinkID].downStreamWaterNodeID;
            var upStreamWaterwayNodeTempFix = waterwayRoutePlannerGraph.m_dicWaterwayNode[upStreamWaterNodeIDTempFix];
            var downStreamWaterwayNodeTempFix = waterwayRoutePlannerGraph.m_dicWaterwayNode[downStreamWaterNodeIDTempFix];

            var insertedTopoNodeCoordinate = ShortestInsertedPointToShapeLink(startCoordinate, insertedShapeLink.shapeNodeWithIndex);
            if (IsSameCoordinate(waterwayRoutePlannerGraph.m_dicWaterwayNode[_startWaterwayNodeID].waterNodeCoordinate, insertedTopoNodeCoordinate))
            {
                return _startWaterwayNodeID;
            }
            var insertedWaterwayTopoNode = new WaterwayTopoNode();
            insertedWaterwayTopoNode.waterNodeID = "START-0000-START";
            insertedWaterwayTopoNode.waterNodeClass = 2;
            insertedWaterwayTopoNode.waterNodeType = 6;
            insertedWaterwayTopoNode.waterNodeCoordinate = insertedTopoNodeCoordinate;
            insertedWaterwayTopoNode.waterNodeInformation = "Start WaterwayTopoNode Mapped on the WaterwayGraph";
            insertedWaterwayTopoNode.waterNodeName = "出发点";

            var insertedWaterwayTopoLinkUp = new WaterwayTopoLink(insertedTopoLink);
            var insertedWaterwayTopoLinkDown = new WaterwayTopoLink(insertedTopoLink);

            if (IsSameCoordinate(new double[2] { insertedTopoLink.channelGeometry[0].X, insertedTopoLink.channelGeometry[0].Y }, upStreamWaterwayNodeTempFix.waterNodeCoordinate))
            {                
                insertedWaterwayTopoLinkUp.waterLinkID = insertedTopoLink.upStreamWaterNodeID + '-' + insertedWaterwayTopoNode.waterNodeID;
                insertedWaterwayTopoLinkUp.downStreamWaterNodeID = insertedWaterwayTopoNode.waterNodeID;
                var insertedWaterwayTopoLinkUpChannelLength = 0.0;
                var insertedWaterwayTopoLinkUpGeometry = new CoordinateList();
                for (var i = 0; i <= insertedShapeLink.shapeNodeWithIndex[0].index; i++)
                {
                    insertedWaterwayTopoLinkUpGeometry.Add(insertedTopoLink.channelGeometry[i]);
                    if (i >= 1)
                    {
                        var coordinateLastTempC = new double[2] { insertedWaterwayTopoLinkUpGeometry[i - 1].X, insertedWaterwayTopoLinkUpGeometry[i - 1].Y };
                        var coordinateCurrentTempC = new double[2] { insertedWaterwayTopoLinkUpGeometry[i].X, insertedWaterwayTopoLinkUpGeometry[i].Y };
                        insertedWaterwayTopoLinkUpChannelLength += GreatCircleDistance(coordinateLastTempC, coordinateCurrentTempC);
                    }
                }
                insertedWaterwayTopoLinkUpGeometry.Add(new GeoAPI.Geometries.Coordinate(insertedTopoNodeCoordinate[0], insertedTopoNodeCoordinate[1]));
                var insertedWaterwayTopoLinkUpGeometryCount = insertedWaterwayTopoLinkUpGeometry.Count;
                var coordinateLastTemp = new double[2] { insertedWaterwayTopoLinkUpGeometry[insertedWaterwayTopoLinkUpGeometryCount - 2].X, insertedWaterwayTopoLinkUpGeometry[insertedWaterwayTopoLinkUpGeometryCount - 2].Y };
                var coordinateCurrentTemp = new double[2] { insertedWaterwayTopoLinkUpGeometry[insertedWaterwayTopoLinkUpGeometryCount - 1].X, insertedWaterwayTopoLinkUpGeometry[insertedWaterwayTopoLinkUpGeometryCount - 1].Y };
                insertedWaterwayTopoLinkUpChannelLength += GreatCircleDistance(coordinateLastTemp, coordinateCurrentTemp);
                insertedWaterwayTopoLinkUp.channelLength = insertedWaterwayTopoLinkUpChannelLength;
                insertedWaterwayTopoLinkUp.channelGeometry = insertedWaterwayTopoLinkUpGeometry.ToArray();
                waterwayRoutePlannerGraph.m_dicWaterwayLink.Add(insertedWaterwayTopoLinkUp.waterLinkID, insertedWaterwayTopoLinkUp);

                
                insertedWaterwayTopoLinkDown.waterLinkID = insertedWaterwayTopoNode.waterNodeID + '-' + insertedTopoLink.downStreamWaterNodeID;
                insertedWaterwayTopoLinkDown.upStreamWaterNodeID = insertedWaterwayTopoNode.waterNodeID;
                insertedWaterwayTopoLinkDown.channelLength = insertedTopoLink.channelLength - insertedWaterwayTopoLinkUp.channelLength;
                var insertedWaterwayTopoLinkDownGeometry = new CoordinateList();
                insertedWaterwayTopoLinkDownGeometry.Add(new GeoAPI.Geometries.Coordinate(insertedTopoNodeCoordinate[0], insertedTopoNodeCoordinate[1]));
                for (var i = insertedShapeLink.shapeNodeWithIndex[1].index; i < insertedTopoLink.channelGeometry.Length; i++)
                {
                    insertedWaterwayTopoLinkDownGeometry.Add(insertedTopoLink.channelGeometry[i]);
                }
                insertedWaterwayTopoLinkDown.channelGeometry = insertedWaterwayTopoLinkDownGeometry.ToArray();
                waterwayRoutePlannerGraph.m_dicWaterwayLink.Add(insertedWaterwayTopoLinkDown.waterLinkID, insertedWaterwayTopoLinkDown);
            }
            else
            {
                insertedWaterwayTopoLinkUp.waterLinkID = insertedTopoLink.upStreamWaterNodeID + '-' + insertedWaterwayTopoNode.waterNodeID;
                insertedWaterwayTopoLinkUp.downStreamWaterNodeID = insertedWaterwayTopoNode.waterNodeID;
                var insertedWaterwayTopoLinkUpChannelLength = 0.0;
                var insertedWaterwayTopoLinkUpGeometry = new CoordinateList();
                for (var i = insertedTopoLink.channelGeometry.Length - 1; i >= insertedShapeLink.shapeNodeWithIndex[1].index; i--)
                {
                    insertedWaterwayTopoLinkUpGeometry.Add(insertedTopoLink.channelGeometry[i]);
                    if (i <= insertedTopoLink.channelGeometry.Length - 2)
                    {
                        var coordinateLastTempC = new double[2] { insertedTopoLink.channelGeometry[i + 1].X, insertedTopoLink.channelGeometry[i + 1].Y };
                        var coordinateCurrentTempC = new double[2] { insertedTopoLink.channelGeometry[i].X, insertedTopoLink.channelGeometry[i].Y };
                        insertedWaterwayTopoLinkUpChannelLength += GreatCircleDistance(coordinateLastTempC, coordinateCurrentTempC);
                    }
                }
                insertedWaterwayTopoLinkUpGeometry.Add(new GeoAPI.Geometries.Coordinate(insertedTopoNodeCoordinate[0], insertedTopoNodeCoordinate[1]));
                var insertedWaterwayTopoLinkUpGeometryCount = insertedWaterwayTopoLinkUpGeometry.Count;
                var coordinateLastTemp = new double[2] { insertedWaterwayTopoLinkUpGeometry[insertedWaterwayTopoLinkUpGeometryCount - 2].X, insertedWaterwayTopoLinkUpGeometry[insertedWaterwayTopoLinkUpGeometryCount - 2].Y };
                var coordinateCurrentTemp = new double[2] { insertedWaterwayTopoLinkUpGeometry[insertedWaterwayTopoLinkUpGeometryCount - 1].X, insertedWaterwayTopoLinkUpGeometry[insertedWaterwayTopoLinkUpGeometryCount - 1].Y };
                insertedWaterwayTopoLinkUpChannelLength += GreatCircleDistance(coordinateLastTemp, coordinateCurrentTemp);
                insertedWaterwayTopoLinkUp.channelLength = insertedWaterwayTopoLinkUpChannelLength;
                insertedWaterwayTopoLinkUp.channelGeometry = insertedWaterwayTopoLinkUpGeometry.ToArray();
                waterwayRoutePlannerGraph.m_dicWaterwayLink.Add(insertedWaterwayTopoLinkUp.waterLinkID, insertedWaterwayTopoLinkUp);


                insertedWaterwayTopoLinkDown.waterLinkID = insertedWaterwayTopoNode.waterNodeID + '-' + insertedTopoLink.downStreamWaterNodeID;
                insertedWaterwayTopoLinkDown.upStreamWaterNodeID = insertedWaterwayTopoNode.waterNodeID;
                insertedWaterwayTopoLinkDown.channelLength = insertedTopoLink.channelLength - insertedWaterwayTopoLinkUp.channelLength;
                var insertedWaterwayTopoLinkDownGeometry = new CoordinateList();
                insertedWaterwayTopoLinkDownGeometry.Add(new GeoAPI.Geometries.Coordinate(insertedTopoNodeCoordinate[0], insertedTopoNodeCoordinate[1]));
                for (var i = insertedShapeLink.shapeNodeWithIndex[0].index; i >= 0 ; i--)
                {
                    insertedWaterwayTopoLinkDownGeometry.Add(insertedTopoLink.channelGeometry[i]);
                }
                insertedWaterwayTopoLinkDown.channelGeometry = insertedWaterwayTopoLinkDownGeometry.ToArray();
                waterwayRoutePlannerGraph.m_dicWaterwayLink.Add(insertedWaterwayTopoLinkDown.waterLinkID, insertedWaterwayTopoLinkDown);
            }
            

            if (!insertedTopoLink.oneWay)
            {
                insertedWaterwayTopoNode.waterLinkInNumber = 2;
                insertedWaterwayTopoNode.waterLinkOutNumber = 2;
                insertedWaterwayTopoNode.waterLinkInList = new string[2] { insertedWaterwayTopoLinkUp.waterLinkID, insertedWaterwayTopoLinkDown.waterLinkID };
                insertedWaterwayTopoNode.waterLinkOutList = new string[2] { insertedWaterwayTopoLinkUp.waterLinkID, insertedWaterwayTopoLinkDown.waterLinkID };

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
            var candidateShapeLink = new List<ShapeLinkWithShortestDistance>();
            foreach (var currentAvailableWaterwayLink in availableWaterwayLinkList)
            {
                var currentCandidateShapeLink = new List<ShapeLinkWithShortestDistance>();
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
                    currentCandidateShapeLink.Add(new ShapeLinkWithShortestDistance(currentAvailableWaterwayLink, shapeLinkTemp, ShortestDistanceToShapeLink(goalCoordinate, shapeLinkTemp)));
                }
                else if (nearestShapeNode[0].index == (currentAvailableWaterwayLinkCoordinatesLength - 1))
                {
                    var shapeLinkTemp = new ShapeNodeWithIndex[] { currentAvailablecandidateShapeLNodeList[currentAvailableWaterwayLinkCoordinatesLength - 2], currentAvailablecandidateShapeLNodeList[currentAvailableWaterwayLinkCoordinatesLength - 1] };
                    currentCandidateShapeLink.Add(new ShapeLinkWithShortestDistance(currentAvailableWaterwayLink, shapeLinkTemp, ShortestDistanceToShapeLink(goalCoordinate, shapeLinkTemp)));
                }
                else
                {
                    var shapeLinkTempUp = new ShapeNodeWithIndex[] { currentAvailablecandidateShapeLNodeList[nearestShapeNode[0].index - 1], currentAvailablecandidateShapeLNodeList[nearestShapeNode[0].index] };
                    var shapeLinkTempDown = new ShapeNodeWithIndex[] { currentAvailablecandidateShapeLNodeList[nearestShapeNode[0].index], currentAvailablecandidateShapeLNodeList[nearestShapeNode[0].index + 1] };
                    currentCandidateShapeLink.Add(new ShapeLinkWithShortestDistance(currentAvailableWaterwayLink, shapeLinkTempUp, ShortestDistanceToShapeLink(goalCoordinate, shapeLinkTempUp)));
                    currentCandidateShapeLink.Add(new ShapeLinkWithShortestDistance(currentAvailableWaterwayLink, shapeLinkTempDown, ShortestDistanceToShapeLink(goalCoordinate, shapeLinkTempDown)));
                }
                currentCandidateShapeLink.Sort(ShapeLinkWithDistanceCompare);
                candidateShapeLink.Add(currentCandidateShapeLink[0]);
            }
            candidateShapeLink.Sort(ShapeLinkWithDistanceCompare);
            var insertedShapeLink = candidateShapeLink[0];
            var insertedTopoLink = waterwayRoutePlannerGraph.m_dicWaterwayLink[insertedShapeLink.affiliatedTopoLinkID];
            var insertedTopoNodeCoordinate = ShortestInsertedPointToShapeLink(goalCoordinate, insertedShapeLink.shapeNodeWithIndex);

            var upStreamWaterNodeIDTempFix = waterwayRoutePlannerGraph.m_dicWaterwayLink[insertedTopoLink.waterLinkID].upStreamWaterNodeID;
            var downStreamWaterNodeIDTempFix = waterwayRoutePlannerGraph.m_dicWaterwayLink[insertedTopoLink.waterLinkID].downStreamWaterNodeID;
            var upStreamWaterwayNodeTempFix = waterwayRoutePlannerGraph.m_dicWaterwayNode[upStreamWaterNodeIDTempFix];
            var downStreamWaterwayNodeTempFix = waterwayRoutePlannerGraph.m_dicWaterwayNode[downStreamWaterNodeIDTempFix];

            if (IsSameCoordinate(waterwayRoutePlannerGraph.m_dicWaterwayNode[_goalWaterwayNodeID].waterNodeCoordinate, insertedTopoNodeCoordinate))
            {
                return _goalWaterwayNodeID;
            }

            var insertedWaterwayTopoNode = new WaterwayTopoNode();
            insertedWaterwayTopoNode.waterNodeID = "GOAL-9999-GOAL";
            insertedWaterwayTopoNode.waterNodeClass = 2;
            insertedWaterwayTopoNode.waterNodeType = 6;
            insertedWaterwayTopoNode.waterNodeCoordinate = insertedTopoNodeCoordinate;
            insertedWaterwayTopoNode.waterNodeInformation = "Goal WaterwayTopoNode Mapped on the WaterwayGraph";
            insertedWaterwayTopoNode.waterNodeName = "目标点";

            var insertedWaterwayTopoLinkUp = new WaterwayTopoLink(insertedTopoLink);
            var insertedWaterwayTopoLinkDown = new WaterwayTopoLink(insertedTopoLink);

            if (IsSameCoordinate(new double[2] { insertedTopoLink.channelGeometry[0].X, insertedTopoLink.channelGeometry[0].Y }, upStreamWaterwayNodeTempFix.waterNodeCoordinate))
            {
                // 处理插入目标点上游拓扑线的属性
                insertedWaterwayTopoLinkUp.waterLinkID = insertedTopoLink.upStreamWaterNodeID + '-' + insertedWaterwayTopoNode.waterNodeID;
                insertedWaterwayTopoLinkUp.downStreamWaterNodeID = insertedWaterwayTopoNode.waterNodeID;
                var insertedWaterwayTopoLinkUpChannelLength = 0.0;
                var insertedWaterwayTopoLinkUpGeometry = new CoordinateList();
                for (var i = 0; i <= insertedShapeLink.shapeNodeWithIndex[0].index; i++)
                {
                    insertedWaterwayTopoLinkUpGeometry.Add(insertedTopoLink.channelGeometry[i]);
                    if (i >= 1)
                    {
                        var coordinateLastTempC = new double[2] { insertedWaterwayTopoLinkUpGeometry[i - 1].X, insertedWaterwayTopoLinkUpGeometry[i - 1].Y };
                        var coordinateCurrentTempC = new double[2] { insertedWaterwayTopoLinkUpGeometry[i].X, insertedWaterwayTopoLinkUpGeometry[i].Y };
                        insertedWaterwayTopoLinkUpChannelLength += GreatCircleDistance(coordinateLastTempC, coordinateCurrentTempC);
                    }
                }
                insertedWaterwayTopoLinkUpGeometry.Add(new GeoAPI.Geometries.Coordinate(insertedTopoNodeCoordinate[0], insertedTopoNodeCoordinate[1]));
                var insertedWaterwayTopoLinkUpGeometryCount = insertedWaterwayTopoLinkUpGeometry.Count;
                var coordinateLastTemp = new double[2] { insertedWaterwayTopoLinkUpGeometry[insertedWaterwayTopoLinkUpGeometryCount - 2].X, insertedWaterwayTopoLinkUpGeometry[insertedWaterwayTopoLinkUpGeometryCount - 2].Y };
                var coordinateCurrentTemp = new double[2] { insertedWaterwayTopoLinkUpGeometry[insertedWaterwayTopoLinkUpGeometryCount - 1].X, insertedWaterwayTopoLinkUpGeometry[insertedWaterwayTopoLinkUpGeometryCount - 1].Y };
                insertedWaterwayTopoLinkUpChannelLength += GreatCircleDistance(coordinateLastTemp, coordinateCurrentTemp);
                insertedWaterwayTopoLinkUp.channelLength = insertedWaterwayTopoLinkUpChannelLength;
                insertedWaterwayTopoLinkUp.channelGeometry = insertedWaterwayTopoLinkUpGeometry.ToArray();
                waterwayRoutePlannerGraph.m_dicWaterwayLink.Add(insertedWaterwayTopoLinkUp.waterLinkID, insertedWaterwayTopoLinkUp);

                // 处理插入目标点下游拓扑线的属性
                insertedWaterwayTopoLinkDown.waterLinkID = insertedWaterwayTopoNode.waterNodeID + '-' + insertedTopoLink.downStreamWaterNodeID;
                insertedWaterwayTopoLinkDown.upStreamWaterNodeID = insertedWaterwayTopoNode.waterNodeID;
                insertedWaterwayTopoLinkDown.channelLength = insertedTopoLink.channelLength - insertedWaterwayTopoLinkUp.channelLength;
                var insertedWaterwayTopoLinkDownGeometry = new CoordinateList();
                insertedWaterwayTopoLinkDownGeometry.Add(new GeoAPI.Geometries.Coordinate(insertedTopoNodeCoordinate[0], insertedTopoNodeCoordinate[1]));
                for (var i = insertedShapeLink.shapeNodeWithIndex[1].index; i < insertedTopoLink.channelGeometry.Length; i++)
                {
                    insertedWaterwayTopoLinkDownGeometry.Add(insertedTopoLink.channelGeometry[i]);
                }
                insertedWaterwayTopoLinkDown.channelGeometry = insertedWaterwayTopoLinkDownGeometry.ToArray();
                waterwayRoutePlannerGraph.m_dicWaterwayLink.Add(insertedWaterwayTopoLinkDown.waterLinkID, insertedWaterwayTopoLinkDown);
            }
            else
            {

                insertedWaterwayTopoLinkUp.waterLinkID = insertedTopoLink.upStreamWaterNodeID + '-' + insertedWaterwayTopoNode.waterNodeID;
                insertedWaterwayTopoLinkUp.downStreamWaterNodeID = insertedWaterwayTopoNode.waterNodeID;
                var insertedWaterwayTopoLinkUpChannelLength = 0.0;
                var insertedWaterwayTopoLinkUpGeometry = new CoordinateList();
                for (var i = insertedTopoLink.channelGeometry.Length - 1; i >= insertedShapeLink.shapeNodeWithIndex[1].index; i--)
                {
                    insertedWaterwayTopoLinkUpGeometry.Add(insertedTopoLink.channelGeometry[i]);
                    if (i <= insertedTopoLink.channelGeometry.Length - 2)
                    {
                        var coordinateLastTempC = new double[2] { insertedTopoLink.channelGeometry[i + 1].X, insertedTopoLink.channelGeometry[i + 1].Y };
                        var coordinateCurrentTempC = new double[2] { insertedTopoLink.channelGeometry[i].X, insertedTopoLink.channelGeometry[i].Y };
                        insertedWaterwayTopoLinkUpChannelLength += GreatCircleDistance(coordinateLastTempC, coordinateCurrentTempC);
                    }
                }
                insertedWaterwayTopoLinkUpGeometry.Add(new GeoAPI.Geometries.Coordinate(insertedTopoNodeCoordinate[0], insertedTopoNodeCoordinate[1]));
                var insertedWaterwayTopoLinkUpGeometryCount = insertedWaterwayTopoLinkUpGeometry.Count;
                var coordinateLastTemp = new double[2] { insertedWaterwayTopoLinkUpGeometry[insertedWaterwayTopoLinkUpGeometryCount - 2].X, insertedWaterwayTopoLinkUpGeometry[insertedWaterwayTopoLinkUpGeometryCount - 2].Y };
                var coordinateCurrentTemp = new double[2] { insertedWaterwayTopoLinkUpGeometry[insertedWaterwayTopoLinkUpGeometryCount - 1].X, insertedWaterwayTopoLinkUpGeometry[insertedWaterwayTopoLinkUpGeometryCount - 1].Y };
                insertedWaterwayTopoLinkUpChannelLength += GreatCircleDistance(coordinateLastTemp, coordinateCurrentTemp);
                insertedWaterwayTopoLinkUp.channelLength = insertedWaterwayTopoLinkUpChannelLength;
                insertedWaterwayTopoLinkUp.channelGeometry = insertedWaterwayTopoLinkUpGeometry.ToArray();
                waterwayRoutePlannerGraph.m_dicWaterwayLink.Add(insertedWaterwayTopoLinkUp.waterLinkID, insertedWaterwayTopoLinkUp);


                insertedWaterwayTopoLinkDown.waterLinkID = insertedWaterwayTopoNode.waterNodeID + '-' + insertedTopoLink.downStreamWaterNodeID;
                insertedWaterwayTopoLinkDown.upStreamWaterNodeID = insertedWaterwayTopoNode.waterNodeID;
                insertedWaterwayTopoLinkDown.channelLength = insertedTopoLink.channelLength - insertedWaterwayTopoLinkUp.channelLength;
                var insertedWaterwayTopoLinkDownGeometry = new CoordinateList();
                insertedWaterwayTopoLinkDownGeometry.Add(new GeoAPI.Geometries.Coordinate(insertedTopoNodeCoordinate[0], insertedTopoNodeCoordinate[1]));
                for (var i = insertedShapeLink.shapeNodeWithIndex[0].index; i >= 0; i--)
                {
                    insertedWaterwayTopoLinkDownGeometry.Add(insertedTopoLink.channelGeometry[i]);
                }
                insertedWaterwayTopoLinkDown.channelGeometry = insertedWaterwayTopoLinkDownGeometry.ToArray();
                waterwayRoutePlannerGraph.m_dicWaterwayLink.Add(insertedWaterwayTopoLinkDown.waterLinkID, insertedWaterwayTopoLinkDown);
            }            

            if (!insertedTopoLink.oneWay)
            {
                insertedWaterwayTopoNode.waterLinkInNumber = 2;
                insertedWaterwayTopoNode.waterLinkOutNumber = 2;
                insertedWaterwayTopoNode.waterLinkInList = new string[2] { insertedWaterwayTopoLinkUp.waterLinkID, insertedWaterwayTopoLinkDown.waterLinkID };
                insertedWaterwayTopoNode.waterLinkOutList = new string[2] { insertedWaterwayTopoLinkUp.waterLinkID, insertedWaterwayTopoLinkDown.waterLinkID };

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

        public string[] InsertTemporaryTopoFeatures(double[] startCoordinate, double[] goalCoordinat)
        {
            var _startWaterwayNodeIDOnNet = "";
            _startWaterwayNodeIDOnNet = GetNearestWaterwayNodeByCoorninate(waterwayRoutePlannerGraph, startCoordinate);
            _startWaterwayNodeIDOnNet = InsertStartTopoFeatures(_startWaterwayNodeIDOnNet, startCoordinate);

            var _goalWaterwayNodeIDOnNet = "";
            var _goalWaterwayNodeIDOnNetExcStart = GetNearestWaterwayNodeByCoorninate(waterwayRoutePlannerGraph, goalCoordinat);
            var distance1 = GetDistanceByCoordinate(goalCoordinat, waterwayRoutePlannerGraph.m_dicWaterwayNode[_goalWaterwayNodeIDOnNetExcStart].waterNodeCoordinate);
            var distance2 = GetDistanceByCoordinate(goalCoordinat, waterwayRoutePlannerGraph.m_dicWaterwayNode[_startWaterwayNodeIDOnNet].waterNodeCoordinate);

            if (distance1 <= distance2)
            {
                _goalWaterwayNodeIDOnNet = _goalWaterwayNodeIDOnNetExcStart;
            }
            else
            {
                _goalWaterwayNodeIDOnNet = _startWaterwayNodeIDOnNet;
            }
            _goalWaterwayNodeIDOnNet = InsertGoalTopoFeatures(_goalWaterwayNodeIDOnNet, goalCoordinat);

            StartWaterwayNodeID = _startWaterwayNodeIDOnNet;
            GoalWaterwayNodeID = _goalWaterwayNodeIDOnNet;

            return new string[2] { StartWaterwayNodeID, GoalWaterwayNodeID };
        }

        public WaterwayRoutePlanner()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        /// <summary>
        /// 指定航道网络起止点ID搜索最优路径的方法
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="vehicle"></param>
        /// <param name="start"></param>
        /// <param name="goal"></param>
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


        /// <summary>
        /// 指定起止点坐标搜索最优路径的方法
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="vehicle"></param>
        /// <param name="startCoordinate"></param>
        /// <param name="goalCoordinate"></param>
        public WaterwayRoutePlanner(WaterwayGraph graph, WaterwayVehicle vehicle, double[] startCoordinate, double[] goalCoordinate)
        {
            waterwayRoutePlannerGraph = WaterwayGraph.ConstructNewInstanceFromExistClass(graph);

            var StartGoalWaterwayNodeIDOnNetArray = InsertTemporaryTopoFeatures(startCoordinate, goalCoordinate);

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
                        //double priority = newCost + GreatCircleDistance(waterwayRoutePlannerGraph.m_dicWaterwayNode[StartGoalWaterwayNodeIDOnNetArray[0]], waterwayRoutePlannerGraph.m_dicWaterwayNode[StartGoalWaterwayNodeIDOnNetArray[1]], waterwayRoutePlannerGraph.m_dicWaterwayNode[next.nextWaterwayNodeID]);
                        frontier.Enqueue(next.nextWaterwayNodeID, priority);
                        cameFrom[next.nextWaterwayNodeID] = new string[2] { current, next.toNextWaterwayNodeLinkID };
                    }
                }
            }
        }


        public WaterwayRoutePlanner(WaterwayGraph graph, WaterwayVehicle vehicle, double[] startCoordinate, double[] goalCoordinate, int method)
        {
            if (method == 2)
            {
                waterwayRoutePlannerGraph = WaterwayGraph.ConstructNewInstanceFromExistClass(graph);

                var StartGoalWaterwayNodeIDOnNetArray = InsertTemporaryTopoFeatures(startCoordinate, goalCoordinate);

                SimplePriorityQueue<string, double> frontier = new SimplePriorityQueue<string, double>();
                frontier.Enqueue(StartGoalWaterwayNodeIDOnNetArray[0], 0.0);
 
                cameFrom.Add(StartGoalWaterwayNodeIDOnNetArray[0], new string[] { StartWaterwayNodeID, "START" });
                costSoFar[StartGoalWaterwayNodeIDOnNetArray[0]] = Cost(0.0);

                while (frontier.Count > 0)
                {
                    var current = frontier.Dequeue();

                    //if (current == "GZGHD-0009")
                    //{
                    //    Console.WriteLine(current);
                    //}

                    if (current.Equals(StartGoalWaterwayNodeIDOnNetArray[1]))
                    {
                        isFindOptimalRoute = true;
                        break;
                    }

                    foreach (var next in waterwayRoutePlannerGraph.Neighbors(current, vehicle, method))
                    {
                        double newCost = costSoFar[current] + Cost(next.toNextWaterwayNodeCost);
                        if (!costSoFar.ContainsKey(next.nextWaterwayNodeID) || newCost < costSoFar[next.nextWaterwayNodeID])
                        {
                            costSoFar[next.nextWaterwayNodeID] = newCost;
                            double priority = newCost + GreatCircleDistance(waterwayRoutePlannerGraph.m_dicWaterwayNode[StartGoalWaterwayNodeIDOnNetArray[0]], waterwayRoutePlannerGraph.m_dicWaterwayNode[StartGoalWaterwayNodeIDOnNetArray[1]], waterwayRoutePlannerGraph.m_dicWaterwayNode[next.nextWaterwayNodeID]);
                            frontier.Enqueue(next.nextWaterwayNodeID, priority);
                            cameFrom[next.nextWaterwayNodeID] = new string[2] { current, next.toNextWaterwayNodeLinkID };
                        }
                    }
                }


            }
        }
    }
}