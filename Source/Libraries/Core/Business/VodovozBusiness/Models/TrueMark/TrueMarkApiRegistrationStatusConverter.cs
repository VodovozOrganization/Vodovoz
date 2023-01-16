using Vodovoz.Domain.Client;

namespace TrueMarkApi.Library.Converters
{
	public class TrueMarkApiRegistrationStatusConverter
	{
		public RegistrationInChestnyZnakStatus? ConvertToChestnyZnakStatus(string apiResponseStatus)
		{
			switch(apiResponseStatus)
			{
				case "Зарегистрирован":
				case "Восстановлен":
					return RegistrationInChestnyZnakStatus.Registered;
					break;
				case "Предварительная регистрация началась":
				case "Предварительная регистрация производителя":
				case "Предварительная регистрация продавца":
					return RegistrationInChestnyZnakStatus.InProcess;
					break;
				case "Заблокирован":
					return RegistrationInChestnyZnakStatus.Blocked;
					break;
				case "Не зарегистрирован":
				case "Удален":
					return RegistrationInChestnyZnakStatus.Unknown;
				default:
					return null;
			}
		}
	}
}
