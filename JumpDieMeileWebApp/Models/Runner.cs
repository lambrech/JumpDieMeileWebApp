namespace JumpDieMeileWebApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using JumpDieMeileWebApp.Common;

    public class Runner : ModelBase
    {
        public const string OtherRunnersHelperKey = "Helper_OtherRunners";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Der Vorname darf nicht leer sein.")]
        [StringLength(40, ErrorMessage = "Der Vorname muss aus mindestens 2 Zeichen und maximal aus 40 Zeichen bestehen.", MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Der Nachname darf nicht leer sein.")]
        [StringLength(40, ErrorMessage = "Der Nachname muss aus mindestens 2 Zeichen und maximal aus 40 Zeichen bestehen.", MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Der Nutzername darf nicht leer sein.")]
        [StringLength(40, ErrorMessage = "Der Nutzername muss aus mindestens 4 Zeichen und maximal aus 40 Zeichen bestehen.", MinimumLength = 4)]
        [CustomValidation(typeof(Runner), nameof(CustomUsernameValidation))]
        public string Username { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Die angegebene Email ist ungültig.")]
        public string Email { get; set; } = string.Empty;

        public string FullDisplayName => $"{this.Username} - {this.Id.ToString().Substring(0, 6)}";

        public static ValidationResult? CustomUsernameValidation(object _, ValidationContext context)
        {
            try
            {
                if (context.ObjectInstance is Runner runnerToValidate && runnerToValidate.GetValue(OtherRunnersHelperKey) is IList<Runner> otherRunners)
                {
                    return ValidateUserName(runnerToValidate.Username, otherRunners)
                        ? ValidationResult.Success
                        : new ValidationResult("Der angegebene Nutzername ist leider bereits vergeben. Bitte wähle einen anderen.");
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
}