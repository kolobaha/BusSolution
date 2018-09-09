namespace BusSolOnDB.Models
{
    // Класс для хранения исходных данных о переездах.
    public class Move 
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
