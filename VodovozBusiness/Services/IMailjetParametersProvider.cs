using System;
namespace Vodovoz.Services
{
	public interface IMailjetParametersProvider
	{
		string MailjetUserId { get; }
		string MailjetSecretKey { get; }
	}
}
