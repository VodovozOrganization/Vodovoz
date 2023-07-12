using Vodovoz.Domain.Cash.FinancialCategoriesGroups;

namespace Vodovoz.Domain.Cash
{
	public static class DocumentTypesExtensions
	{
		public static TargetDocument? ToTargetDocument(this IncomeInvoiceDocumentType incomeInvoiceDocumentType)
		{
			switch(incomeInvoiceDocumentType)
			{
				case IncomeInvoiceDocumentType.IncomeInvoice:
					return TargetDocument.Invoice;
				case IncomeInvoiceDocumentType.IncomeTransferDocument:
					return TargetDocument.Transfer;
				case IncomeInvoiceDocumentType.IncomeInvoiceSelfDelivery:
					return TargetDocument.SelfDelivery;
				default:
					return null;
			}
		}

		public static TargetDocument? ToTargetDocument(this ExpenseInvoiceDocumentType expenseInvoiceDocumentType)
		{
			switch(expenseInvoiceDocumentType)
			{
				case ExpenseInvoiceDocumentType.ExpenseInvoice:
					return TargetDocument.Invoice;
				case ExpenseInvoiceDocumentType.ExpenseTransferDocument:
					return TargetDocument.Transfer;
				case ExpenseInvoiceDocumentType.ExpenseInvoiceSelfDelivery:
					return TargetDocument.SelfDelivery;
				default:
					return null;
			}
		}

		public static IncomeInvoiceDocumentType? ToIncomeInvoiceDocument(this TargetDocument targetDocument)
		{
			switch(targetDocument)
			{
				case TargetDocument.Invoice:
					return IncomeInvoiceDocumentType.IncomeInvoice;
				case TargetDocument.SelfDelivery:
					return IncomeInvoiceDocumentType.IncomeInvoiceSelfDelivery;
				case TargetDocument.Transfer:
					return IncomeInvoiceDocumentType.IncomeTransferDocument;
				default:
					return null;
			}
		}

		public static ExpenseInvoiceDocumentType? ToExpenseInvoiceDocument(this TargetDocument targetDocument)
		{
			switch(targetDocument)
			{
				case TargetDocument.Invoice:
					return ExpenseInvoiceDocumentType.ExpenseInvoice;
				case TargetDocument.SelfDelivery:
					return ExpenseInvoiceDocumentType.ExpenseInvoiceSelfDelivery;
				case TargetDocument.Transfer:
					return ExpenseInvoiceDocumentType.ExpenseTransferDocument;
				default:
					return null;
			}
		}
	}
}
