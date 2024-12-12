using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Complaints
{
	public enum ComplaintStatuses
	{
		[Display(Name = "Не взята в работу")]
		NotTakenInProcess,
		[Display(Name = "В работе")]
		InProcess,
		[Display(Name = "На проверке")]
		Checking,
		[Display(Name = "Ожидает реакции")]
		WaitingForReaction,
		[Display(Name = "Закрыт")]
		Closed
	}
}
