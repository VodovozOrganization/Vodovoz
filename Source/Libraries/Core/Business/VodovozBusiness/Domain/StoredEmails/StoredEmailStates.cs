using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.StoredEmails
{
	public enum StoredEmailStates
	{
		[Display(Name = "Подготовка к отправке")]
		PreparingToSend,
		[Display(Name = "Ожидание отправки")]
		WaitingToSend,
		[Display(Name = "Ошибка отправки")]
		SendingError,
		[Display(Name = "Успешно отправлено")]
		SendingComplete,
		[Display(Name = "Недоставлено")]
		Undelivered,
		[Display(Name = "Доставлено")]
		Delivered,
		[Display(Name = "Открыто")]
		Opened,
		[Display(Name = "Отмечено пользователем как спам")]
		MarkedAsSpam,
	}
}
