using EmailService.Core.Services;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace EmailService.Core
{
    /// <summary>
    /// Implements a dummy email sender service by writing the contents of the message to the console window.
    /// </summary>
    public sealed class DebugSender : IEmailTransport
    {
        private static readonly Lazy<DebugSender> _Instance = new Lazy<DebugSender>(() =>
        {
            return new DebugSender();
        }, true);

        private DebugSender()
        {
        }

        public static DebugSender Instance => _Instance.Value;

        public Task<bool> SendAsync(SenderParams args)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(JsonConvert.SerializeObject(args, Formatting.Indented));
            Console.ForegroundColor = currentColor;

            return Task.FromResult(true);
        }
    }
}
