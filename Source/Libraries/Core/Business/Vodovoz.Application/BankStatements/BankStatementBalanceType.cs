using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Application.BankStatements
{
	/// <summary>
	/// Начала строк с исходящим балансом расчетного счета
	/// </summary>
	public enum BankStatementBalanceType
	{
		[Display(Name = "исходящий остаток")]
		OutgoingBalance,
		[Display(Name = "остаток исходящий")]
		BalanceOutgoing,
		[Display(Name = "исх. сальдо")]
		OutSaldo,
		[Display(Name = "исходящее сальдо")]
		OutgoingSaldo
	}
}
