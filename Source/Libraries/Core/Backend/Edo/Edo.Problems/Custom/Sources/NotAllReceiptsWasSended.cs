using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	public class NotAllReceiptsWasSended : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.NotAllReceiptsWasSended";
		public override string Message => "Не все чеки удалось отправить";
		public override string Description => "Проверяет есть ли чеки которые не удалось отправить в кассу";
		public override string Recommendation => "Обратитесь за технической поддержкой";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
