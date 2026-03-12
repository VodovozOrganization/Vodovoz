using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.SecureCodes;

namespace SecureCodeSenderApi.Services.Validators
{
	/// <summary>
	/// Интерфейс валидатора номера телефона
	/// </summary>
	public interface IUserPhoneValidator
	{
		/// <summary>
		/// Проверка телефона пользователя
		/// </summary>
		/// <param name="sendTo">Куда отправка <see cref="SendTo"/></param>
		/// <param name="userPhone">Номер телефона пользователя</param>
		/// <param name="target">Номер телефона для отправки</param>
		/// <returns>Список результатов с ошибками или пустой список</returns>
		IEnumerable<ValidationResult> Validate(SendTo sendTo, string userPhone, string target);
		/// <summary>
		/// Проверка телефона пользователя
		/// </summary>
		/// <param name="userPhone">Номер телефона пользователя</param>
		/// <param name="target">Номер телефона для отправки</param>
		/// <returns>Список результатов с ошибками или пустой список</returns>
		IEnumerable<ValidationResult> Validate(string userPhone, string target);
	}
}
