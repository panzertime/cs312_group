using System;

namespace TSP
{

    /// <summary>
    /// Represents a city, which is a node in the Traveling Salesman Problem
    /// </summary>
    internal class City
    {

        public City(double x, double y)
        {
            X = x;
            Y = y;
            _elevation = 0.0;
        }

        public City(double x, double y, double elevation)
        {
            X = x;
            Y = y;
            _elevation = elevation;
        }

        private readonly double _elevation;
        private const double ScaleFactor = 1000;

        // These are C# property accessors
        public double X { get; set; }

        public double Y { get; set; }


        /// <summary>
        /// How much does it cost to get from this city to the destination?
        /// Note that this is an asymmetric cost function.
        /// 
        /// In advanced mode, it returns infinity when there is no connection.
        /// </summary>
        public double CostToGetTo (City destination) 
        {
            // Cartesian distance
            var magnitude = Math.Sqrt(Math.Pow(X - destination.X, 2) + Math.Pow(Y - destination.Y, 2));

            // For Medium and Hard modes, add in an asymmetric cost (in easy mode it is zero).
            magnitude += (destination._elevation - _elevation);
            if (magnitude < 0.0) magnitude = 0.0;

            magnitude *= ScaleFactor;

            // In hard mode, remove edges; this slows down the calculation...
            if (_modeManager.IsEdgeRemoved(this,destination))
                magnitude = double.PositiveInfinity;

            return Math.Round(magnitude);
        }


        // This is makes distances asymmetric
        // 0 <= Maximum elevation <= 1
        public const double MaxElevation = 0.10;

        // The mode manager applies to all the cities...
        private static HardMode _modeManager;
        public static void SetModeManager(HardMode modeManager)
        {
            City._modeManager = modeManager;
        }

    }
}
