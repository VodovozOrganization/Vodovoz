using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.TaskValidation
{
	public interface IEdoTaskValidator
	{
		string Name { get; }
		EdoValidationImportance Importance { get; }
		string Message { get; }
		string Description { get; }
		string Recommendation { get; }

		bool IsApplicable(EdoTask edoTask);
		string GetTemplatedMessage(EdoTask edoTask);
		Task<EdoValidationResult> ValidateAsync(EdoTask edoTask, IServiceProvider serviceProvider, CancellationToken cancellationToken);
	}
}
