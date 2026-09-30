using System;
using System.Collections.Generic;

namespace Edo.Contracts.Messages.Dto
{
	/// <summary>
	/// Ответ со статусом документооборота
	/// </summary>
	public class DocflowStatusResponse
	{
		/// <summary>
		/// Операция успешна
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// Сообщение об ошибке
		/// </summary>
		public string ErrorMessage { get; set; }

		/// <summary>
		/// Статус документооборота
		/// </summary>
		public string Status { get; set; }

		/// <summary>
		/// Дата и время изменения статуса
		/// </summary>
		public DateTime StatusChangeDateTime { get; set; }

		/// <summary>
		/// Описание ошибки
		/// </summary>
		public string ErrorDescription { get; set; }
	}
}
