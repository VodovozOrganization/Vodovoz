using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	/// <summary>
	/// Вид оплаты
	/// </summary>
	[Appellative(
		Nominative = "Вид оплаты",
		NominativePlural = "Виды оплат",
		GenitivePlural = "Видов оплат")]
	public enum PaymentType
	{
		/// <summary>
		/// Наличная
		/// </summary>
		[Display(Name = "Наличная", ShortName = "нал.")]
		Cash,
		/// <summary>
		/// Терминал
		/// </summary>
		[Display(Name = "Терминал", ShortName = "терм.")]
		Terminal,
		/// <summary>
		/// МП водителя (QR-код)
		/// </summary>
		[Display(Name = "МП водителя (QR-код)", ShortName = "МП вод.")]
		DriverApplicationQR,
		/// <summary>
		/// SMS (QR-код)
		/// </summary>
		[Display(Name = "SMS (QR-код)", ShortName = "смс qr")]
		SmsQR,
		/// <summary>
		/// Оплачено онлайн
		/// </summary>
		[Display(Name = "Оплачено онлайн", ShortName = "онлайн")]
		PaidOnline,
		/// <summary>
		/// Бартер
		/// </summary>
		[Display(Name = "Бартер", ShortName = "бар.")]
		Barter,
		/// <summary>
		/// Контрактная документация
		/// </summary>
		[Display(Name = "Контрактная документация", ShortName = "контрактн.")]
		ContractDocumentation,
		/// <summary>
		/// Безналичная
		/// </summary>
		[Display(Name = "Безналичная", ShortName = "б/н.")]
		Cashless,
	}
}
