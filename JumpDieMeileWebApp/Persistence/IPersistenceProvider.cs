namespace JumpDieMeileWebApp.Persistence
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using JumpDieMeileWebApp.Models;

    public interface IPersistenceProvider
    {
        Task<PersistResult> PersistRunner(Runner runner);

        Task<IList<Runner>> GetAllPersistedRunners();

        Task<PersistResult> PersistRun(Run run);

        Task<IList<Run>> GetAllPersistedRuns();

        Task<PersistResult> PersistSponsoringEntry(SponsoringEntry sponsoringEntry);

        Task<IList<SponsoringEntry>> GetAllPersistedSponsoringEntries();

        Task<decimal> GetDistanceSumOfAllRuns();
    }
}