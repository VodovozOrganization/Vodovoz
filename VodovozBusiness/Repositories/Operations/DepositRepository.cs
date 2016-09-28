using System;
using System.Linq;
using NHibernate.Transform;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Repository.Operations
{
	public static class DepositRepository
	{
		/// <summary>
		/// Получаем текущие депозиты для точки
		/// </summary>
		/// <param name="type">Указываем тип дипозита, если null то по берем все типы.</param>
		public static decimal GetDepositsAtDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint, DepositType? depositType, DateTime? before = null)
		{
			DepositOperation operationAlias = null;
			DepositsQueryResult result = null;
			var queryResult = UoW.Session.QueryOver<DepositOperation>(() => operationAlias)
				.Where(() => operationAlias.DeliveryPoint.Id == deliveryPoint.Id);
			if (before.HasValue)
				queryResult.Where(() => operationAlias.OperationTime < before);
			if (depositType.HasValue)
				queryResult.Where(() => operationAlias.DepositType == depositType);			
			
			var deposits = queryResult.SelectList(list => list
					.SelectSum(() => operationAlias.ReceivedDeposit).WithAlias(() => result.Received)
					.SelectSum(() => operationAlias.RefundDeposit).WithAlias(() => result.Refund)
				).TransformUsing(Transformers.AliasToBean<DepositsQueryResult>()).List<DepositsQueryResult>()
				.FirstOrDefault()?.Deposits ?? 0;
			return deposits;
		}

		public static decimal GetDepositsAtCounterparty(IUnitOfWork UoW, Counterparty counterparty, DepositType? depositType, DateTime? before = null)
		{
			DepositOperation operationAlias = null;
			DepositsQueryResult result = null;
			var queryResult = UoW.Session.QueryOver<DepositOperation>(() => operationAlias)
				.Where(() => operationAlias.Counterparty.Id == counterparty.Id);
			if (before.HasValue)
				queryResult.Where(() => operationAlias.OperationTime < before);			
			if (depositType.HasValue)
				queryResult.Where(() => operationAlias.DepositType == depositType);			
			
			var deposits = queryResult.SelectList(list => list
					.SelectSum(() => operationAlias.ReceivedDeposit).WithAlias(() => result.Received)
					.SelectSum(() => operationAlias.RefundDeposit).WithAlias(() => result.Refund)
				).TransformUsing(Transformers.AliasToBean<DepositsQueryResult>()).List<DepositsQueryResult>()
				.FirstOrDefault()?.Deposits ?? 0;
			return deposits;
		}

		class DepositsQueryResult
		{
			public decimal Received{get;set;}
			public decimal Refund{get;set;}
			public decimal Deposits{
				get{
					return Received - Refund;
				}
			}
		}
	}


}