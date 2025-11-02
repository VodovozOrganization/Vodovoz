using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Models.TrueMark
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
