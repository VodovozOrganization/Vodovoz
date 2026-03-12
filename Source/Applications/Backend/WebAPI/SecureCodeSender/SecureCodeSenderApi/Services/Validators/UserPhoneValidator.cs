using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Utilities.Numeric;
using Vodovoz.Core.Domain.SecureCodes;

namespace SecureCodeSenderApi.Services.Validators
{
	public class UserPhoneValidator : IUserPhoneValidator
	{
		private readonly PhoneValidator _phoneValidator;
		private readonly IEmailMethodValidator _emailMethodValidator;

		public UserPhoneValidator(
			PhoneValidator phoneValidator,
			IEmailMethodValidator emailMethodValidator)
		{
			_phoneValidator = phoneValidator ?? throw new ArgumentNullException(nameof(phoneValidator));
			_emailMethodValidator = emailMethodValidator ?? throw new ArgumentNullException(nameof(emailMethodValidator));
		}
		
		public IEnumerable<ValidationResult> Validate(SendTo sendTo, string userPhone, string target)
		{
			if(sendTo is SendTo.Phone or SendTo.Telegram)
			{
				if(string.IsNullOrWhiteSpace(target))
				{
					yield return new ValidationResult($"Должен быть заполнен номер телефона в параметре { nameof(target) }");
				}
				else
				{
					var result = _phoneValidator.Validate('+' + target, onlyMobile: true);

					if(!result)
					{
						yield return new ValidationResult($"Передан неверный формат номера телефона в параметре {nameof(target)}");
					}
				}
			}

			foreach (var validationResult in ValidateUserPhone(userPhone))
			{
				yield return validationResult;
			}
		}

		private IEnumerable<ValidationResult> ValidateUserPhone(string userPhone)
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
					yield return new ValidationResult($"Передан неверный формат номера телефона в параметре {nameof(userPhone)}");
				}
			}
		}

		public IEnumerable<ValidationResult> Validate(string userPhone, string target)
		{
			if(string.IsNullOrWhiteSpace(target))
			{
				yield return new ValidationResult($"Должен быть заполнен номер телефона, почта и т.д. в параметре { nameof(target) }");
			}
			else
			{
				var resultPhoneCheck = _phoneValidator.Validate('+' + userPhone, onlyMobile: true);

				if(!resultPhoneCheck)
				{
					var emailCheckResult = _emailMethodValidator.ValidateEmailFormat(target);

					if(emailCheckResult.Any())
					{
						yield return new ValidationResult($"Передан неверный формат в параметре {nameof(target)}");
					}
				}
			}
			
			foreach (var validationResult in ValidateUserPhone(userPhone))
			{
				yield return validationResult;
			}
		}
	}
}
