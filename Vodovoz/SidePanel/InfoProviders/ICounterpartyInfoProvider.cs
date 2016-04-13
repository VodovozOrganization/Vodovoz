using System;
using Vodovoz.Domain.Client;

namespace Vodovoz.Panel
{
	public interface ICounterpartyInfoProvider:IInfoProvider
	{		
		Counterparty Counterparty{get;}
	}
}

