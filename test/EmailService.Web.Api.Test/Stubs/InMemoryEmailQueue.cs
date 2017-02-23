using EmailService.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailService.Core;
using System.Threading;

namespace EmailService.Web.Api.Test.Stubs
{
    public class InMemoryEmailQueue : IEmailQueueSender, IEmailQueueReceiver<BasicEmailQueueMessage>
    {
        private static readonly Lazy<Queue<BasicEmailQueueMessage>> _Queue = new Lazy<Queue<BasicEmailQueueMessage>>(() => new Queue<BasicEmailQueueMessage>(), false);
        private static readonly Lazy<Queue<BasicEmailQueueMessage>> _Poison = new Lazy<Queue<BasicEmailQueueMessage>>(() => new Queue<BasicEmailQueueMessage>(), false);

        public InMemoryEmailQueue()
        {
        }

        public static Queue<BasicEmailQueueMessage> Queue => _Queue.Value;

        public static Queue<BasicEmailQueueMessage> Poison => _Poison.Value;

        public int MaxMessagesToRetrieve => 1;

        public Task CompleteAsync(BasicEmailQueueMessage message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var items = Queue.ToList();
            Queue.Clear();
            foreach (var item in items)
            {
                if (item.MessageId != message.MessageId)
                {
                    Queue.Enqueue(item);
                }
            }

            return Task.FromResult(0);
        }

        public Task MoveToPoisonQueueAsync(BasicEmailQueueMessage message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Poison.Enqueue(message);
            return Task.FromResult(0);
        }

        public Task<IEnumerable<BasicEmailQueueMessage>> ReceiveAsync(int number, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var list = new List<BasicEmailQueueMessage>();
            for (int i = 0; i < number; i++)
            {
                if (Queue.Count == 0)
                {
                    break;
                }

                list.Add(Queue.Peek());
            }

            return Task.FromResult(list.AsEnumerable());
        }

        public Task SendAsync(EmailQueueToken token, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var message = new BasicEmailQueueMessage { Token = token, DequeueCount = 0 };
            Queue.Enqueue(message);
            return Task.FromResult(0);
        }
    }
}