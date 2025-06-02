using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Complaints
{
	/// <summary>
	/// Статусы рекламаций
	/// </summary>
	public enum ComplaintStatuses
	{
		/// <summary>
		/// Не взята в работу
		/// </summary>
		[Display(Name = "Не взята в работу")]
		NotTakenInProcess,
		/// <summary>
		/// В работе
		/// </summary>
		[Display(Name = "В работе")]
		InProcess,
		/// <summary>
		/// На проверке
		/// </summary>
		[Display(Name = "На проверке")]
		Checking,
		/// <summary>
		/// Ожидает реакции
		/// </summary>
		[Display(Name = "Ожидает реакции")]
		WaitingForReaction,
		/// <summary>
		/// Закрыта
		/// </summary>
		[Display(Name = "Закрыт")]
		Closed
	}
}
