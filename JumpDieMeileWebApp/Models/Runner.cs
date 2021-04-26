namespace JumpDieMeileWebApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using JumpDieMeileWebApp.Common;
    using JumpDieMeileWebApp.Persistence;

    public class Runner : ModelBase, IValidatableObject
    {
        public const string OtherRunnersHelperKey = "Helper_OtherRunners";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Der Vorname darf nicht leer sein.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Der Nachname darf nicht leer sein.")]
        public string LastName { get; set; } = string.Empty;

        private string username = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Der Nutzername darf nicht leer sein.")]
        [CustomValidation(typeof(Runner), nameof(CustomUsernameValidation))]
        public string Username
        {
            get => this.username;

            set
            {
                if (this.username == value)
                {
                    return;
                }

                this.username = value;
                this.UsernameChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler? UsernameChanged;

        [EmailAddress(ErrorMessage = "Die angegebene Email ist ungültig.")]
        public string Email { get; set; } = string.Empty;

        public string FullDisplayName => $"{this.Username} - {this.Id.ToString().Substring(0, 6)}";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var list = new List<ValidationResult>();

            Console.WriteLine(validationContext.DisplayName);
            Console.WriteLine(validationContext.ObjectType);
            Console.WriteLine(validationContext.ObjectInstance.GetType());

            if (validationContext.ObjectInstance is IHaveUserNameValidation usernameValidator)
            {
                list.Add(usernameValidator.ValidateUserName(this.Username));
            }

            return list;
        }

        public static ValidationResult CustomUsernameValidation(object _, ValidationContext context)
        {
            try
            {
                if (context.ObjectInstance is Runner runnerToValidate && runnerToValidate.GetValue(OtherRunnersHelperKey) is IList<Runner> otherRunners)
                {
                    if (ValidateUserName(runnerToValidate.Username, otherRunners))
                    {
                        return ValidationResult.Success!;
                    }
                    else
                    {
                        return new ValidationResult("Der angegebene Nutzername ist leider bereits vergeben. Bitte wähle einen anderen.");
                    }
                }
            }
            catch (Exception)
            {
                // it's okay
            }

            return new ValidationResult("Die Validierung des Nutzernamens wurde unerwartet beendet.");
        }

        public static bool ValidateUserName(string usernameToValidate, IList<Runner> existingRunners)
        {
            return existingRunners.All(x => x.Username != usernameToValidate);
        }
    }

    public interface IHaveUserNameValidation
    {
        ValidationResult ValidateUserName(string userName);
    }
}