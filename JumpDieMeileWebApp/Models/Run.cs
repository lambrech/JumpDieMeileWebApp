namespace JumpDieMeileWebApp.Models
{
    using System;

    public class Run : ModelBase
    {
        public Run(Runner runner, decimal distanceKm, DateTime startTimestampUtc, TimeSpan? duration)
        {
            this.Runner = runner;
            this.DistanceKm = distanceKm;
            this.StartTimestampUtc = startTimestampUtc;
            this.Duration = duration;
        }

        public Runner? Runner { get; }

        public decimal DistanceKm { get; }

        public DateTime StartTimestampUtc { get; }

        public TimeSpan? Duration { get; }

        public decimal? AverageSpeedKmh => this.Duration.HasValue ? this.DistanceKm / (decimal)this.Duration.Value.TotalHours : null;
    }
}