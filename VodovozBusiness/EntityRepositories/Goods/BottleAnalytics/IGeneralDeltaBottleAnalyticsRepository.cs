using System;
using NHibernate;
using QS.DomainModel.UoW;

namespace Vodovoz.EntityRepositories.Goods.BottleAnalytics
{
	public interface IGeneralDeltaBottleAnalyticsRepository
	{
		IFutureEnumerable<AmountOnDateNode> GetCounterpartyReturnLoss(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds);

		IFutureEnumerable<AmountOnDateNode> GetCounterpartyReturnIncome(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds);

		IFutureEnumerable<AmountOnDateNode> GetRegradingIncome(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds);

		IFutureEnumerable<AmountOnDateNode> GetRegradingLoss(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds);

		IFutureEnumerable<AmountOnDateNode> GetWriteoffLoss(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds);

		IFutureEnumerable<AmountOnDateNode> GetInventorizationLossByDates(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds);

		IFutureEnumerable<AmountOnDateNode> GetInventorizationIncome(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds);

		IFutureEnumerable<AmountOnDateNode> GetIncomingInvoiceIncome(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds);

		IFutureEnumerable<AmountOnDateNode> GetDriverReturnLoss(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds);

		IFutureEnumerable<AmountOnDateNode> GetDriverReturnIncome(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds);

		IFutureEnumerable<AmountOnDateNode> GetSelfDeliveryIncome(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds);

		IFutureEnumerable<AmountOnDateNode> GetSelfDeliveryLoss(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds);
	}
}
