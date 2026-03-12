using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Domain.Extensions
{
	/// <summary>
	/// Методы расширения для преобразования строки ответа от API Честного Знака в статус регистрации в Честном Знаке
	/// </summary>
	public static class TrueMarkRegistrationStatusExtensions
	{
		/// <summary>
		/// Преобразование строки ответа значения статуса от API Честного Знака в статус регистрации в Честном Знаке
		/// </summary>
		public static RegistrationInChestnyZnakStatus? ToRegistrationInChestnyZnakStatus(this string apiResponseStatus)
		{
			switch(apiResponseStatus)
			{
				case "Зарегистрирован":
				case "Восстановлен":
					return RegistrationInChestnyZnakStatus.Registered;
				case "Предварительная регистрация началась":
				case "Предварительная регистрация производителя":
				case "Предварительная регистрация продавца":
					return RegistrationInChestnyZnakStatus.InProcess;
				case "Заблокирован":
					return RegistrationInChestnyZnakStatus.Blocked;
				case "Не зарегистрирован":
				case "Удален":
					return RegistrationInChestnyZnakStatus.Unknown;
				default:
					return null;
			}
		}
	}
}
