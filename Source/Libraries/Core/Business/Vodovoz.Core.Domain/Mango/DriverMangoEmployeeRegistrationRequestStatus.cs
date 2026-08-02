using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Mango
{
	/// <summary>
	/// Статус заявки на регистрацию водителя как сотрудника Манго
	/// </summary>
	public enum DriverMangoEmployeeRegistrationRequestStatus
	{
		/// <summary>
		/// Новая, ещё не обработанная заявка
		/// </summary>
		[Display(Name = "Новый")]
		New,

		/// <summary>
		/// Заявка успешно обработана, сотрудник создан в Манго
		/// </summary>
		[Display(Name = "Успешно")]
		Completed,

		/// <summary>
		/// Обработка завершилась ошибкой, см. сообщение об ошибке
		/// </summary>
		[Display(Name = "Ошибка")]
		Error,

		/// <summary>
		/// У водителя уже есть активный добавочный номер, повторная регистрация не требуется
		/// </summary>
		[Display(Name = "Повторный запрос")]
		Duplicate
	}
}
