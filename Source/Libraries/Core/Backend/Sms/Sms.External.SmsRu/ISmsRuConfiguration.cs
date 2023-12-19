namespace Sms.External.SmsRu
{
	public interface ISmsRuConfiguration
	{
		string ApiId { get; }

		string Email { get; }

		string EmailToSmsGateEmail { get; }

		string Login { get; }

		string PartnerId { get; }

		string Password { get; }

		string SmtpLogin { get; }

		string SmtpPassword { get; }

		int SmtpPort { get; }

		string SmtpServer { get; }

		bool SmtpUseSSL { get; }

		bool Test { get; }

		bool Translit { get; }

		string SmsNumberFrom { get; }

		string BaseUrl { get; }
	}
}
