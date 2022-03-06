namespace JumpDieMeileWebApp.Models
{
    using System;

    public class ModelBase
    {
        public ModelBase()
        {
            this.Id = Guid.NewGuid();
            this.ModelVersion = 1; // increased to 1 in 2022 for changes with bike data
            this.CreationTimestampUtc = DateTime.UtcNow;
        }

        public Guid Id { get; init; }

        public int ModelVersion { get; init; }

        public DateTime CreationTimestampUtc { get; init; }
    }
}