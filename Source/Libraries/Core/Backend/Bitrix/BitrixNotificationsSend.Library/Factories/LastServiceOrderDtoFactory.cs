using BitrixNotificationsSend.Contracts.Dto;
using System;
using Vodovoz.Core.Domain.Orders;

namespace BitrixNotificationsSend.Library.Factories
{
	/// <inheritdoc cref="ILastServiceOrderDtoFactory"/>
	public class LastServiceOrderDtoFactory : ILastServiceOrderDtoFactory
	{
		/// <inheritdoc/>
		public LastServiceOrderDto CreateLastServiceOrderDto(LastServiceOrder lastServiceOrder)
		{
			if(lastServiceOrder is null)
			{
				throw new ArgumentNullException(nameof(lastServiceOrder));
			}

			return new LastServiceOrderDto
			{
				LastServiceOrderId = lastServiceOrder.Id,
				CounterpartyId = lastServiceOrder.CounterpartyId,
				CounterpartyName = lastServiceOrder.CounterpartyName,
				DeliveryPointAddress = lastServiceOrder.DeliveryPointAddress,
				PhoneNumber = lastServiceOrder.PhoneNumber,
				EmailAddress = lastServiceOrder.EmailAddress,
				LastOrderDeliveryDate = lastServiceOrder.LastOrderDeliveryDate
			};
		}
	}
}
