using System;

namespace EmailService.Core
{
    /// <summary>
    /// Defines the result of an operation from <see cref="EmailSender"/>. 
    /// </summary>
    public class EmailSenderResult
    {
        public static class ErrorCodes
        {
            public const string Unhandled = "F_UNHANDLED";
            public const string TemplateNotFound = "E_INVALID_TEMPLATE";
            public const string MissingBody = "E_MISSING_BODY";
            public const string MissingRecipient = "E_MISSING_RECIPIENT";
        }

        /// <summary>
        /// Success result.
        /// </summary>
        public static readonly EmailSenderResult Success = new EmailSenderResult(true);

        private EmailSenderResult(bool succeeded)
        {
            Succeeded = succeeded;
        }

        private EmailSenderResult(string errorMessage, string errorCode, Exception exception = null)
        {
            Succeeded = false;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
            Exception = exception;
        }

        /// <summary>
        /// Gets a value indicating whether the operation succeeded.
        /// </summary>
        public bool Succeeded { get; }

        /// <summary>
        /// Gets an error message if the operation failed.
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Gets the error code if the operation failed.
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// Gets the exception if this represents a fault.
        /// </summary>
        public Exception Exception { get; }

        public static EmailSenderResult Error(string message, string code)
        {
            return new EmailSenderResult(message, code);
        }

        public static EmailSenderResult Fault(Exception ex)
        {
            return new EmailSenderResult(ex.GetBaseException().Message, ErrorCodes.Unhandled, ex);
        }
    }
}
