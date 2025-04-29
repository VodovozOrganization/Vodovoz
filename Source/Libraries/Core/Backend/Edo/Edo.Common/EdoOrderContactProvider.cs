using Core.Infrastructure;
using System.Linq;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Core.Domain.Orders;

namespace Edo.Common
{
	public class EdoOrderContactProvider : IEdoOrderContactProvider
	{
		/// <summary>
		/// Возврат первого попавшегося контакта из цепочки:<br/>
		/// 0. Почта для чеков в контрагенте<br/>
		/// 1. Почта для счетов в контрагенте<br/>
		/// 2. Телефон для чеков в точке доставки<br/>
		/// 3. Телефон для чеков в контрагенте<br/>
		/// 4. Телефон личный в ТД<br/>
		/// 5. Телефон личный в контрагенте<br/>
		/// 6. Иная почта в контрагенте<br/>
		/// 7. Городской телефон в ТД<br/>
		/// 8. Городской телефон в контрагенте<br/>
		/// </summary>
		/// <returns>Контакт с минимальным весом.<br/>Телефоны возвращает в формате +7</returns>
		public virtual EdoOrderAnyContact GetContact(OrderEntity order)
		{
			if(order.Client == null)
			{
				throw new OrderContactMissingException();
			}
			EdoOrderAnyContact result;

			// 0. Почта для чеков в контрагенте
			result = new EdoOrderAnyContact(GetCounterpartyEmailForReceipt(order));
			if(!string.IsNullOrWhiteSpace(result.StringValue))
			{
				return result;
			}

			// 1. Почта для счетов в контрагенте
			result = new EdoOrderAnyContact(GetCounterpartyEmailForBills(order));
			if(!string.IsNullOrWhiteSpace(result.StringValue))
			{
				return result;
			}

			// 2. Телефон для чеков в точке доставки
			result = new EdoOrderAnyContact(GetDeliveryPointPhoneForReceipts(order));
			if(!string.IsNullOrWhiteSpace(result.StringValue))
			{
				return result;
			}

			// 3. Телефон для чеков в контрагенте
			result = new EdoOrderAnyContact( GetCounterpartyPhoneForReceipts(order));
			if(!string.IsNullOrWhiteSpace(result.StringValue))
			{
				return result;
			}

			// 4. Телефон личный в ТД
			result = new EdoOrderAnyContact(GetDeliveryPointPersonalPhone(order));
			if(!string.IsNullOrWhiteSpace(result.StringValue))
			{
				return result;
			}

			// 5. Телефон личный в контрагенте
			result = new EdoOrderAnyContact(GetCounterpartyPersonalPhone(order));
			if(!string.IsNullOrWhiteSpace(result.StringValue))
			{
				return result;
			}

			// 6. Иная почта в контрагенте
			result = new EdoOrderAnyContact(GetCounterpartyOtherEmail(order));
			if(!string.IsNullOrWhiteSpace(result.StringValue))
			{
				return result;
			}

			// 7. Городской телефон в ТД
			result = new EdoOrderAnyContact(GetDeliveryPointCityPhone(order));
			if(!string.IsNullOrWhiteSpace(result.StringValue))
			{
				return result;
			}

			// 8. Городской телефон в контрагенте
			result = new EdoOrderAnyContact(GetCounterpartyCityPhone(order));
			if(!string.IsNullOrWhiteSpace(result.StringValue))
			{
				return result;
			}

			throw new OrderContactMissingException();
		}

		/// <summary>
		/// 0. Почта для чеков в контрагенте
		/// </summary>
		private EmailEntity GetCounterpartyEmailForReceipt(OrderEntity order)
		{
			var receiptEmail = order.Client.Emails
				.Where(e => !e.Address.IsNullOrWhiteSpace())
				.Where(e => e.EmailType?.EmailPurpose == EmailPurpose.ForReceipts)
				.FirstOrDefault();

			return receiptEmail;
		}

