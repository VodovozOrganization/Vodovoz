using System.Collections.Generic;

namespace Vodovoz.Core.Application.Receipts.Correction
{
	public class ExplanatoryNoteBuildContext
	{
		public string OrganizationName { get; set; }

		public string LeaderFullName { get; set; }

		public bool IsIndividualEntrepreneur { get; set; }

		public int OrderId { get; set; }
	}
}
