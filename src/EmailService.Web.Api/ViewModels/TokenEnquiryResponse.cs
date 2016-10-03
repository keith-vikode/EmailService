﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace EmailService.Web.Api.ViewModels
{
    public class TokenEnquiryResponse
    {
        public Guid Token { get; set; }

        public DateTime Submitted { get; set; }

        public DateTime? Processed { get; set; }

        public int DequeueCount { get; set; }

        public string Status { get; set; } // TODO: use enum here
    }
}
