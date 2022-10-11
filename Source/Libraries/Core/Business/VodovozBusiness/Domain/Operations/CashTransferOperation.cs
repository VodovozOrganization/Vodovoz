using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Cash.CashTransfer;

namespace Vodovoz.Domain.Operations
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "операции транспортировки денег",
		Nominative = "операция транспортировки денег")]
	public class CashTransferOperation : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private CashTransferDocumentBase cashTransferDocument;
		[Display(Name = "Документ транспортировки денежных средств")]
		public CashTransferDocumentBase CashTransferDocument {
			get => cashTransferDocument;
			set => SetField(ref cashTransferDocument, value, () => CashTransferDocument);
		}

		private Subdivision subdivisionFrom;
		[Display(Name = "Касса отправитель")]
		public Subdivision SubdivisionFrom {
			get => subdivisionFrom;
			set => SetField(ref subdivisionFrom, value, () => SubdivisionFrom);
		}

		private Subdivision subdivisionTo;
		[Display(Name = "Касса получатель")]
		public Subdivision SubdivisionTo {
			get => subdivisionTo;
			set => SetField(ref subdivisionTo, value, () => SubdivisionTo);
		}

		private decimal transferedSum;
		[Display(Name = "Транспортируемая сумма")]
		public decimal TransferedSum {
			get => transferedSum;
			set => SetField(ref transferedSum, value, () => TransferedSum);
		}

		private DateTime sendTime;
		[Display(Name = "Дата отправки")]
		public DateTime SendTime {
			get => sendTime;
			set => SetField(ref sendTime, value, () => SendTime);
		}

		private DateTime? receiveTime;
		[Display(Name = "Дата получения")]
		public DateTime? ReceiveTime {
			get => receiveTime;
			set => SetField(ref receiveTime, value, () => ReceiveTime);
		}
	}
}
