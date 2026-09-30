using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	/// <summary>
	/// Общая ошибка отправки чека, если конкретную причину определить не удалось.
	/// Заменяет исторический источник Custom.NotAllReceiptsWasSended.
	/// </summary>
	public class ReceiptSendingFailed : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.ReceiptSendingFailed";
		public override string Message => "Не удалось отправить один или несколько чеков в кассу";
		public override string Description => "Возникает при ошибке отправки фискальных документов, если не удалось определить более конкретную причину";
		public override string Recommendation => "Обратитесь за технической поддержкой";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
