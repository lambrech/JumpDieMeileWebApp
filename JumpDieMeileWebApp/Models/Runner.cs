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

        [Required(AllowEmptyStrings = false, ErrorMessage = "Der Wohnort darf nicht leer sein.")]
        public string Location { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Die PLZ darf nicht leer sein.")]
        public string Postcode { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Straße und Hausnummer dürfen nicht leer sein.")]
        public string StreetHouseNr { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Es muss ein Geschlecht angegeben werden.")]
        public Gender? Gender { get; set; }

        public string Comment { get; set; } = string.Empty;

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

    public enum Gender
    {
        Male = 0,
        Female = 1,
        Divers = 2,
    }

    public static class DisplayExtensions
    {
        public static string ToDisplayString(this Gender? gender)
        {
            // should not be possible - but whatever
            if (gender == null)
            {
                return "Keine Angabe";
            }

            return gender.Value switch
            {
                Gender.Male => "Männlich",
                Gender.Female => "Weiblich",
                Gender.Divers => "Divers",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}