using System;

namespace Vodovoz.Settings.Counterparty
{
	/// <summary>
	/// Настройки напоминаний об оплате по заказам сетей и тендерных клиентов
	/// </summary>
	public interface IChainStoreDelayLettersSettings
	{
		/// <summary>
		/// Включена ли отправка писем
		/// </summary>
		bool SendEnabled { get; }

		/// <summary>
		/// Интервал работы воркера писем
		/// </summary>
		TimeSpan SendInterval { get; }

		/// <summary>
		/// Время начала окна отправки писем
		/// </summary>
		TimeSpan SendTimeFrom { get; }

		/// <summary>
		/// Время конца окна отправки писем
		/// </summary>
		TimeSpan SendTimeTo { get; }

		/// <summary>
		/// За сколько дней до расчётной даты платежа отправляется письмо
		/// </summary>
		int DaysBeforePaymentDate { get; }

		/// <summary>
		/// Контактный телефон для текста письма
		/// </summary>
		string ContactPhone { get; }
	}
}
