using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Validation
{
	public class EdoTaskValidatorsProvider
	{
		private readonly EdoTaskValidatorsPersister _edoTaskValidatorsPersister;

		public EdoTaskValidatorsProvider(EdoTaskValidatorsPersister edoTaskValidatorsPersister)
		{
			_edoTaskValidatorsPersister = edoTaskValidatorsPersister ?? throw new ArgumentNullException(nameof(edoTaskValidatorsPersister));
		}

		public IEnumerable<IEdoTaskValidator> GetValidatorsFor(EdoTask task)
		{
			var allValidators = _edoTaskValidatorsPersister.GetEdoTaskValidators();
			return allValidators.Where(x => x.IsApplicable(task));
		}
	}
}
