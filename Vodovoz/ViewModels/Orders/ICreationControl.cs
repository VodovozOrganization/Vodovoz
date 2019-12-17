using System;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Orders
{
	public interface ICreationControl
	{
		event Action<PromotionalSetActionBase> AcceptCreation;
		event Action CancelCreation;
	}
}
