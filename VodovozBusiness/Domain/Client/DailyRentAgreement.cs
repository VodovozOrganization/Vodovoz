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
			uow.Root.Contract = uow.GetById<CounterpartyContract>(contract.Id);
			uow.Root.AgreementNumber = AdditionalAgreement.GetNumber (uow.Root.Contract);
			return uow;
		}
	}
	
}
