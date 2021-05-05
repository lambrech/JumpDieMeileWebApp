namespace JumpDieMeileWebApp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class SponsoringEntry : PersonData
    {
        [Required(ErrorMessage = "Er muss ein Sponsoring Modus ausgewählt werden.")]
        public SponsoringMode? SponsoringMode { get; set; }

        [CustomValidation(typeof(SponsoringEntry), nameof(CustomSponsoringEntryValidation))]
        public Runner? SponsoredRunner { get; set; }

        [CustomValidation(typeof(SponsoringEntry), nameof(CustomSponsoringEntryValidation))]
        public decimal? ImmediateInEuro { get; set; }

        [CustomValidation(typeof(SponsoringEntry), nameof(CustomSponsoringEntryValidation))]
        public decimal? PerKmInEuro { get; set; }

        public static ValidationResult? CustomSponsoringEntryValidation(object _, ValidationContext context)
        {
            try
            {
                if (context.ObjectInstance is SponsoringEntry obj)
                {
                    if (context.MemberName is nameof(SponsoredRunner))
                    {
                        return (obj.SponsoringMode == Models.SponsoringMode.SingleRunner && obj.SponsoredRunner == null)
                            ? new ValidationResult("Es muss ein Läufer angegeben werden.")
                            : null;
                    }

                    if (context.MemberName is (nameof(ImmediateInEuro)) or (nameof(PerKmInEuro)))
                    {
                        return (!obj.ImmediateInEuro.HasValue && !obj.PerKmInEuro.HasValue)
                            ? new ValidationResult(
                                "Es muss mindestens ein Sponsoring Betrag angegeben werden.",
                                new[] { nameof(ImmediateInEuro), nameof(PerKmInEuro) })
                            : null;
                    }
                }
            }
            catch (Exception)
            {
                // it's okay
            }

            return new ValidationResult("Die Validierung des Nutzernamens wurde unerwartet beendet.");
        }
    }

    public enum SponsoringMode
    {
        SingleRunner,

        WholeProject,
    }
}