		/// <summary>
		/// 1. Почта для счетов в контрагенте
		/// </summary>
		private EmailEntity GetCounterpartyEmailForBills(OrderEntity order)
		{
			var receiptEmail = order.Client.Emails
				.Where(e => !e.Address.IsNullOrWhiteSpace())
				.Where(e => e.EmailType?.EmailPurpose == EmailPurpose.ForBills)
				.FirstOrDefault();

			return receiptEmail;
		}

		/// <summary>
		/// 2. Телефон для чеков в точке доставки
		/// </summary>
		private PhoneEntity GetDeliveryPointPhoneForReceipts(OrderEntity order)
		{
			if(order.DeliveryPoint == null)
			{
				return null;
			}

			var phone = order.DeliveryPoint.Phones
				.Where(p => !p.DigitsNumber.IsNullOrWhiteSpace())
				.Where(p => !p.IsArchive)
				.Where(p => p.PhoneType?.PhonePurpose == PhonePurpose.ForReceipts)
				.FirstOrDefault();

			return phone;
		}

		/// <summary>
		/// 3. Телефон для чеков в контрагенте
		/// </summary>
		private PhoneEntity GetCounterpartyPhoneForReceipts(OrderEntity order)
		{
			var phone = order.Client.Phones
				.Where(p => !p.DigitsNumber.IsNullOrWhiteSpace())
				.Where(p => !p.IsArchive)
				.Where(p => p.PhoneType?.PhonePurpose == PhonePurpose.ForReceipts)
				.FirstOrDefault();

			return phone;
		}

		/// <summary>
		/// 4. Телефон личный в ТД
		/// </summary>
		private PhoneEntity GetDeliveryPointPersonalPhone(OrderEntity order)
		{
			if(order.DeliveryPoint == null)
			{
				return null;
			}

			var phone = order.DeliveryPoint.Phones
				.Where(p => !p.DigitsNumber.IsNullOrWhiteSpace())
				.Where(p => !p.IsArchive)
				.Where(p => p.DigitsNumber.Substring(0, 1) == "9")
				.FirstOrDefault();

			return phone;
		}


		/// <summary>
		/// 5. Телефон личный в контрагенте
		/// </summary>
		private PhoneEntity GetCounterpartyPersonalPhone(OrderEntity order)
		{
			var phone = order.Client.Phones
				.Where(p => !p.DigitsNumber.IsNullOrWhiteSpace())
				.Where(p => !p.IsArchive)
				.Where(p => p.DigitsNumber.Substring(0, 1) == "9")
				.FirstOrDefault();
			
			return phone;
		}


		/// <summary>
		/// 6. Иная почта в контрагенте
		/// </summary>
		private EmailEntity GetCounterpartyOtherEmail(OrderEntity order)
		{
			var email = order.Client.Emails
				.Where(e => !e.Address.IsNullOrWhiteSpace())
				.Where(e => e.EmailType?.EmailPurpose != EmailPurpose.ForBills)
				.Where(e => e.EmailType?.EmailPurpose != EmailPurpose.ForReceipts)
				.FirstOrDefault();
			
			return email;
		}

		/// <summary>
		/// 7. Городской телефон в ТД
		/// </summary>
		private PhoneEntity GetDeliveryPointCityPhone(OrderEntity order)
		{
			if(order.DeliveryPoint == null)
			{
				return null;
			}

			var phone = order.DeliveryPoint.Phones
				.Where(p => !p.DigitsNumber.IsNullOrWhiteSpace())
				.Where(p => !p.IsArchive)
				.Where(p => p.DigitsNumber.Substring(0, 1) != "9")
				.FirstOrDefault();
			
			return phone;
		}

		/// <summary>
		/// 8. Городской телефон в контрагенте
		/// </summary>
		private PhoneEntity GetCounterpartyCityPhone(OrderEntity order)
		{
			var phone = order.Client.Phones
				.Where(p => !p.DigitsNumber.IsNullOrWhiteSpace())
				.Where(p => !p.IsArchive)
				.Where(p => p.DigitsNumber.Substring(0, 1) != "9")
				.FirstOrDefault();
			
			return phone;
		}
	}
}
