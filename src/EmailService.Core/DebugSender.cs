using EmailService.Core.Services;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace EmailService.Core
{
    /// <summary>
    /// Implements a dummy email sender service by writing the contents of the message to the console window.
    /// </summary>
    public class DebugSender : IEmailTransport
    {
        public Task SendAsync(SenderParams args)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(JsonConvert.SerializeObject(args, Formatting.Indented));
            Console.ForegroundColor = currentColor;

            return Task.FromResult(0);
        }
    }
}
