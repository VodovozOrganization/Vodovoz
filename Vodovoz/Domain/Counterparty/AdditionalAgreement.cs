using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using Gamma.Utilities;
using QSOrmProject;

namespace Vodovoz.Domain
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "дополнительные соглашения",
		Nominative = "дополнительное соглашение",
		Accusative = "дополнительное соглашение",
		Genitive = "дополнительного соглашения"
	)]
	public class AdditionalAgreement : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		string agreementNumber;

		[Required (ErrorMessage = "Номер доп. соглашения должен быть заполнен.")]
		[Display (Name = "Номер")]
		public virtual string AgreementNumber { 
			get { return agreementNumber; } 
			set { SetField (ref agreementNumber, value, () => AgreementNumber); }
		}

		[Display (Name = "Тип доп. соглашения")]
		public virtual AgreementType Type {
			get {	 
				if (this is DailyRentAgreement)
					return AgreementType.DailyRent;
				if (this is NonfreeRentAgreement)
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

		[Display (Name = "Закрыто")]
		public virtual bool IsCancelled { get; set; }

		public virtual string AgreementDeliveryPoint { get { return DeliveryPoint != null ? DeliveryPoint.CompiledAddress : "Не указана"; } }

		public virtual string AgreementTypeTitle { get { return Type.GetEnumTitle (); } }

		public virtual string DocumentDate { get { return String.Format ("От {0}", StartDate.ToShortDateString ()); } }

		public virtual string Title { get { return String.Format ("Доп. соглашение №{0} от {1}", AgreementNumber, StartDate.ToShortDateString ()); } }

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

	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "доп. соглашения платной аренды",
		Nominative = "доп. соглашение платной аренды")]
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

	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "доп. соглашения посуточной аренды",
		Nominative = "доп. соглашение посуточной аренды")]
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

		public virtual DateTime EndDate{
			get{
				return base.StartDate.AddDays(RentDays);
			}
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

	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "доп. соглашения бесплатной аренды",
		Nominative = "доп. соглашение бесплатной аренды")]
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

	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "доп. соглашения продажи воды",
		Nominative = "доп. соглашение продажи воды")]
	public class WaterSalesAgreement : AdditionalAgreement
	{
		public virtual bool IsFixedPrice { get; set; }

		[Display (Name = "Фиксированная стоимость воды")]
		public virtual decimal FixedPrice { get; set; }

		public override IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			foreach (ValidationResult result in base.Validate (validationContext))
				yield return result;
			if (Contract.CheckWaterSalesAgreementExists (Id, DeliveryPoint)) {
				if (DeliveryPoint != null)
					yield return new ValidationResult ("Доп. соглашение для данной точки доставки уже существует. " +
					"Пожалуйста, закройте действующее соглашение для создания нового.", new[] { "DeliveryPoint" });
				else
					yield return new ValidationResult ("Общее доп. соглашение по продаже воды уже существует. " +
					"Пожалуйста, закройте действующее соглашение для создания нового.", new[] { "DeliveryPoint" });
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

	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "доп. соглашения сервиса",
		Nominative = "доп. соглашение сервиса")]
	public class RepairAgreement : AdditionalAgreement
	{
		public static IUnitOfWorkGeneric<RepairAgreement> Create (CounterpartyContract contract)
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<RepairAgreement> ();
			uow.Root.Contract = contract;
			uow.Root.DeliveryPoint = null;
			uow.Root.AgreementNumber = AdditionalAgreement.GetNumber (contract);
			return uow;
		}
	}

	public enum AgreementType
	{
		[Display (Name = "Долгосрочая аренда")]
		NonfreeRent,
		[Display (Name = "Посуточная аренда")]
		DailyRent,
		[Display (Name = "Бесплатная аренда")]
		FreeRent,
		[Display (Name = "Продажа воды")]
		WaterSales,
		[Display (Name = "Ремонт")]
		Repair
	}

	public class AgreementTypeStringType : NHibernate.Type.EnumStringType
	{
		public AgreementTypeStringType () : base (typeof(AgreementType))
		{
		}
	}

	public enum OrderAgreementType
	{
		[Display (Name = "Долгосрочная аренда")]
		NonfreeRent,
		[Display (Name = "Посуточная аренда")]
		DailyRent,
		[Display (Name = "Бесплатная аренда")]
		FreeRent
	}

	public interface IAgreementSaved
	{
		event EventHandler<AgreementSavedEventArgs> AgreementSaved;
	}

	public class AgreementSavedEventArgs : EventArgs
	{
		public AdditionalAgreement Agreement { get; private set; }

		public AgreementSavedEventArgs (AdditionalAgreement agreement)
		{
			Agreement = agreement;
		}
	}
}

