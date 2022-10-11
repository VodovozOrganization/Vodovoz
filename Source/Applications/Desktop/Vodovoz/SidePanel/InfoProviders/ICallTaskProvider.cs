using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.SidePanel.InfoProviders
{
	public interface ICallTaskProvider : IInfoProvider
	{
		Order Order { get; }
	}
}
