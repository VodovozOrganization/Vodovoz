using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Domain.Rules.Edo
{
	/// <summary>
	/// Определяет статусы задачи трансфера, связанные с отменой.
	/// </summary>
	public static class TransferEdoTaskCancellationRule
	{
		/// <summary>
		/// Статусы отменяемого или отменённого трансфера.
		/// </summary>
		public static IReadOnlyCollection<EdoTaskStatus> CancellationStatuses { get; } =
			Array.AsReadOnly(new[] { EdoTaskStatus.InCancellation, EdoTaskStatus.Cancelled });

		/// <summary>
		/// Проверяет, отменяется ли задача трансфера или уже отменена.
		/// </summary>
		/// <param name="status">Статус задачи трансфера.</param>
		/// <returns>Признак отмены или процесса отмены.</returns>
		public static bool IsCancelledOrCancelling(EdoTaskStatus status) => CancellationStatuses.Contains(status);
	}
}
