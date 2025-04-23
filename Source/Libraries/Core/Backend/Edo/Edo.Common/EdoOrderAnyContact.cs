using OneOf;
using Vodovoz.Core.Domain.Contacts;

namespace Edo.Common
{
	/// <summary>
	/// Контакт заказа ЭДО
	/// </summary>
	public class EdoOrderAnyContact : OneOfBase<EmailEntity, PhoneEntity>
	{
		public EdoOrderAnyContact(OneOf<EmailEntity, PhoneEntity> input) : base(input)
		{
		}

		private string GetPhone(PhoneEntity phone)
		{
			if(phone == null)
			{
				return null;
			}

			return "+7" + phone.DigitsNumber;
		}

		public bool IsValid => Match(
			email => email?.IsValidEmail ?? false,
			phone => phone?.IsValidPhoneNumber ?? false);
		
		public string StringValue => Match(
			email => email?.Address,
			phone => GetPhone(phone));
	}
}
