using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SecureCodeSenderApi.Services.Validators
{
	public class SecureCodeValidator : ISecureCodeValidator
	{
		private const string _codePattern = @"^\d{6}$";
		
		public IEnumerable<ValidationResult> Validate(string secureCode)
		{
			if(string.IsNullOrWhiteSpace(secureCode))
			{
				yield return new ValidationResult("Должен быть заполнен код авторизации");
			}
			else
			{
				if(!Regex.IsMatch(secureCode, _codePattern, RegexOptions.None, TimeSpan.FromSeconds(1)))
				{
					yield return new ValidationResult("Код авторизации имеет неверный формат");
				}
			}
		}
	}
}
