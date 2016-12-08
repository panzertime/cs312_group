using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using Priority_Queue;

namespace TSP
{
    internal class ProblemAndSolver
    {
        #region Private Methods

        /// <summary>
        ///     Reset the problem instance.
        /// </summary>
        private void ResetData()
        {
            _cities = new City[Size];
            _route = new List<City>(Size);
            _bssf = null;

            if (_mode == HardMode.Modes.Easy)
                for (var i = 0; i < Size; i++)
                    _cities[i] = new City(_rnd.NextDouble(), _rnd.NextDouble());
            else // Medium and hard
                for (var i = 0; i < Size; i++)
                    _cities[i] = new City(_rnd.NextDouble(), _rnd.NextDouble(), _rnd.NextDouble() * City.MaxElevation);

            var mm = new HardMode(_mode, _rnd, _cities);
            if (_mode == HardMode.Modes.Hard)
            {
                var edgesToRemove = (int)(Size * FractionOfPathsToRemove);
                mm.RemovePaths(edgesToRemove);
            }
            City.SetModeManager(mm);

            _cityBrushStyle = new SolidBrush(Color.Black);
            _cityBrushStartStyle = new SolidBrush(Color.Red);
            _routePenStyle = new Pen(Color.Blue, 1);
            _routePenStyle.DashStyle = DashStyle.Solid;
        }

        #endregion

        private class TspSolution
        {
            /// <summary>
            ///     we use the representation [cityB,cityA,cityC]
            ///     to mean that cityB is the first city in the solution, cityA is the second, cityC is the third
            ///     and the edge from cityC to cityB is the final edge in the path.
            ///     You are, of course, free to use a different representation if it would be more convenient or efficient
            ///     for your data structure(s) and search algorithm.
            /// </summary>
            public readonly List<City> Route;

            /// <summary>
            ///     constructor
            /// </summary>
            /// <param name="iroute">a (hopefully) valid tour</param>
            public TspSolution(IEnumerable<City> iroute)
            {
                Route = new List<City>(iroute);
            }

            /// <summary>
            ///     Compute the cost of the current route.
            ///     Note: This does not check that the route is complete.
            ///     It assumes that the route passes from the last city back to the first city.
            /// </summary>
            /// <returns></returns>
            public double CostOfRoute()
            {
                // go through each edge in the route and add up the cost. 
                int x;
                City here;
                var cost = 0D;

                for (x = 0; x < Route.Count - 1; x++)
                {
                    here = Route[x];
                    cost += here.CostToGetTo(Route[x + 1]);
                }

                // go from the last city to the first. 
                here = Route[Route.Count - 1];
                cost += here.CostToGetTo(Route[0]);
                return cost;
            }
        }

        #region Private members 

        /// <summary>
        ///     Default number of cities (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Problem Size text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int DefaultSize = 25;

        /// <summary>
        ///     Default time limit (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Time text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int TimeLimit = 60; //in seconds

        private const int CityIconSize = 5;


        // For normal and hard modes:
        // hard mode only
        private const double FractionOfPathsToRemove = 0.20;

        /// <summary>
        ///     the cities in the current problem.
        /// </summary>
        private City[] _cities;

        /// <summary>
        ///     a route through the current problem, useful as a temporary variable.
        /// </summary>
        private List<City> _route;

        /// <summary>
        ///     best solution so far.
        /// </summary>
        private TspSolution _bssf;

        /// <summary>
        ///     how to color various things.
        /// </summary>
        private Brush _cityBrushStartStyle;

        private Brush _cityBrushStyle;
        private Pen _routePenStyle;


        /// <summary>
        ///     Difficulty level
        /// </summary>
        private HardMode.Modes _mode;

        /// <summary>
        ///     random number generator.
        /// </summary>
        private readonly Random _rnd;

        /// <summary>
        ///     time limit in milliseconds for state space search
        ///     can be used by any solver method to truncate the search and return the BSSF
        /// </summary>
        private int _timeLimit;

