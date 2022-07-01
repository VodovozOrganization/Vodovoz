using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "рекламации",
		Nominative = "рекламация",
		Prepositional = "рекламации",
		PrepositionalPlural = "рекламацях"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class Complaint : BusinessObjectBase<Complaint>, IDomainObject, IValidatableObject
	{
		private const int _phoneLimit = 45;

		public virtual int Id { get; set; }

		DateTime version;
		[Display(Name = "Версия")]
		public virtual DateTime Version {
			get => version;
			set => SetField(ref version, value, () => Version);
		}

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

		private ComplaintType complaintType;
		[Display(Name = "Тип рекламации")]
		public virtual ComplaintType ComplaintType {
			get => complaintType;
			set => SetField(ref complaintType, value, () => ComplaintType);
		}

		private Counterparty counterparty;
		[Display(Name = "Клиент")]
		public virtual Counterparty Counterparty {
			get => counterparty;
			set => SetField(ref counterparty, value, () => Counterparty);
		}

		DeliveryPoint deliveryPoint;
		[Display(Name = "Точка доставки")]
		public virtual DeliveryPoint DeliveryPoint {
			get => deliveryPoint;
			set => SetField(ref deliveryPoint, value);
		}

		private string complainantName;
		[Display(Name = "Имя заявителя рекламации")]
		public virtual string ComplainantName {
			get => complainantName;
			set => SetField(ref complainantName, value, () => ComplainantName);
		}

		private string complaintText;
		[Display(Name = "Текст рекламации")]
		public virtual string ComplaintText {
			get => complaintText;
			set => SetField(ref complaintText, value, () => ComplaintText);
		}

		private Order order;
		[Display(Name = "Заказ")]
		public virtual Order Order {
			get => order;
			set => SetField(ref order, value);
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
			protected set => SetField(ref status, value, () => Status);
		}

		private DateTime plannedCompletionDate;
		[Display(Name = "Дата планируемого завершения")]
		public virtual DateTime PlannedCompletionDate {
			get => plannedCompletionDate;
			set => SetField(ref plannedCompletionDate, value, () => PlannedCompletionDate);
		}

		private string resultText;
		[Display(Name = "Описание результата")]
		public virtual string ResultText {
			get => resultText;
			set => SetField(ref resultText, value, () => ResultText);
		}

		private ComplaintResultOfCounterparty _complaintResultOfCounterparty;
		[Display(Name = "Результат по клиенту")]
		public virtual ComplaintResultOfCounterparty ComplaintResultOfCounterparty {
			get => _complaintResultOfCounterparty;
			set => SetField(ref _complaintResultOfCounterparty, value, () => ComplaintResultOfCounterparty);
		}
		
		private ComplaintResultOfEmployees _complaintResultOfEmployees;
		[Display(Name = "Результат по сотрудникам")]
		public virtual ComplaintResultOfEmployees ComplaintResultOfEmployees {
			get => _complaintResultOfEmployees;
			set => SetField(ref _complaintResultOfEmployees, value);
		}

		private DateTime? actualCompletionDate;
		[Display(Name = "Дата фактического завершения")]
		public virtual DateTime? ActualCompletionDate {
			get => actualCompletionDate;
			set => SetField(ref actualCompletionDate, value, () => ActualCompletionDate);
		}

		ComplaintKind complaintKind;
		[Display(Name = "Вид рекламации")]
		public virtual ComplaintKind ComplaintKind {
			get => complaintKind;
			set => SetField(ref complaintKind, value);
		}

		string arrangement;
		[Display(Name = "Мероприятия")]
		public virtual string Arrangement {
			get => arrangement;
			set => SetField(ref arrangement, value);
		}

		int driverRating;
		[Display(Name = "Оценка водителя")]
		public virtual int DriverRating
		{
			get => driverRating;
			set => SetField(ref driverRating, value);
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
		[Display(Name = "Виновные в рекламации")]
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

		IList<ComplaintFile> files = new List<ComplaintFile>();
		[Display(Name = "Файлы")]
		public virtual IList<ComplaintFile> Files {
			get => files;
			set => SetField(ref files, value, () => Files);
		}

		GenericObservableList<ComplaintFile> observableFiles;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ComplaintFile> ObservableFiles {
			get {
				if(observableFiles == null)
					observableFiles = new GenericObservableList<ComplaintFile>(Files);
				return observableFiles;
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

		public virtual void AddFile(ComplaintFile file)
		{
			if(ObservableFiles.Contains(file)) {
				return;
			}
			file.Complaint = this;
			ObservableFiles.Add(file);
		}

		public virtual void RemoveFile(ComplaintFile file)
		{
			if(ObservableFiles.Contains(file)) {
				ObservableFiles.Remove(file);
			}
		}

		public virtual void AttachSubdivisionToDiscussions(Subdivision subdivision)
		{
			if(subdivision == null) {
				throw new ArgumentNullException(nameof(subdivision));
			}

			if(ObservableComplaintDiscussions.Any(x => x.Subdivision.Id == subdivision.Id)) {
				return;
			}

			ComplaintDiscussion newDiscussion = new ComplaintDiscussion();
			newDiscussion.StartSubdivisionDate = DateTime.Now;
			newDiscussion.PlannedCompletionDate = DateTime.Today;
			newDiscussion.Complaint = this;
			newDiscussion.Subdivision = subdivision;
			ObservableComplaintDiscussions.Add(newDiscussion);
			SetStatus(ComplaintStatuses.InProcess);
		}

		public virtual void UpdateComplaintStatus()
		{
			if(ObservableComplaintDiscussions.Any(x => x.Status == ComplaintStatuses.InProcess))
				SetStatus(ComplaintStatuses.InProcess);
			else
				SetStatus(ComplaintStatuses.Checking);
		}

		public virtual IList<string> SetStatus(ComplaintStatuses newStatus)
		{
			IList<string> result = new List<string>();
			if(newStatus == ComplaintStatuses.Closed)
			{
				if(ComplaintResultOfCounterparty == null)
				{
					result.Add("Заполните поле \"Итог работы по клиенту\".");
				}
				
				if(ComplaintResultOfEmployees == null)
				{
					result.Add("Заполните поле \"Итог работы по сотрудникам\".");
				}

				if(string.IsNullOrWhiteSpace(ResultText))
				{
					result.Add("Заполните поле \"Результат\".");
				}
			}

			if(!result.Any())
			{
				Status = newStatus;
			}

			return result;
		}

		public virtual string Title => string.Format("Рекламация №{0}", Id);

		public virtual string GetFineReason()
		{
			string result = $"Рекламация №{Id} от {CreationDate.ToShortDateString()}";
			if(Counterparty == null && Order == null) {
				return result;
			}
			string clientName = Counterparty == null ? Order.Client.Name : Counterparty.Name;
			string clientInfo = $", {clientName}";
			return result + clientInfo;
		}

		public virtual bool Close(ref string message)
		{
			var res = SetStatus(ComplaintStatuses.Closed);
			if(!res.Any()) {
				ActualCompletionDate = DateTime.Now;
				return true;
			}
			message = string.Join<string>("\n", res);
			return false;
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(ComplaintText)) {
				yield return new ValidationResult("Необходимо ввести текст рекламации");
			}

			if(ComplaintType == ComplaintType.Client) {
				if(ComplaintSource == null) {
					yield return new ValidationResult("Необходимо выбрать источник");
				}
			}
			
			if(Phone != null && Phone.Length > _phoneLimit)
			{
				yield return new ValidationResult($"Длина поля телефон превышена на {Phone.Length - _phoneLimit}",
					new[] { nameof(Phone) });
			}

			if(Status == ComplaintStatuses.Closed) 
			{
				if(ComplaintResultOfCounterparty == null)
				{
					yield return new ValidationResult(
						"Заполните поле \"Итог работы по клиенту\".", new[] { nameof(ComplaintResultOfCounterparty) });
				}
				
				if(ComplaintResultOfEmployees == null)
				{
					yield return new ValidationResult(
						"Заполните поле \"Итог работы по сотрудникам\".", new[] { nameof(ComplaintResultOfEmployees) });
				}

				if(string.IsNullOrWhiteSpace(ResultText))
				{
					yield return new ValidationResult("Заполните поле \"Результат\".", new[] { nameof(ResultText) });
				}
			}
		}

		#endregion IValidatableObject implementation
	}
}
