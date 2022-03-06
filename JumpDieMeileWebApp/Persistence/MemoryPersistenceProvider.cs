namespace JumpDieMeileWebApp.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JumpDieMeileWebApp.Models;

    public class MemoryPersistenceProvider : IPersistenceProvider
    {
        private readonly List<Runner> persistedRunners = new ();

        private readonly List<Run> persistedRuns = new ();

        private readonly List<SponsoringEntry> sponsoringEntries = new ();

        public async Task<PersistResult> PersistRunner(Runner runner)
        {
            await Task.Delay(0);
            try
            {
                if (runner.Username.ToLowerInvariant().Contains("nazi"))
                {
                    return new PersistResultError { ErrorMessage = "Das ist nicht witzig!" };
                }

                if (this.persistedRunners.Any(x => x.Id == runner.Id))
                {
                    return new PersistResultError
                    {
                        ErrorMessage =
                            "Ein Sportler mit gleicher ID existiert bereits! Das sollte technisch nicht möglich sein ...  Bitte lade die Seite neu und versuche es nochmal. Sorry!"
                    };
                }

                this.persistedRunners.Add(runner);
                return new PersistResultSuccess();
            }
            catch (Exception e)
            {
                return new PersistResultError { ErrorMessage = e.ToString() };
            }
        }

        public async Task<IList<Runner>> GetAllPersistedRunners()
        {
            await Task.Delay(0);
            return this.persistedRunners;
        }

        public async Task<PersistResult> PersistRun(Run run)
        {
            await Task.Delay(0);
            this.persistedRuns.Add(run);
            return new PersistResultSuccess();
        }

        public async Task<IList<Run>> GetAllPersistedRuns()
        {
            await Task.Delay(0);
            return this.persistedRuns;
        }

        public async Task<PersistResult> PersistSponsoringEntry(SponsoringEntry sponsoringEntry)
        {
            await Task.Delay(0);
            this.sponsoringEntries.Add(sponsoringEntry);
            return new PersistResultSuccess();
        }

        public async Task<IList<SponsoringEntry>> GetAllPersistedSponsoringEntries()
        {
            await Task.Delay(0);
            return this.sponsoringEntries;
        }

        public async Task<decimal> GetDistanceSumOfAllRuns()
        {
            await Task.Delay(0);
            return this.persistedRuns.Sum(x => x.DistanceKm);
        }
    }
}