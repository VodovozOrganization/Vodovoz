using System.Collections.Generic;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Validation
{
	public class EdoValidationResult
	{
		public static EdoValidationResult Valid(IEdoTaskValidator validator)
		{
			return new EdoValidationResult(validator)
			{
				IsValid = true
			};
		}

		public static EdoValidationResult Valid(IEdoTaskValidator validator, IEnumerable<EdoTaskItem> problemItems)
		{
			return new EdoValidationResult(validator)
			{
				IsValid = true,
				ProblemItems = problemItems
			};
		}

		public static EdoValidationResult Invalid(IEdoTaskValidator validator)
		{
			return new EdoValidationResult(validator)
			{
				IsValid = false
			};
		}

		public static EdoValidationResult Invalid(IEdoTaskValidator validator, IEnumerable<EdoTaskItem> problemItems)
		{
			return new EdoValidationResult(validator)
			{
				IsValid = false,
				ProblemItems = problemItems
			};
		}

		private EdoValidationResult(IEdoTaskValidator validator)
		{
			Validator = validator ?? throw new System.ArgumentNullException(nameof(validator));
		}

		public bool IsValid { get; private set; }
		public IEdoTaskValidator Validator { get; private set; }
		public IEnumerable<EdoTaskItem> ProblemItems { get; private set; } = new List<EdoTaskItem>();
	}
}
