using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	public class DocflowCouldNotBeCompleted : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.DocflowCouldNotBeCompleted";
		public override string Message => "Документооброт не смог успешно завершится";
		public override string Description => "Проверяет что документооборот завершился со статусом (Завершен успешно)";
		public override string Recommendation => "Проверьте связанный документооборот в соответствующем ЭДО провайдере";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