        #endregion

        #region Public members

        /// <summary>
        ///     These three constants are used for convenience/clarity in populating and accessing the results array that is passed
        ///     back to the calling Form
        /// </summary>
        public const int Cost = 0;

        public const int Time = 1;
        public const int Count = 2;

        public int Size { get; private set; }

        public int Seed { get; }

        #endregion

        #region Constructors

        public ProblemAndSolver()
        {
            Seed = 1;
            _rnd = new Random(1);
            Size = DefaultSize;
            _timeLimit = TimeLimit * 1000; // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            ResetData();
        }

        public ProblemAndSolver(int seed)
        {
            Seed = seed;
            _rnd = new Random(seed);
            Size = DefaultSize;
            _timeLimit = TimeLimit * 1000; // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            ResetData();
        }

        public ProblemAndSolver(int seed, int size)
        {
            Seed = seed;
            Size = size;
            _rnd = new Random(seed);
            _timeLimit = TimeLimit * 1000; // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            ResetData();
        }

        public ProblemAndSolver(int seed, int size, int time)
        {
            Seed = seed;
            Size = size;
            _rnd = new Random(seed);
            _timeLimit = time * 1000; // time is entered in the GUI in seconds, but timer wants it in milliseconds

            ResetData();
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode)
        {
            Size = size;
            _mode = mode;
            ResetData();
        }

        /// <summary>
        ///     make a new problem with the given size, now including timelimit paremeter that was added to form.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode, int timelimit)
        {
            Size = size;
            _mode = mode;
            _timeLimit = timelimit * 1000; //convert seconds to milliseconds
            ResetData();
        }

        /// <summary>
        ///     return a copy of the cities in this problem.
        /// </summary>
        /// <returns>array of cities</returns>
        public City[] GetCities()
        {
            var retCities = new City[_cities.Length];
            Array.Copy(_cities, retCities, _cities.Length);
            return retCities;
        }

