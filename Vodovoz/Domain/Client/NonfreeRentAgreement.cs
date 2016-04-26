using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QSOrmProject;

namespace Vodovoz.Domain.Client
{

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
	
}
