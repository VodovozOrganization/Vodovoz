using System;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Client;
using Gdk;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Orders;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;
using System.Data.Bindings.Collections.Generic;

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

		private Employee createdBy;
		[Display(Name = "Кем создан")]
		public virtual Employee CreatedBy {
			get => createdBy;
			set => SetField(ref createdBy, value, () => CreatedBy);
		}

		private DateTime creationDate;
		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate {
			get => creationDate;
			set => SetField(ref creationDate, value, () => CreationDate);
		}

		private Employee changedBy;
		[Display(Name = "Кем изменен")]
		public virtual Employee ChangedBy {
			get => changedBy;
			set => SetField(ref changedBy, value, () => ChangedBy);
		}

		private DateTime changedDate;
		[Display(Name = "Дата изменения")]
		public virtual DateTime ChangedDate {
			get => changedDate;
			set => SetField(ref changedDate, value, () => ChangedDate);
		}

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
		[Display(Name = "Заказ")]
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

		private DateTime plannedCompletionDate;
		[Display(Name = "Дата планируемого завершения")]
		public virtual DateTime PlannedCompletionDate {
			get => plannedCompletionDate;
			set => SetField(ref plannedCompletionDate, value, () => PlannedCompletionDate);
		}

		private string resultText;
		[Display(Name = "Результат")]
		public virtual string ResultText {
			get => resultText;
			set => SetField(ref resultText, value, () => ResultText);
		}

		private ComplaintResult complaintResult;
		[Display(Name = "Результат")]
		public virtual ComplaintResult ComplaintResult {
			get => complaintResult;
			set => SetField(ref complaintResult, value, () => ComplaintResult);
		}

		IList<Fine> fines = new List<Fine>();
		[Display(Name = "Штрафы")]
		public virtual IList<Fine> Fines {
			get => fines;
			set => SetField(ref fines, value, () => Fines);
		}

		GenericObservableList<Fine> observableFines;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Fine> ObservableFines {
			get {
				if(observableFines == null)
					observableFines = new GenericObservableList<Fine>(Fines);
				return observableFines;
			}
		}

		IList<ComplaintDiscussion> complaintDiscussions = new List<ComplaintDiscussion>();
		[Display(Name = "Обсуждения")]
		public virtual IList<ComplaintDiscussion> ComplaintDiscussions {
			get => complaintDiscussions;
			set => SetField(ref complaintDiscussions, value, () => ComplaintDiscussions);
		}

		GenericObservableList<ComplaintDiscussion> observableComplaintDiscussions;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ComplaintDiscussion> ObservableComplaintDiscussions {
			get {
				if(observableComplaintDiscussions == null)
					observableComplaintDiscussions = new GenericObservableList<ComplaintDiscussion>(ComplaintDiscussions);
				return observableComplaintDiscussions;
			}
		}

		IList<ComplaintGuiltyItem> guilties = new List<ComplaintGuiltyItem>();
		[Display(Name = "Виновные в жалобе")]
		public virtual IList<ComplaintGuiltyItem> Guilties {
			get => guilties;
			set => SetField(ref guilties, value, () => Guilties);
		}

		GenericObservableList<ComplaintGuiltyItem> observableGuilties;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ComplaintGuiltyItem> ObservableGuilties {
			get {
				if(observableGuilties == null)
					observableGuilties = new GenericObservableList<ComplaintGuiltyItem>(Guilties);
				return observableGuilties;
			}
		}

		public virtual void AddFine(Fine fine)
		{
			if(ObservableFines.Contains(fine)) {
				return;
			}
			ObservableFines.Add(fine);
		}

		public virtual void RemoveFine(Fine fine)
		{
			if(ObservableFines.Contains(fine)) {
				ObservableFines.Remove(fine);
			}
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
