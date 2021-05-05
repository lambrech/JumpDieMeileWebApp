namespace JumpDieMeileWebApp.Pages
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;
    using JumpDieMeileWebApp.Common;
    using JumpDieMeileWebApp.Models;
    using JumpDieMeileWebApp.Persistence;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;
    using MudBlazor;

    [Route(PageRoutes.RegisterSponsorRoute)]
    public partial class RegisterSponsor
    {
        private IList<Runner> allPersistedRunners = new List<Runner>();

        [Inject]
        public IPersistenceProvider PersistenceProvider { get; private set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; private set; } = null!;

        private SponsoringEntry newSponsoringEntry = new();

        private SponsoringEntry NewSponsoringEntry
        {
            get => this.newSponsoringEntry;

            set
            {
                if (this.newSponsoringEntry == value)
                {
                    return;
                }

                this.newSponsoringEntry = value;
                this.SetValue(SponsoringEntry.InstanceToValidateHelperKey, this.newSponsoringEntry);
            }
        }

        private bool RegistrationDone { get; set; }

        private SponsoringEntry? RegisteredSponsoringEntry { get; set; }

        private MudForm CurrentForm { get; set; } = null!;

        private string SaveFailedErrorText { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            this.RegisteredSponsoringEntry = null;
            this.RegistrationDone = false;
            this.NewSponsoringEntry = new SponsoringEntry();
            this.CurrentForm = new MudForm();
            await this.ReloadPersistedRunners();
            await base.OnInitializedAsync();
        }

        private async Task ReloadPersistedRunners()
        {
            this.allPersistedRunners = await this.PersistenceProvider.GetAllPersistedRunners();
        }

        private async Task HandleValidSubmit()
        {
            this.CurrentForm.Validate();
            if (!this.CurrentForm.IsValid)
            {
                return;
            }

            var list = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(this.NewSponsoringEntry, new ValidationContext(this.NewSponsoringEntry), list, true);

            if (!isValid)
            {
                return;
            }

            var result = await this.PersistenceProvider.PersistSponsoringEntry(this.NewSponsoringEntry);

            if (result.Success)
            {
                this.SaveFailedErrorText = string.Empty;
                this.RegistrationDone = true;
                this.RegisteredSponsoringEntry = this.NewSponsoringEntry;
                this.NewSponsoringEntry = new SponsoringEntry();
                this.StateHasChanged();
            }
            else
            {
                Console.WriteLine("Saving failed");
                this.SaveFailedErrorText = RegisterRunner.SaveErrorText;
            }
        }

        private async Task<IEnumerable<Runner?>> RunnerSearchFunc(string? arg)
        {
            return await RegisterRun.GetMatchingRunners(arg, this.allPersistedRunners);
        }

        private string? CurrentRunnerErrorText()
        {
            var res = new List<ValidationResult>();
            Validator.TryValidateProperty(this.NewSponsoringEntry.SponsoredRunner, new ValidationContext(this.NewSponsoringEntry) { MemberName = nameof(SponsoringEntry.SponsoredRunner) }, res);
            return res.FirstOrDefault()?.ErrorMessage;
        }

        private Func<T, string?> ValidateProp<T>(string propName)
        {
            return val =>
            {
                var res = new List<ValidationResult>();
                Validator.TryValidateProperty(
                    val,
                    new ValidationContext(this.NewSponsoringEntry) { MemberName = propName },
                    res);
                return res.FirstOrDefault()?.ErrorMessage;
            };
        }
    }
}