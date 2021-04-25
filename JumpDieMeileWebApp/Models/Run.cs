namespace JumpDieMeileWebApp.Models
{
    using System;

    public class Run : ModelBase
    {
        public Run(Runner runner)
        {
            this.Runner = runner;
        }

        public Runner Runner { get; set; }

        public decimal DistanceKm { get; set; }

        public DateTime StartTimestampUtc { get; set; }

        public TimeSpan? Duration { get; set; }
    }
}