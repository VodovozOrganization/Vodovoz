using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SecureCodeSenderApi.Services.Validators
{
	/// <summary>
	/// Интерфейс валидатора кода авторизации
	/// </summary>
	public interface ISecureCodeValidator
	{
		/// <summary>
		/// Проверка кода авторизации
		/// </summary>
		/// <param name="secureCode">Код авторизации</param>
		/// <returns>Список результатов с ошибками или пустой список</returns>
		IEnumerable<ValidationResult> Validate(string secureCode);
	}
}
