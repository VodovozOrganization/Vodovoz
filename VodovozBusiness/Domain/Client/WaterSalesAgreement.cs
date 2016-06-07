using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;

namespace Vodovoz.Domain.Client
{

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
			uow.Root.Contract = uow.GetById<CounterpartyContract>(contract.Id);
			uow.Root.AgreementNumber = AdditionalAgreement.GetNumber (uow.Root.Contract);
			return uow;
		}
	}
	
}
