using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Clients;
using Vodovoz.Core.Data.Goods;
using Vodovoz.Domain.Client;

namespace Vodovoz.Converters
{
	public class CounterpartyConverter : ICounterpartyConverter
	{
		private readonly ISpecialNomenclatureConverter _specialNomenclatureConverter;

		public CounterpartyConverter(ISpecialNomenclatureConverter specialNomenclatureConverter)
		{
			_specialNomenclatureConverter =
				specialNomenclatureConverter ?? throw new ArgumentNullException(nameof(specialNomenclatureConverter));
		}
		
		public CounterpartyInfoForEdo ConvertCounterpartyToCounterpartyInfoForEdo(Counterparty counterparty)
		{
			var specialNomenclatures = ConvertCounterpartyDeliveryPointsToDeliveryPointInfoForEdo(counterparty.SpecialNomenclatures);
			
			var counterpartyInfo = new CounterpartyInfoForEdo
			{
				Id = counterparty.Id,
				INN = counterparty.INN,
				KPP = counterparty.KPP,
				FullName = counterparty.FullName,
				PersonType = counterparty.PersonType,
				SpecialCustomer = counterparty.SpecialCustomer,
				PayerSpecialKPP = counterparty.PayerSpecialKPP,
				CargoReceiver = counterparty.CargoReceiver,
				SpecialContractName = counterparty.SpecialContractName,
				SpecialContractDate = counterparty.SpecialContractDate,
				SpecialContractNumber = counterparty.SpecialContractNumber,
				UseSpecialDocFields = counterparty.UseSpecialDocFields,
				JurAddress = counterparty.JurAddress,
				PersonalAccountIdInEdo = counterparty.PersonalAccountIdInEdo,
				ReasonForLeaving = counterparty.ReasonForLeaving,
				CargoReceiverSource = counterparty.CargoReceiverSource,
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
