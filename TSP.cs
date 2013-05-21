using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TravelingSalesPerson {

    class Program {

        static void Main(string[] args) {

            var lines = File.ReadAllLines(@"..\..\Cities.txt");

            var cities = lines.Take(35).Select(l => new City {Name = l, Neighbours = new List<Road>()}).ToList();

            lines.Skip(37).Select((l, i) => new {line = l, index = i+1}).ToList()
                .ForEach(i => {
                    i.line.Split(' ').Select(l => l.Trim()).Where(l => l != "").Select((l, j) => new {line = l, index = j}).ToList()
                     .ForEach(j => {
                         cities[i.index].Neighbours.Add(new Road { Start = cities[i.index], Destination = cities[j.index], Distance = int.Parse(j.line) });
                         cities[j.index].Neighbours.Add(new Road { Start = cities[j.index], Destination = cities[i.index], Distance = int.Parse(j.line) });
                     });
                });

            var s = CreateInitialSolution(cities);

            var best = s;
            var t = 3000.0;
            const double c = 0.8;

            var n = 0;
            for (var i = 0; i < 200; i++ ) {
                for (var j = 0; j < 200; j++) {
                    var ss = CreateModifiedSolution(s);
                    //Console.WriteLine(best);
                    if (ss.Distance < s.Distance || Mv(s.Distance, ss.Distance, t)) {
                        s = ss;
                    }
                    if (s.Distance < best.Distance) {
                        best = s;
                        Console.WriteLine(best);
                    }
                }
                t *= c;
                n++;
            }

            Console.WriteLine(best);
            
            Console.Read();
        }

        private static bool Mv(int e0, int e1, double t) {
            var r = Math.Exp(-((double)e1 - e0)/t);
            return new Random().NextDouble() < r;
        }

        private static Solution CreateInitialSolution(List<City> cities) {

            var start = cities[0];

            var solution = new Solution();
            solution.Start = start;
            cities.Remove(start);

            var current = start;
            while (cities.Count > 0) {
                var next = current.Neighbours.Where(r => cities.Contains(r.Destination)).OrderBy(r => r.Distance).First();
                cities.Remove(next.Destination);
                solution.Itinerary.Add(next);
                current = next.Destination;
            }

            solution.Itinerary.Add(current.Neighbours.Single(r => r.Destination == start));

            return solution;
        }

        private static Solution CreateModifiedSolution(Solution solution) {
            var modified = new Solution {Start = solution.Start};

            var random = new Random();

            var index0 = random.Next(0, solution.Itinerary.Count);
            var index1 = index0;
            while (Math.Abs(index0 - index1) <= 2 || Math.Abs(index0 - index1) == solution.Itinerary.Count -1) {
                index1 = random.Next(0, solution.Itinerary.Count);
            }
            if (index0 > index1) {
                var tmp = index1;
                index1 = index0;
                index0 = tmp;
            }

            var swapper0 = solution.Itinerary[index0];
            var swapper1 = solution.Itinerary[index1];
            var swapped0 = swapper0.Start.Neighbours.Single(r => r.Destination == swapper1.Start);
            var swapped1 = swapper0.Destination.Neighbours.Single(r => r.Destination == swapper1.Destination);

            for (var i = 0; i < index0; i++) {
                var road = solution.Itinerary[i];
                modified.Itinerary.Add(road);
            }
            modified.Itinerary.Add(swapped0);
            for (var i = index1 - 1; i > index0; --i) {
                var road = solution.Itinerary[i];
                modified.Itinerary.Add(road.Destination.Neighbours.Single(x => x.Destination == road.Start));
            }
            modified.Itinerary.Add(swapped1);
            for (var i = index1+1; i < solution.Itinerary.Count; i++) {
                var road = solution.Itinerary[i];
                modified.Itinerary.Add(road);
            }

            return modified;
        }

        private class City {
            public string Name { get; set; }
            public List<Road> Neighbours { get; set; }

            public override string ToString() {
                return Name;
            }
        }

        private class Road {
            public City Start { get; set; }
            public City Destination { get; set; }
            public int Distance { get; set; }

            public override string ToString() {
                return Start + "=>" + Destination + " (" + Distance + ")";
            }
        }

        private class Solution {

            public Solution() {
                Itinerary = new List<Road>();
            }

            public List<Road> Itinerary { get; private set; }

            public City Start { get; set; }

            public int Distance {
                get {
                    return Itinerary.Sum(i => i.Distance);
                }
            }

            public override string ToString() {
                return Start.Name + "=>" + string.Join("=>", Itinerary.Select(c => c.Destination.Name)) + ": " + Distance;
            }

        }

    }

}
