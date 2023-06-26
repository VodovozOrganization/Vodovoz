using QS.Project.Journal;
using QS.Utilities.Text;
using System;
using Vodovoz.Domain.Cash.CashTransfer;

namespace Vodovoz.ViewModels.Cash.TransferDocumentsJournal
{
	public class DocumentNode : JournalEntityNodeBase
	{
		public override string Title => Name;

		public string Name
		{
			get
			{
				if(EntityType == typeof(IncomeCashTransferDocument))
				{
					return "По ордерам";
				}
				else if(EntityType == typeof(CommonCashTransferDocument))
				{
					return "На сумму";
				}
				else
				{
					return "Перемещение д/с";
				}
			}
		}

		public DateTime CreationDate { get; set; }
		public CashTransferDocumentStatuses Status { get; set; }

		public string AuthorSurname { get; set; }
		public string AuthorName { get; set; }
		public string AuthorPatronymic { get; set; }
		public string AuthorShortFullName => PersonHelper.PersonNameWithInitials(AuthorSurname, AuthorName, AuthorPatronymic);

		public decimal TransferedSum { get; set; }
		public string SubdivisionFrom { get; set; }
		public string SubdivisionTo { get; set; }
		public DateTime? SendTime { get; set; }
		public DateTime? ReceiveTime { get; set; }

		public string CashierSenderSurname { get; set; }
		public string CashierSenderName { get; set; }
		public string CashierSenderPatronymic { get; set; }
		public string CashierSenderShortFullName => PersonHelper.PersonNameWithInitials(CashierSenderSurname, CashierSenderName, CashierSenderPatronymic);

		public string CashierReceiverSurname { get; set; }
		public string CashierReceiverName { get; set; }
		public string CashierReceiverPatronymic { get; set; }
		public string CashierReceiverShortFullName => PersonHelper.PersonNameWithInitials(CashierReceiverSurname, CashierReceiverName, CashierReceiverPatronymic);

		public string Comment { get; set; }
	}
}
