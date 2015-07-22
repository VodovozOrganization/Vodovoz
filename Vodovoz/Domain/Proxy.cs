using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;
using QSContacts;
using QSOrmProject;
using QSValidation.Attributes;

namespace Vodovoz.Domain
{
	[OrmSubject( Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "доверенности",
		Nominative = "доверенность",
		Genitive = "доверенности",
		Accusative = "доверенность"
	)]
	public class Proxy : BaseNotifyPropertyChanged, IDomainObject, IValidatableObject
	{
		#region Свойства
		public virtual int Id { get; set; }

		[Display(Name = "Контрагент")]
		[Required (ErrorMessage = "В доверенности должен быть указан контрагент.")]
		public virtual Counterparty Counterparty { get; set; }

		[Display(Name = "Номер")]
		[Required (ErrorMessage = "Номер доверенности должен быть заполнен.")]
		public virtual string Number { get; set; }

		[Display(Name = "Дата подписания")]
		[DateRequired (ErrorMessage = "Дата доверености должна быть указана.")]
		public virtual DateTime IssueDate { get; set; }

		[Display(Name = "Начало действия")]
		public virtual DateTime StartDate { get; set; }

		[Display(Name = "Окончание действия")]
		public virtual DateTime ExpirationDate { get; set; }

		[Display(Name = "Список лиц")]
		public virtual IList<Person> Persons { get; set; }

		[Display(Name = "Точка доставки")]
		public virtual DeliveryPoint DeliveryPoint { get; set; }
		#endregion

		public Proxy ()
		{
			Number = String.Empty;
		}

		public virtual string Title { 
			get { return String.Format ("Доверенность №{0} от {1:d}", Number, IssueDate); }
		}

		public bool IsActiveProxy(DateTime onDate)
		{
			return (onDate >= StartDate && onDate <= ExpirationDate);
		}

		#region IValidatableObject implementation

		public IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (StartDate != default(DateTime) && StartDate < IssueDate)
				yield return new ValidationResult ("Нельзя установить дату начала действия доверенности раньше даты ее выдачи.",
					new[] { this.GetPropertyName (o => o.StartDate), this.GetPropertyName (o => o.IssueDate)});
			if (ExpirationDate != default(DateTime) && ExpirationDate < StartDate)
				yield return new ValidationResult ("Нельзя установить дату окончания действия доверенности раньше даты начала ее действия.",
					new[] { this.GetPropertyName (o => o.StartDate), this.GetPropertyName (o => o.ExpirationDate)});
		}

		#endregion

		//Конструкторы
		public static IUnitOfWorkGeneric<Proxy> Create(Counterparty counterparty)
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<Proxy> ();
			uow.Root.Counterparty = counterparty;
			return uow;
		}
	}

	public interface IProxyOwner
	{
		IList<Proxy> Proxies { get; set;}
	}
}

