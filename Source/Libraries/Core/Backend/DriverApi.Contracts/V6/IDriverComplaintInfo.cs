namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Интерфейс для информации о рекламации водителя.
	/// </summary>
	public interface IDriverComplaintInfo
	{
		/// <summary>
		/// Рейтинг
		/// </summary>
		int Rating { get; }

		/// <summary>
		/// Идентификатор причины рекламации водителя
		/// </summary>
		int DriverComplaintReasonId { get; }

		/// <summary>
		/// Комментарий при выбранной причине рекламации Другая
		/// </summary>
		string OtherDriverComplaintReasonComment { get; }
	}
}
