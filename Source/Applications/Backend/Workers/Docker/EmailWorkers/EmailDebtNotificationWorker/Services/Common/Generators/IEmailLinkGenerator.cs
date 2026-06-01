using System;

namespace EmailDebtNotificationWorker.Services.Common.Generators
{
	public interface IEmailLinkGenerator
	{
		/// <summary>
		/// Получить ссылку для отписки
		/// </summary>
		/// <param name="guid">GUID сохраненного сообщения</param>
		/// <returns>Ссылка для отписки</returns>
		string GetUnsubscribeLink(Guid guid);
	}
}
