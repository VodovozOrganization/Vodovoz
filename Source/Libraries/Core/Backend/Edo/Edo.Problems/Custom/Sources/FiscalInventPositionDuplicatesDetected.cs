using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	public class FiscalInventPositionDuplicatesDetected : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.FiscalInventPositionMarkedDuplicatesDetected";
		public override string Message => "Обнаружены дубликаты строк фискального документа";
		public override string Description => "Сверяет кол-во по строк по указанным в них кодам";
		public override string Recommendation => "Обратитесь за технической поддержкой";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
