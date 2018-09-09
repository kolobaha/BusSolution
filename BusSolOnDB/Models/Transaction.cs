using System.Collections.Generic;

namespace BusSolOnDB.Models
{
    // Класс описывает все возможные переезды для исходных данных. 
    // Эксземпляр класса - значение одного конкретного оп времени и маршруту переезда.
    public class Transaction
    {
        // 2 неатомарынх атрибута, хранящие историю о переездах, предшевствующих данному.
        public List<int> PassedBuses { get; set; }
        public List<int> PassedStations { get; set; }
        // Переезды.
        public string Way { get; set; } 
        public int BusId { get; set; }
        public int StartStation { get; set; }
        public int EndStation { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        // Сумма проезда по данной транзакии на текущий момент.
        public int Cost { get; set; }

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

        public Transaction(Transaction oldTansact, int period) 
            : this(oldTansact.BusId, oldTansact.StartStation, 
                  oldTansact.StartTime, oldTansact.EndStation, 
                  oldTansact.EndTime, oldTansact.Cost)
        {
            StartTime = oldTansact.StartTime + period;
            EndTime = oldTansact.EndTime + period;
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
                + " Starts at: " + Constans.GetTimeFromMinutes(StartTime)
                + " Ends at: " + Constans.GetTimeFromMinutes(EndTime)
                + " Cost: " + Cost;
        }
    }
}
