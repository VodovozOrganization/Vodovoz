using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Common
{
	public class TrueMarkTaskValidationResult
	{
		public TrueMarkTaskValidationResult(IEnumerable<TrueMarkCodeValidationResult> codeResults)
		{
			CodeResults = codeResults ?? throw new ArgumentNullException(nameof(codeResults));
			IsAllValid = codeResults.All(x => x.IsValid);
			ReadyToSell = codeResults.All(x => x.ReadyToSell);
		}

		public bool IsAllValid { get; }
		public bool ReadyToSell { get; }
		public IEnumerable<TrueMarkCodeValidationResult> CodeResults { get; set; }

	}
}
