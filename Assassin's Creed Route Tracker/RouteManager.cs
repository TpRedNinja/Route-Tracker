using System;
using System.IO;

namespace Assassin_s_Creed_Route_Tracker
{
    public class RouteManager
    {
        private string routeFilePath;

        public RouteManager(string routeFilePath)
        {
            this.routeFilePath = routeFilePath;
        }

        public string[] LoadRoute()
        {
            if (File.Exists(routeFilePath))
            {
                return File.ReadAllLines(routeFilePath);
            }
            else
            {
                throw new FileNotFoundException("Route file not found.");
            }
        }

        // Add methods to process and display the route as needed
    }
}
