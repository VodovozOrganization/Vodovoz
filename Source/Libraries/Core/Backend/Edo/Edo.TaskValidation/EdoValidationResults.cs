using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;

namespace Edo.TaskValidation
{
	public class EdoValidationResults
	{
		public EdoValidationResults(IEnumerable<EdoValidationResult> results)
		{
			Results = results;
			IsValid = Results.All(x => x.IsValid);
			Importance = AggregateImportance(results);
		}

		public bool IsValid { get; private set; }
		public EdoValidationImportance? Importance { get; private set; }
		public IEnumerable<EdoValidationResult> Results { get; private set; }

		private EdoValidationImportance? AggregateImportance(IEnumerable<EdoValidationResult> results)
		{
			if(IsValid)
			{
				return null;
			}

			if(results.Any(x => x.Validator.Importance == EdoValidationImportance.Problem))
			{
				return EdoValidationImportance.Problem;
			}

			if(results.Any(x => x.Validator.Importance == EdoValidationImportance.Waiting))
			{
				return EdoValidationImportance.Waiting;
			}

			throw new NotSupportedException($"Неизвестная важность валидатора ЭДО " +
				$"задачи: {results.First().Validator.Importance}. " +
				$"При добавлении нового значения важности необходимо правильно реализовать приоритет ее выбора");
		}
	}
}
