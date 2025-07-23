using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.Utilities.Numeric;

namespace SecureCodeSenderApi.Services.Validators
{
	public class UserPhoneValidator : IUserPhoneValidator
	{
		private readonly PhoneValidator _phoneValidator;

		public UserPhoneValidator(PhoneValidator phoneValidator)
		{
			_phoneValidator = phoneValidator ?? throw new ArgumentNullException(nameof(phoneValidator));
		}
		
		public IEnumerable<ValidationResult> Validate(string userPhone)
		{
			if(string.IsNullOrWhiteSpace(userPhone))
			{
				yield return new ValidationResult("Телефон пользователя должен быть заполнен");
			}
			else
			{
				var result = _phoneValidator.Validate('+' + userPhone, onlyMobile: true);

				if(!result)
				{
					yield return new ValidationResult("Передан неверный формат номера телефона");
				}
			}
		}
	}
}
