namespace JumpDieMeileWebApp.Persistence
{
    public abstract record PersistResult
    {
        public PersistResult(bool success)
        {
            this.Success = success;
        }

        public bool Success { get; }
    }

    public record PersistResultSuccess : PersistResult
    {
        public PersistResultSuccess() : base(true)
        {
        }
    }

    public record PersistResultError : PersistResult
    {
        public PersistResultError() : base(false)
        {
        }

        public string? ErrorMessage { get; init; }
    }
}