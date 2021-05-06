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
                Items = GetCalculatedSmsPaymentItemDTOs(order.OrderItems)
            };

            return newSmsPaymentDTO;
        }

        private List<SmsPaymentItemDTO> GetCalculatedSmsPaymentItemDTOs(IList<OrderItem> itemList)
        {
            List<SmsPaymentItemDTO> smsPaymentDTOList = new List<SmsPaymentItemDTO>();

            foreach (var item in itemList)
            {
                decimal price = decimal.Round(item.Sum / item.Count, 2, MidpointRounding.AwayFromZero);
                bool isDevided = item.Sum == price * item.Count;

                if (isDevided)
                {
                    smsPaymentDTOList.Add(
                        new SmsPaymentItemDTO()
                        {
                            Name = item.Nomenclature.OfficialName,
                            Quantity = item.CurrentCount,
                            Price = price
                        });
                }
                else
                {
                    smsPaymentDTOList.Add(
                        new SmsPaymentItemDTO()
                        {
                            Name = item.Nomenclature.OfficialName,
                            Quantity = item.CurrentCount - 1,
                            Price = price
                        });

                    smsPaymentDTOList.Add(
                        new SmsPaymentItemDTO()
                        {
                            Name = item.Nomenclature.OfficialName,
                            Quantity = 1,
                            Price = item.Sum - (price * (item.Count - 1))
                        });
                }
            }

            return smsPaymentDTOList;
        }
    }
}
