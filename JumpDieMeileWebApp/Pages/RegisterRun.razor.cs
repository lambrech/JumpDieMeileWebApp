namespace JumpDieMeileWebApp.Pages
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using JumpDieMeileWebApp.Models;
    using JumpDieMeileWebApp.Persistence;
    using Microsoft.AspNetCore.Components;
    using MudBlazor;

    [Route(PageRoutes.RegisterRunRoute)]
    public partial class RegisterRun
    {
        private IList<Runner> allPersistedRunners = new List<Runner>();

        [Inject]
        public IPersistenceProvider PersistenceProvider { get; private set; } = null!;

        [Required(ErrorMessage = "Bitte wähle den Sportler aus, für welchen diese Aktivität gemeldet werden soll. Kannst du den gesuchten Namen nicht finden, muss du diesen Sportler vermutlich erst noch registrieren.")]
        public Runner? SelectedRunner { get; set; }

        [Required(ErrorMessage = "Es muss eine Sportart ausgewählt werden.")]
        public RunMode? SelectedRunMode { get; set; }

        [Required(ErrorMessage = "Bitte gebe einen validen Zahlenwert für die Strecke an.")]
        [Range(0, 500, ErrorMessage = "Der angegebene Wert muss zwischen 0 und 500km liegen.")]
        public double? Distance { get; set; }

        [Required(ErrorMessage = "Bitte gib das Datum an, an welchem deine Aktivität stattgefunden hat.")]
        public DateTime? StartTimeLocalDate { get; set; }

        [Required(ErrorMessage = "Bitte gib die (ungefähre) Uhrzeit an, zu welcher du deine Aktivität begonnen hast. Eingabeformat <HH:mm>. Z.B. 17:45. Warum Startzeit? Zur Unterscheidung mehrerer Strecken die am selben Tag zurückgelegt wurden.")]
        public TimeSpan? StartTimeLocalTime { get; set; }

        public decimal? DurationInMinutes { get; set; }

        [Parameter]
        public bool RunRegistered { get; set; }

        [Parameter]
        public Run? RegisteredRun { get; set; }

        private MudForm CurrentForm { get; set; } = null!;
        
        private OwnMudTimePicker CurrentTimePicker { get; set; } = null!;

        private string SaveFailedErrorText { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            this.CurrentForm = new MudForm();
            this.CurrentTimePicker = new OwnMudTimePicker();
            await this.ReloadPersistedRunners();
            await base.OnInitializedAsync();
        }

        private static TimeSpan? TryGetDurationTimespan(decimal? durationInMinutes)
        {
            return durationInMinutes.HasValue && (durationInMinutes.Value > 0) ? TimeSpan.FromMinutes((double)durationInMinutes.Value) : null;
        }

        private async Task HandleValidSubmit()
        {
            await this.CurrentForm.Validate();
            if (!this.CurrentForm.IsValid)
            {
                return;
            }

            if ((this.SelectedRunner != null) &&
                this.SelectedRunMode.HasValue &&
                this.Distance.HasValue &&
                this.StartTimeLocalDate.HasValue &&
                this.StartTimeLocalTime.HasValue)
            {
                var localTime = this.StartTimeLocalDate.Value.Add(this.StartTimeLocalTime.Value);
                this.RegisteredRun = new Run(
                    this.SelectedRunner,
                    this.SelectedRunMode,
                    (decimal)this.Distance.Value,
                    localTime.ToUniversalTime(),
                    TryGetDurationTimespan(this.DurationInMinutes));
                var result = await this.PersistenceProvider.PersistRun(this.RegisteredRun);

                if (result.Success)
                {
                    this.SaveFailedErrorText = string.Empty;
                    this.RunRegistered = true;

                    // reset the form to reduce possibility of double entry
                    this.CurrentForm.Reset();
                    this.StateHasChanged();
                }
                else
                {
                    Console.WriteLine("Saving failed");
                    this.SaveFailedErrorText = RegisterRunner.SaveErrorText;
                }
            }
        }

        private async Task ReloadPersistedRunners()
        {
            this.allPersistedRunners = await this.PersistenceProvider.GetAllPersistedRunners();
        }

        private async Task<IEnumerable<Runner?>> RunnerSearchFunc(string? arg)
        {
            return await GetMatchingRunners(arg, this.allPersistedRunners);
        }

        public static async Task<IEnumerable<Runner?>> GetMatchingRunners(string? arg, IList<Runner> runners)
        {
            if ((arg == null) || (arg.Length < 3))
            {
                return new List<Runner>();
            }

            await Task.Delay(50);
            var matches = runners
                         .Where(x => x.FullDisplayName.ToLower(CultureInfo.CurrentCulture).Contains(arg.ToLower(CultureInfo.CurrentCulture)))
                         .ToList();

            return matches;
        }

        private void Reset()
        {
            this.RegisteredRun = null;
            this.RunRegistered = false;
            this.StartTimeLocalTime = null;
            this.StartTimeLocalDate = null;
            this.Distance = null;
            this.DurationInMinutes = null;

#pragma warning disable BL0005 // Component parameter should not be set outside of its component. - seems to be bugged
            this.CurrentTimePicker.Text = string.Empty;
#pragma warning restore BL0005 // Component parameter should not be set outside of its component.
            this.CurrentForm.Reset();
            this.StateHasChanged();
        }
    }
}