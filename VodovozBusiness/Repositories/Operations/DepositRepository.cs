using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Repository.Operations
{
	[Obsolete("Используйте одноимённый класс из Vodovoz.EntityRepositories.Operations")]
	public static class DepositRepository
	{
		/// <summary>
		/// Получаем текущие депозиты для точки
		/// </summary>
		/// <param name="depositType">Указываем тип депозита, если null то по берем все типы.</param>
		[Obsolete]
		public static decimal GetDepositsAtDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint, DepositType? depositType, DateTime? before = null)
		{
			return new EntityRepositories.Operations.DepositRepository().GetDepositsAtDeliveryPoint(UoW, deliveryPoint, depositType, before);
		}

		[Obsolete]
		public static decimal GetDepositsAtCounterparty(IUnitOfWork UoW, Counterparty counterparty, DepositType? depositType, DateTime? before = null)
		{
			return new EntityRepositories.Operations.DepositRepository().GetDepositsAtCounterparty(UoW, counterparty, depositType, before);
		}

		[Obsolete]
		public static decimal GetDepositsAtCounterpartyAndDeliveryPoint(IUnitOfWork UoW, Counterparty counterparty, DeliveryPoint deliveryPoint, DepositType? depositType, DateTime? before = null)
		{
			return new EntityRepositories.Operations.DepositRepository().GetDepositsAtCounterpartyAndDeliveryPoint(UoW, counterparty, deliveryPoint, depositType, before);
		}

		class DepositsQueryResult
		{
			public decimal Received { get; set; }
			public decimal Refund { get; set; }
			public decimal Deposits => Received - Refund;
		}
	}
}