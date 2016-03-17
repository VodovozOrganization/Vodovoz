using System;
using Vodovoz.Domain;

namespace Vodovoz.Panel
{
	public interface ICounterpartyInfoProvider:IInfoProvider
	{		
		Counterparty Counterparty{get;}
	}
}

