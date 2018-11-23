using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 最优航线规划模型原型程序
/// QIAN LONG 2018/10/28
/// </summary>
namespace IWRPM
{
    class AStarSearch
    {
        // TODO
    }

    class BreadthFirstSearch
    {
        // TODO
    }

    class Program
    {
        static void OutputRouteResults(WaterwayGraph waterwayGraph, WaterwayRoutePlanner channelRoutePlanner, string start , string goal)
        {
            if (channelRoutePlanner.isFindOptimalRoute)
            {
                Stack<string> routeResultsStack = new Stack<string>();

                var currentResearchElement = goal;
                do
                {
                    routeResultsStack.Push(currentResearchElement);
                    routeResultsStack.Push(channelRoutePlanner.cameFrom[currentResearchElement][1]);
                    currentResearchElement = channelRoutePlanner.cameFrom[currentResearchElement][0];
                }
                while (currentResearchElement != start);
                routeResultsStack.Push(currentResearchElement);

                int routeResultsStackCount = routeResultsStack.Count;
                for (var i = 0; i < routeResultsStackCount; i++)
                {
                    string currentRouteResult = routeResultsStack.Pop();
                    Console.WriteLine(currentRouteResult);
                }
            }
            else
            {
                Console.WriteLine("Can not reach the destination ...");
            }
            
        }

        static void Main(string[] args)
        {
            var guangDongWaterwayGraph = new WaterwayGraph();
            guangDongWaterwayGraph.LoadWaterwayNetworkDatasets();

            var testBulkCarrier = new WaterwayVehicle("FreedomWhuSGG", 300000f, 339f, 58.0f, 30.0f, 23.0f, 10.0f);
            var testPassenger = new WaterwayVehicle("Passenger", 0f, 0f, 0.0f, 0.0f, 0.0f, 0.0f);
            //var startWaterwayNode = "XJ3JKZ-0001";
            //var goalWaterwayNode = "BJ1-9001";
            //var guangDongChannelRoutePlanner = new WaterwayRoutePlanner(guangDongWaterwayGraph, testPassenger, startWaterwayNode, goalWaterwayNode);
            //OutputRouteResults(guangDongWaterwayGraph, guangDongChannelRoutePlanner, startWaterwayNode, goalWaterwayNode);

            //var startWaterwayNodeCoordiante = new double[] { 111.580000, 23.280000 };
            //var goalWaterwayNodeCoordiante = new double[] { 112.882832, 23.430000 };
            //var startWaterwayNodeCoordiante = new double[] { 113.264213, 23.117617 };
            //var goalWaterwayNodeCoordiante = new double[] { 111.8523483, 23.1409737 };
            //var guangDongChannelRoutePlanner = new WaterwayRoutePlanner(guangDongWaterwayGraph, testPassenger, startWaterwayNodeCoordiante, goalWaterwayNodeCoordiante);
            //OutputRouteResults(guangDongWaterwayGraph, guangDongChannelRoutePlanner, guangDongChannelRoutePlanner.StartWaterwayNodeID, guangDongChannelRoutePlanner.GoalWaterwayNodeID);

            var startWaterwayNodeCoordiante = new double[] { 113.264213, 23.117617 };
            var goalWaterwayNodeCoordiante = new double[] { 111.8523483, 23.1409737 };
            var guangDongChannelRoutePlanner = new WaterwayRoutePlanner(guangDongWaterwayGraph, testPassenger, startWaterwayNodeCoordiante, goalWaterwayNodeCoordiante, 2);
            OutputRouteResults(guangDongWaterwayGraph, guangDongChannelRoutePlanner, guangDongChannelRoutePlanner.StartWaterwayNodeID, guangDongChannelRoutePlanner.GoalWaterwayNodeID);

            Console.ReadLine();  
        }
    }
}
