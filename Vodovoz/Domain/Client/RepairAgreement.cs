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
	
}
