using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash
{
	public enum IncomeInvoiceDocumentType
	{
		[Display(Name = "Приходный ордер")]
		IncomeInvoice,
		[Display(Name = "Приходный ордер для документа перемещения ДС")]
		IncomeTransferDocument,
		[Display(Name = "Приходный ордер для самовывоза")]
		IncomeInvoiceSelfDelivery,
	}

}

