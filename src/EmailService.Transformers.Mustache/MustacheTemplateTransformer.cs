using EmailService.Core;
using EmailService.Core.Services;
using Mustache;
using System.Threading.Tasks;

namespace EmailService.Transformers.Mustache
{
    public class MustacheTemplateTransformer : ITemplateTransformer
    {
        public Task<EmailTemplate> TransformTemplateAsync(EmailTemplate template, object data)
        {
            var transformed = new EmailTemplate(
                TransformText(template.Subject, data),
                TransformText(template.Body, data));

            return Task.FromResult(transformed);
        }

        private static string TransformText(string text, object data)
        {
            var compiler = new FormatCompiler();
            var generator = compiler.Compile(text);
            return generator.Render(data);
        }
    }
}
