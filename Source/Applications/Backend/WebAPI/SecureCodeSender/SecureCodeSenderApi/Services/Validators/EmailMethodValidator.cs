using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Core.Domain.SecureCodes;

namespace SecureCodeSenderApi.Services.Validators
{
	public class EmailMethodValidator : IEmailMethodValidator
	{
		public IEnumerable<ValidationResult> Validate(string target, SendTo sendTo = SendTo.Email)
		{
			if(sendTo != SendTo.Email)
			{
				yield return new ValidationResult("Отправка не на электронную почту не поддерживается");
			}
			else
			{
				if(string.IsNullOrWhiteSpace(target))
				{
					yield return new ValidationResult("Должен быть заполнен адрес электронной почты");
				}
				else
				{
					if(!Regex.IsMatch(target, EmailEntity.EmailRegEx, RegexOptions.None, TimeSpan.FromSeconds(1)))
					{
						yield return new ValidationResult("Неизвестный формат электронной почты");
					}
				}
			}
		}
	}
}
