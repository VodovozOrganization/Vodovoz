using System;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Client;
using Gdk;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Orders;
using System.Collections.Generic;

namespace Vodovoz.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "жалобы",
		Nominative = "жалоба",
		Prepositional = "жалобе",
		PrepositionalPlural = "жалобах"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class Complaint : BusinessObjectBase<Complaint>, IDomainObject, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public virtual int Id { get; set; }

		private Counterparty counterparty;
		[Display(Name = "Клиент")]
		public virtual Counterparty Counterparty {
			get => counterparty;
			set => SetField(ref counterparty, value, () => Counterparty);
		}

		private string complainantName;
		[Display(Name = "Имя заявителя жалобы")]
		public virtual string ComplainantName {
			get => complainantName;
			set => SetField(ref complainantName, value, () => ComplainantName);
		}

		private string complaintText;
		[Display(Name = "Текст жалобы")]
		public virtual string ComplaintText {
			get => complaintText;
			set => SetField(ref complaintText, value, () => ComplaintText);
		}

		private Order order;
		[Display(Name = "Текст жалобы")]
		public virtual Order Order {
			get => order;
			set => SetField(ref order, value, () => Order);
		}

		private string phone;
		[Display(Name = "Телефон")]
		public virtual string Phone {
			get => phone;
			set => SetField(ref phone, value, () => Phone);
		}

		private ComplaintSource complaintSource;
		[Display(Name = "Источник")]
		public virtual ComplaintSource ComplaintSource {
			get => complaintSource;
			set => SetField(ref complaintSource, value, () => ComplaintSource);
		}

		private ComplaintStatuses status;
		[Display(Name = "Статус")]
		public virtual ComplaintStatuses Status {
			get => status;
			set => SetField(ref status, value, () => Status);
		}

		#region IValidatableObject implementation

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(ComplaintSource == null) {
				yield return new ValidationResult("Необходимо выбрать источник");
			}

			if(string.IsNullOrWhiteSpace(ComplaintText)) {
				yield return new ValidationResult("Необходимо ввести текст жалобы");
			}
		}

		#endregion IValidatableObject implementation
	}
}
