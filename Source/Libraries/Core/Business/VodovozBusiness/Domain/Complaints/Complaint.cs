﻿using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
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
		private DateTime _version;
		private Employee _createdBy;
		private DateTime _creationDate;
		private Employee _changedBy;
		private DateTime _changedDate;
		private ComplaintType _complaintType;
		private Counterparty _counterparty;
		private DeliveryPoint _deliveryPoint;
		private string _complainantName;
		private string _complaintText;
		private Order _order;
		private string _phone;
		private ComplaintSource _complaintSource;
		private ComplaintStatuses _status;
		private DateTime _plannedCompletionDate;
		private string _resultText;
		private ComplaintResultOfCounterparty _complaintResultOfCounterparty;
		private ComplaintResultOfEmployees _complaintResultOfEmployees;
		private DateTime? _actualCompletionDate;
		private ComplaintKind _complaintKind;
		private string _arrangement;
		private int _driverRating;
		private IList<Fine> _fines = new List<Fine>();
		private GenericObservableList<Fine> _observableFines;
		private IList<ComplaintDiscussion> _complaintDiscussions = new List<ComplaintDiscussion>();
		private GenericObservableList<ComplaintDiscussion> _observableComplaintDiscussions;
		private IList<ComplaintGuiltyItem> _guilties = new List<ComplaintGuiltyItem>();
		private GenericObservableList<ComplaintGuiltyItem> _observableGuilties;
		private IList<ComplaintFile> _files = new List<ComplaintFile>();
		private GenericObservableList<ComplaintFile> _observableFiles;
		private ComplaintDetalization _complaintDetalization;

		public virtual int Id { get; set; }

		[Display(Name = "Версия")]
		public virtual DateTime Version
		{
			get => _version;
			set => SetField(ref _version, value);
		}

		[Display(Name = "Кем создан")]
		public virtual Employee CreatedBy
		{
			get => _createdBy;
			set => SetField(ref _createdBy, value);
		}

		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}

		[Display(Name = "Кем изменен")]
		public virtual Employee ChangedBy
		{
			get => _changedBy;
			set => SetField(ref _changedBy, value);
		}

		[Display(Name = "Дата изменения")]
		public virtual DateTime ChangedDate
		{
			get => _changedDate;
			set => SetField(ref _changedDate, value);
		}

		[Display(Name = "Тип рекламации")]
		public virtual ComplaintType ComplaintType
		{
			get => _complaintType;
			set => SetField(ref _complaintType, value);
		}

		[Display(Name = "Клиент")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		[Display(Name = "Точка доставки")]
		public virtual DeliveryPoint DeliveryPoint
		{
			get => _deliveryPoint;
			set => SetField(ref _deliveryPoint, value);
		}

		[Display(Name = "Имя заявителя рекламации")]
		public virtual string ComplainantName
		{
			get => _complainantName;
			set => SetField(ref _complainantName, value);
		}

		[Display(Name = "Текст рекламации")]
		public virtual string ComplaintText
		{
			get => _complaintText;
			set => SetField(ref _complaintText, value);
		}

		[Display(Name = "Заказ")]
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Телефон")]
		public virtual string Phone
		{
			get => _phone;
			set => SetField(ref _phone, value);
		}

		[Display(Name = "Источник")]
		public virtual ComplaintSource ComplaintSource
		{
			get => _complaintSource;
			set => SetField(ref _complaintSource, value);
		}

		[Display(Name = "Статус")]
		public virtual ComplaintStatuses Status
		{
			get => _status;
			protected set => SetField(ref _status, value);
		}

		[Display(Name = "Дата планируемого завершения")]
		public virtual DateTime PlannedCompletionDate
		{
			get => _plannedCompletionDate;
			set => SetField(ref _plannedCompletionDate, value);
		}

		[Display(Name = "Описание результата")]
		public virtual string ResultText
		{
			get => _resultText;
			set => SetField(ref _resultText, value);
		}

		[Display(Name = "Результат по клиенту")]
		public virtual ComplaintResultOfCounterparty ComplaintResultOfCounterparty
		{
			get => _complaintResultOfCounterparty;
			set => SetField(ref _complaintResultOfCounterparty, value);
		}

		[Display(Name = "Результат по сотрудникам")]
		public virtual ComplaintResultOfEmployees ComplaintResultOfEmployees
		{
			get => _complaintResultOfEmployees;
			set => SetField(ref _complaintResultOfEmployees, value);
		}

		[Display(Name = "Дата фактического завершения")]
		public virtual DateTime? ActualCompletionDate
		{
			get => _actualCompletionDate;
			set => SetField(ref _actualCompletionDate, value);
		}

		[Display(Name = "Вид рекламации")]
		public virtual ComplaintKind ComplaintKind
		{
			get => _complaintKind;
			set => SetField(ref _complaintKind, value);
		}

		[Display(Name = "Детализация")]
		public virtual ComplaintDetalization ComplaintDetalization
		{
			get => _complaintDetalization;
			set => SetField(ref _complaintDetalization, value);
		}

		[Display(Name = "Мероприятия")]
		public virtual string Arrangement
		{
			get => _arrangement;
			set => SetField(ref _arrangement, value);
		}

		[Display(Name = "Оценка водителя")]
		public virtual int DriverRating
		{
			get => _driverRating;
			set => SetField(ref _driverRating, value);
		}

		[Display(Name = "Штрафы")]
		public virtual IList<Fine> Fines
		{
			get => _fines;
			set => SetField(ref _fines, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Fine> ObservableFines
		{
			get
			{
				if(_observableFines == null)
				{
					_observableFines = new GenericObservableList<Fine>(Fines);
				}

				return _observableFines;
			}
		}

		[Display(Name = "Обсуждения")]
		public virtual IList<ComplaintDiscussion> ComplaintDiscussions
		{
			get => _complaintDiscussions;
			set => SetField(ref _complaintDiscussions, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ComplaintDiscussion> ObservableComplaintDiscussions
		{
			get
			{
				if(_observableComplaintDiscussions == null)
				{
					_observableComplaintDiscussions = new GenericObservableList<ComplaintDiscussion>(ComplaintDiscussions);
				}

				return _observableComplaintDiscussions;
			}
		}

		[Display(Name = "Ответственные в рекламации")]
		public virtual IList<ComplaintGuiltyItem> Guilties
		{
			get => _guilties;
			set => SetField(ref _guilties, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ComplaintGuiltyItem> ObservableGuilties
		{
			get
			{
				if(_observableGuilties == null)
				{
					_observableGuilties = new GenericObservableList<ComplaintGuiltyItem>(Guilties);
				}

				return _observableGuilties;
			}
		}

		[Display(Name = "Файлы")]
		public virtual IList<ComplaintFile> Files
		{
			get => _files;
			set => SetField(ref _files, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ComplaintFile> ObservableFiles
		{
			get
			{
				if(_observableFiles == null)
				{
					_observableFiles = new GenericObservableList<ComplaintFile>(Files);
				}

				return _observableFiles;
			}
		}

		public virtual string Title => string.Format("Рекламация №{0}", Id);

		public virtual void AddFine(Fine fine)
		{
			if(ObservableFines.Contains(fine))
			{
				return;
			}
			ObservableFines.Add(fine);
		}

		public virtual void RemoveFine(Fine fine)
		{
			if(ObservableFines.Contains(fine))
			{
				ObservableFines.Remove(fine);
			}
		}

		public virtual void AddFile(ComplaintFile file)
		{
			if(ObservableFiles.Contains(file))
			{
				return;
			}
			file.Complaint = this;
			ObservableFiles.Add(file);
		}

		public virtual void RemoveFile(ComplaintFile file)
		{
			if(ObservableFiles.Contains(file))
			{
				ObservableFiles.Remove(file);
			}
		}

		public virtual void AttachSubdivisionToDiscussions(Subdivision subdivision)
		{
			if(subdivision == null)
			{
				throw new ArgumentNullException(nameof(subdivision));
			}

			if(ObservableComplaintDiscussions.Any(x => x.Subdivision.Id == subdivision.Id))
			{
				return;
			}

			ComplaintDiscussion newDiscussion = new ComplaintDiscussion
			{
				StartSubdivisionDate = DateTime.Now,
				PlannedCompletionDate = DateTime.Today,
				Complaint = this,
				Subdivision = subdivision
			};
			ObservableComplaintDiscussions.Add(newDiscussion);
			SetStatus(ComplaintStatuses.InProcess);
		}

		public virtual void UpdateComplaintStatus()
		{
			if(ObservableComplaintDiscussions.Any(x => x.Status == ComplaintDiscussionStatuses.Checking))
			{
				SetStatus(ComplaintStatuses.WaitingForReaction);
				return;
			}

			if(ObservableComplaintDiscussions.All(x => x.Status == ComplaintDiscussionStatuses.Closed))
			{
				SetStatus(ComplaintStatuses.Checking);
				return;
			}
			SetStatus(ComplaintStatuses.InProcess);
		}

		public virtual IList<string> SetStatus(ComplaintStatuses newStatus)
		{
			List<string> result = new List<string>();
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


		public virtual string GetFineReason()
		{
			string result = $"Рекламация №{Id} от {CreationDate.ToShortDateString()}";
			if(Counterparty == null && Order == null)
			{
				return result;
			}
			string clientName = Counterparty == null ? Order.Client.Name : Counterparty.Name;
			string clientInfo = $", {clientName}";
			return result + clientInfo;
		}

		public virtual bool Close(ref string message)
		{
			IList<string> res = SetStatus(ComplaintStatuses.Closed);
			if(!res.Any())
			{
				ActualCompletionDate = DateTime.Now;
				return true;
			}
			message = string.Join<string>("\n", res);
			return false;
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(ComplaintText))
			{
				yield return new ValidationResult("Необходимо ввести текст рекламации");
			}

			if(ComplaintType == ComplaintType.Client)
			{
				if(ComplaintSource == null)
				{
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
