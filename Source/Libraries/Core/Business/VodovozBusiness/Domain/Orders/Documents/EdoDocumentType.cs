using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders.Documents
{
	/// <summary>
	/// WS - используется потому, что enum в NHibernate не может быть более 36 символов для 1 значения
	/// </summary>
	public enum EdoDocumentType
	{
		[Display(Name = "УПД")]
		Upd,
		[Display(Name = "Счёт")]
		Bill,
		[Display(Name = "Счет без отгрузки на предоплату")]
		BillWSForAdvancePayment,
		[Display(Name = "Cчет без отгрузки на долг")]
		BillWSForDebt,
		[Display(Name = "Cчет без отгрузки на постоплату")]
		BillWSForPayment
	}
}
