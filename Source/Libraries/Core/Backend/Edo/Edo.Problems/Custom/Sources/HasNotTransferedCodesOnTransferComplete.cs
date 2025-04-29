using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	public class HasNotTransferedCodesOnTransferComplete : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.HasNotTransferedCodesOnTransferComplete";
		public override string Message => "Обнаружены коды которые не завершили трансфер на нужную организацию";
		public override string Description => "Проверяет коды в честном знаке после завершения трансфера на предмет наличия в требуемой организации";
		public override string Recommendation => "Проверьте трансфер в ЭДО, дождитесь завершения трансфера";
		public override EdoProblemImportance Importance => EdoProblemImportance.Waiting;
	}
}
