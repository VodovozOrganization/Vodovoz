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
		public virtual string GetContact(OrderEntity order)
		{
			if(order.Client == null)
			{
				throw new OrderContactMissingException();
			}
			string result;

			// 0. Почта для чеков в контрагенте
			result = GetCounterpartyEmailForReceipt(order);
			if(!result.IsNullOrWhiteSpace())
			{
				return result;
			}

			// 1. Почта для счетов в контрагенте
			result = GetCounterpartyEmailForBills(order);
			if(!result.IsNullOrWhiteSpace())
			{
				return result;
			}

			// 2. Телефон для чеков в точке доставки
			result = GetDeliveryPointPhoneForReceipts(order);
			if(!result.IsNullOrWhiteSpace())
			{
				return result;
			}

			// 3. Телефон для чеков в контрагенте
			result = GetCounterpartyPhoneForReceipts(order);
			if(!result.IsNullOrWhiteSpace())
			{
				return result;
			}

			// 4. Телефон личный в ТД
			result = GetDeliveryPointPersonalPhone(order);
			if(!result.IsNullOrWhiteSpace())
			{
				return result;
			}

			// 5. Телефон личный в контрагенте
			result = GetCounterpartyPersonalPhone(order);
			if(!result.IsNullOrWhiteSpace())
			{
				return result;
			}

			// 6. Иная почта в контрагенте
			result = GetCounterpartyOtherEmail(order);
			if(!result.IsNullOrWhiteSpace())
			{
				return result;
			}

			// 7. Городской телефон в ТД
			result = GetDeliveryPointCityPhone(order);
			if(!result.IsNullOrWhiteSpace())
			{
				return result;
			}

			// 8. Городской телефон в контрагенте
			result = GetCounterpartyCityPhone(order);
			if(!result.IsNullOrWhiteSpace())
			{
				return result;
			}

			throw new OrderContactMissingException();
		}

		/// <summary>
		/// 0. Почта для чеков в контрагенте
		/// </summary>
		private string GetCounterpartyEmailForReceipt(OrderEntity order)
		{
			var receiptEmail = order.Client.Emails
				.Where(e => !e.Address.IsNullOrWhiteSpace())
				.Where(e => e.EmailType?.EmailPurpose == EmailPurpose.ForReceipts)
				.FirstOrDefault();

			if(receiptEmail == null)
			{
				return null;
			}

			return receiptEmail.Address;
		}

		/// <summary>
		/// 1. Почта для счетов в контрагенте
		/// </summary>
		private string GetCounterpartyEmailForBills(OrderEntity order)
		{
			var receiptEmail = order.Client.Emails
				.Where(e => !e.Address.IsNullOrWhiteSpace())
				.Where(e => e.EmailType?.EmailPurpose == EmailPurpose.ForBills)
				.FirstOrDefault();

			if(receiptEmail == null)
			{
				return null;
			}

			return receiptEmail.Address;
		}

		/// <summary>
		/// 2. Телефон для чеков в точке доставки
		/// </summary>
		private string GetDeliveryPointPhoneForReceipts(OrderEntity order)
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

			return GetPhone(phone);
		}

		/// <summary>
		/// 3. Телефон для чеков в контрагенте
		/// </summary>
		private string GetCounterpartyPhoneForReceipts(OrderEntity order)
		{
			var phone = order.Client.Phones
				.Where(p => !p.DigitsNumber.IsNullOrWhiteSpace())
				.Where(p => !p.IsArchive)
				.Where(p => p.PhoneType?.PhonePurpose == PhonePurpose.ForReceipts)
				.FirstOrDefault();

			return GetPhone(phone);
		}

		/// <summary>
		/// 4. Телефон личный в ТД
		/// </summary>
		private string GetDeliveryPointPersonalPhone(OrderEntity order)
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

			return GetPhone(phone);
		}


		/// <summary>
		/// 5. Телефон личный в контрагенте
		/// </summary>
		private string GetCounterpartyPersonalPhone(OrderEntity order)
		{
			var phone = order.Client.Phones
				.Where(p => !p.DigitsNumber.IsNullOrWhiteSpace())
				.Where(p => !p.IsArchive)
				.Where(p => p.DigitsNumber.Substring(0, 1) == "9")
				.FirstOrDefault();
			return GetPhone(phone);
		}


		/// <summary>
		/// 6. Иная почта в контрагенте
		/// </summary>
		private string GetCounterpartyOtherEmail(OrderEntity order)
		{
			var email = order.Client.Emails
				.Where(e => !e.Address.IsNullOrWhiteSpace())
				.Where(e => e.EmailType?.EmailPurpose != EmailPurpose.ForBills)
				.Where(e => e.EmailType?.EmailPurpose != EmailPurpose.ForReceipts)
				.FirstOrDefault();
			if(email == null)
			{
				return null;
			}
			return email.Address;
		}

		/// <summary>
		/// 7. Городской телефон в ТД
		/// </summary>
		private string GetDeliveryPointCityPhone(OrderEntity order)
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
			return GetPhone(phone);
		}

		/// <summary>
		/// 8. Городской телефон в контрагенте
		/// </summary>
		private string GetCounterpartyCityPhone(OrderEntity order)
		{
			var phone = order.Client.Phones
				.Where(p => !p.DigitsNumber.IsNullOrWhiteSpace())
				.Where(p => !p.IsArchive)
				.Where(p => p.DigitsNumber.Substring(0, 1) != "9")
				.FirstOrDefault();
			return GetPhone(phone);
		}

		private string GetPhone(PhoneEntity phone)
		{
			if(phone == null)
			{
				return null;
			}

			return "+7" + phone.DigitsNumber;
		}
	}
}
