namespace BusSolOnDB
{
    public static class Constans
    {
        public const int MinutesInHour = 60;
        public const int HoursInDay = 24;
        public static string GetTimeFromMinutes(int minutes)
        {
            return minutes / MinutesInHour + ":" + minutes % MinutesInHour;
        }
    }
}
