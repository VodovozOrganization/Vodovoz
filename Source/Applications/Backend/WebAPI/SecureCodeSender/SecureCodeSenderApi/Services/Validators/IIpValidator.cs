using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SecureCodeSenderApi.Services.Validators
{
	/// <summary>
	/// Интерфейс валидатора ip адреса
	/// </summary>
	public interface IIpValidator
	{
		/// <summary>
		/// Проверка ip адреса
		/// </summary>
		/// <param name="ip">Ip адрес</param>
		/// <returns>Список результатов с ошибками или пустой список</returns>
		IEnumerable<ValidationResult> Validate(string ip);
	}
}
