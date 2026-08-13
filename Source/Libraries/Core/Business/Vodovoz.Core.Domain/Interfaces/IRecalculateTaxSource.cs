using System;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Domain.Interfaces
{
	public interface IRecalculateTaxSource
	{
		DateTime? DeliveryDate { get; }
		IUsnModeOrganization Organization { get; }
	}
}
