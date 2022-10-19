using System;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.Utilities.Text;
using Vodovoz.Domain.Fuel;

namespace Vodovoz.ViewModels.Journals.Nodes.Cash
{
	public class FuelDocumentJournalNode<TDocument> : FuelDocumentJournalNode
	{
		public FuelDocumentJournalNode() : base(typeof(TDocument))
		{
		}
	}
	
	public class FuelDocumentJournalNode : JournalEntityNodeBase
	{
		public FuelDocumentJournalNode(Type type) : base(type)
		{
		}
		
		public override string Title {
			get {
				if(EntityType == typeof(FuelIncomeInvoice)) {
					return "Входящая накладная";
				} else if(EntityType == typeof(FuelTransferDocument)) {
					return "Перемещение";
				} else if(EntityType == typeof(FuelWriteoffDocument)) {
					return "Акт выдачи";
				} else {
					return typeof(FuelTransferDocument).GetAttribute<AppellativeAttribute>(true)?.Nominative;
				}
			}
		}
		
		public DateTime CreationDate { get; set; }

		public FuelTransferDocumentStatuses TransferDocumentStatus { get; set; }
		public string Status {
			get {
				if(EntityType == typeof(FuelTransferDocument)) {
					return TransferDocumentStatus.GetEnumTitle();
				}
				return "";
			}
		}

		public decimal Liters { get; set; }
		public string SubdivisionFrom { get; set; }
		public string SubdivisionTo { get; set; }
		public DateTime? SendTime { get; set; }
		public DateTime? ReceiveTime { get; set; }

		public string AuthorSurname { get; set; }
		public string AuthorName { get; set; }
		public string AuthorPatronymic { get; set; }
		public string Author => PersonHelper.PersonNameWithInitials(AuthorSurname, AuthorName, AuthorPatronymic);

		public string EmployeeSurname { get; set; }
		public string EmployeeName { get; set; }
		public string EmployeePatronymic { get; set; }
		public string Employee => PersonHelper.PersonNameWithInitials(EmployeeSurname, EmployeeName, EmployeePatronymic);

		public string ExpenseCategory { get; set; }

		public string Comment { get; set; }
	}
}
