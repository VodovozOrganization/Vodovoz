using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Goods;
using QSHistoryLog;

namespace Vodovoz.Domain.Client
{
	[OrmSubject(Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "доп. соглашения продажи оборудования",
		Nominative = "доп. соглашение продажи оборудования")]
	public class SalesEquipmentAgreement : AdditionalAgreement
	{
		[HistoryDeepCloneItems]
		IList<SalesEquipment> salesEqipments = new List<SalesEquipment>();

		[Display(Name = "Оборудование на продажу")]
		public virtual IList<SalesEquipment> SalesEqipments {
			get { return salesEqipments; }
			set { SetField(ref salesEqipments, value, () => SalesEqipments); }
		}

		GenericObservableList<SalesEquipment> observableSalesEqipments;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		[IgnoreHistoryTrace]
		public virtual GenericObservableList<SalesEquipment> ObservableSalesEqipments {
			get {
				if(observableSalesEqipments == null)
					observableSalesEqipments = new GenericObservableList<SalesEquipment>(SalesEqipments);
				return observableSalesEqipments;
			}
		}


		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			foreach(ValidationResult result in base.Validate(validationContext))
				yield return result;
		}

		public virtual void AddEquipment(Nomenclature nomenclature)
		{
			var salesEquipment = new SalesEquipment {
				Nomenclature = nomenclature,
				AdditionalAgreement = this,
				Price = nomenclature.GetPrice(1),
				Count = 1
			};

			ObservableSalesEqipments.Add(salesEquipment);
		}

		public static IUnitOfWorkGeneric<SalesEquipmentAgreement> Create(CounterpartyContract contract)
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<SalesEquipmentAgreement>();
			uow.Root.Contract = uow.GetById<CounterpartyContract>(contract.Id);
			uow.Root.AgreementNumber = AdditionalAgreement.GetNumberWithType(uow.Root.Contract, AgreementType.EquipmentSales);
			return uow;
		}
	}
}