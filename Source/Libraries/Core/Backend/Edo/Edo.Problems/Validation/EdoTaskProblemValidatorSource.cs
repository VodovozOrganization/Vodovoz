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
		public abstract Task<bool> NotValidCondition(
			EdoTask edoTask,
			IServiceProvider serviceProvider,
			CancellationToken cancellationToken
		);

		public virtual string GetTemplatedMessage(EdoTask edoTask)
		{
			return Message;
		}

		public virtual async Task<EdoValidationResult> ValidateAsync(
			EdoTask edoTask,
			IServiceProvider serviceProvider,
			CancellationToken cancellationToken
			)
		{
			if(!IsApplicable(edoTask))
			{
				throw new EdoTaskValidationException($"Валидатор {Name} не применим к задаче {edoTask.GetType().Name}");
			}

			var notValidCondition = await NotValidCondition(edoTask, serviceProvider, cancellationToken);
			if(notValidCondition)
			{
				return EdoValidationResult.NotValid(this);
			}

			return EdoValidationResult.Valid(this);
		}
	}
}
