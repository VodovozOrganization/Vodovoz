using System.Threading.Tasks;
using System;
using Vodovoz.Core.Domain.Edo;
using System.Threading;

namespace Edo.Problems.Validation
{
	public abstract class EdoTaskProblemValidatorSource : EdoTaskProblemValidatorSourceEntity, IEdoTaskValidator
	{
		public abstract override string Name { get; }
		public abstract bool IsApplicable(EdoTask edoTask);
		public abstract Task<EdoValidationResult> ValidateAsync(
			EdoTask edoTask,
			IServiceProvider serviceProvider,
			CancellationToken cancellationToken
		);

		public virtual string GetTemplatedMessage(EdoTask edoTask)
		{
			return Message;
		}
	}
}
