using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Priority_Queue;

namespace TSP
{
    internal static class ArrayExtension
    {
        public static T[] AppendToCopy<T>(this T[] arr, T value)
        {
            var n = arr.Length;
            var result = new T[n + 1];
            for (var i = 0; i < n; ++i)
            {
                result[i] = arr[i];
            }
            result[n] = value;
            return result;
        }
    }

    internal class TspState : FastPriorityQueueNode
    {
        private const float DepthScale = 3000;

        /// <summary>
        /// In row-major order (row first, then column).
        /// CostMatrix[city1, city2] represents the cost of going from city 1 to city 2
        /// </summary>
        public double[,] CostMatrix { get; }
        public double LowerBound { get; private set; }

        /// <summary>
        /// City indices
        /// </summary>
        public int[] Path { get; }

        public int Depth => Path.Length;

        public int CurrentCity { get; }

        public bool NearComplete => Depth == _size;
        public bool IsComplete => Depth == _size + 1;

        private readonly int _size;

        public float Heuristic()
        {
            //return (float) LowerBound;
            // Prefer lower bounds and higher depths
            return (float) LowerBound - DepthScale * Depth;
        }
        
        private TspState(TspState fromState, int toCity)
        {
            _size = fromState._size;
            CostMatrix = (double[,]) fromState.CostMatrix.Clone();
            LowerBound = fromState.LowerBound;
            var fromCity = fromState.CurrentCity;

            Debug.Assert(toCity < _size && toCity >= 0);
            Debug.Assert(fromCity == -1 || !double.IsPositiveInfinity(CostMatrix[fromCity, toCity]), "Navigating to illegal city");
            Debug.Assert(fromState.NearComplete ? fromState.Path[0] == toCity : !fromState.Path.Contains(toCity), "Path contains illegal duplicate");
            Path = fromState.Path.AppendToCopy(toCity);
            CurrentCity = toCity;

            // The cost matrix does not change when starting from nothing
            if (fromCity == -1) return;

            // Cannot go from toCity to CurrentCity (backwards)
            CostMatrix[toCity, fromCity] = double.PositiveInfinity;
            // The lower bound is increased by the cost of the path
            LowerBound += CostMatrix[fromCity, toCity];

            // Nothing else can go to toCity (inf toCity column)
            for (var row = 0; row < _size; ++row)
                CostMatrix[row, toCity] = double.PositiveInfinity;

            // The current node now can't go to anything as it's no longer the head of the path (inf fromCity row)
            for (var col = 0; col < _size; ++col)
                CostMatrix[fromCity, col] = double.PositiveInfinity;

            // Reduce the new state
            Reduce();
        }

        public TspState(IReadOnlyList<City> cities)
        {
            _size = cities.Count;
            CostMatrix = new double[_size, _size];
            for (var fromCity = 0; fromCity < _size; ++fromCity)
            {
                for (var toCity = 0; toCity < _size; ++toCity)
                {
                    if (fromCity == toCity)
                        CostMatrix[fromCity, toCity] = double.PositiveInfinity;
                    else
                        CostMatrix[fromCity, toCity] = cities[fromCity].CostToGetTo(cities[toCity]);
                }
            }
            CurrentCity = -1;
            Path = new int[0];
            Reduce();
        }

        /// <summary>
        /// Visit a path in the matrix.
        /// If there is no current city, start the TSP there.
        /// </summary>
        /// <returns>A new state that followed that path</returns>
        public TspState Visit(int toCity)
        {
            return new TspState(this, toCity);
        }

        /// <summary>
        /// Decide whether we could visit a given city.
        /// Does not bounds check!
        /// </summary>
        /// <param name="toCity"></param>
        /// <returns></returns>
        public bool CanVisit(int toCity)
        {
            // Can't visit the current city
            if (CurrentCity == toCity)
                return false;
            // Basically can always visit if there's no defined city.
            if (CurrentCity == -1)
                return true;
            // If it costs infinity to get there, there's no available route
            if (double.IsPositiveInfinity(CostMatrix[CurrentCity, toCity]))
                return false;
            // Cannot visit the start node if it's not the only option
            if (!NearComplete && Path[0] == toCity)
                return false;
            return true;
        }

        /// <summary>
        /// Reduce a specific column, ensuring there is at least one 0.
        /// O(n).
        /// </summary>
        /// <param name="col"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReduceColumn(int col)
        {
            var min = double.PositiveInfinity;
            for (var row = 0; row < _size; ++row)
                min = Math.Min(min, CostMatrix[row, col]);
            Debug.Assert(min >= 0);
            if (min < double.Epsilon || double.IsPositiveInfinity(min))
                return;
            for (var row = 0; row < _size; ++row)
                CostMatrix[row, col] -= min;
            LowerBound += min;
        }

        /// <summary>
        /// Reduce a specific row, ensuring there is at least one 0.
        /// O(n).
        /// </summary>
        /// <param name="row"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReduceRow(int row)
        {
            var min = double.PositiveInfinity;
            for (var col = 0; col < _size; ++col)
                min = Math.Min(min, CostMatrix[row, col]);
            Debug.Assert(min >= 0);
            if (min < double.Epsilon || double.IsPositiveInfinity(min))
                return;
            for (var col = 0; col < _size; ++col)
                CostMatrix[row, col] -= min;
            LowerBound += min;
        }

        /// <summary>
        /// Reduce the entire cost matrix, ensuring there is at least one 0 in each row and column.
        /// O(n^2).
        /// </summary>
        private void Reduce()
        {
            for (var i = 0; i < _size; ++i)
            {
                ReduceColumn(i);
                ReduceRow(i);
            }
        }
    }
}