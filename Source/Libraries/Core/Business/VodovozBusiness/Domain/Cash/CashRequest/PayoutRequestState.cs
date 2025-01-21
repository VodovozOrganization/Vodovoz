using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash
{
	/// <summary>
	/// Состояние заявки на выдачу средств
	/// </summary>
	public enum PayoutRequestState
	{
		/// <summary>
		/// Новая
		/// </summary>
		[Display(Name = "Новая")]
		New,

		/// <summary>
		/// На уточнении
		/// </summary>
		[Display(Name = "На уточнении")]
		OnClarification, // после отправки на пересогласование

		/// <summary>
		/// Подана
		/// </summary>
		[Display(Name = "Подана")]
		Submited, // после подтверждения

		/// <summary>
		/// Согласована руководителем отдела
		/// </summary>
		[Display(Name = "Согласована руководителем отдела")]
		AgreedBySubdivisionChief, // после согласования руководителем

		/// <summary>
		/// Согласована исполнительным директором
		/// </summary>
		[Display(Name = "Согласована исполнительным директором")]
		Agreed, // после согласования исполнительным директором

		/// <summary>
		/// Передана на выдачу
		/// </summary>
		[Display(Name = "Передана на выдачу")]
		GivenForTake,

		/// <summary>
		/// Частично закрыта
		/// </summary>
		[Display(Name = "Частично закрыта")]
		PartiallyClosed, // содержит не выданные суммы

		/// <summary>
		/// Отменена
		/// </summary>
		[Display(Name = "Отменена")]
		Canceled,

		/// <summary>
		/// Закрыта
		/// </summary>
		[Display(Name = "Закрыта")]
		Closed // все суммы выданы
	}
}
