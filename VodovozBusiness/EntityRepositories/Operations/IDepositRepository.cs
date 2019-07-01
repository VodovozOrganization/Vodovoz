using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;

namespace Vodovoz.EntityRepositories.Operations
{
	public interface IDepositRepository
	{
		decimal GetDepositsAtDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint, DepositType? depositType, DateTime? before = null);
		decimal GetDepositsAtCounterparty(IUnitOfWork UoW, Counterparty counterparty, DepositType? depositType, DateTime? before = null);
		decimal GetDepositsAtCounterpartyAndDeliveryPoint(IUnitOfWork UoW, Counterparty counterparty, DeliveryPoint deliveryPoint, DepositType? depositType, DateTime? before = null);
	}
}
