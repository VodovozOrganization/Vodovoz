using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	public class ReceiptSendPausedByNightTime : EdoTaskProblemCustomSource
	{
		public static string SourceName => "Custom.ReceiptSendPausedByNightTime";
		public override string Name => SourceName;
		public override string Message => "Отправка чека отложена на утро";
		public override string Description => "Чек не отправляется ночью и ожидает повторной отправки утром";
		public override string Recommendation => "Дождитесь автоматической повторной отправки чека утром";
		public override EdoProblemImportance Importance => EdoProblemImportance.Waiting;
	}
}
