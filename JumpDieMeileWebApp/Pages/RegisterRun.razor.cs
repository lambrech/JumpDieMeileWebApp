namespace JumpDieMeileWebApp.Pages
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Threading.Tasks;
    using JumpDieMeileWebApp.Models;
    using JumpDieMeileWebApp.Persistence;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;

    [Route(PageRoutes.RegisterRunRoute)]
    public partial class RegisterRun
    {
        [Inject]
        public IPersistenceProvider PersistenceProvider { get; private set; } = null!;

        [Required(ErrorMessage = "Bitte wähle den Läufer aus, für welchen dieser Lauf gemeldet werden soll.")]
        public Runner SelectedRunner { get; set; } = null!;

        [Required(ErrorMessage = "Bitte gebe einen validen Zahlenwert für die Strecke an.")]
        public decimal? Distance { get; set; }

        [Required(ErrorMessage = "Bitte gib das Datum an, an welchem der Lauf stattgefunden hat.")]
        public DateTime? StartTimeLocalDate { get; set; }

        [Required(ErrorMessage = "Bitte gib die Uhrzeit an, zu welcher du deinen Lauf begonnen hast.")]
        [CustomValidation(typeof(RegisterRun), nameof(IsValidTimespanString))]
        public string StartTimeLocalTime { get; set; } = string.Empty;

        [CustomValidation(typeof(RegisterRun), nameof(IsValidTimespanString))]
        public string Duration { get; set; } = string.Empty;

        [Parameter]
        public bool RunRegistered { get; set; }

        [Parameter]
        public Run? RegisteredRun { get; set; }

        private EditContext CurrentEditContext { get; set; } = null!;

        private IList<Runner> allPersistedRunners = new List<Runner>();

        private async Task HandleValidSubmit()
        {
            if (this.SelectedRunner != null &&
                this.Distance.HasValue &&
                this.StartTimeLocalDate.HasValue &&
                TryGetTimeSpan(this.StartTimeLocalTime) is { } startTime)
            {
                var localTime = this.StartTimeLocalDate.Value.Add(startTime);
                this.RegisteredRun = new Run(this.SelectedRunner, this.Distance.Value, localTime.ToUniversalTime(), TryGetTimeSpan(this.Duration));
                await this.PersistenceProvider.PersistRun(this.RegisteredRun);
                this.RunRegistered = true;
                this.StateHasChanged();
            }
        }

        private async Task ReloadPersistedRunners()
        {
            Console.WriteLine("Reloading runners from db");
            this.allPersistedRunners = await this.PersistenceProvider.GetAllPersistedRunners();
        }

        protected override async Task OnInitializedAsync()
        {
            this.CurrentEditContext = new EditContext(this);
            await this.ReloadPersistedRunners();
            await base.OnInitializedAsync();
        }

        public static ValidationResult? IsValidTimespanString(string timespanString, ValidationContext _)
        {
            return string.IsNullOrWhiteSpace(timespanString) || TryGetTimeSpan(timespanString).HasValue
                ? ValidationResult.Success
                : new ValidationResult("Die Zeiteingabe entspricht nicht dem erwarteten Format.");
        }

        public static TimeSpan? TryGetTimeSpan(string timespanString)
        {
            if (TimeSpan.TryParseExact(timespanString, "HH:mm:ss", CultureInfo.CurrentCulture, out var ts))
            {
                return ts;
            }

            return null;
        }
    }
}
