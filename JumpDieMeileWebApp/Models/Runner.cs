namespace JumpDieMeileWebApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using JumpDieMeileWebApp.Persistence;

    public class Runner : ModelBase, IValidatableObject
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Der Vorname darf nicht leer sein.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Der Nachname darf nicht leer sein.")]
        public string LastName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Der Nutzername darf nicht leer sein.")]
        public string Username { get; set; } = string.Empty;

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
    }

    public interface IHaveUserNameValidation
    {
        ValidationResult ValidateUserName(string userName);
    }
}