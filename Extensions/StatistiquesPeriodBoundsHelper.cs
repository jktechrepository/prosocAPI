namespace ProsocAPI.Extensions
{
    public static class StatistiquesPeriodBoundsHelper
    {
        public static (DateTime Start, DateTime End) CurrentMonth(DateTime now)
        {
            var start = new DateTime(now.Year, now.Month, 1);
            return (start, now);
        }

        public static (DateTime Start, DateTime End) PreviousMonth(DateTime now)
        {
            var startCurrent = new DateTime(now.Year, now.Month, 1);
            var start = startCurrent.AddMonths(-1);
            var end = startCurrent.AddTicks(-1);
            return (start, end);
        }

        public static (DateTime Start, DateTime End) CustomOrDefaultYearToDate(DateTime now, DateTime? dateDebut, DateTime? dateFin)
        {
            if (dateDebut.HasValue || dateFin.HasValue)
            {
                var start = dateDebut ?? new DateTime(now.Year, 1, 1);
                var end = dateFin ?? now;
                return (start, end);
            }

            return (new DateTime(now.Year, 1, 1), now);
        }
    }
}
