using Vodovoz.Core.Domain.Edo;

namespace Edo.TaskValidation.Validators
{
	public abstract class OrderEdoValidatorBase : EdoValidatorBase
	{
		public override bool IsApplicable(EdoTask edoTask)
		{
			var orderEdoRequest = GetOrderEdoRequest(edoTask);
			return orderEdoRequest != null;
		}

		protected virtual OrderEdoRequest GetOrderEdoRequest(EdoTask edoTask)
		{
			return (edoTask as DocumentEdoTask)?.CustomerEdoRequest as OrderEdoRequest;
		}
	}
}
