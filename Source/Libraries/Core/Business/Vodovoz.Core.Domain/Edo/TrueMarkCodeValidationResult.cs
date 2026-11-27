using System;

namespace Vodovoz.Core.Domain.Edo
{
	public class TrueMarkCodeValidationResult
	{
		private readonly TrueMarkIdentificationCode _code;

		public TrueMarkCodeValidationResult(
			TrueMarkIdentificationCode code,
			EdoTaskItem edoTaskItem
			)
		{
			Code = code ?? throw new ArgumentNullException(nameof(code));
			EdoTaskItem = edoTaskItem ?? throw new ArgumentNullException(nameof(edoTaskItem));
		}

		public TrueMarkCodeValidationResult() { }

		public bool IsValid { get; set; } = true;
		public bool ReadyToSell { get; set; } = true;
		public TrueMarkIdentificationCode Code { get; set; }
		public EdoTaskItem EdoTaskItem { get; set; }
		public bool IsOwnedByOurOrganization { get; set; } = true;
		public bool IsOurGtin { get; set; } = true;
		public bool IsIntroduced { get; set; } = true;
		public bool IsExpired { get; set; } = false;
		public bool IsOwnedBySeller { get; set; } = true;
		public string CodeString { get; set; }
	}
}
