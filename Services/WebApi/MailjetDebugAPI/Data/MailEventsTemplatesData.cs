using Mailjet.Api.Abstractions.Events;
using System;
using System.Collections.Generic;

namespace MailjetDebugAPI.Data
{
	public static class MailEventsTemplatesData
	{
		private static long _currentTimestamp => DateTimeOffset.Now.ToLocalTime().ToUnixTimeSeconds();

		public static MailSentEvent SentTemplate =>
		new MailSentEvent
		{
			EventType = MailEventType.sent,
			Time = _currentTimestamp,
			MessageId = 19421777835146490,
			MessageGuid = Guid.NewGuid().ToString(),
			EmailAddress = "api@mailjet.com",
			MailjetCampaignId = 7257,
			MailjetContactId = 4,
			CustomCampaign = "",
			MailjetMessageId = "19421777835146490",
			SmtpReply = "sent (250 2.0.0 OK 1433333948 fa5si855896wjc.199 - gsmtp)",
			CustomId = "helloworld",
			Payload = PayloadTemplate
		};

		public static MailOpenEvent OpenTemplate =>
		new MailOpenEvent
		{
			EventType = MailEventType.open,
			Time = _currentTimestamp,
			MessageId = 19421777396190490,
			MessageGuid = Guid.NewGuid().ToString(),
			EmailAddress = "api@mailjet.com",
			MailjetCampaignId = 7173,
			MailjetContactId = 320,
			CustomCampaign = "",
			CustomId = "helloworld",
			Payload = PayloadTemplate,
			IpAddress = "127.0.0.1",
			Geo = "US",
			Agent = "Mozilla/5.0 (Windows NT 5.1; rv:11.0) Gecko Firefox/11.0"
		};

		public static MailClickEvent ClickTemplate =>
		new MailClickEvent
		{
			EventType = MailEventType.click,
			Time = _currentTimestamp,
			MessageId = 19421777396190490,
			MessageGuid = Guid.NewGuid().ToString(),
			EmailAddress = "api@mailjet.com",
			MailjetCampaignId = 7272,
			MailjetContactId = 4,
			CustomCampaign = "",
			CustomId = "helloworld",
			Payload = PayloadTemplate,
			Url = "https://mailjet.com",
			IpAddress = "127.0.0.1",
			Geo = "FR",
			Agent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_0) AppleWebKit/537.36"
		};

		public static MailBounceEvent BounceTemplate =>
		new MailBounceEvent
		{
			EventType = MailEventType.bounce,
			Time = _currentTimestamp,
			MessageId = 13792286917004336,
			MessageGuid = Guid.NewGuid().ToString(),
			EmailAddress = "bounce@mailjet.com",
			MailjetCampaignId = 0,
			MailjetContactId = 0,
			CustomCampaign = "",
			CustomId = "helloworld",
			Payload = PayloadTemplate,
			Blocked = false,
			HardBounce = true,
			ErrorRelatedTo = "recipient",
			Error = "user unknown",
			Comment = "Host or domain name not found. Name service error for name=lbjsnrftlsiuvbsren.com type=A: Host not found"
		};

		public static MailBlockedEvent BlockedTemplate =>
		new MailBlockedEvent
		{
			EventType = MailEventType.blocked,
			Time = _currentTimestamp,
			MessageId = 13792286917004336,
			MessageGuid = Guid.NewGuid().ToString(),
			EmailAddress = "bounce@mailjet.com",
			MailjetCampaignId = 0,
			MailjetContactId = 0,
			CustomCampaign = "",
			CustomId = "helloworld",
			Payload = PayloadTemplate,
			ErrorRelatedTo = "recipient",
			Error = "user unknown"
		};

		public static MailSpamEvent SpamTemplate =>
		new MailSpamEvent
		{
			EventType = MailEventType.spam,
			Time = _currentTimestamp,
			MessageId = 13792286917004336,
			MessageGuid = Guid.NewGuid().ToString(),
			EmailAddress = "bounce@mailjet.com",
			MailjetCampaignId = 0,
			MailjetContactId = 0,
			CustomCampaign = "",
			CustomId = "helloworld",
			Payload = PayloadTemplate,
			Source = "JMRPP"
		};

		public static MailUnsubscribeEvent UnsubscribeTemplate =>
		new MailUnsubscribeEvent
		{
			EventType = MailEventType.unsub,
			Time = _currentTimestamp,
			MessageId = 20547674933128000,
			MessageGuid = Guid.NewGuid().ToString(),
			EmailAddress = "api@mailjet.com",
			MailjetCampaignId = 7276,
			MailjetContactId = 126,
			CustomCampaign = "",
			CustomId = "helloworld",
			Payload = PayloadTemplate,
			MailjetListId = 1,
			IpAddress =  "127.0.0.1",
			Geo = "FR",
			Agent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36"
		};

		public static Dictionary<string, string> PossibleErrors =>
		new Dictionary<string, string>
		{
			{ "recipient", "user unknown" },
			{ "mailbox inactive", "Account has been inactive for too long (likely that it doesn't exist anymore)." },
			{ "quota exceeded", "Even though this is a non-permanent error, most of the time when accounts are over-quota, it means they are inactive." },
			{ "blacklisted", "You tried to send to a blacklisted recipient for this account." },
			{ "spam reporter", "You tried to send to a recipient that has reported a previous message from this account as spam." },
			{ "domain", "invalid domain" },
			{ "no mail host", "Nobody answers when we knock at the door." },
			{ "relay/access denied", "The destination mail server is refusing to talk to us." },
			{ "greylisted", "This is a temporary error due to possible unrecognized senders. Delivery will be re-attempted." },
			{ "typofix", "The domain part of your recipient email address was not valid." },
			{ "content", "bad or empty template" },
			{ "error in template language", "Your content contains a template language error, you can refer to the error reporting functionalities to get more information." },
			{ "spam", "sender blocked" },
			{ "content blocked", "Something in your email has triggered an anti-spam filter and your email was rejected. Please contact us so we can review the email content and report any false positives." },
			{ "policy issue", "We do our best to avoid these errors with outbound throttling and following best practices. Although we do receive alerts when this happens, make sure to contact us for further information and a workaround" },
			{ "system", "system issue" },
			{ "protocol issue", "Something went wrong with our servers. This should not happen, and never be permanent !" },
			{ "connection issue", "Something went wrong with our servers. This should not happen, and never be permanent !" },
			{ "mailjet", "preblocked" },
			{ "duplicate in campaign", "You used X-Mailjet-DeduplicateCampaign and sent more than one email to a single recipient. Only the first email was sent; the others were blocked." },
		};

		public static string PayloadTemplate =>
			"{\n" +
			"\t\"Id\": 0,\n" +
			"\t\"Trackable\": false,\n" +
			"\t\"InstanceId\": 0\n" +
			"}";
	}
}