        /// <summary>
        ///     draw the cities in the problem.  if the bssf member is defined, then
        ///     draw that too.
        /// </summary>
        /// <param name="g">where to draw the stuff</param>
        public void Draw(Graphics g)
        {
            var width = g.VisibleClipBounds.Width - 45F;
            var height = g.VisibleClipBounds.Height - 45F;
            var labelFont = new Font("Arial", 10);

            // Draw lines
            if (_bssf != null)
            {
                // make a list of points. 
                var ps = new Point[_bssf.Route.Count];
                var index = 0;
                foreach (var c in _bssf.Route)
                {
                    if (index < _bssf.Route.Count - 1)
                        g.DrawString(" " + index + "(" + c.CostToGetTo(_bssf.Route[index + 1]) + ")", labelFont,
                            _cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    else
                        g.DrawString(" " + index + "(" + c.CostToGetTo(_bssf.Route[0]) + ")", labelFont,
                            _cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    ps[index++] = new Point((int)(c.X * width) + CityIconSize / 2, (int)(c.Y * height) + CityIconSize / 2);
                }

                if (ps.Length > 0)
                {
                    g.DrawLines(_routePenStyle, ps);
                    g.FillEllipse(_cityBrushStartStyle, (float)_cities[0].X * width - 1, (float)_cities[0].Y * height - 1,
                        CityIconSize + 2, CityIconSize + 2);
                }

                // draw the last line. 
                g.DrawLine(_routePenStyle, ps[0], ps[ps.Length - 1]);
            }

            // Draw city dots
            foreach (var c in _cities)
                g.FillEllipse(_cityBrushStyle, (float)c.X * width, (float)c.Y * height, CityIconSize, CityIconSize);
        }

        /// <summary>
        ///     return the cost of the best solution so far.
        /// </summary>
        /// <returns></returns>
        public double CostOfBssf()
        {
            if (_bssf != null)
                return _bssf.CostOfRoute();
            return -1D;
        }

        /// <summary>
        ///     This is the entry point for the default solver
        ///     which just finds a valid random tour
        /// </summary>
        /// <returns>
        ///     results array for GUI that contains three ints: cost of solution, time spent to find solution, number of
        ///     solutions found during search (not counting initial BSSF estimate)
        /// </returns>
        public string[] DefaultSolveProblem()
        {
            var count = 0;
            var results = new string[3];
            var perm = new int[_cities.Length];
            _route = new List<City>();
            var rnd = new Random();
            var timer = new Stopwatch();

            timer.Start();

            do
            {
                int i;
                for (i = 0; i < perm.Length; i++) // create a random permutation template
                    perm[i] = i;
                for (i = 0; i < perm.Length; i++)
                {
                    var swap = i;
                    while (swap == i)
                        swap = rnd.Next(0, _cities.Length);
                    var temp = perm[i];
                    perm[i] = perm[swap];
                    perm[swap] = temp;
                }
                _route.Clear();
                for (i = 0; i < _cities.Length; i++) // Now build the route using the random permutation 
                    _route.Add(_cities[perm[i]]);
                _bssf = new TspSolution(_route);
                count++;
            } while (double.IsPositiveInfinity(CostOfBssf())); // until a valid route is found
            timer.Stop();

            results[Cost] = CostOfBssf().ToString(CultureInfo.CurrentCulture); // load results array
            results[Time] = timer.Elapsed.ToString();
            results[Count] = count.ToString();

            return results;
        }

        private TspState GreedyCalc(TspState state)
        {
            var n = state.CostMatrix.GetLength(0);
            while (!state.IsComplete)
            {
                TspState cheapestBranch = null;
                var cheapestBranchValue = double.PositiveInfinity;
                for (var toCity = 0; toCity < n; ++toCity)
                {
                    if (!state.CanVisit(toCity))
                        continue;
                    var testBranch = state.Visit(toCity);
                    if (testBranch.LowerBound >= cheapestBranchValue)
                        continue;
                    cheapestBranch = testBranch;
                    cheapestBranchValue = testBranch.LowerBound;
                }
                if (cheapestBranch == null)
                    return null;
                state = cheapestBranch;
            }
            return state;
        }

        private Tuple<double, int> CheapestNextCity(double[,] costMatrix, int fromCity, HashSet<int> toCities)
        {
            var nextCity = -1;
            var nextCost = double.PositiveInfinity;
            foreach (var toCity in toCities)
            {
                var thisCost = costMatrix[fromCity, toCity];
                if (thisCost >= nextCost) continue;
                nextCity = toCity;
                nextCost = thisCost;
            }
            return new Tuple<double, int>(nextCost, nextCity);
        }

        private Tuple<double, List<int>> FastGreedyCalc(int startCity, double[,] costMatrix)
        {
            var n = costMatrix.GetLength(0);
            var toCities = new HashSet<int>(Enumerable.Range(0, n));
            var path = new List<int> { startCity };
            var start = path[0];
            toCities.Remove(startCity);

            var head = path[path.Count - 1];
            var totalLength = 0.0;
            while (toCities.Count > 0)
            {
                var data = CheapestNextCity(costMatrix, head, toCities);
                head = data.Item2;
                totalLength += data.Item1;
                path.Add(head);
                toCities.Remove(head);
            }
            path.Add(start);
            return new Tuple<double, List<int>>(totalLength, path);
        }


        private TspSolution StateToSolution(TspState state)
        {
            return new TspSolution(state.Path.Select(cityIndex => _cities[cityIndex]));
        }

        /// <summary>
        ///     performs a Branch and Bound search of the state space of partial tours
        ///     stops when time limit expires and uses BSSF as solution
        /// </summary>
        /// <returns>
        ///     results array for GUI that contains three ints: cost of solution, time spent to find solution, number of
        ///     solutions found during search (not counting initial BSSF estimate)
        /// </returns>
        public string[] BranchBoundSolveProblem()
        {
            var results = new string[3];

            var timer = new Stopwatch();

            Debug.Assert(_cities.Length > 0);
            var baseState = new TspState(_cities);

            timer.Start();

            var n = _cities.Length;
            var queue = new FastPriorityQueue<TspState>(n * n * n);
            baseState = baseState.Visit(0);
            queue.Enqueue(baseState, baseState.Heuristic());
            // The current best complete route
            var bestState = GreedyCalc(baseState);
            var upperBound = bestState?.LowerBound ?? double.PositiveInfinity;
            var stored = 0;
            var pruned = 0;
            var maxStates = 0;
            var updates = 0;
            while (queue.Count > 0)
            {
                if (queue.Count > maxStates)
                    maxStates = queue.Count;
                // Get best state to check based on heuristic.
                // Runs in O(log n) time.
                var state = queue.Dequeue();
                // Branch
                for (var toCity = 0; toCity < n; ++toCity)
                {
                    // If we can't visit the city, no need to consider it
                    if (!state.CanVisit(toCity))
                        continue;
                    var branchState = state.Visit(toCity);
                    // Bound
                    if (branchState.LowerBound < upperBound)
                    {
                        if (branchState.IsComplete)
                        {
                            ++updates;
                            // On a complete instance, no need to add it to the queue.
                            Debug.Assert(branchState.CostMatrix.Cast<double>().All(double.IsPositiveInfinity),
                                "Cost Matrix is not all infinity");
                            upperBound = branchState.LowerBound;
                            bestState = branchState;
                            continue;
                        }
                        ++stored;
                        if (queue.Count + 5 >= queue.MaxSize)
                            queue.Resize(queue.MaxSize * 2);
                        // Runs in O(log n) time
                        queue.Enqueue(branchState, branchState.Heuristic());
                    }
                    else
                        ++pruned;
                }
                // Abandon ship and give the best result otherwise
                if (timer.ElapsedMilliseconds > _timeLimit)
                    break;
            }
            timer.Stop();

            Debug.Assert(bestState != null && bestState.IsComplete);
            _bssf = StateToSolution(bestState);

            results[Cost] = CostOfBssf().ToString(CultureInfo.InvariantCulture); // load results array
            results[Time] = timer.Elapsed.ToString();
            results[Count] = $"{maxStates}/{updates}/{stored}/{pruned}";

            return results;
        }

        //if (_mode == HardMode.Modes.Hard)
        //{
        //    var upperBound = double.PositiveInfinity;
        //    for (var startCity = 0; startCity < _cities.Length; ++startCity)
        //    {
        //        var testState = BranchBoundCalc(baseState, startCity);
        //        if (testState == null || testState.LowerBound >= upperBound) continue;
        //        upperBound = testState.LowerBound;
        //        bestState = testState;
        //    }
        //}
        //else

        /////////////////////////////////////////////////////////////////////////////////////////////
        // These additional solver methods will be implemented as part of the group project.
        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///     finds the greedy tour starting from each city and keeps the best (valid) one
        /// </summary>
        /// <returns>
        ///     results array for GUI that contains three ints: cost of solution, time spent to find solution, number of
        ///     solutions found during search (not counting initial BSSF estimate)
        /// </returns>
        public string[] GreedySolveProblem()
        {
            var results = new string[3];
            var timer = new Stopwatch();

            Debug.Assert(_cities.Length > 0);
            timer.Start();
            var costMatrix = new TspState(_cities).CostMatrix;
            if (_mode == HardMode.Modes.Hard)
            {
                // CostOfBssf() is still used because new TspState reduces the matrix once
                var cost = double.PositiveInfinity;
                for (var startCity = 0; startCity < _cities.Length; ++startCity)
                {
                    var thisResult = FastGreedyCalc(startCity, costMatrix);
                    if (thisResult.Item1 >= cost) continue;
                    cost = thisResult.Item1;
                    _bssf = new TspSolution(thisResult.Item2.Select(i => _cities[i]));
                }
            }
            else
            {
                var cityIndexes = FastGreedyCalc(0, costMatrix);
                _bssf = new TspSolution(cityIndexes.Item2.Select(i => _cities[i]));
            }
            timer.Stop();

            results[Cost] = CostOfBssf().ToString(CultureInfo.InvariantCulture);
            results[Time] = timer.Elapsed.ToString();
            results[Count] = (_cities.Length + 1).ToString();

            return results;
        }

        public string[] TwoOptSolveProblem()
        {
            var results = new string[3];

            // get initial bssf using default algorithm
            int i, swap, temp, count = 0;
            int[] perm = new int[_cities.Length];
            _route = new List<City>();
            Random rnd = new Random();

            do
            {
                for (i = 0; i < perm.Length; i++)                                 // create a random permutation template
                    perm[i] = i;
                for (i = 0; i < perm.Length; i++)
                {
                    swap = i;
                    while (swap == i)
                        swap = rnd.Next(0, _cities.Length);
                    temp = perm[i];
                    perm[i] = perm[swap];
                    perm[swap] = temp;
                }
                _route.Clear();
                for (i = 0; i < _cities.Length; i++)                            // Now build the route using the random permutation 
                {
                    _route.Add(_cities[perm[i]]);
                }
                _bssf = new TspSolution(_route);
            } while (CostOfBssf() == double.PositiveInfinity);                // until a valid route is found



            Stopwatch timer = new Stopwatch();
            timer.Start();
            count = 0;
            TspSolution prevSolution = _bssf;

            bool found = false;
            do
            {
                double bestDist = _bssf.CostOfRoute();
                for (i = 0; i < _bssf.Route.Count; i++)
                {
                    found = false;
                    for (int k = i + 1; k < _bssf.Route.Count; k++)
                    {
                        TspSolution newRoute = twoOptSwap(_bssf, i, k);
                        double newDist = newRoute.CostOfRoute();
                        if (isCompleteSolution(newRoute) && newDist < bestDist)
                        {
                            prevSolution = _bssf;
                            _bssf = newRoute;
                            found = true;
                            count++;
                            break;
                        }
                    }
                    if (found)
                        break;
                }
            } while (found);


            timer.Stop();

            results[Cost] = _bssf.CostOfRoute().ToString();
            results[Time] = timer.Elapsed.ToString();
            results[Count] = count.ToString();

            return results;
        }

        //O(n^2)
        private bool isCompleteSolution(TspSolution tspSolution)
        {
            if (tspSolution.Route.Count != _cities.Length)
                return false;
            //O(n^2)
            for (int i = 0; i < _cities.Length; i++)
            {
                //O(n)
                if (!tspSolution.Route.Contains(_cities[i]))
                    return false;
                if (i + 1 < _cities.Length && tspSolution.Route[i].CostToGetTo(tspSolution.Route[i + 1]) == double.PositiveInfinity)
                    return false;
            }
            return true;
        }

        /* 2-Opt solution is based on psuedo code found at https://en.wikipedia.org/wiki/2-opt */
        private TspSolution twoOptSwap(TspSolution curRoute, int i, int k)
        {
            List<City> newRoute = new List<City>();
            // 1.take route[1] to route[i - 1] and add them in order to new_route
            for (int p = 0; p < i; p++)
            {
                newRoute.Add(curRoute.Route[p]);
            }
            // 2.take route[i] to route[k] and add them in reverse order to new_route
            for (int p = k; p >= i; p--)
            {
                newRoute.Add(curRoute.Route[p]);
            }
            // 3.take route[k + 1] to end and add them in order to new_route
            for (int p = k + 1; p < curRoute.Route.Count; p++)
            {
                newRoute.Add(curRoute.Route[p]);
            }
            return new TspSolution(newRoute);
        }

        public string[] ThreeOptSolveProblem()
        {
            var results = new string[3];

            // get initial bssf using default algorithm
            int i, swap, temp, count = 0;
            int[] perm = new int[_cities.Length];
            _route = new List<City>();
            Random rnd = new Random();

            do
            {
                for (i = 0; i < perm.Length; i++)                                 // create a random permutation template
                    perm[i] = i;
                for (i = 0; i < perm.Length; i++)
                {
                    swap = i;
                    while (swap == i)
                        swap = rnd.Next(0, _cities.Length);
                    temp = perm[i];
                    perm[i] = perm[swap];
                    perm[swap] = temp;
                }
                _route.Clear();
                for (i = 0; i < _cities.Length; i++)                            // Now build the route using the random permutation 
                {
                    _route.Add(_cities[perm[i]]);
                }
                _bssf = new TspSolution(_route);
            } while (CostOfBssf() == double.PositiveInfinity);                // until a valid route is found


            Stopwatch timer = new Stopwatch();
            timer.Start();
            count = 0;
            TspSolution prevSolution = _bssf;

            bool found = false;
            do
            {
                double bestDist = _bssf.CostOfRoute();
                found = false;
                for (i = 0; i < _bssf.Route.Count - 2; i++)
                {
                    for (int j = i + 1; j < _bssf.Route.Count - 1; j++)
                    {
                        for (int k = j + 1; k < _bssf.Route.Count; k++)
                        {
                            TspSolution newRoute1 = threeOptSwapA(_bssf, i, j, k);
                            TspSolution newRoute2 = threeOptSwapB(_bssf, i, j, k);
                            TspSolution newRoute3 = threeOptSwapC(_bssf, i, j, k);
                            TspSolution newRoute4 = twoOptSwap(_bssf, i, j);
                            TspSolution newRoute5 = twoOptSwap(_bssf, j, k);
                            TspSolution newRoute6 = twoOptSwap(_bssf, i, k);

                            double newDist1 = newRoute1.CostOfRoute();
                            double newDist2 = newRoute2.CostOfRoute();
                            double newDist3 = newRoute3.CostOfRoute();
                            double newDist4 = newRoute4.CostOfRoute();
                            double newDist5 = newRoute5.CostOfRoute();
                            double newDist6 = newRoute6.CostOfRoute();


                            if (isCompleteSolution(newRoute1) && newDist1 < bestDist && newDist1 <= newDist2 && newDist1 <= newDist3 && newDist1 <= newDist4 && newDist1 <= newDist5 && newDist1 <= newDist6)
                            {
                                prevSolution = _bssf;
                                _bssf = newRoute1;
                                found = true;
                                count++;
                                break;
                            }
                            if (isCompleteSolution(newRoute2) && newDist2 < bestDist && newDist2 <= newDist1 && newDist2 <= newDist3 && newDist2 <= newDist4 && newDist2 <= newDist5 && newDist2 <= newDist6)
                            {
                                prevSolution = _bssf;
                                _bssf = newRoute2;
                                found = true;
                                count++;
                                break;
                            }
                            if (isCompleteSolution(newRoute3) && newDist3 < bestDist && newDist3 <= newDist2 && newDist3 <= newDist1 && newDist3 <= newDist4 && newDist3 <= newDist5 && newDist3 <= newDist6)
                            {
                                prevSolution = _bssf;
                                _bssf = newRoute3;
                                found = true;
                                count++;
                                break;
                            }
                            if (isCompleteSolution(newRoute4) && newDist4 < bestDist && newDist4 <= newDist2 && newDist4 <= newDist1 && newDist4 <= newDist3 && newDist4 <= newDist5 && newDist4 <= newDist6)
                            {
                                prevSolution = _bssf;
                                _bssf = newRoute4;
                                found = true;
                                count++;
                                break;
                            }
                            if (isCompleteSolution(newRoute5) && newDist5 < bestDist && newDist5 <= newDist2 && newDist5 <= newDist1 && newDist5 <= newDist4 && newDist5 <= newDist3 && newDist5 <= newDist6)
                            {
                                prevSolution = _bssf;
                                _bssf = newRoute5;
                                found = true;
                                count++;
                                break;
                            }
                            if (isCompleteSolution(newRoute6) && newDist6 < bestDist && newDist6 <= newDist2 && newDist6 <= newDist1 && newDist6 <= newDist4 && newDist6 <= newDist5 && newDist6 <= newDist3)
                            {
                                prevSolution = _bssf;
                                _bssf = newRoute6;
                                found = true;
                                count++;
                                break;
                            }
                        }
                        if (found)
                            break;
                    }
                    if (found)
                        break;
                }
            } while (found);


            timer.Stop();

            results[Cost] = _bssf.CostOfRoute().ToString();
            results[Time] = timer.Elapsed.ToString();
            results[Count] = count.ToString();

            return results;
        }

        /* 2-Opt solution is based on psuedo code found at https://en.wikipedia.org/wiki/2-opt */
        private TspSolution threeOptSwapA(TspSolution curRoute, int i, int j, int k)
        {
            List<City> newRoute = new List<City>();

            curRoute = new TspSolution(pre_shift(curRoute.Route, i));

            // 2.take route[i] to route[k] and add them in reverse order to new_route
            for (int p = j; p >= 0; p--)
            {
                newRoute.Add(curRoute.Route[p]);
            }
            // 3.take route[k + 1] to end and add them in order to new_route
            for (int p = j + 1; p < k; p++)
            {
                newRoute.Add(curRoute.Route[p]);
            }
            for (int p = curRoute.Route.Count - 1; p >= k; p--)
            {
                newRoute.Add(curRoute.Route[p]);
            }
            return new TspSolution(post_shift(newRoute, i));
        }

        /* 2-Opt solution is based on psuedo code found at https://en.wikipedia.org/wiki/2-opt */
        private TspSolution threeOptSwapB(TspSolution curRoute, int i, int j, int k)
        {
            List<City> newRoute = new List<City>();

            curRoute = new TspSolution(pre_shift(curRoute.Route, i));

            // 2.take route[i] to route[k] and add them in reverse order to new_route
            for (int p = j; p >= 0; p--)
            {
                newRoute.Add(curRoute.Route[p]);
            }
            for (int p = k - 1; p > j; p--)
            {
                newRoute.Add(curRoute.Route[p]);
            }
            for (int p = k; p < curRoute.Route.Count; p++)
            {
                newRoute.Add(curRoute.Route[p]);
            }
            return new TspSolution(post_shift(newRoute, i));
        }

        /* 2-Opt solution is based on psuedo code found at https://en.wikipedia.org/wiki/2-opt */
        private TspSolution threeOptSwapC(TspSolution curRoute, int i, int j, int k)
        {
            List<City> newRoute = new List<City>();

            curRoute = new TspSolution(pre_shift(curRoute.Route, i));

            // 1.take route[1] to route[i - 1] and add them in order to new_route
            for (int p = 0; p <= j; p++)
            {
                newRoute.Add(curRoute.Route[p]);
            }
            // 2.take route[i] to route[k] and add them in reverse order to new_route
            for (int p = k - 1; p > j; p--)
            {
                newRoute.Add(curRoute.Route[p]);
            }
            for (int p = curRoute.Route.Count - 1; p >= k; p--)
            {
                newRoute.Add(curRoute.Route[p]);
            }
            return new TspSolution(post_shift(newRoute, i));
        }

        private List<City> pre_shift(List<City> x, int i)
        {
            List<City> newRoute = new List<City>();

            for (int p = i; p < x.Count; p++)
            {
                newRoute.Add(x[p]);
            }
            for (int p = 0; p < i; p++)
            {
                newRoute.Add(x[p]);
            }

            return newRoute;
        }

        private List<City> post_shift(List<City> x, int i)
        {
            List<City> newRoute = new List<City>();

            for (int p = x.Count - i; p < x.Count; p++)
            {
                newRoute.Add(x[p]);
            }
            for (int p = i; p < x.Count - i; p++)
            {
                newRoute.Add(x[p]);
            }

            return newRoute;
        }
        #endregion
    }
}
