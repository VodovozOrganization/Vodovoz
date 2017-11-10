using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QSOrmProject;

namespace Vodovoz.Domain.Client
{

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
			set {; }
		}

		public override IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			foreach (ValidationResult result in base.Validate (validationContext))
				yield return result;
			
			if (RentDays < 1)
				yield return new ValidationResult ("Срок аренды не может быть меньше одного дня.", new[] { "RentDays" });

			if (DeliveryPoint == null)
				yield return new ValidationResult("Необходимо указать точку доставки.", new[] { "DeliveryPoint" });

			if (Equipment.Count < 1)
				yield return new ValidationResult("Необходимо добавить в список оборудование.", new[] { "Equipment" });
		}

		public static IUnitOfWorkGeneric<DailyRentAgreement> Create (CounterpartyContract contract)
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<DailyRentAgreement> ();
			uow.Root.Contract = uow.GetById<CounterpartyContract>(contract.Id);
			uow.Root.AgreementNumber = AdditionalAgreement.GetNumberWithType (uow.Root.Contract, AgreementType.DailyRent);
			return uow;
		}

		public virtual void RemoveEquipment(PaidRentEquipment paidEquipment)
		{
			foreach(PaidRentEquipment eq in this.ObservableEquipment.CreateList())
			{
				if(eq == paidEquipment)
				{
					ObservableEquipment.Remove(eq);
				}
			}
		}

	}
	
}
