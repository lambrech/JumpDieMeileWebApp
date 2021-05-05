namespace JumpDieMeileWebApp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class PersonData : ModelBase
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Der Vorname darf nicht leer sein.")]
        [StringLength(40, ErrorMessage = "Der Vorname muss aus mindestens 2 Zeichen und maximal aus 40 Zeichen bestehen.", MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Der Nachname darf nicht leer sein.")]
        [StringLength(40, ErrorMessage = "Der Nachname muss aus mindestens 2 Zeichen und maximal aus 40 Zeichen bestehen.", MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Die angegebene Email ist ungültig.")]
        public string Email { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Der Wohnort darf nicht leer sein.")]
        public string Location { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Die PLZ darf nicht leer sein.")]
        public string Postcode { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Straße und Hausnummer dürfen nicht leer sein.")]
        public string StreetHouseNr { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Es muss ein Geschlecht angegeben werden.")]
        public Gender? Gender { get; set; }
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

        public static string ToDisplayString(this SponsoringMode? mode)
        {
            // should not be possible - but whatever
            if (mode == null)
            {
                return "Keine Angabe";
            }

            return mode.Value switch
            {
                SponsoringMode.SingleRunner => "Einen Läufer unterstützen",
                SponsoringMode.WholeProject => "Das Projekt 'Homecoming' unterstützen",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}