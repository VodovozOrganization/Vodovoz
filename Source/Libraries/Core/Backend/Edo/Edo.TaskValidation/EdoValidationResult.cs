using System.Collections.Generic;
using Vodovoz.Core.Domain.Edo;

namespace Edo.TaskValidation
{
	public class EdoValidationResult
	{
		public static EdoValidationResult Valid(EdoTask validatedTask, IEdoTaskValidator validator)
		{
			return new EdoValidationResult(validatedTask, validator)
			{
				IsValid = true
			};
		}

		public static EdoValidationResult Valid(EdoTask validatedTask, IEdoTaskValidator validator, IEnumerable<EdoTaskItem> problemItems)
		{
			return new EdoValidationResult(validatedTask, validator)
			{
				IsValid = true,
				ProblemItems = problemItems
			};
		}

		public static EdoValidationResult NotValid(EdoTask validatedTask, IEdoTaskValidator validator)
		{
			return new EdoValidationResult(validatedTask, validator)
			{
				IsValid = false
			};
		}

		public static EdoValidationResult NotValid(EdoTask validatedTask, IEdoTaskValidator validator, IEnumerable<EdoTaskItem> problemItems)
		{
			return new EdoValidationResult(validatedTask, validator)
			{
				IsValid = false,
				ProblemItems = problemItems
			};
		}

		private EdoValidationResult(EdoTask validatedTask, IEdoTaskValidator validator)
		{
			ValidatedTask = validatedTask ?? throw new System.ArgumentNullException(nameof(validatedTask));
			Validator = validator ?? throw new System.ArgumentNullException(nameof(validator));
		}

		public bool IsValid { get; private set; }
		public EdoTask ValidatedTask { get; private set; }
		public IEdoTaskValidator Validator { get; private set; }
		public IEnumerable<EdoTaskItem> ProblemItems { get; private set; } = new List<EdoTaskItem>();
	}
}
