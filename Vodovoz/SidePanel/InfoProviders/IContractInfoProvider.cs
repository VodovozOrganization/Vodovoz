using System;
using Vodovoz.Domain.Client;

namespace Vodovoz.SidePanel.InfoProviders
{
	public interface IContractInfoProvider
	{
		CounterpartyContract Contract { get; }
	}
}
