using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Domain.Client;

namespace EmailDebtNotificationWorker.Services.Common.Selectors
{
	public interface IClientEmailSelector
	{
		/// <summary>
		/// Получить почту для рассылки
		/// </summary>
		/// <param name="client">Клиент</param>
		/// <param name="purpose">Предназначение почты</param>
		/// <returns>Почта для рассылки</returns>
		string? SelectEmailForDebtNotification(Counterparty client, EmailPurpose? purpose = null);
	}
}
