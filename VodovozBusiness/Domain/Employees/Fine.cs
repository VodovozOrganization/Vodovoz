using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QSOrmProject;
using QSProjectsLib;
using System.Linq;
using Gamma.Utilities;

namespace Vodovoz.Domain.Employees
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "штрафы сотрудникам",
		Nominative = "штраф сотрудникам")]
	public class Fine: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		private DateTime date = DateTime.Today;

		[Display (Name = "Дата")]
		public virtual DateTime Date {
			get { return date; }
			set {
				SetField (ref date, value, () => Date);
			}
		}

		decimal totalMoney;

		[Display (Name = "Всего денег")]
		public virtual decimal TotalMoney {
			get { return totalMoney; }
			set { SetField (ref totalMoney, value, () => TotalMoney); }
		}

		IList<FineItem> items = new List<FineItem> ();

		[Display (Name = "Строки")]
		public virtual IList<FineItem> Items {
			get { return items; }
			set {
				SetField (ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<FineItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<FineItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<FineItem> (Items);
				return observableItems;
			}
		}

		IList<FineNomenclature> nomenclatures = new List<FineNomenclature> ();

		[Display (Name = "Номенклатура")]
		public virtual IList<FineNomenclature> Nomenclatures {
			get { return nomenclatures; }
			set {
				SetField (ref nomenclatures, value, () => Nomenclatures);
				//observableItems = null;
			}
		}

		#region Расчетные

		public virtual string Title { 
			get { return String.Format ("Штраф №{0} от {1:d}", Id, Date); }
		}

		public virtual string Description
		{
			get
			{
				if (Items.Count == 0)
					return CurrencyWorks.GetShortCurrencyString(TotalMoney);
				string persons;
				if (Items.Count <= 3)
					persons = String.Join(", ", Items.Select(x => x.Employee.ShortName));
				else
					persons = RusNumber.FormatCase(Items.Count, "{0} сотрудник", "{0} сотрудника", "{0} сотрудников");
				return String.Format("({0}) = {1}", persons,
					CurrencyWorks.GetShortCurrencyString(TotalMoney));
			}
		}

		#endregion

		public virtual void AddItem (Employee employee)
		{
			var item = new FineItem()
			{
				Employee = employee,
				Fine = this
			};
			ObservableItems.Add (item);
		}

		public Fine ()
		{
		}

		#region Функции

		public virtual void DivideAtAll()
		{
			if (Items.Count == 0)
				return;
			var part = Math.Round(TotalMoney / Items.Count, 2);
			foreach(var item in Items)
			{
				item.Money = part;
			}
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Items.Count == 0)
				yield return new ValidationResult (String.Format("Отсутствуют сотрудники на которых назначен штраф."),
					new[] { this.GetPropertyName (o => o.Items) });

			var totalSum = Items.Sum(x => x.Money);
			if(totalSum != TotalMoney)
				yield return new ValidationResult (String.Format("Общая сумма штрафа {0:C}, отличается от суммы штрафов всех сотрудников {1:C}.",
					TotalMoney, totalSum),
					new[] { this.GetPropertyName (o => o.Items) });
		}
		

		#endregion
	}
}

