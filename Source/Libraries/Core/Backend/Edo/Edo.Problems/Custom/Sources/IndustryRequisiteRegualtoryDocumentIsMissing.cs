using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	public class IndustryRequisiteRegualtoryDocumentIsMissing : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.IndustryRequisiteRegualtoryDocumentIsMissing";
		public override string Message => "Не найден регламентирующий документ отраслевого стандарта, " +
			"для установки разрешительного режима";
		public override string Description => "Проверяет указана ли ссылка на документ в параметрах базы";
		public override string Recommendation => "Необходима настройка параметров базы. Обратитесь за технической поддержкой.";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
