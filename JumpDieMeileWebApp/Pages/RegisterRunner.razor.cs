namespace JumpDieMeileWebApp.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using JumpDieMeileWebApp.Common;
    using JumpDieMeileWebApp.Models;
    using JumpDieMeileWebApp.Persistence;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;

    public partial class RegisterRunner
    {
        [Inject]
        public IPersistenceProvider PersistenceProvider { get; private set; } = null!;

        private Runner newRunner = new();
        
        [Parameter]
        public Runner NewRunner
        {
            get => this.newRunner;

            set
            {
                if (this.newRunner == value)
                {
                    return;
                }

                this.newRunner.UsernameChanged -= this.OnNewRunnerUsernameChanged;
                this.newRunner = value;
                this.newRunner.UsernameChanged += this.OnNewRunnerUsernameChanged;
            }
        }

        private void OnNewRunnerUsernameChanged(object? sender, EventArgs e)
        {
            this.CurrentEditContext.Validate();
        }

        [Parameter]
        public bool RegistrationDone { get; set; }

        [Parameter]
        public Runner? RegisteredRunner { get; set; }

        private EditContext CurrentEditContext { get; set; } = null!;

        private IList<Runner> allPersistedRunners = new List<Runner>();

        private async Task HandleValidSubmit()
        {
            if (!await this.ReloadPersistedRunnersAndValidateUserName())
            {
                return;
            }

            await this.PersistenceProvider.PersistRunner(this.NewRunner);
            this.RegistrationDone = true;
            this.RegisteredRunner = this.NewRunner;
            this.NewRunner = new ();
            this.StateHasChanged();
        }

        public bool IsCurrentUserNameValid => Runner.ValidateUserName(this.NewRunner.Username, this.allPersistedRunners);

        private void Revalidate()
        {
            Console.WriteLine("Revalidating");
            this.CurrentEditContext?.Validate();
        }

        private async Task<bool> ReloadPersistedRunnersAndValidateUserName()
        {
            Console.WriteLine("Reloading runners from db");
            this.allPersistedRunners = await this.PersistenceProvider.GetAllPersistedRunners();
            this.NewRunner.SetValue(Runner.OtherRunnersHelperKey, this.allPersistedRunners);
            return this.IsCurrentUserNameValid;
        }

        protected override async Task OnInitializedAsync()
        {
            this.NewRunner = new();
            this.CurrentEditContext = new EditContext(this.NewRunner);
            await this.ReloadPersistedRunnersAndValidateUserName();
            await base.OnInitializedAsync();
        }
    }
}
