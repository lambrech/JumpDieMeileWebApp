namespace JumpDieMeileWebApp.Models
{
    using System;

    // CL 05.03.2022 - I can't remember why, but validation for this class is done in the RegisterRun.razor.cs class.
    // I guess because I write in tmp properties first, validate them and then create the Run object only with valid data.
    // Still guessing: Maybe because of the complexity with start time with date and time
    public class Run : ModelBase
    {
        public Run(Runner runner, RunMode? runMode, decimal distanceKm, DateTime startTimestampUtc, TimeSpan? duration)
        {
            this.Runner = runner;
            this.RunMode = runMode;
            this.DistanceKm = distanceKm;
            this.StartTimestampUtc = startTimestampUtc;
            this.Duration = duration;
        }

        public Runner? Runner { get; }

        public RunMode? RunMode { get; }

        public decimal DistanceKm { get; }

        public DateTime StartTimestampUtc { get; }

        public TimeSpan? Duration { get; }

        public decimal? AverageSpeedKmh => this.Duration.HasValue ? this.DistanceKm / (decimal)this.Duration.Value.TotalHours : null;
    }

    public enum RunMode
    {
        Foot,
        Bike,
    }
}