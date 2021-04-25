namespace JumpDieMeileWebApp.Pages
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;
    using JumpDieMeileWebApp.Models;
    using JumpDieMeileWebApp.Persistence;
    using Microsoft.AspNetCore.Components;

    public partial class RegisterRunner : IHaveUserNameValidation
    {
        [Inject]
        public IPersistenceProvider PersistenceProvider { get; private set; } = null!;

        [Parameter]
        public Runner NewRunner { get; set; } = new ();

        [Parameter]
        public bool RegistrationDone { get; set; }

        [Parameter]
        public Runner? RegisteredRunner { get; set; }

        private IList<Runner> allPersistedRunners = new List<Runner>();

        private async Task HandleValidSubmit()
        {
            if (!await this.ReloadAndValidateUserName())
            {
                return;
            }

            await this.PersistenceProvider.PersistRunner(this.NewRunner);
            this.RegistrationDone = true;
            this.RegisteredRunner = this.NewRunner;
            this.NewRunner = new ();
            this.StateHasChanged();
        }

        private bool ValidateUserName()
        {
            return this.allPersistedRunners.All(x => x.Username != this.NewRunner.Username);
        }

        public bool IsCurrentUserNameValid => this.ValidateUserName();

        private async Task<bool> ReloadAndValidateUserName()
        {
            this.allPersistedRunners = await this.PersistenceProvider.GetAllPersistedRunners();
            return this.IsCurrentUserNameValid;
        }

        protected override async Task OnInitializedAsync()
        {
            this.allPersistedRunners = await this.PersistenceProvider.GetAllPersistedRunners();
            await base.OnInitializedAsync();
        }

        public ValidationResult ValidateUserName(string userName)
        {
            if (this.IsCurrentUserNameValid)
            {
                return ValidationResult.Success!;
            }

            return new ValidationResult("Der Nutzername ist kaputt");
        }
    }
}
