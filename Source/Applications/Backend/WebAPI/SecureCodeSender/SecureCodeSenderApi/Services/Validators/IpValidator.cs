using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SecureCodeSenderApi.Services.Validators
{
	public class IpValidator : IIpValidator
	{
		private const string _ipV4Pattern = @"\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.|$)){4}\b";
		
		public IEnumerable<ValidationResult> Validate(string ip)
		{
			if(string.IsNullOrWhiteSpace(ip))
			{
				yield return new ValidationResult("Должен быть заполнен Ip адрес");
			}
			else
			{

				if(!Regex.IsMatch(ip, _ipV4Pattern))
				{
					yield return new ValidationResult("Передан неверный формат ip адреса");
				}
			}
		}
	}
}
