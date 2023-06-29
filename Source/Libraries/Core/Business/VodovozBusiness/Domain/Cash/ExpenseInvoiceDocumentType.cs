using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash
{
	public enum ExpenseInvoiceDocumentType
	{
		[Display(Name = "Расходный ордер")]
		ExpenseInvoice,
		[Display(Name = "Расходный ордер для документа перемещения ДС")]
		ExpenseTransferDocument,
		[Display(Name = "Расходный ордер для самовывоза")]
		ExpenseInvoiceSelfDelivery,
	}
}
