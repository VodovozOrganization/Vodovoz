using System;
using System.Collections.Generic;
using System.Linq;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdo.Contracts.Goods;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using VodovozBusiness.Converters;
using VodovozBusiness.Domain.Client;

namespace Vodovoz.Converters
{
	public class CounterpartyConverter : ICounterpartyConverter
	{
		private readonly ISpecialNomenclatureConverter _specialNomenclatureConverter;
		private readonly IPersonTypeConverter _personTypeConverter;
		private readonly IReasonForLeavingConverter _reasonForLeavingConverter;
		private readonly ICargoReceiverSourceConverter _cargoReceiverSourceConverter;

		public CounterpartyConverter(
			ISpecialNomenclatureConverter specialNomenclatureConverter,
			IPersonTypeConverter personTypeConverter,
			IReasonForLeavingConverter reasonForLeavingConverter,
			ICargoReceiverSourceConverter cargoReceiverSourceConverter)
		{
			_specialNomenclatureConverter =
				specialNomenclatureConverter ?? throw new ArgumentNullException(nameof(specialNomenclatureConverter));
			_personTypeConverter = personTypeConverter ?? throw new ArgumentNullException(nameof(personTypeConverter));
			_reasonForLeavingConverter = reasonForLeavingConverter ?? throw new ArgumentNullException(nameof(reasonForLeavingConverter));
			_cargoReceiverSourceConverter =
				cargoReceiverSourceConverter ?? throw new ArgumentNullException(nameof(cargoReceiverSourceConverter));
		}
		
		public CounterpartyInfoForEdo ConvertCounterpartyToCounterpartyInfoForEdo(
			Counterparty counterparty, CounterpartyEdoAccount counterpartyEdoAccount)
		{
			var specialNomenclatures = ConvertCounterpartyDeliveryPointsToDeliveryPointInfoForEdo(counterparty.SpecialNomenclatures);
			
			var counterpartyInfo = new CounterpartyInfoForEdo
			{
				Id = counterparty.Id,
				Inn = counterparty.INN,
				Kpp = counterparty.KPP,
				FullName = counterparty.FullName,
				PersonType = _personTypeConverter.ConvertPersonTypeToCounterpartyInfoType(counterparty.PersonType),
				SpecialCustomer = counterparty.SpecialCustomer,
				PayerSpecialKpp = counterparty.PayerSpecialKPP,
				CargoReceiver = counterparty.CargoReceiver,
				SpecialContractName = counterparty.SpecialContractName,
				SpecialContractDate = counterparty.SpecialContractDate,
				SpecialContractNumber = counterparty.SpecialContractNumber,
				UseSpecialDocFields = counterparty.UseSpecialDocFields,
				JurAddress = counterparty.JurAddress,
				PersonalAccountIdInEdo = counterpartyEdoAccount.PersonalAccountIdInEdo,
				ReasonForLeaving = _reasonForLeavingConverter.ConvertReasonForLeavingToReasonForLeavingType(counterparty.ReasonForLeaving),
				CargoReceiverSource = 
					_cargoReceiverSourceConverter.ConvertCargoReceiverSourceToCargoReceiverSourceType(counterparty.CargoReceiverSource),
				SpecialNomenclatures = specialNomenclatures
			};

			return counterpartyInfo;
		}

		private IList<SpecialNomenclatureInfoForEdo> ConvertCounterpartyDeliveryPointsToDeliveryPointInfoForEdo(
			IEnumerable<SpecialNomenclature> specialNomenclatures)
		{
			return specialNomenclatures.Select(specialNomenclature =>
				_specialNomenclatureConverter.ConvertSpecialNomenclatureToSpecialNomenclatureInfoForEdo(specialNomenclature))
				.ToList();
		}
	}
}
