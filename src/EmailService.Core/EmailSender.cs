using EmailService.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Core
{
    /// <summary>
    /// Provides the ability to send emails.
    /// </summary>
    /// <remarks>
    /// <para>
    ///     Emailing requires three components:
    /// </para>
    /// <list type="bullet">
    ///     <item>sender - sends the emails via SMTP or some other API</item>
    ///     <item>template store - loads the template text</item>
    ///     <item>template transformer - formats the email template using the supplied data</item>
    /// </list>
    /// <para>
    ///     The object stores no state, and is cheap to construct.
    /// </para>
    /// </remarks>
    public class EmailSender
    {
        private IEmailTransport _sender;
        private ITemplateStore _templateStore;
        private ITemplateTransformer _templateTransformer;

        /// <summary>
        /// Initializes a new instance of the EmailManager class.
        /// </summary>
        /// <param name="sender">Object that sends emails</param>
        /// <param name="templateStore">Object to load email templates</param>
        /// <param name="templateTransformer">Object to transform email templates</param>
        public EmailSender(IEmailTransport sender, ITemplateStore templateStore, ITemplateTransformer templateTransformer)
        {
            _sender = sender;
            _templateStore = templateStore;
            _templateTransformer = templateTransformer;
        }

        /// <summary>
        /// Sends a new email.
        /// </summary>
        /// <param name="args">Email recipient and template arguments</param>
        /// <returns>The result of the sending request.</returns>
        public async Task<EmailSenderResult> SendEmailAsync(EmailParams args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var result = EmailSenderResult.Success;

            try
            {
                var culture = CultureInfo.InvariantCulture;

                if (!string.IsNullOrEmpty(args.Culture))
                {
                    culture = new CultureInfo(args.Culture);
                }

                var template = await _templateStore.LoadTemplateAsync(args.Template, culture);
                if (template != null)
                {
                    template = await _templateTransformer.TransformTemplateAsync(template, args.Data);
                    await _sender.SendAsync(new SenderParams
                    {
                        To = args.To,
                        CC = args.CC,
                        Subject = template.Subject,
                        Body = template.Body,
                        SenderEmail = args.SenderEmail,
                        SenderName = args.SenderName
                    });
                }
                else
                {
                    result = EmailSenderResult.Error($"Invalid template '{args.Template}'", EmailSenderResult.ErrorCodes.TemplateNotFound);
                }
            }
            catch (Exception ex)
            {
                result = EmailSenderResult.Fault(ex);
            }

            return result;
        }

        /// <summary>
        /// Sends an email using arbitrary values, with no template transformation.
        /// </summary>
        /// <param name="to">Recipients - separate multiple recipients with a comma</param>
        /// <param name="subject">Email subject text</param>
        /// <param name="body">Email body text (will be sent as HTML)</param>
        /// <param name="cc">CC list - separate multiple recipients with a comma</param>
        /// <param name="senderEmail">Email address of the sender (uses default if blank)</param>
        /// <param name="senderName">Name of the sender (uses default if blank)</param>
        /// <returns>A <see cref="StoreResult"/> representing the outcome of the task.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 'to' is a required parameter
        /// or
        /// 'body' is a required parameter
        /// </exception>
        public async Task<EmailSenderResult> SendEmailAsync(
            string to,
            string subject,
            string body,
            string cc = null,
            string senderEmail = null,
            string senderName = null)
        {
            if (string.IsNullOrWhiteSpace(to))
            {
                return EmailSenderResult.Error("'to' is a required parameter", EmailSenderResult.ErrorCodes.MissingRecipient);
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return EmailSenderResult.Error("'body' is a required parameter", EmailSenderResult.ErrorCodes.MissingBody);
            }

            var args = new SenderParams
            {
                Subject = subject,
                Body = body,
                To = to?.Split(',').ToList() ?? new List<string>(),
                CC = cc?.Split(',').ToList() ?? new List<string>(),
                SenderEmail = senderEmail,
                SenderName = senderName
            };

            try
            {
                await _sender.SendAsync(args);
                return EmailSenderResult.Success;
            }
            catch (Exception ex)
            {
                return EmailSenderResult.Fault(ex);
            }
        }
    }
}
