using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;

namespace SmsPaymentService
{
    [DataContract]
    public class SmsPaymentDTO
    {
        /// <summary>
        /// Сумма в рублях
        /// </summary>
        [DataMember] public decimal Amount { get; set; }
        
        /// <summary>
        /// Номер заказа
        /// </summary>
        [DataMember] public int OrderId { get; set; }
        
        /// <summary>
        /// Время создания платежа
        /// </summary>
        [DataMember] public DateTime PaymentCreationDate { get; set; }
        
        /// <summary>
        /// Время оплаты платежа
        /// </summary>
        [DataMember] public DateTime PaymentPaidDate { get; set; }
        
        /// <summary>
        /// Статус платежа
        /// </summary>
        [DataMember] public SmsPaymentStatus PaymentStatus { get; set; }
        
        /// <summary>
        /// Номер телефона клиента
        /// </summary>
        [DataMember] public string PhoneNumber { get; set; }
        
        /// <summary>
        /// Клиент
        /// </summary>
        [DataMember] public string Recepient { get; set; }
        
        /// <summary>
        /// Id клиента
        /// </summary>
        [DataMember] public int RecepientId { get; set; }
        
        /// <summary>
        /// Тип клиента
        /// </summary>
        [DataMember] public PersonType RecepientType { get; set; }
        
        /// <summary>
        /// Код организации, от имени которой отправится ссылка на оплату
        /// </summary>
        [DataMember] public int OrganizationId { get; set; }
        
        /// <summary>
        /// Товары
        /// </summary>
        [DataMember] public IList<SmsPaymentItemDTO> Items { get; set; }
    }

    public class SmsPaymentItemDTO
    {
        [DataMember] public string Name { get; set; }
        [DataMember] public decimal Quantity { get; set; }
        [DataMember] public decimal Price { get; set; }
    }
}
