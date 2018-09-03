using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusSolOnDB
{
    public class Transaction// Класс описывает все возможные переезды для исходных данных. Эксземпляр класса - значение одного конкретного оп времени и маршруту переезда
    {
        public List<int> PassedBuses { get; set; } // 2 неатомарынх атрибута, хранящие историю о переездах, предшевствующих данному.
        public List<int> PassedStations { get; set; }
        public string Way { get; set; } //Переезды 
        public int BusId { get; set; }
        public int StartStation { get; set; }
        public int EndStation { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public int Cost { get; set; }// Сумма проезда по данной транзакии на текущий момент
        public Transaction(int busId, int startSt, int startTime, int endSt, int time, int cost) : this()
        {
            BusId = busId;
            StartStation = startSt;
            EndStation = endSt;
            StartTime = startTime;
            EndTime = StartTime + time;
            Cost = cost;
            Way += startSt.ToString() + "(" + BusId.ToString() + ")";
        }
        public Transaction()
        {
            PassedBuses = new List<int>();
            PassedStations = new List<int>();
            Way = "";
        }

        public Transaction(Transaction oldTansact, int period) : this(oldTansact.BusId,oldTansact.StartStation,oldTansact.StartTime,oldTansact.EndStation,oldTansact.EndTime,oldTansact.Cost)
        {
            //BusId = oldTansact.BusId;
            //StartStation = oldTansact.StartStation;
            //EndStation = oldTansact.EndStation;
            StartTime = oldTansact.StartTime + period;
            EndTime = oldTansact.EndTime + period;
            //Cost = oldTansact.Cost;
            //Way += oldTansact.StartStation.ToString() + "(" + oldTansact.BusId.ToString() + ")";
        }
        public override string ToString()
        {
            string stations = "";
            string buses = "";
            foreach (var station in PassedStations)
            {
                stations += station.ToString() + " ";
            }
            foreach (var bus in PassedBuses)
            {
                buses += bus.ToString() + " ";
            }
            return "Way: " + Way
                + " Stations: " + stations
                + " Current Bus:  " + BusId
                + " Starts: " + StartStation
                + " Ends: " + EndStation
                + " Starts at: " + Constans.GetTimeFromNimutes(StartTime)
                + " Ends at: " + Constans.GetTimeFromNimutes(EndTime)
                + " Cost: " + Cost;
        }
    }
    public class Bus// Класс для хранения исходных данных об автобусах
    {
        public int Id { get; set; }
        public int Cost { get; set; }
        public int StartTime { get; set; } //Время в минутах от 0 до 1440  
        public int Period { get; set; } // Период цикла
        public Bus(int id, int cost, int start)
        {
            Id = id;
            Cost = cost;
            StartTime = start;
        }
        public override string ToString()
        {
            return "Bus Num: " + Id
                + " Cost: " + Cost
                + " Starts at: " + Constans.GetTimeFromNimutes(StartTime)
                + " Period: " + Period;
        }
    }
    public class Move // Классд для хранения исходных данных о переездах
    {
        public int BusId { get; set; }
        public int StationFrom { get; set; }
        public int StationTo { get; set; }
        public int Time { get; set; }
        public Move(int busId, int start, int end, int time)
        {
            BusId = busId;
            StationFrom = start;
            StationTo = end;
            Time = time;
        }

        public override string ToString()
        {
            return "Bus " + BusId 
                + " Start from: " + StationFrom 
                + " Moves to: " + StationTo 
                + " Time: " + Time;
        }
    }
}
