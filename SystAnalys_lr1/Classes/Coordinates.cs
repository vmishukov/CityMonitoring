using MetroFramework.Forms;
using SystAnalys_lr1.Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystAnalys_lr1
{
    public class Coordinates
    {
        public double GetDistance(double x1, double y1, double x2, double y2)
        {
            return (int)Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }

        public void CreateOneRouteCoordinates(string i)
        {
            if (Data.Routes[i].Count >= 2)
            {
                Data.AllCoordinates[i] = new List<Point>();
                Data.AllGridsInRoutes[i] = new List<int>();
                Data.AllCoordinates[i].AddRange(GetPoints(Data.Routes[i], i));

            }
            Bus.ScrollX = Main.scrollX;
            Bus.ScrollY = Main.scrollY;

            try
            {
                foreach (var bus in Data.Buses)
                {
                    bus.Coordinates = Data.AllCoordinates[bus.GetRoute()];
                }
            }
            catch (Exception)
            {

                CreateAllCoordinates();
                foreach (var bus in Data.Buses)
                {
                    bus.Coordinates = Data.AllCoordinates[bus.GetRoute()];
                }
            }


        }
      
        private void Shuffle<T>( IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        Random rnd = new Random();
        public IEnumerable<int> AdjacentVertex(Vertex vertex)
        {
            foreach (var e in Data.E)
                if (e.V1.Equals(Data.V.IndexOf(vertex)))
                    yield return e.V2;
                else if (e.V2.Equals(Data.V.IndexOf(vertex)))
                    yield return e.V1;

        }
        
        private List<Vertex> GenerateRandomRoute()
        {
            List<Vertex> randVertexes = new List<Vertex>();
            randVertexes.Add(Data.V[rnd.Next(1, Data.V.Count - 1)]);
            var previous = randVertexes.Last();
            for (int i = 0; i < 20; i++)
            {

               // var vertlist = Data.V[AdjacentVertex(randVertexes.Last()).ToList().OrderBy(x => Guid.NewGuid()).FirstOrDefault()];
                var vertlist = AdjacentVertex(randVertexes.Last()).ToList();
                Shuffle(vertlist);
                foreach (var vert in vertlist)
                {
                    if (vert != Data.V.IndexOf(previous))
                    {
                        randVertexes.Add(Data.V[vert]);
                        break;
                    }
                }
                if (randVertexes.Count>1)
                {
                    previous = randVertexes[randVertexes.Count-2];
                }
                
            }
            return randVertexes;
        }
        public List<Point> CreateOneRouteRandomCoordinates()
        {
            List<Point> RandCoordinates = new List<Point>();

            if (Data.V.Count >= 2)
            {
                List<int> RandGridsInRoutes = new List<int>();
                List<Vertex> RandomRoute = GenerateRandomRoute();
                RandCoordinates.AddRange(GetPoints(RandomRoute, null));
            }
            Bus.ScrollX = Main.scrollX;
            Bus.ScrollY = Main.scrollY;

            return RandCoordinates;

        }
        public async void AsyncCreateAllCoordinates()
        {
            await Task.Run(() =>
            {
                CreateAllCoordinates();
            });
        }

        public List<Point> GetPoints(List<Vertex> routeVertexes, string route)
        {
            var points = new List<Point>();
            int RectCheckCount = 0;

            if (routeVertexes.Count > 1)
            {
                for (int i = 0; i < routeVertexes.Count - 1; i++)
                {
                    Point p1 = new Point(routeVertexes[i].X, routeVertexes[i].Y);
                    Point p2 = new Point(routeVertexes[i + 1].X, routeVertexes[i + 1].Y);
                    int ydiff = p2.Y - p1.Y, xdiff = p2.X - p1.X;
                    double slope = (double)(p2.Y - p1.Y) / (p2.X - p1.X);
                    double x, y;
                    int quantity = (int)GetDistance(p1.X, p1.Y, p2.X, p2.Y) * 2;
                    for (double j = 0; j < quantity; j++)
                    {
                        y = slope == 0 ? 0 : ydiff * (j / quantity);
                        x = slope == 0 ? xdiff * (j / quantity) : y / slope;
                        points.Add(new Point((int)Math.Round(x) + p1.X, (int)Math.Round(y) + p1.Y));
                        if (RectCheckCount == 10)
                        {
                            RectCheckCount = 0;
                            if (route != null)
                            {
                                GetOneRouteGrids(points, route);
                            }

                        }
                        else
                        {
                            RectCheckCount++;
                        }

                    }
                    points.Add(p2);
                    if (route != null)
                    {
                        GetOneRouteGrids(points, route);
                    }
                }
            }
            return points;
        }
        public void GetOneRouteGrids(List<Point> points, string route)
        {
            int Locate = 0;
            int LastLocate = 0;
            foreach (var gridpart in Data.TheGrid)
            {
                if ((points.Last().X > gridpart.X) && ((points.Last().X) < gridpart.X + GridPart.Width) && ((points.Last().Y) > gridpart.Y) && ((points.Last().Y) < (gridpart.Y + GridPart.Height)))
                {

                    Locate = Data.TheGrid.IndexOf(gridpart);
                }
            }
            for (int k = 0; k < Data.TheGrid.Count; k++)
            {
                if (Locate == k)
                {
                    if (LastLocate != Locate)
                    {
                        Data.AllGridsInRoutes[route].Add(k);

                        LastLocate = Locate;
                    }
                }
            }
        }


        //функция, которая создает все координаты для всех маршрутов
        public void CreateAllCoordinates()
        {
            Data.AllCoordinates = new SerializableDictionary<string, List<Point>>();
            Data.AllGridsInRoutes = new SerializableDictionary<string, List<int>>();
            for (int i = 0; i < Data.Routes.Count; i++)
            {
                Data.AllCoordinates.Add(Data.Routes.ElementAt(i).Key, new List<Point>());
                Data.AllGridsInRoutes.Add(Data.Routes.ElementAt(i).Key, new List<int>());
                if (Data.Routes.ElementAt(i).Value.Count >= 2)
                {
                    Data.AllCoordinates[Data.AllCoordinates.ElementAt(i).Key].AddRange(GetPoints(Data.Routes.ElementAt(i).Value, Data.Routes.ElementAt(i).Key));
                }
                Bus.ScrollX = Main.scrollX;
                Bus.ScrollY = Main.scrollY;

            }

            foreach (var bus in Data.Buses)
            {
                bus.Coordinates = Data.AllCoordinates[bus.GetRoute()];
            }


        }

    }
}
