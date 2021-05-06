namespace JumpDieMeileWebApp.Pages
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JumpDieMeileWebApp.Common;
    using JumpDieMeileWebApp.Models;
    using JumpDieMeileWebApp.Persistence;
    using Microsoft.AspNetCore.Components;

    public partial class Index
    {
        [Inject]
        public IPersistenceProvider PersistenceProvider { get; private set; } = null!;

        private IList<Run>? Runs { get; set; }

        private IList<SponsoringEntry>? SponsoringEntries { get; set; }

        private decimal TotalDistance { get; set; }

        private decimal CurrentEuros { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await this.Refresh();
            await base.OnInitializedAsync();
        }

        private async Task Refresh()
        {
            this.TotalDistance = await this.PersistenceProvider.GetDistanceSumOfAllRuns();
            this.StateHasChanged();
        }

        public NotifyTaskCompletion? CalculateCurrentSponsoringStateTaskCompletion { get; set; }

        private void StartCalculateCurrentSponsoringState()
        {
            if (this.CalculateCurrentSponsoringStateTaskCompletion?.IsCompleted ?? false)
            {
                return;
            }

            this.CalculateCurrentSponsoringStateTaskCompletion = new NotifyTaskCompletion(this.CalculateCurrentSponsoringState(), _ => this.StateHasChanged());
            this.StateHasChanged();
        }

        private async Task CalculateCurrentSponsoringState()
        {
            this.Runs = await this.PersistenceProvider.GetAllPersistedRuns();
            this.SponsoringEntries = await this.PersistenceProvider.GetAllPersistedSponsoringEntries();
            this.Calc();
        }

        private void Calc()
        {
            if (this.Runs == null || this.SponsoringEntries == null)
            {
                return;
            }
            var dist = this.TotalDistance;

            var runnersRunsDict = this.Runs.Where(x => x.Runner != null).GroupBy(x => x.Runner)
                                      .ToDictionary(x => x.Key!, x => x.Sum(run => run.DistanceKm));

            var allEuros = (decimal)0;
            foreach (var sponsoringEntry in this.SponsoringEntries)
            {
                switch (sponsoringEntry.SponsoringMode)
                {
                    case SponsoringMode.SingleRunner:
                        if (sponsoringEntry.SponsoredRunner != null && runnersRunsDict.TryGetValue(sponsoringEntry.SponsoredRunner, out var runs))
                        {
                            if (sponsoringEntry.ImmediateInEuro.HasValue && runs > 0)
                            {
                                allEuros += sponsoringEntry.ImmediateInEuro.Value;
                            }
                            if (sponsoringEntry.PerKmInEuro.HasValue)
                            {
                                allEuros += runs * sponsoringEntry.PerKmInEuro.Value;
                            }
                        }

                        break;
                    case SponsoringMode.WholeProject:
                        if (sponsoringEntry.ImmediateInEuro.HasValue && dist > 0)
                        {
                            allEuros += sponsoringEntry.ImmediateInEuro.Value;
                        }
                        if (sponsoringEntry.PerKmInEuro.HasValue)
                        {
                            allEuros += dist * sponsoringEntry.PerKmInEuro.Value;
                        }
                        break;
                }
            }

            this.CurrentEuros = allEuros;
        }
    }
}
