using System;
using System.Collections;
using System.Collections.Generic;

namespace EmailService.Web.Api.ViewModels
{
    public class TemplateListingViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Subject { get; set; }

        public IEnumerable<string> Translations { get; set; }
    }
}
