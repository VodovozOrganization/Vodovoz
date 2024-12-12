using System;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Core.Messages.Commands
{
	/// <summary>
	/// Начало перерыва администратором
	/// </summary>
	public class AdminStartBreak : OperatorCommand
	{
		/// <summary>
		/// Идентификатор администратора
		/// </summary>
		public int AdminId { get; set; }

		/// <summary>
		/// Тип перерыва
		/// </summary>
		public OperatorBreakType BreakType { get; set; }

		/// <summary>
		/// Причина начала перерыва
		/// </summary>
		public string Reason { get; set; }
	}
}
