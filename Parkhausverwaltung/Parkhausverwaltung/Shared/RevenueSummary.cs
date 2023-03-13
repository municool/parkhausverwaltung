namespace Parkhausverwaltung.Shared
{
    public class RevenueSummary
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal VisitorRevenue { get; set; }
        public int VisitorCount { get; set; }
        public decimal MieterRevenue { get; set; }
        public int MieterCount { get; set; }

    }
}
