using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Validation.Sources
{
	public abstract class OrderEdoValidatorBase : EdoTaskProblemValidatorSource
	{
		public override bool IsApplicable(EdoTask edoTask)
		{
			return edoTask is OrderEdoTask;
		}

		protected virtual PrimaryEdoRequest GetOrderEdoRequest(EdoTask edoTask)
		{
			return ((OrderEdoTask)edoTask).OrderEdoRequest;
		}
	}
}
