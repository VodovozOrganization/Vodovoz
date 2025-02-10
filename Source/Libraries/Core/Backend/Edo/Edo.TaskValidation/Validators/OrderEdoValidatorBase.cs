using Vodovoz.Core.Domain.Edo;

namespace Edo.TaskValidation.Validators
{
	public abstract class OrderEdoValidatorBase : EdoValidatorBase
	{
		public override bool IsApplicable(EdoTask edoTask)
		{
			return edoTask is OrderEdoTask;
		}

		protected virtual OrderEdoRequest GetOrderEdoRequest(EdoTask edoTask)
		{
			return ((OrderEdoTask)edoTask).OrderEdoRequest;
		}
	}
}
