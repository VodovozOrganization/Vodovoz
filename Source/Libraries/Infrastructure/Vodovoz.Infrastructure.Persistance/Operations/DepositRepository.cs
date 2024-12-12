using System;
using System.Linq;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;
using Vodovoz.EntityRepositories.Operations;

namespace Vodovoz.Infrastructure.Persistance.Operations
{
	internal sealed class DepositRepository : IDepositRepository
	{
		/// <summary>
		/// Получаем текущие депозиты для точки
		/// </summary>
		/// <param name="depositType">Указываем тип депозита, если null то по берем все типы.</param>
		public decimal GetDepositsAtDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint, DepositType? depositType, DateTime? before = null)
		{
			DepositOperation operationAlias = null;
			DepositsQueryResult result = null;
			var queryResult = UoW.Session.QueryOver(() => operationAlias)
				.Where(() => operationAlias.DeliveryPoint.Id == deliveryPoint.Id);
			if(before.HasValue)
				queryResult.Where(() => operationAlias.OperationTime < before);
			if(depositType.HasValue)
				queryResult.Where(() => operationAlias.DepositType == depositType);

			var deposits = queryResult.SelectList(list => list
					.SelectSum(() => operationAlias.ReceivedDeposit).WithAlias(() => result.Received)
					.SelectSum(() => operationAlias.RefundDeposit).WithAlias(() => result.Refund)
				).TransformUsing(Transformers.AliasToBean<DepositsQueryResult>()).List<DepositsQueryResult>()
				.FirstOrDefault()?.Deposits ?? 0;
			return deposits;
		}

		public decimal GetDepositsAtCounterparty(IUnitOfWork UoW, Counterparty counterparty, DepositType? depositType, DateTime? before = null)
		{
			DepositOperation operationAlias = null;
			DepositsQueryResult result = null;
			var queryResult = UoW.Session.QueryOver(() => operationAlias)
				.Where(() => operationAlias.Counterparty.Id == counterparty.Id);
			if(before.HasValue)
				queryResult.Where(() => operationAlias.OperationTime < before);
			if(depositType.HasValue)
				queryResult.Where(() => operationAlias.DepositType == depositType);

			var deposits = queryResult.SelectList(list => list
					.SelectSum(() => operationAlias.ReceivedDeposit).WithAlias(() => result.Received)
					.SelectSum(() => operationAlias.RefundDeposit).WithAlias(() => result.Refund)
				).TransformUsing(Transformers.AliasToBean<DepositsQueryResult>()).List<DepositsQueryResult>()
				.FirstOrDefault()?.Deposits ?? 0;
			return deposits;
		}

		public decimal GetDepositsAtCounterpartyAndDeliveryPoint(IUnitOfWork UoW, Counterparty counterparty, DeliveryPoint deliveryPoint, DepositType? depositType, DateTime? before = null)
		{
			DepositOperation operationAlias = null;
			DepositsQueryResult result = null;
			var queryResult = UoW.Session.QueryOver(() => operationAlias)
										 .Where(() => operationAlias.Counterparty == counterparty)
										 .Where(() => operationAlias.DeliveryPoint == deliveryPoint)
										 ;
			if(before.HasValue)
				queryResult.Where(() => operationAlias.OperationTime < before);
			if(depositType.HasValue)
				queryResult.Where(() => operationAlias.DepositType == depositType);

			var deposits = queryResult.SelectList(list => list
					.SelectSum(() => operationAlias.ReceivedDeposit).WithAlias(() => result.Received)
					.SelectSum(() => operationAlias.RefundDeposit).WithAlias(() => result.Refund)
				).TransformUsing(Transformers.AliasToBean<DepositsQueryResult>()).List<DepositsQueryResult>()
				.FirstOrDefault()?
				.Deposits ?? 0;
			return deposits;
		}
	}
}
