using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.SecureCodes;

namespace SecureCodeSenderApi.Services.Validators
{
	/// <summary>
	/// Интерфейс валидатора отправки по email
	/// </summary>
	public interface IEmailMethodValidator
	{
		/// <summary>
		/// Проверка отправки по email
		/// </summary>
		/// <param name="target">Назначение отправки</param>
		/// <param name="sendTo">Куда отправляется код <see cref="SendTo"/></param>
		/// <returns>Список результатов с ошибками или пустой список</returns>
		IEnumerable<ValidationResult> Validate(string target, SendTo sendTo = SendTo.Email);
	}
}
