using System;
using QSOrmProject;
using System.Data.Bindings;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;

namespace Vodovoz.Domain
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "дополнительные соглашения",
		Nominative = "дополнительное соглашение")]
	public class AdditionalAgreement : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		[Required (ErrorMessage = "Номер доп. соглашения должен быть заполнен.")]
		[Display (Name = "Номер")]
		public virtual string AgreementNumber { get; set; }

		[Display (Name = "Тип доп. соглашения")]
		public virtual AgreementType Type {
			get {	 
				if (this is DailyRentAgreement)		//Не менять Daily и Nonfree местами!
					return AgreementType.DailyRent;
				if (this is NonfreeRentAgreement)	//Иначе из-за наследования тип будет определен некорректно.
					return AgreementType.NonfreeRent;
				if (this is FreeRentAgreement)
					return AgreementType.FreeRent;
				if (this is WaterSalesAgreement)
					return AgreementType.WaterSales;
				return AgreementType.Repair;
			}

		}

		[Required (ErrorMessage = "Договор должен быть указан.")]
		[Display (Name = "Договор")]
		public virtual CounterpartyContract Contract { get; set; }

		[Required (ErrorMessage = "Дата создания должна быть указана.")]
		[Display (Name = "Дата подписания")]
		public virtual DateTime IssueDate { get; set; }

		[Required (ErrorMessage = "Дата начала действия должна быть указана.")]
		[Display (Name = "Дата начала")]
		public virtual DateTime StartDate { get; set; }

		[Display (Name = "Точка доставки")]
		public virtual DeliveryPoint DeliveryPoint { get; set; }

		public virtual string AgreementDeliveryPoint { get { return DeliveryPoint != null ? DeliveryPoint.Point : "Не указана"; } }

		public virtual string AgreementTypeTitle { get { return Type.GetEnumTitle (); } }

		public virtual bool IsNew { get; set; }

		public AdditionalAgreement ()
		{
			AgreementNumber = String.Empty;
			IssueDate = StartDate = DateTime.Now;
		}

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			int count = 0;
			foreach (AdditionalAgreement agreement in Contract.AdditionalAgreements)
				if (agreement.AgreementNumber == this.AgreementNumber)
					count++;
			if (count > 1)
				yield return new ValidationResult ("Доп. соглашение с таким номером уже существует.", new[] { "AgreementNumber" });
		}

		public static string GetNumber (CounterpartyContract contract)
		{
			//Вычисляем номер для нового соглашения.
			var additionalAgreements = contract.AdditionalAgreements;
			var numbers = new List<int> ();
			foreach (AdditionalAgreement a in additionalAgreements) {
				int res;
				if (Int32.TryParse (a.AgreementNumber, out res))
					numbers.Add (res);
			}
			numbers.Sort ();
			string number = "00";
			if (numbers.Count > 0) {
				number += (numbers [numbers.Count - 1] + 1).ToString ();
				number = number.Substring (number.Length - 3, 3);
			} else
				number += "1";
			return number;
		}
	}

	public class NonfreeRentAgreement : AdditionalAgreement
	{
		IList<PaidRentEquipment> equipment = new List<PaidRentEquipment> ();

		[Display (Name = "Список оборудования")]
		public virtual IList<PaidRentEquipment> Equipment {
			get { return equipment; }
			set { SetField (ref equipment, value, () => Equipment); }
		}

		GenericObservableList<PaidRentEquipment> observableEquipment;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<PaidRentEquipment> ObservableEquipment {
			get {
				if (observableEquipment == null)
					observableEquipment = new GenericObservableList<PaidRentEquipment> (Equipment);
				return observableEquipment;
			}
		}

		public virtual string Title { 
			get { return String.Format ("Доп. соглашение платной аренды №{0} от {1:d}", Id, IssueDate); }
		}

		public override IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			foreach (ValidationResult result in base.Validate (validationContext))
				yield return result;
			if (DeliveryPoint == null)
				yield return new ValidationResult ("Необходимо указать точку доставки.", new[] { "DeliveryPoint" });
		}

		public static IUnitOfWorkGeneric<NonfreeRentAgreement> Create (CounterpartyContract contract)
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<NonfreeRentAgreement> ();
			uow.Root.Contract = contract;
			uow.Root.AgreementNumber = AdditionalAgreement.GetNumber (contract);
			return uow;
		}
	}

	public class DailyRentAgreement : AdditionalAgreement
	{
		[Display (Name = "Количество дней аренды")]
		public virtual int RentDays { get; set; }

		IList<PaidRentEquipment> equipment = new List<PaidRentEquipment> ();

		[Display (Name = "Список оборудования")]
		public virtual IList<PaidRentEquipment> Equipment {
			get { return equipment; }
			set { SetField (ref equipment, value, () => Equipment); }
		}

		GenericObservableList<PaidRentEquipment> observableEquipment;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<PaidRentEquipment> ObservableEquipment {
			get {
				if (observableEquipment == null)
					observableEquipment = new GenericObservableList<PaidRentEquipment> (Equipment);
				return observableEquipment;
			}
		}

		public virtual string Title { 
			get { return String.Format ("Доп. соглашение посуточной аренды №{0} от {1:d}", Id, IssueDate); }
		}

		public override IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			foreach (ValidationResult result in base.Validate (validationContext))
				yield return result;
			if (RentDays < 1)
				yield return new ValidationResult ("Срок аренды не может быть меньше одного дня.", new[] { "RentDays" });
		}

		public static IUnitOfWorkGeneric<DailyRentAgreement> Create (CounterpartyContract contract)
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<DailyRentAgreement> ();
			uow.Root.Contract = contract;
			uow.Root.AgreementNumber = AdditionalAgreement.GetNumber (contract);
			return uow;
		}
	}

	public class FreeRentAgreement : AdditionalAgreement
	{
		IList<FreeRentEquipment> equipment = new List<FreeRentEquipment> ();

		[Display (Name = "Список оборудования")]
		public virtual IList<FreeRentEquipment> Equipment {
			get { return equipment; }
			set { SetField (ref equipment, value, () => Equipment); }
		}

		GenericObservableList<FreeRentEquipment> observableEquipment;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<FreeRentEquipment> ObservableEquipment {
			get {
				if (observableEquipment == null)
					observableEquipment = new GenericObservableList<FreeRentEquipment> (Equipment);
				return observableEquipment;
			}
		}

		public virtual string Title { 
			get { return String.Format ("Доп. соглашение бесплатной аренды №{0} от {1:d}", Id, IssueDate); }
		}

		public override IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			foreach (ValidationResult result in base.Validate (validationContext))
				yield return result;
			if (DeliveryPoint == null)
				yield return new ValidationResult ("Необходимо указать точку доставки.", new[] { "DeliveryPoint" });
		}

		public static IUnitOfWorkGeneric<FreeRentAgreement> Create (CounterpartyContract contract)
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<FreeRentAgreement> ();
			uow.Root.Contract = contract;
			uow.Root.AgreementNumber = AdditionalAgreement.GetNumber (contract);
			return uow;
		}
	}

	public class WaterSalesAgreement : AdditionalAgreement
	{
		public virtual bool IsFixedPrice { get; set; }

		[Display (Name = "Фиксированная стоимость воды")]
		public virtual decimal FixedPrice { get; set; }

		public virtual string Title { 
			get { return String.Format ("Доп. соглашение продажи воды №{0} от {1:d}", Id, IssueDate); }
		}

		public override IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			foreach (ValidationResult result in base.Validate (validationContext))
				yield return result;
			var agreements = new List<AdditionalAgreement> ();
			foreach (AdditionalAgreement agreement in Contract.AdditionalAgreements)
				if (agreement is WaterSalesAgreement)
					agreements.Add (agreement);
			if (agreements.FindAll (m => m.DeliveryPoint == this.DeliveryPoint).Count > 1) {
				if (DeliveryPoint != null)
					yield return new ValidationResult ("Доп. соглашение для данной точки доставки уже существует.", new[] { "DeliveryPoint" });
				else
					yield return new ValidationResult ("Общее доп. соглашение по продаже воды уже существует. " +
					"Пожалуйста, укажите точку доставки или перейдите к существующему соглашению.", new[] { "DeliveryPoint" });
			}
		}

		public static IUnitOfWorkGeneric<WaterSalesAgreement> Create (CounterpartyContract contract)
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<WaterSalesAgreement> ();
			uow.Root.Contract = contract;
			uow.Root.AgreementNumber = AdditionalAgreement.GetNumber (contract);
			return uow;
		}
	}

	public class RepairAgreement : AdditionalAgreement
	{
		public virtual string Title { 
			get { return String.Format ("Доп. соглашение сервиса №{0} от {1:d}", Id, IssueDate); }
		}

		public static IUnitOfWorkGeneric<RepairAgreement> Create (CounterpartyContract contract)
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<RepairAgreement> ();
			uow.Root.Contract = contract;
			uow.Root.AgreementNumber = AdditionalAgreement.GetNumber (contract);
			return uow;
		}
	}

	public enum AgreementType
	{
		[ItemTitleAttribute ("Долгосрочая аренда")]
		NonfreeRent,
		[ItemTitleAttribute ("Посуточная аренда")]
		DailyRent,
		[ItemTitleAttribute ("Бесплатная аренда")]
		FreeRent,
		[ItemTitleAttribute ("Продажа воды")]
		WaterSales,
		[ItemTitleAttribute ("Ремонт")]
		Repair
	}

	public class AgreementTypeStringType : NHibernate.Type.EnumStringType
	{
		public AgreementTypeStringType () : base (typeof(AgreementType))
		{
		}
	}

	public enum PaidRentAgreementType
	{
		[ItemTitleAttribute ("Посуточная аренда")]
		NonfreeRent,
		[ItemTitleAttribute ("Посуточная аренда")]
		DailyRent
	}
}

