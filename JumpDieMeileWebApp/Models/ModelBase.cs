namespace JumpDieMeileWebApp.Models
{
    using System;

    public class ModelBase
    {
        public ModelBase()
        {
            this.Id = Guid.NewGuid();
            this.ModelVersion = 0;
            this.CreationTimestampUtc = DateTime.UtcNow;
        }

        public Guid Id { get; init; }

        public int ModelVersion { get; init; }

        public DateTime CreationTimestampUtc { get; init; }
    }
}