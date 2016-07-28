using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Web.ViewModels
{
    public static class Cultures
    {
        private static readonly Lazy<IEnumerable<CultureInfo>> _AllCultures =
            new Lazy<IEnumerable<CultureInfo>>(() =>
            {
                return new List<CultureInfo>
                {
                    // English
                    new CultureInfo("en-GB"),
                    new CultureInfo("en-US"),
                    new CultureInfo("en-CA"),
                    new CultureInfo("en-AU"),

                    // European
                    new CultureInfo("fr-FR"),
                    new CultureInfo("de-DE"),
                    new CultureInfo("es-ES"),
                    new CultureInfo("it-IT"),
                    new CultureInfo("el-GR"),
                    new CultureInfo("nl-NL"),
                    new CultureInfo("pt-PT"),
                    new CultureInfo("pl-PL"),
                    new CultureInfo("ro-RO"),
                    new CultureInfo("ru-RU"),
                    new CultureInfo("sv-SE"),
                    new CultureInfo("da-DK"),

                    // Middle East
                    new CultureInfo("he-IL"),
                    new CultureInfo("ar-EG"),

                    // Asian
                    new CultureInfo("hi-IN"),
                    new CultureInfo("pa-IN"),
                    new CultureInfo("gu-IN"),
                    new CultureInfo("ur-PK"),
                    new CultureInfo("ja-JP"),
                    new CultureInfo("zh-CN"),
                    new CultureInfo("ko-KR")
                };
            }, true);

        public static IEnumerable<CultureInfo> AllCultures => _AllCultures.Value;
    }
}
