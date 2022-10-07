using System;
using Vodovoz.Domain.Client;

namespace Vodovoz.SidePanel.InfoProviders
{
	public interface ICounterpartyInfoProvider:IInfoProvider
	{		
		Counterparty Counterparty{get;}
	}
}

