using System;
using System.Collections.Generic;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;

namespace SmsPaymentService
{
    public class SmsPaymentDTOFactory
    {
        public SmsPaymentDTO CreateSmsPaymentDTO(SmsPayment smsPayment, Order order)
        {
            var newSmsPaymentDTO = new SmsPaymentDTO {
                Recepient = smsPayment.Recepient.Name,
                RecepientId = smsPayment.Recepient.Id,
                PhoneNumber = smsPayment.PhoneNumber,
                PaymentStatus = SmsPaymentStatus.WaitingForPayment,
                OrderId = smsPayment.Order.Id,
                PaymentCreationDate = smsPayment.CreationDate,
                Amount = smsPayment.Amount,
                RecepientType = smsPayment.Recepient.PersonType,
                Items = new List<SmsPaymentItemDTO>()
            };

            foreach(var orderItem in order.OrderItems) {
                newSmsPaymentDTO.Items.Add(new SmsPaymentItemDTO {
                    Name = orderItem.Nomenclature.OfficialName,
                    Quantity = orderItem.CurrentCount,
                    Price = orderItem.Sum / orderItem.Count
                });
            }

            return newSmsPaymentDTO;
        }
    }
}