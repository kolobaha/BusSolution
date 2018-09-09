namespace BusSolOnDB.Models
{
    // Класс для хранения исходных данных об автобусах.
    public class Bus
    {
        public int Id { get; set; }
        public int Cost { get; set; }
        // Время в минутах от 0 до 1440.
        public int StartTime { get; set; }
        // Период цикла.
        public int Period { get; set; }

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
                + " Starts at: " + Constans.GetTimeFromMinutes(StartTime)
                + " Period: " + Period;
        }
    }
}
