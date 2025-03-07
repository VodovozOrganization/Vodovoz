using System;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Common
{
	public class TrueMarkCodeValidationResult
	{
		public TrueMarkCodeValidationResult(
			TrueMarkWaterIdentificationCode code,
			EdoTaskItem edoTaskItem
			)
		{
			Code = code ?? throw new ArgumentNullException(nameof(code));
			EdoTaskItem = edoTaskItem ?? throw new ArgumentNullException(nameof(edoTaskItem));
		}

		public TrueMarkCodeValidationResult(TrueMarkWaterIdentificationCode code)
		{
			Code = code ?? throw new ArgumentNullException(nameof(code));
		}

		public bool IsValid { get; set; } = true;
		public bool ReadyToSell { get; set; } = true;
		public TrueMarkWaterIdentificationCode Code { get; set; }
		public EdoTaskItem EdoTaskItem { get; set; }
		public bool IsOwnedByOurOrganization { get; set; } = true;
		public bool IsOurGtin { get; set; } = true;
		public bool IsIntroduced { get; set; } = true;
		public bool IsOwnedBySeller { get; set; } = true;
	}
}
