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
                            "Ein Läufer mit gleicher ID existiert bereits! Das sollte technisch nicht möglich sein ...  Bitte lade die Seite neu und versuche es nochmal. Sorry!"
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

        public Task<IList<Runner>> GetAllPersistedRunners()
        {
            return Task.FromResult((IList<Runner>)this.persistedRunners);
        }

        public Task<PersistResult> PersistRun(Run run)
        {
            throw new System.NotImplementedException();
        }

        public Task<IList<Run>> GetAllPersistedRuns()
        {
            throw new System.NotImplementedException();
        }
    }
}