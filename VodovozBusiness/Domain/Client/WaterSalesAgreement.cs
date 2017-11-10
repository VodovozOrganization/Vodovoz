using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Client
{

	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "доп. соглашения продажи воды",
		Nominative = "доп. соглашение продажи воды")]
	public class WaterSalesAgreement : AdditionalAgreement
	{
		public virtual bool IsFixedPrice { get; set; }


		IList<WaterSalesAgreementFixedPrice> fixedPrices = new List<WaterSalesAgreementFixedPrice> ();

		[Display (Name = "Фиксированные цены")]
		public virtual IList<WaterSalesAgreementFixedPrice> FixedPrices {
			get { return fixedPrices; }
			set { SetField (ref fixedPrices, value, () => FixedPrices); }
		}

		GenericObservableList<WaterSalesAgreementFixedPrice> observableFixedPrices;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WaterSalesAgreementFixedPrice> ObservablFixedPrices {
			get {
				if (observableFixedPrices == null)
					observableFixedPrices = new GenericObservableList<WaterSalesAgreementFixedPrice> (FixedPrices);
				return observableFixedPrices;
			}
		}


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

		public virtual void  AddFixedPrice(Nomenclature nomenclature, decimal price)
		{
			var nomenculaturePrice = new WaterSalesAgreementFixedPrice{
				Nomenclature = nomenclature,
				AdditionalAgreement = this,
				Price = price
			};

			ObservablFixedPrices.Add(nomenculaturePrice);
		}

		public static IUnitOfWorkGeneric<WaterSalesAgreement> Create (CounterpartyContract contract)
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<WaterSalesAgreement> ();
			uow.Root.Contract = uow.GetById<CounterpartyContract>(contract.Id);
			uow.Root.AgreementNumber = AdditionalAgreement.GetNumberWithType (uow.Root.Contract, AgreementType.WaterSales);
			return uow;
		}
	}
	
}
