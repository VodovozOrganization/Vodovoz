using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Models;

namespace SmsPaymentService
{
	public class SmsPaymentDTOFactory : ISmsPaymentDTOFactory
	{
		private readonly IOrganizationProvider _organizationProvider;

		public SmsPaymentDTOFactory(IOrganizationProvider organizationProvider)
		{
			_organizationProvider = organizationProvider ?? throw new ArgumentNullException(nameof(organizationProvider));
		}

		public SmsPaymentDTO CreateSmsPaymentDTO(IUnitOfWork uow, SmsPayment smsPayment, Order order, PaymentFrom paymentFrom)
		{
			if(order == null)
			{
				throw new ArgumentNullException(nameof(order));
			}
			if(!order.DeliveryDate.HasValue)
			{
				throw new InvalidOperationException("Order delivery date cannot be null");
			}

			var newSmsPaymentDTO = new SmsPaymentDTO
			{
				Recepient = smsPayment.Recepient.Name,
				RecepientId = smsPayment.Recepient.Id,
				PhoneNumber = smsPayment.PhoneNumber,
				PaymentStatus = SmsPaymentStatus.WaitingForPayment,
				OrderId = smsPayment.Order.Id,
				PaymentCreationDate = smsPayment.CreationDate,
				Amount = smsPayment.Amount,
				RecepientType = smsPayment.Recepient.PersonType,
				Items = GetCalculatedSmsPaymentItemDTOs(order.OrderItems),
				OrganizationId =
					_organizationProvider.GetOrganization(
						uow,
						PaymentType.ByCard,
						order.SelfDelivery,
						order.DeliveryDate.Value,
						order.OrderItems,
						paymentFrom,
						order.DeliveryPoint?.District?.GeographicGroup
					).Id
			};

			return newSmsPaymentDTO;
		}

		private List<SmsPaymentItemDTO> GetCalculatedSmsPaymentItemDTOs(IList<OrderItem> itemList)
		{
			List<SmsPaymentItemDTO> smsPaymentDTOList = new List<SmsPaymentItemDTO>();

			SmsPaymentItemDTO compensatingItem = null;
			decimal remains = 0;

			foreach(var item in itemList)
			{
				decimal price = decimal.Round(item.ActualSum / item.CurrentCount, 2, MidpointRounding.AwayFromZero);
				bool isDivided = item.ActualSum == price * item.CurrentCount;

				if(isDivided)
				{
					smsPaymentDTOList.Add(
						new SmsPaymentItemDTO
						{
							Name = item.Nomenclature.OfficialName,
							Quantity = item.CurrentCount,
							Price = price
						});
				}
				else
				{
					smsPaymentDTOList.Add(
						new SmsPaymentItemDTO
						{
							Name = item.Nomenclature.OfficialName,
							Quantity = item.CurrentCount - (compensatingItem == null ? 1 : 0),
							Price = price
						});

					remains += item.ActualSum - price * item.CurrentCount;

					if(compensatingItem == null)
					{
						compensatingItem = new SmsPaymentItemDTO
						{
							Name = item.Nomenclature.OfficialName,
							Quantity = 1,
							Price = price
						};

						smsPaymentDTOList.Add(compensatingItem);
					}
				}
			}

			if(compensatingItem != null)
			{
				compensatingItem.Price += remains;
			}

			return smsPaymentDTOList;
		}
	}
}
