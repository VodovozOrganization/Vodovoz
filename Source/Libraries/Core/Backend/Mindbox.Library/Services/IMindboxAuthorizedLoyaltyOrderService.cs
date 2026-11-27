using System;
using System.Threading;
using System.Threading.Tasks;
using Mindbox.Library.Contracts;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Создание заказа с бонусами в Mindbox
	/// </summary>
	public interface IMindboxAuthorizedLoyaltyOrderService
	{
		Task<MindboxLoyaltyOrderResult> CreateAsync(
			MindboxLoyaltyOrderRequest request,
			int onlineOrderId,
			Guid externalOrderId,
			int erpCounterpartyId,
			Guid transactionId,
			CancellationToken cancellationToken);
	}
}
