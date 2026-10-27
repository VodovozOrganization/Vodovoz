using System.ComponentModel;
using CustomerOrdersApi.Library.V7.Dto.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Sale.RequestsForCall;
using RequestForCallType = CustomerOrdersApi.Library.V7.Dto.Orders.RequestsForCall.RequestForCallType;

namespace CustomerOrdersApi.Library.V7.Extensions
{
	public static class CreatingRequestForCallDtoExtensions
	{
		public static RequestForCallBase ToRequestForCall(
			this CreatingRequestForCallDto source,
			Nomenclature nomenclature,
			Counterparty counterparty
			)
		{
			switch(source.Type)
			{
				case RequestForCallType.General:
					return RequestForCall.Create(
						source.Source,
						source.ContactName,
						source.PhoneNumber,
						nomenclature,
						counterparty
					);
				case RequestForCallType.Service:
					return ServiceRequestForCall.Create(
						source.Source,
						source.ContactName,
						source.PhoneNumber,
						nomenclature,
						counterparty
					);
				default:
					throw new InvalidEnumArgumentException(
						$"Неизвестный тип заявки на звонок {source.Type}. Нужно добавить это значение в {nameof(RequestForCallType)} апи заказов");
			}
		}
	}
}
