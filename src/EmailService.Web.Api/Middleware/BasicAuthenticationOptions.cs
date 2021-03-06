﻿using Microsoft.AspNetCore.Builder;
using System;

namespace EmailService.Web.Api.Middleware
{
    /// <summary>
    /// Contains the options used by the BasicAuthenticationMiddleware
    /// </summary>
    public class BasicAuthenticationOptions : AuthenticationOptions
    {
        private string _realm;

        /// <summary>
        /// Create an instance of the options initialized with the default values
        /// </summary>
        public BasicAuthenticationOptions() : base()
        {
            AuthenticationScheme = "Basic";
            AutomaticAuthenticate = true;
            AutomaticChallenge = true;
        }

        /// <summary>
        /// Gets or sets the Realm sent in the WWW-Authenticate header.
        /// </summary>
        /// <remarks>
        /// The realm value (case-sensitive), in combination with the canonical root URL 
        /// of the server being accessed, defines the protection space. 
        /// These realms allow the protected resources on a server to be partitioned into a 
        /// set of protection spaces, each with its own authentication scheme and/or 
        /// authorization database. 
        /// </remarks>
        public string Realm
        {
            get
            {
                return _realm;
            }

            set
            {
                if (!IsAscii(value))
                {
                    throw new ArgumentOutOfRangeException("Realm", "Realm must be US ASCII");
                }

                _realm = value;
            }
        }

        private bool IsAscii(string input)
        {
            foreach (char c in input)
            {
                if (c < 32 || c >= 127)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
