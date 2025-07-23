using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
		/// <param name="userPhone">Номер телефона</param>
		/// <returns>Список результатов с ошибками или пустой список</returns>
		IEnumerable<ValidationResult> Validate(string userPhone);
	}
}
