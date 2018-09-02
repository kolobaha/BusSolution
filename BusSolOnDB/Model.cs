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
        public List<int> Buses { get; set; } //Переезды 
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
            PassedBuses.Add(busId);
            PassedStations.Add(startSt);
        }
        public Transaction()
        {
            PassedBuses = new List<int>();
            PassedStations = new List<int>();
        }

        public Transaction(Transaction oldTansact, int period) : this()
        {
            BusId = oldTansact.BusId;
            StartStation = oldTansact.StartStation;
            EndStation = oldTansact.EndStation;
            StartTime = oldTansact.StartTime + period;
            EndTime = oldTansact.EndTime + period;
            Cost = oldTansact.Cost;
            //PassedBuses.Add(oldTansact.BusId);
            //PassedStations.Add(oldTansact.StartStation);
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
            return "Buses: " + buses + " | " + "Stations: " + stations + " | " + BusId + " | " + StartStation + " | " + EndStation + " | " + StartTime + " | " + EndTime + " | " + Cost + " | ";
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
            return Id + " | " + Cost + " | " + StartTime + " | " + Period;
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
            return BusId + " | " + StationFrom + " | " + StationTo + " | " + Time;
        }
    }
}
