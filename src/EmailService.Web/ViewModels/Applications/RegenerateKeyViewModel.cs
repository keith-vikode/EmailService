using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using EmailService.Core.Entities;
using EmailService.Core.Services;

namespace EmailService.Web.ViewModels.Applications
{
    public class RegenerateKeyViewModel : IValidatableObject
    {
        public const string Primary = nameof(Primary);
        public const string Secondary = nameof(Secondary);

        public Guid Id { get; set; }

        public string Name { get; set; }

        [Required]
        public string Key { get; set; }

        public bool IsPrimary => string.Equals(Key, Primary, StringComparison.OrdinalIgnoreCase);

        public bool IsSecondary => string.Equals(Key, Secondary, StringComparison.OrdinalIgnoreCase);

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsPrimary && !IsSecondary)
            {
                yield return new ValidationResult($"Invalid key type: must be '{Primary}' or '{Secondary}'", new string[] { nameof(Key) });
            }
        }

        public async Task SaveChangesAsync(EmailServiceContext ctx, ICryptoServices crypto)
        {
            var app = await ctx.FindApplicationAsync(Id);
            if (app != null)
            {
                if (IsPrimary)
                {
                    app.PrimaryApiKey = crypto.GenerateKey();
                }
                else if (IsSecondary)
                {
                    app.SecondaryApiKey = crypto.GenerateKey();
                }

                await ctx.SaveChangesAsync();
            }
        }
    }
}
