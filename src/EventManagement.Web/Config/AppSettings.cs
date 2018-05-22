﻿using System;
namespace losol.EventManagement.Config
{
	public class AppSettings
	{
		public EmailProvider EmailProvider { get; set; }
		public SmsProvider SmsProvider { get; set; }
		public bool UsePowerOffice { get; set; }
        public bool UseStripeInvoice { get; set; }
	}

	public enum EmailProvider
	{
		SendGrid,
		SMTP,
		File,
		Mock
	}

	public enum SmsProvider
	{
		Twilio,
		Mock
	}
}
