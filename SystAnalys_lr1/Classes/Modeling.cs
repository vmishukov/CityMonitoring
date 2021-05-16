//This is a personal academic project. Dear PVS-Studio, please check it.
//PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
namespace SystAnalys_lr1.Classes
{
    static class Modeling
    {
        private static Random rnd = new Random();
        private static int s_t;
        private static List<int?> s_resultFromModeling = new List<int?>();
        public static int T { get => s_t; set => s_t = value; }
        public static List<int?> ResultFromModeling { get => s_resultFromModeling; set => s_resultFromModeling = value; }
        public static void StartModeling()

        {
            int AllTics = 0;
            ConcurrentQueue<Bus> cqBus = new ConcurrentQueue<Bus>();
            Data.Buses.ForEach((b) => cqBus.Enqueue((Bus)b.Clone()));
            int small = -1;
            bool found = false;
            foreach (var bus in cqBus)
            {
                bus.PositionAt = rnd.Next(1, bus.GridCoordinates.Count - 2);
                bus.accident_check = false;
            }
            foreach (var item in Data.Stations)
            {
                item.HaveInfo = false;
            }
            while (AllTics < T)
            {
                foreach (var bus in cqBus)
                {
                    bus.TickCount_ = 0;
                    if (bus.Skips.SkipTrafficLights > 0)
                        bus.Skips.SkipTrafficLights -= 1;

                    if (Data.CarAccidentsInGrids.Contains(bus.GridCoordinates[bus.PositionAt]))
                    {
                        bus.accident_check = true;
                    }
                    Data.SationsInGrids.ForEach((Action<int>)(st =>
                    {
                        if (bus.GridCoordinates[bus.PositionAt] == st && bus.accident_check)
                        {
                            Data.Stations[Data.SationsInGrids.IndexOf(st)].HaveInfo = true;
                        }
                    }));

                    foreach (var bus2 in cqBus)
                    {
                        if (bus.GridCoordinates[bus.PositionAt] == bus2.GridCoordinates[bus2.PositionAt])
                        {
                            if (!bus2.accident_check)
                            {
                                bus2.accident_check = true;
                            }
                        }
                    }

                    bus.MoveWithoutGraphicsByGrids();
                    //if (Data.TraficLightsInGrids.Contains(Data.AllGridsInRoutes[bus.GetRoute()][(int)bus.PositionAt]))
                    //{
                    //    if (bus.Skips.SkipTrafficLights == 0)
                    //    {
                    //        foreach (var sp in Data.TraficLights)
                    //        {
                    //            if (sp.Status != LightStatus.RED)
                    //            {
                    //                bus.Skips.SkipTrafficLights = sp.GreenTime;
                    //                break;
                    //            }
                    //            if (sp.Status == LightStatus.GREEN || sp.Status == LightStatus.YELLOW)
                    //            {
                    //                bus.TickCount_ += sp.Bal;

                    //                bus.Skips.SkipTrafficLights = sp.GreenTime;
                    //                break;
                    //            }
                    //        }
                    //    }
                    //}               

                }
                int yabidlo = 0;        
                foreach (var item in Data.Stations)
                {
                    if (item.HaveInfo == true)
                    {
                        yabidlo++;
                    }

                }
                if (yabidlo == Data.Stations.Count)
                {
                    if (!found)
                    {
                        found = true;
                        small = AllTics;
                    }
                }
                AllTics++;


            }

            if (small == -1)
                ResultFromModeling.Add(null);
            else
            {
                ResultFromModeling.Add(small * 20);
            }

        }
    }

}
