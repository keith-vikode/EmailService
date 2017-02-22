using Mustache;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace EmailService.Core.Templating
{
    public class MustacheTemplateTransformer : ITemplateTransformer
    {
        private static readonly Lazy<MustacheTemplateTransformer> _Instance =
            new Lazy<MustacheTemplateTransformer>(() => new MustacheTemplateTransformer(), true);

        private MustacheTemplateTransformer()
        {
        }

        public static MustacheTemplateTransformer Instance => _Instance.Value;

        public Task<EmailTemplate> TransformTemplateAsync(EmailTemplate template, object data, IFormatProvider formatter)
        {
            var transformed = new EmailTemplate(
                TransformText(template.Subject, data, formatter),
                TransformText(template.Body, data, formatter));

            return Task.FromResult(transformed);
        }

        public Task<string> TransformTextAsync(string template, object data, IFormatProvider formatter)
        {
            var transformed = TransformText(template, data, formatter);
            return Task.FromResult(transformed);
        }

        private static string TransformText(string text, object data, IFormatProvider formatter)
        {
            // if we don't have any data, or, if data is a dictionary, it has
            // no values, skip formatting and just return the original text
            if (data == null || (data as IDictionary)?.Count == 0)
            {
                return text;
            }

            var compiler = new FormatCompiler();
            var generator = compiler.Compile(text);
            return generator.Render(formatter, data);
        }
    }
}
