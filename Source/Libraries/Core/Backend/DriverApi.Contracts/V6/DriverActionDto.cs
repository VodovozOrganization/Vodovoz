using System;

namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Действие в мобильном приложении водителей
	/// </summary>
	public class DriverActionDto
	{
		/// <summary>
		/// Тип действия
		/// </summary>
		public ActionDtoType ActionType { get; set; }

		/// <summary>
		/// Время операции на стороне мобильного приложения водителя
		/// </summary>
		public DateTime ActionTimeUtc { get; set; }
	}
}
