using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Payments
{
	/// <summary>
	/// Тип данных из выписки
	/// </summary>
	public enum BankAccountMovementDataType
	{
		/// <summary>
		/// Входящий остаток
		/// </summary>
		[Display(Name = "Входящий остаток")]
		InitialBalance,
		/// <summary>
		/// Кредит(всего поступило)
		/// </summary>
		[Display(Name = "Кредит")]
		TotalReceived,
		/// <summary>
		/// Дебет(всего списано)
		/// </summary>
		[Display(Name = "Дебет")]
		TotalWrittenOff,
		/// <summary>
		/// Исходящий остаток
		/// </summary>
		[Display(Name = "Исходящий остаток")]
		FinalBalance
	}
}
