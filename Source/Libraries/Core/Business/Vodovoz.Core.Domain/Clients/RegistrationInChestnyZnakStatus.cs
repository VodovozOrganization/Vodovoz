using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	public enum RegistrationInChestnyZnakStatus
	{
		[Display(Name = "Неизвестно")]
		Unknown,
		[Display(Name = "В процессе регистрации")]
		InProcess,
		[Display(Name = "Зарегистрирован")]
		Registered,
		[Display(Name = "Заблокирован")]
		Blocked
	}
}
