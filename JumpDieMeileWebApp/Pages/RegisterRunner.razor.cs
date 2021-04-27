﻿namespace JumpDieMeileWebApp.Pages
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

    [Route(PageRoutes.RegisterRunnerRoute)]
    public partial class RegisterRunner
    {
        private IList<Runner> allPersistedRunners = new List<Runner>();

        [Inject]
        public IPersistenceProvider PersistenceProvider { get; private set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; private set; } = null!;

        private Runner NewRunner { get; set; } = new();

        private bool RegistrationDone { get; set; }

        private Runner? RegisteredRunner { get; set; }

        private EditContext CurrentEditContext { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            this.RegisteredRunner = null;
            this.RegistrationDone = false;
            this.NewRunner = new Runner();
            this.CurrentEditContext = new EditContext(this.NewRunner);
            await this.ReloadPersistedRunnersAndValidateUserName();
            await base.OnInitializedAsync();
        }

        private async Task HandleValidSubmit()
        {
            await this.ReloadPersistedRunnersAndValidateUserName();

            var list = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(this.NewRunner, new ValidationContext(this.NewRunner), list, true);

            if (!isValid)
            {
                return;
            }

            await this.PersistenceProvider.PersistRunner(this.NewRunner);
            this.RegistrationDone = true;
            this.RegisteredRunner = this.NewRunner;
            this.NewRunner = new Runner();
            this.StateHasChanged();
        }

        private async Task ReloadPersistedRunnersAndValidateUserName()
        {
            Console.WriteLine($"[{DateTime.Now}]: Reloading runners from db");
            this.allPersistedRunners = await this.PersistenceProvider.GetAllPersistedRunners();
            Console.WriteLine($"[{DateTime.Now}]: Finished reload");
            this.NewRunner.SetValue(Runner.OtherRunnersHelperKey, this.allPersistedRunners);
            this.CurrentEditContext?.NotifyValidationStateChanged();
        }

        private string? CurrentUserNameErrorText()
        {
            var res = new List<ValidationResult>();
            Validator.TryValidateProperty(this.NewRunner.Username, new ValidationContext(this.NewRunner) { MemberName = nameof(Runner.Username) }, res);
            return res.FirstOrDefault()?.ErrorMessage;
        }
    }
}