using System;
using System.Collections.Generic;

// TODO: rename this as EdgeRemover

namespace TSP
{
    /// <summary>
    /// This class is used in "hard" mode, where edges are selectively removed. - APS
    /// </summary>
    internal class HardMode
    {
        // List of edges that are removed
        private HashSet<Edge> _removedEdges;

        // Keep a local copy of the list of cities
        private readonly City[] _cities;

        // Local reference to the random number generator, to allow repeatable runs
        private readonly Random _rnd;

        private Modes _mode;

        // Edge object, used only to keep track of edge removal
        private struct Edge
        {
            public City City1, City2;
            public Edge(City city1, City city2)
            {
                this.City1 = city1;
                this.City2 = city2;
            }
        }

        public HardMode(Modes mode, Random rnd, City[] cities)
        {
            this._mode = mode;
            this._rnd = rnd;
            this._cities = cities;
            _removedEdges = new HashSet<Edge>();
        }

        // removes a specified number of paths, 
        // (one-way only, e.g., if we remove a path from A to B, the return path from B to A still exists)
        public void RemovePaths(int numberToRemove)
        {
            // Make sure we don't remove an impossible number of edges
            // N^2 - N(diagonals) - N (remaining paths) = (N)(N-2)
            var maxPathsToRemove = _cities.Length * (_cities.Length - 2);
            if (numberToRemove > maxPathsToRemove)
                numberToRemove = maxPathsToRemove;

            _removedEdges = new HashSet<Edge>();
            // The reference path ensures that a valid path always remains after our deleting frenzy.
            var referencePath = GenerateReferencePath(_cities);
            for (var i = 0; i < numberToRemove; i++)
            {
                var removed = false;
                while (!removed)
                {
                    var candidateEdge = new Edge(
                        _cities[_rnd.Next(_cities.Length)],
                        _cities[_rnd.Next(_cities.Length)]
                        );
                    if (!candidateEdge.City1.Equals(candidateEdge.City2) &&
                        !referencePath.Contains(candidateEdge) &&
                        !_removedEdges.Contains(candidateEdge))
                    {
                        _removedEdges.Add(candidateEdge);
                        removed = true;
                    }
                }

            }

        }

        // This is an optimization -- we cache one object to avoid repeated memory allocation & deallocation
        // It should only be used in isRemoved and should never be returned to user code, since its internal 
        // values will keep changing.
        static Edge _tempEdge;

        // this method is not thread-safe, i.e., it should not be called from two separate threads
        public bool IsEdgeRemoved(City city1, City city2)
        {
            _tempEdge.City1 = city1;
            _tempEdge.City2 = city2;
            return _removedEdges.Contains(_tempEdge);
        }

        // Shuffles cities to generate a temporary reference path.  The reference path
        // guarantees that we don't create an impossible graph when removing edges.
        private HashSet<Edge> GenerateReferencePath(IReadOnlyList<City> cities) //List<City> cities)
        {
            var referencePath = new City[cities.Count];
            var remainingCities = new City[cities.Count];
            for (var i = 0; i < cities.Count; i++)
                remainingCities[i] = cities[i];
            var remainingSize = remainingCities.Length;
            for (var i = 0; i < referencePath.Length; i++)
            {
                var index = _rnd.Next() % remainingSize;
                referencePath[i] = remainingCities[index];
                remainingCities[index] = remainingCities[remainingSize - 1];
            }
            // put the loop into a HashSet of Edges...
            var referenceSet = new HashSet<Edge>();
            for (var i = 0; i < cities.Count - 1; i++)
                referenceSet.Add(new Edge(cities[i], cities[(i + 1) % cities.Count]));
            return referenceSet;
        }

        /// <summary>
        /// Difficulty Modes:
        /// <ul>
        /// <li>Easy:   Distances are symmetric (for debugging)</li>
        /// <li>Normal: Distances are asymmetric</li>
        /// <li>Hard:   Asymmetric distances; some paths are blocked</li>
        /// </ul>
        /// </summary>
        public enum Modes { Easy = 0, Normal, Hard }
        private const Modes DefaultMode = Modes.Normal;

        public static Modes GetMode(string modeName)
        {
            string[] modeNames = { "Easy", "Normal", "Hard" }; // in corresponding order

            for (var i = 0; i < modeNames.Length; i++)
                if (modeNames[i].Equals(modeName, StringComparison.OrdinalIgnoreCase))
                    return (Modes)i;
            return DefaultMode;
        }
    }

 }
