using System;

namespace Email.Infrastructure.Generators
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
