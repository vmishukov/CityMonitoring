using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SystAnalys_lr1.Classes
{
    public struct Skips
    {
        private bool skipTraffics;
        private int skipStops;
        private int skipEnd;
        private int skipTrafficLights;

        public bool SkipTraffics { get => skipTraffics; set => skipTraffics = value; }
        public int SkipStops { get => skipStops; set => skipStops = value; }
        public int SkipEnd { get => skipEnd; set => skipEnd = value; }
        public int SkipTrafficLights { get => skipTrafficLights; set => skipTrafficLights = value; }

        public Skips(bool skipTraffics, int skipStops, int skipEnd, int skipTrafficLights)
        {
            this.skipTraffics = skipTraffics;
            this.skipStops = skipStops;
            this.skipEnd = skipEnd;
            this.skipTrafficLights = skipTrafficLights;
        }
    }

    struct CheckStops
    {
        private int checkStop;
        private int checkStoppedBus;

        public int CheckStop { get => checkStop; set => checkStop = value; }
        public int CheckStoppedBus { get => checkStoppedBus; set => checkStoppedBus = value; }

        public CheckStops(int checkStop, int checkStoppedBus)
        {
            this.checkStop = checkStop;
            this.checkStoppedBus = checkStoppedBus;
        }
    }

    public class Bus : ICloneable
    {
        delegate void Dpoint(Point pos);

        public Skips Skips;

        public static string TrackerBusImg { get => s_tracker_busImg; set => s_tracker_busImg = value; }
        public static string TrackerOffBusImg { get => s_tracker_busImgoffBusImg; set => s_tracker_busImgoffBusImg = value; }


        public static string BusImg { get => s_busImg; set => s_busImg = value; }
        public static string OffBusImg { get => s_busImgoffBusImg; set => s_busImgoffBusImg = value; }


        public static int ScrollX { get => s_scrollX; set => s_scrollX = value; }
        public static int ScrollY { get => s_scrollY; set => s_scrollY = value; }

        private readonly Random rnd = new Random();

        public int PositionAt { get => _positionAt; set => _positionAt = value; }
        public bool TurnBack { get => _turnBack; set => _turnBack = value; }

        [XmlIgnore, JsonIgnore]
        public Image BusPic { get => _busPic; set => _busPic = value; }
        public string Route { get => _route; set => _route = value; }

        private int? Locate = null;
        public List<Point> Coordinates { get => _coordinates; set => _coordinates = value; }
        public List<int> GridCoordinates { get => _gridCoordinates; set => _gridCoordinates = value; }
        public int R { get => _r; set => _r = value; }
        //сколько автобусу нужно проехать в тиках
        public int TickCount_ { get => _tickCount_; set => _tickCount_ = value; }

        //за сколько времени автобус нашел эпицентр
        static public int? ZoomCoef { get => s_zoomCoef; set => s_zoomCoef = value; }

        
        //есть ли инфа об аварии 
        public bool tracker { get => _accident_check; set => _accident_check = value; }
        //может ли передавать инфу на датчик
        public bool accident_check { get => _tracker; set => _tracker = value; }
        private bool haveTracker;

        private int speed;
        private int changeSpeed;
        private CheckStops CheckStops;
        private bool stopOnBusStop = false;
        private static string s_tracker_busImg = "../../Resources/newbus.PNG";
        private static string s_tracker_busImgoffBusImg = "../../Resources/bus.PNG";
        private static string s_busImg = "../../Resources/bus_no_cheked.PNG";
        private static string s_busImgoffBusImg = "../../Resources/bus_no.PNG";
        private static int s_scrollX;
        private static int s_scrollY;
        private int _positionAt;
        private bool _turnBack;
        private bool _stopAtStationByGrid = false;
        private Image _busPic;
        private string _route;
        private List<Point> _coordinates;
        private List<int> _gridCoordinates;
        private int _r = 7;
        private int _tickCount_;


        private int SlowDown { get; set; }
        public bool HaveTracker { get => haveTracker; set => haveTracker = value; }


        private static int? s_zoomCoef = 1;
        private bool _tracker;
        private bool _accident_check;

        public object Clone()
        {
            return MemberwiseClone();
        }

        ~Bus()
        { }

        public Bus()
        { }

        public Bus(Image BusPic, int PositionAt, bool Turn, string Route, List<Point> Coordinates, bool tracker)
        {
            this.tracker = tracker;
            this.BusPic = BusPic;
            this.PositionAt = PositionAt;
            TurnBack = Turn;
            this.Route = Route;
            this.Coordinates = Coordinates;
            Skips = new Skips(false, 1, 5, 1);
            CheckStops = new CheckStops(0, 0);
            changeSpeed = rnd.Next(20, 50);
        }

     


        public void MoveWithoutGraphicsByGrids()
        {
           
                if (TurnBack == false)
                {
                    if (PositionAt < GridCoordinates.Count - 1)
                    {
                        PositionAt++;
                    }
                    else
                    {
                        TurnBack = true;
                        PositionAt--;
                    }
                }
                else
                {
                    if (PositionAt > 0)
                    {
                        PositionAt--;
                    }
                    else
                    {
                        TurnBack = false;
                        PositionAt++;
                    }
                }          
        }

        private void StopDown()
        {
            if (changeSpeed != 0)
                changeSpeed -= 1;

            if (SlowDown > 3)
            {
                changeSpeed = rnd.Next(BusStop.StopTime / 10, 70);
                speed = 1;
            }
            if (changeSpeed == 0)
            {
                changeSpeed = rnd.Next(BusStop.StopTime / 10, 70);
                speed = rnd.Next(1, 3);
            }
            if (Skips.SkipTrafficLights != 0)
                Skips.SkipTrafficLights -= 1;
            if (Skips.SkipStops != 0)
                Skips.SkipStops -= 1;
            if (Skips.SkipEnd != 0)
                Skips.SkipEnd -= 1;
            if (CheckStops.CheckStop != 0)
                CheckStops.CheckStop -= 1;
            if (CheckStops.CheckStoppedBus != 0)
                CheckStops.CheckStoppedBus -= 1;
        }

        public void TurnBackOn()
        {
            if (PositionAt - speed > 0)
                PositionAt -= speed;
            else
                PositionAt--;
        }

        public void TurnBackOff()
        {
            if (PositionAt + speed < Coordinates.Count - 1)
                PositionAt += speed;
            else
                PositionAt++;
        }

        public void CheckBus(Bus bus)
        {
            CheckStops.CheckStop = bus.CheckStops.CheckStop;
            Skips.SkipTrafficLights = BusStop.StopTime / 2;
            Skips.SkipStops = rnd.Next(BusStop.StopTime, BusStop.StopTime + 50);
        }

        public void CheckStop()
        {
            CheckStops.CheckStop = rnd.Next(0, BusStop.StopTime);
            Skips.SkipStops = BusStop.StopTime + 50 - CheckStops.CheckStop;
            CheckStops.CheckStoppedBus = CheckStops.CheckStop + 100;
            stopOnBusStop = true;
        }

        public void CheckTrafficLight()
        {
            foreach (var sp in Data.TraficLights)
            {
                if ((Math.Pow((double.Parse((sp.X * (int)ZoomCoef - Coordinates[PositionAt].X * (int)ZoomCoef).ToString())), 2) + Math.Pow((double.Parse(((sp.Y * (int)ZoomCoef - Coordinates[PositionAt].Y * (int)ZoomCoef)).ToString())), 2) <= Main.G.R * (int)ZoomCoef * Main.G.R * (int)ZoomCoef * (Main.G.R * (int)ZoomCoef)) && sp.Status != LightStatus.RED)
                {
                    Skips.SkipTrafficLights = BusStop.StopTime / 2;
                    CheckStops.CheckStoppedBus = BusStop.StopTime / 2;
                    break;
                }
                else
                if ((Math.Pow((double.Parse((sp.X * (int)ZoomCoef - Coordinates[PositionAt].X * (int)ZoomCoef).ToString())), 2) + Math.Pow((double.Parse(((sp.Y * (int)ZoomCoef - Coordinates[PositionAt].Y * (int)ZoomCoef)).ToString())), 2) <= Main.G.R * (int)ZoomCoef * Main.G.R * (int)ZoomCoef * (Main.G.R * (int)ZoomCoef)) && sp.Status == LightStatus.RED)
                {
                    if (sp.Bal == 0)
                    {
                        Skips.SkipTrafficLights = (sp.Bal + 2) * (BusStop.StopTime / 10 + 10) + (BusStop.StopTime / 10 * 4);
                        CheckStops.CheckStop = 80;
                    }
                    else
                    {
                        Skips.SkipTrafficLights = (sp.Bal + 2) * (BusStop.StopTime / 10 + 10) + (BusStop.StopTime / 10 * 4);
                        CheckStops.CheckStop = (sp.Bal + 2) * (BusStop.StopTime / 10 + 10);
                    }
                    CheckStops.CheckStoppedBus = BusStop.StopTime / 2;
                    break;

                }
            }
        }

        private void ProvideInfo(List<Bus> buses, Graphics G)
        {
            Data.Stations.ForEach((Action<Station>)(st =>
            {
                if (this.accident_check == true)
                {
                    if (Math.Pow((double.Parse((st.X * (int)ZoomCoef - Coordinates[PositionAt].X * (int)ZoomCoef).ToString())), 2) + Math.Pow((double.Parse(((st.Y * (int)ZoomCoef - Coordinates[PositionAt].Y * (int)ZoomCoef)).ToString())), 2) <= Main.G.R * (int)ZoomCoef * 700)
                    {
                        G.FillEllipse(Brushes.Green, (st.X - Main.zoom * 2), (st.Y - Main.zoom * 2), 4 * Main.zoom * 2, 4 * Main.zoom * 2);
                        G.FillEllipse(new SolidBrush(Color.FromArgb(72, 17, 187, 17)), (st.X - 30 * Main.zoom * 2), (st.Y - 30 * Main.zoom * 2), 60 * Main.zoom * 2, 60 * Main.zoom * 2);
                        return;
                    }
                }
            })
            );
        }

        public void MoveWithGraphics(Graphics G)
        {
            Bus thisBus = new Bus(BusPic, PositionAt, TurnBack, Route, Coordinates, accident_check);
            //Data.Buses;
            Data.Buses.Remove(thisBus);
            //ProvideInfo(buses, G);
            if (Coordinates.Any())
            {
                if (CheckStops.CheckStop == 0)
                {
                    if (TurnBack == false)
                    {
                        if (PositionAt < Coordinates.Count - 1)
                        {
                            if (Data.Buses.Count != 0)
                            {
                                foreach (var bus in Data.Buses)
                                {
                                    if ((PositionAt < Coordinates.Count) && (bus.PositionAt < bus.Coordinates.Count))
                                    {
                                        if (Math.Pow((bus.Coordinates[bus.PositionAt].X * (int)ZoomCoef - (Coordinates[PositionAt].X * (int)ZoomCoef)), 2) + Math.Pow((bus.Coordinates[bus.PositionAt].Y * (int)ZoomCoef - (Coordinates[PositionAt].Y * (int)ZoomCoef)), 2) <= bus.R * bus.R && bus.TurnBack == TurnBack && !bus.stopOnBusStop && bus.CheckStops.CheckStop != 0)
                                        {
                                            CheckBus(bus);
                                            break;
                                        }
                                        if (Math.Pow((bus.Coordinates[bus.PositionAt].X * (int)ZoomCoef - (Coordinates[PositionAt].X * (int)ZoomCoef)), 2) + Math.Pow((bus.Coordinates[bus.PositionAt].Y * (int)ZoomCoef - (Coordinates[PositionAt].Y * (int)ZoomCoef)), 2) <= bus.R * bus.R)
                                        {
                                            if (bus.TurnBack == TurnBack && bus.PositionAt > PositionAt)
                                            {
                                                SlowDown += 1;
                                                break;
                                            }
                                            else
                                            {
                                                if (SlowDown != 0)
                                                    SlowDown -= 1;
                                            }
                                        }
                                    }
                                }
                            }

                            if (Data.TraficLights.Count != 0)
                            {
                                if (Skips.SkipTrafficLights == 0)
                                {
                                    CheckTrafficLight();
                                }
                            }
                            G.DrawImage(BusPic, Coordinates[PositionAt].X * (int)ZoomCoef - BusPic.Width / 2, Coordinates[PositionAt].Y * (int)ZoomCoef - BusPic.Height / 2);
                            StopDown();
                            TurnBackOff();

                        }
                        else
                        {
                            StopDown();
                            Skips.SkipEnd = 0;// rnd.Next(0, BusStop.StopTime * 3);
                            if (PositionAt < Coordinates.Count)
                            {
                                G.DrawImage(BusPic, Coordinates[PositionAt].X * (int)ZoomCoef - BusPic.Width / 2, Coordinates[PositionAt].Y * (int)ZoomCoef - BusPic.Height / 2);
                            }
                            if (Skips.SkipEnd == 0)
                            {
                                TurnBack = true;
                                TurnBackOn();

                            }
                        }
                    }
                    else
                    {
                        if (PositionAt > 0)
                        {
                            if (PositionAt < Coordinates.Count)
                            {
                                if (CheckStops.CheckStoppedBus == 0)
                                {
                                    if (Data.Buses.Count != 0)
                                    {
                                        foreach (var bus in Data.Buses)
                                        {
                                            if ((PositionAt < Coordinates.Count) && (bus.PositionAt < bus.Coordinates.Count))
                                            {
                                                if (Math.Pow((bus.Coordinates[bus.PositionAt].X * (int)ZoomCoef - (Coordinates[PositionAt].X * (int)ZoomCoef)), 2) + Math.Pow((bus.Coordinates[bus.PositionAt].Y * (int)ZoomCoef - (Coordinates[PositionAt].Y * (int)ZoomCoef)), 2) <= bus.R * bus.R && bus.TurnBack == TurnBack && !bus.stopOnBusStop && bus.CheckStops.CheckStop != 0)
                                                {
                                                    CheckBus(bus);
                                                    break;
                                                }
                                                if (Math.Pow((bus.Coordinates[bus.PositionAt].X * (int)ZoomCoef - (Coordinates[PositionAt].X * (int)ZoomCoef)), 2) + Math.Pow((bus.Coordinates[bus.PositionAt].Y * (int)ZoomCoef - (Coordinates[PositionAt].Y * (int)ZoomCoef)), 2) <= bus.R * bus.R)
                                                {
                                                    if (bus.TurnBack == TurnBack && bus.PositionAt < PositionAt)
                                                    {
                                                        SlowDown += 1;
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        if (SlowDown != 0)
                                                            SlowDown -= 1;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                if (Data.TraficLights.Count != 0)
                                {
                                    if (Skips.SkipTrafficLights == 0)
                                    {
                                        CheckTrafficLight();
                                    }
                                }

                                G.DrawImage(BusPic, Coordinates[PositionAt].X * (int)ZoomCoef - BusPic.Width / 2, Coordinates[PositionAt].Y * (int)ZoomCoef - BusPic.Height / 2);
                                StopDown();
                                TurnBackOn();
                            }
                        }
                        else
                        {
                            StopDown();
                            Skips.SkipEnd = 0; // rnd.Next(0, BusStop.StopTime * 3);
                            G.DrawImage(BusPic, Coordinates[PositionAt].X * (int)ZoomCoef - BusPic.Width / 2, Coordinates[PositionAt].Y * (int)ZoomCoef - BusPic.Height / 2);
                            if (Skips.SkipEnd == 0)
                            {
                                TurnBack = false;
                                TurnBackOff();
                            }

                        }

                    }
                }
                else
                {
                    StopDown();
                    G.DrawImage(BusPic, Coordinates[PositionAt].X * (int)ZoomCoef - BusPic.Width / 2, Coordinates[PositionAt].Y * (int)ZoomCoef - BusPic.Height / 2);
                }
            }
            

        }

        public string GetRoute()
        {
            return Route;
        }


        public async Task AsyncDetectRectangle()
        {
            await Task.Run(() => DetectRectangle());
        }
        //определение квадрата по новому
        public void DetectRectangle()
        {

            for (int i = 0; i < Data.TheGrid.Count; i++)
            {
                if ((PositionAt < Coordinates.Count))

                    if (((Coordinates[PositionAt].X) > Data.TheGrid[i].X) && ((Coordinates[PositionAt].X) < Data.TheGrid[i].X + GridPart.Width) && ((Coordinates[PositionAt].Y) > Data.TheGrid[i].Y) && ((Coordinates[PositionAt].Y) < (Data.TheGrid[i].Y + GridPart.Height)))
                    {
                        Locate = i;
                    }
            }

        }

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
