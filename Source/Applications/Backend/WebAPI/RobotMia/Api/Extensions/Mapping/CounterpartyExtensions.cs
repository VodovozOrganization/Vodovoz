using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.RobotMia.Api.Extensions.Mapping;
using Vodovoz.RobotMia.Contracts.Responses.V1;

namespace Vodovoz.RobotMia.Api.Extensions.Mapping
{
	/// <summary>
	/// Расширение функционала <see cref="Counterparty"/>
	/// </summary>
	public static class CounterpartyExtensions
	{
		/// <summary>
		/// Маппинг контрагента в <see cref="CounterpartyDto"/>
		/// </summary>
		/// <returns></returns>
		public static CounterpartyDto MapToCounterpartyDtoV1(this Counterparty counterparty) => new CounterpartyDto
		{
			Id = counterparty.Id,
			DeliveryPoints = counterparty.DeliveryPoints.MapToDeliveryPointDtoV1(),
			Fio = counterparty.FullName,
			Inn = counterparty.INN,
			Type = counterparty.PersonType.MapToApiPersonTypeV1()
		};

		/// <summary>
		/// Маппинг контрагентов в IEnumerable <see cref="CounterpartyDto"/>
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<CounterpartyDto> MapToCounterpartyDtoV1(this IEnumerable<Counterparty> counterparties)
			=> counterparties.Select(MapToCounterpartyDtoV1);
	}
}
