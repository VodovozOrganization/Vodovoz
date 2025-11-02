using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Domain.Client
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "доверенности",
		Nominative = "доверенность",
		Genitive = "доверенности",
		Accusative = "доверенность"
	)]
	[EntityPermission]
	public class Proxy : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }


		private Counterparty counterparty;

		[Display (Name = "Контрагент")]
		[Required (ErrorMessage = "В доверенности должен быть указан контрагент.")]
		public virtual Counterparty Counterparty
		{
			get { return counterparty; }
			set { SetField(ref counterparty, value, () => Counterparty); }
		}


		private string number;

		[Display(Name = "Номер")]
		[Required (ErrorMessage = "Номер доверенности должен быть заполнен.")]
		public virtual string Number
		{
			get { return number; }
			set { SetField(ref number, value, () => Number); }
		}

		private DateTime issueDate;

		[Display(Name = "Дата подписания")]
		public virtual DateTime IssueDate
		{
			get { return issueDate; }
			set { SetField(ref issueDate, value, () => IssueDate); }
		}

		private DateTime startDate;

		[Display(Name = "Начало действия")]
		public virtual DateTime StartDate
		{
			get { return startDate; }
			set { SetField(ref startDate, value, () => StartDate); }
		}

		private DateTime expirationDate;

		[Display (Name = "Окончание действия")]
		public virtual DateTime ExpirationDate
		{
			get { return expirationDate; }
			set { SetField(ref expirationDate, value, () => ExpirationDate); }
		}

		private IList<Person> persons;

		[Display (Name = "Список лиц")]
		public virtual IList<Person> Persons
		{
			get { return persons; }
			set { SetField(ref persons, value, () => Persons); }
		}

		private IList<DeliveryPoint> deliveryPoints = new List<DeliveryPoint>();

		[Display (Name = "Точки доставки")]
		public virtual IList<DeliveryPoint> DeliveryPoints {
			get { return deliveryPoints; }
			set { SetField (ref deliveryPoints, value, () => DeliveryPoints); }
		}

		GenericObservableList<DeliveryPoint> observableDeliveryPoints;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryPoint> ObservableDeliveryPoints {
			get {
				if (observableDeliveryPoints == null)
					observableDeliveryPoints = new GenericObservableList<DeliveryPoint> (DeliveryPoints);
				return observableDeliveryPoints;
			}
		}

		#endregion

		public Proxy ()
		{
			Number = String.Empty;
		}

		public virtual string Title { 
			get { return String.Format ("Доверенность №{0} от {1:d}", Number, IssueDate); }
		}

		public virtual bool IsActiveProxy (DateTime onDate)
		{
			return (onDate >= StartDate && onDate <= ExpirationDate);
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if(IssueDate == default(DateTime)) {
				yield return new ValidationResult("Дата доверености должна быть указана.");
			}
			if(ExpirationDate == default(DateTime)) {
				yield return new ValidationResult("Дата окончания доверености должна быть указана.");
			}
			if(StartDate != default(DateTime) && StartDate < IssueDate)
				yield return new ValidationResult ("Нельзя установить дату начала действия доверенности раньше даты ее выдачи.",
					new[] { this.GetPropertyName (o => o.StartDate), this.GetPropertyName (o => o.IssueDate) });
			if(ExpirationDate != default(DateTime) && ExpirationDate < StartDate)
				yield return new ValidationResult("Нельзя установить дату окончания действия доверенности раньше даты начала ее действия.",
					new[] { this.GetPropertyName(o => o.StartDate), this.GetPropertyName(o => o.ExpirationDate) });
		}

		#endregion

		public virtual void AddDeliveryPoint(DeliveryPoint deliveryPoint)
		{
			if (DeliveryPoints.Any(x => x.Id == deliveryPoint.Id))
				return;
			ObservableDeliveryPoints.Add(deliveryPoint);
		}
	}
}

