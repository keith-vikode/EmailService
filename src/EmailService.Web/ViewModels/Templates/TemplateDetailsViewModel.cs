using EmailService.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Web.ViewModels.Templates
{
    public class TemplateDetailsViewModel
    {
        public Guid Id { get; set; }

        public Guid ApplicationId { get; set; }

        public string ApplicationName { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string SubjectTemplate { get; set; }

        public string BodyTemplate { get; set; }

        public bool IsActive { get; set; }

        public string Status => IsActive ? "Active" : "Disabled";

        public string StatusCss => IsActive ? "label-success" : "label-danger";

        public IEnumerable<SelectListItem> Translations { get; set; }

        public static async Task<TemplateDetailsViewModel> LoadAsync(EmailServiceContext _ctx, Guid id)
        {
            var source = await _ctx.Templates
                .Include(t => t.Translations)
                .Include(t => t.Application)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (source != null)
            {
                return new TemplateDetailsViewModel
                {
                    Id = source.Id,
                    Name = source.Name,
                    Description = source.Description,
                    ApplicationId = source.ApplicationId,
                    ApplicationName = source.Application.Name,
                    BodyTemplate = source.BodyTemplate,
                    SubjectTemplate = source.SubjectTemplate,
                    IsActive = source.IsActive,
                    Translations = source.Translations.Select(t => new SelectListItem
                    {
                        Value = t.Language,
                        Text = t.GetCultureName()
                    })
                };
            }

            return null;
        }
    }
}
