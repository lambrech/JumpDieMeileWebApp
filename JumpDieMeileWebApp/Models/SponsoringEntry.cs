namespace JumpDieMeileWebApp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using JumpDieMeileWebApp.Common;

    public class SponsoringEntry : PersonData
    {
        public const string InstanceToValidateHelperKey = "Helper_InstanceToValidate";

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
                SponsoringEntry? obj = null;
                if (context.ObjectInstance is SponsoringEntry tmp)
                {
                    obj = tmp;
                }
                else if (context.ObjectInstance?.GetValue(InstanceToValidateHelperKey) is SponsoringEntry tmp2)
                {
                    obj = tmp2;
                }

                if (obj != null)
                {
                    if (context.MemberName is nameof(SponsoredRunner))
                    {
                        return (obj.SponsoringMode == Models.SponsoringMode.SingleRunner && obj.SponsoredRunner == null)
                            ? new ValidationResult("Es muss ein Läufer angegeben werden.")
                            : null;
                    }

                    if (context.MemberName is (nameof(ImmediateInEuro)) or (nameof(PerKmInEuro)))
                    {
                        if (!obj.ImmediateInEuro.HasValue && !obj.PerKmInEuro.HasValue)
                        {
                            return new ValidationResult(
                                "Es muss mindestens ein Sponsoring Betrag angegeben werden.",
                                new[] { nameof(ImmediateInEuro), nameof(PerKmInEuro) });
                        }

                        if (context.MemberName is nameof(ImmediateInEuro) && obj.ImmediateInEuro.HasValue && (obj.ImmediateInEuro.Value > 5000 || obj.ImmediateInEuro < 1))
                        {
                            return new ValidationResult(
                                "Der Pauschalbetrag muss, wenn angegeben, zwischen 1€ und 5000€ liegen. Wenn du mehr spenden möchtest, melde dich bitte direkt bei JUMP <3",
                                new[] { nameof(ImmediateInEuro) });
                        }

                        if (context.MemberName is nameof(PerKmInEuro) && obj.PerKmInEuro.HasValue)
                        {
                            if (obj.SponsoringMode == Models.SponsoringMode.SingleRunner
                            && (obj.PerKmInEuro.Value > 50 || obj.PerKmInEuro.Value < 0))
                            {
                                return new ValidationResult(
                                    "Der Betrag pro km muss, wenn angegeben, zwischen 0€ und 50€ liegen. Wenn du mehr spenden möchtest, melde dich bitte direkt bei JUMP <3",
                                    new[] { nameof(ImmediateInEuro) });
                            }

                            if (obj.SponsoringMode == Models.SponsoringMode.WholeProject
                                && (obj.PerKmInEuro.Value > (decimal)0.5 || obj.PerKmInEuro.Value < 0))
                            {
                                return new ValidationResult(
                                    "Der Betrag pro km muss, wenn angegeben, zwischen 0€ und 0.5€ liegen. Wenn du mehr spenden möchtest, melde dich bitte direkt bei JUMP <3",
                                    new[] { nameof(ImmediateInEuro) });
                            }
                        }

                        return null;
                    }
                }
            }
            catch (Exception)
            {
                // it's okay
            }

            return new ValidationResult("Die Validierung wurde unerwartet beendet.");
        }
    }

    public enum SponsoringMode
    {
        SingleRunner,

        WholeProject,
    }
}