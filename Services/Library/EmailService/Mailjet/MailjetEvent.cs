using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EmailService.Mailjet
{
	[DataContract]
	public class MailjetEvent
	{
		#region Base properties

		/// <summary>
		/// the event type
		/// </summary>
		[DataMember(Name = "event")]
		public string Event { get; set; }

		/// <summary>
		/// unix timestamp of event
		/// </summary>
		[DataMember(Name = "time")]
		public long Time { get; set; }

		/// <summary>
		/// email address of recipient triggering the even
		/// </summary>
		[DataMember(Name = "email")]
		public string Email { get; set; }

		/// <summary>
		/// internal Mailjet campaign ID associated to the message
		/// </summary>
		[DataMember(Name = "mj_campaign_id")]
		public int MailjetCampaignID { get; set; }

		/// <summary>
		/// internal Mailjet contact ID
		/// </summary>
		[DataMember(Name = "mj_contact_id")]
		public int MailjetContactID { get; set; }

		/// <summary>
		/// value of the X-Mailjet-Campaign header when provided
		/// </summary>
		/// <value>The event.</value>
		[DataMember(Name = "customcampaign")]
		public string CustomCampaign { get; set; }

		/// <summary>
		/// The unique message ID
		/// </summary>
		[DataMember(Name = "MessageID")]
		public long MessageID { get; set; }

		/// <summary>
		/// the custom ID, when provided at send time
		/// </summary>
		[DataMember(Name = "CustomID")]
		public string CustomID { get; set; }

		/// <summary>
		/// the event payload, when provided at send time
		/// </summary>
		[DataMember(Name = "Payload")]
		public string Payload { get; set; }

		#endregion

		#region Sent event

		/// <summary>
		/// The unique message ID as a string (deprecated, see MessageID)
		/// </summary>
		[DataMember(Name = "mj_message_id")]
		public string MailJetMessageId { get; set; }

		/// <summary>
		/// The raw SMTP response message
		/// </summary>
		[DataMember(Name = "smtp_reply")]
		public string SmtpResponse { get; set; }

		#endregion

		#region ClickEvent

		/// <summary>
		/// the link that was clicked
		/// </summary>
		[DataMember(Name = "url")]
		public string Url { get; set; }

		#endregion

		#region BounceEvent

		/// <summary>
		/// true if this bounce leads to the recipient being blocked
		/// </summary>
		[DataMember(Name = "blocked")]
		public string Blocked { get; set; }

		/// <summary>
		/// true if error was permanent
		/// </summary>
		[DataMember(Name = "hard_bounce")]
		public string HardBounce { get; set; }

		#endregion

		#region BounceEvent and BlockedEvent

		/// <summary>
		/// error related to
		/// </summary>
		[DataMember(Name = "error_related_to")]
		public string ErrorRelatedTo { get; set; }

		/// <summary>
		/// error
		/// </summary>
		[DataMember(Name = "error")]
		public string Error { get; set; }

		#endregion

		#region SpamEvent

		/// <summary>
		/// indicates which feedback loop program reported this complaint
		/// </summary>
		[DataMember(Name = "source")]
		public string Source { get; set; }

		#endregion

		#region UnsubEvent

		/// <summary>
		/// internal Mailjet List id for REST API access to lists management
		/// </summary>
		[DataMember(Name = "mj_list_id")]
		public int MailjetListId { get; set; }

		#endregion

		#region OpenEvent and UnsubEvent

		/// <summary>
		/// IP address (can be IPv4 or IPv6) that triggered the event
		/// </summary>
		[DataMember(Name = "ip")]
		public string Ip { get; set; }

		/// <summary>
		/// country code of IP address
		/// </summary>
		[DataMember(Name = "geo")]
		public string Geo { get; set; }

		/// <summary>
		/// User-Agent
		/// </summary>
		[DataMember(Name = "agent")]
		public string Agent { get; set; }

		#endregion

		public string GetErrorInfo()
		{
			if(Error == null) {
				return null;
			}else {
				return string.Format("Error: {0}, Description: {1}", Error, ErrorDecription);
			}
		}

		public string ErrorDecription {
			get{
				if(Error == null) {
					return null;
				}
				if(ErrorsTable.ContainsKey(Error)) {
					return ErrorsTable[Error];
				}
				else {
					return "Error unknown";
				}
			}
		}

		public int AttemptCount { get; set; }

		public override string ToString()
		{
			string result = "";
			result += string.Format("{0}: {1}{2}", nameof(Event), Event, Environment.NewLine);
			result += string.Format("{0}: {1}{2}", nameof(Time), Time, Environment.NewLine);
			result += string.Format("{0}: {1}{2}", nameof(Email), Email, Environment.NewLine);
			result += string.Format("{0}: {1}{2}", nameof(MessageID), MessageID, Environment.NewLine);
			result += string.Format("{0}: {1}{2}", nameof(CustomID), CustomID, Environment.NewLine);
			result += string.Format("{0}: {1}{2}", nameof(Error), GetErrorInfo(), Environment.NewLine);
			return result;
		}

		static Dictionary<string, string> ErrorsTable = new Dictionary<string, string>();
		static MailjetEvent()
		{
			ErrorsTable.Add("user unknown", "Email address doesn't exist, double check it for typos !");
			ErrorsTable.Add("mailbox inactive", "Account has been inactive for too long (likely that it doesn't exist anymore");
			ErrorsTable.Add("quota exceeded", "Even though this is a non-permanent error, most of the time when accounts are over-quota, it means they are inactive.");
			ErrorsTable.Add("blacklisted", "You tried to send to a blacklisted recipient for this account.");
			ErrorsTable.Add("spam reporter", "You tried to send to a recipient that has reported a previous message from this account as spam.");
			ErrorsTable.Add("invalid domain", "There's a typo in the domain name part of the address. Or the address is so old that its domain has expired !");
			ErrorsTable.Add("no mail host", "Nobody answers when we knock at the door");
			ErrorsTable.Add("relay/access denied", "The destination mail server is refusing to talk to us.");
			ErrorsTable.Add("greylisted", "This is a temporary error due to possible unrecognised senders. Delivery will be re-attempted.");
			ErrorsTable.Add("typofix", "The domain part of your recipient email address was not valid.");
			ErrorsTable.Add("bad or empty template", "You should check that the template you are using has a content or is not corrupted.");
			ErrorsTable.Add("error in template language", "Your content contain a template language error , you can refer to the error reporting functionalities to get more information.");
			ErrorsTable.Add("sender blocked", "This is quite bad! You should contact us to investigate this issue.");
			ErrorsTable.Add("content blocked", "Something in your email has triggered an anti-spam filter and your email was rejected. Please contact us so we can review the email content and report any false positives.");
			ErrorsTable.Add("policy issue", "We do our best to avoid these errors with outbound throttling and following best practices. Although we do receive alerts when this happens, make sure to contact us for further information and a workaround");
			ErrorsTable.Add("system issue", "Something went wrong on our server-side. A temporary error. Please contact us if you receive an event of this type.");
			ErrorsTable.Add("protocol issue", "Something went wrong with our servers. This should not happen, and never be permanent !");
			ErrorsTable.Add("connection issue", "Something went wrong with our servers. This should not happen, and never be permanent !");
			ErrorsTable.Add("preblocked", "You tried to send an email to an address that recently (or repeatedly) bounced. We didn't try to send it to avoid damaging your reputation.");
			ErrorsTable.Add("duplicate in campaign", "You used X-Mailjet-DeduplicateCampaign and sent more than one email to a single recipient. Only the first email was sent; the others were blocked.");
		}
	}
}
