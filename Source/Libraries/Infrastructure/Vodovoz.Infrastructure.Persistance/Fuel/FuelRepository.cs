using DateTimeHelpers;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Linq;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Fuel;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Fuel;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Fuel;

namespace Vodovoz.Infrastructure.Persistance.Fuel
{
	internal sealed class FuelRepository : IFuelRepository
	{
		public Dictionary<FuelType, decimal> GetAllFuelsBalance(IUnitOfWork uow)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			FuelType fuelTypeAlias = null;
			FuelExpenseOperation fuelExpenseOperationAlias = null;
			FuelIncomeOperation fuelIncomeOperationAlias = null;

			var fuelExpenseSubquery = QueryOver.Of(() => fuelExpenseOperationAlias)
				.Where(Restrictions.Where(() => fuelExpenseOperationAlias.FuelType.Id == fuelTypeAlias.Id))
				.Select(Projections.Sum(Projections.Property(() => fuelExpenseOperationAlias.FuelLiters)))
				.DetachedCriteria;

			var fuelIncomeSubquery = QueryOver.Of(() => fuelIncomeOperationAlias)
				.Where(Restrictions.Where(() => fuelIncomeOperationAlias.FuelType.Id == fuelTypeAlias.Id))
				.Select(Projections.Sum(Projections.Property(() => fuelIncomeOperationAlias.FuelLiters)))
				.DetachedCriteria;

			var resultList = uow.Session.QueryOver(() => fuelTypeAlias)
				.SelectList(list => list
					.SelectGroup(() => fuelTypeAlias.Id)
					.Select(
						Projections.SqlFunction(
							new SQLFunctionTemplate(NHibernateUtil.Decimal, "( IFNULL(?1, 0) - IFNULL(?2, 0) )"),
								NHibernateUtil.Decimal,
								Projections.SubQuery(fuelIncomeSubquery),
								Projections.SubQuery(fuelExpenseSubquery)
						)
					)
				).TransformUsing(Transformers.PassThrough)
				.List<object[]>()
				.ToDictionary(key => (int)key[0], value => (decimal)(value[1] ?? 0m));
			var fuelTypes = uow.GetAll<FuelType>();
			Dictionary<FuelType, decimal> result = new Dictionary<FuelType, decimal>();
			foreach(var fuelType in fuelTypes)
			{
				result.Add(fuelType, resultList[fuelType.Id]);
			}
			return result;
		}

		public Dictionary<FuelType, decimal> GetAllFuelsBalanceForSubdivision(IUnitOfWork uow, Subdivision subdivision)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(subdivision == null)
			{
				throw new ArgumentNullException(nameof(subdivision));
			}

			FuelType fuelTypeAlias = null;
			FuelExpenseOperation fuelExpenseOperationAlias = null;
			FuelIncomeOperation fuelIncomeOperationAlias = null;
			Subdivision expenseSubdivisionAlias = null;
			Subdivision incomeSubdivisionAlias = null;

			var fuelExpenseSubquery = QueryOver.Of(() => fuelExpenseOperationAlias)
				.Left.JoinAlias(() => fuelExpenseOperationAlias.RelatedToSubdivision, () => expenseSubdivisionAlias)
				.Where(() => fuelExpenseOperationAlias.FuelType.Id == fuelTypeAlias.Id)
				.Where(() => expenseSubdivisionAlias.Id == subdivision.Id)
				.Select(Projections.Sum(Projections.Property(() => fuelExpenseOperationAlias.FuelLiters)))
				.DetachedCriteria;

			var fuelIncomeSubquery = QueryOver.Of(() => fuelIncomeOperationAlias)
				.Left.JoinAlias(() => fuelIncomeOperationAlias.RelatedToSubdivision, () => incomeSubdivisionAlias)
				.Where(Restrictions.Where(() => fuelIncomeOperationAlias.FuelType.Id == fuelTypeAlias.Id))
				.Where(() => incomeSubdivisionAlias.Id == subdivision.Id)
				.Select(Projections.Sum(Projections.Property(() => fuelIncomeOperationAlias.FuelLiters)))
				.DetachedCriteria;

			var resultList = uow.Session.QueryOver(() => fuelTypeAlias)
				.SelectList(list => list
					.SelectGroup(() => fuelTypeAlias.Id)
					.Select(
						Projections.SqlFunction(
							new SQLFunctionTemplate(NHibernateUtil.Decimal, "( IFNULL(?1, 0) - IFNULL(?2, 0) )"),
								NHibernateUtil.Decimal,
								Projections.SubQuery(fuelIncomeSubquery),
								Projections.SubQuery(fuelExpenseSubquery)
						)
					)
				).TransformUsing(Transformers.PassThrough)
				.List<object[]>()
				.ToDictionary(key => (int)key[0], value => (decimal)(value[1] ?? 0m));
			var fuelTypes = uow.GetAll<FuelType>();
			Dictionary<FuelType, decimal> result = new Dictionary<FuelType, decimal>();
			foreach(var fuelType in fuelTypes)
			{
				result.Add(fuelType, resultList[fuelType.Id]);
			}
			return result;
		}

		public decimal GetFuelBalance(IUnitOfWork uow, FuelType fuelType)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(fuelType == null)
			{
				throw new ArgumentNullException(nameof(fuelType));
			}

			FuelExpenseOperation fuelExpenseOperationAlias = null;
			FuelType fuelTypeAlias = null;

			var fuelExpenseSubquery = QueryOver.Of(() => fuelExpenseOperationAlias)
				.Left.JoinAlias(() => fuelExpenseOperationAlias.FuelType, () => fuelTypeAlias)
				.Where(() => fuelTypeAlias.Id == fuelType.Id)
				.Select(Projections.Sum(Projections.Property<FuelExpenseOperation>(x => x.FuelLiters)))
				.DetachedCriteria;

			FuelIncomeOperation fuelIncomeOperationAlias = null;
			var balance = uow.Session.QueryOver(() => fuelIncomeOperationAlias)
				.Left.JoinAlias(() => fuelIncomeOperationAlias.FuelType, () => fuelTypeAlias)
				.Where(() => fuelTypeAlias.Id == fuelType.Id)
				.Select(Projections.SqlFunction(
					new SQLFunctionTemplate(
						NHibernateUtil.Decimal, "( IFNULL(?1, 0) - IFNULL(?2, 0) )"),
						NHibernateUtil.Decimal,
						Projections.Sum(Projections.Property<FuelIncomeOperation>(x => x.FuelLiters)),
						Projections.SubQuery(fuelExpenseSubquery)
					)
				).SingleOrDefault<decimal>();
			return balance;
		}

		public decimal GetFuelBalance(IUnitOfWork uow, Employee driver, Car car, DateTime? before = null, params int[] excludeOperationsIds)
		{
			FuelOperation operationAlias = null;
			FuelQueryResult result = null;
			MileageWriteOff mileageWriteOffAlias = null;

			var fuelOperationsQuery = uow.Session.QueryOver(() => operationAlias);

			if(driver != null)
			{
				fuelOperationsQuery.Where(() => operationAlias.Driver.Id == driver.Id);
			}

			if(car != null)
			{
				fuelOperationsQuery.Where(() => operationAlias.Car.Id == car.Id);
			}

			if(before.HasValue)
			{
				fuelOperationsQuery.Where(() => operationAlias.OperationTime < before);
			}

			if(excludeOperationsIds != null)
			{
				fuelOperationsQuery.Where(() => !operationAlias.Id.IsIn(excludeOperationsIds));
			}

			fuelOperationsQuery.Where(() => !operationAlias.IsFine);

			var operationsSum = fuelOperationsQuery.SelectList(list => list
					.SelectSum(() => operationAlias.LitersGived).WithAlias(() => result.Gived)
					.SelectSum(() => operationAlias.LitersOutlayed).WithAlias(() => result.Outlayed))
				.TransformUsing(Transformers.AliasToBean<FuelQueryResult>())
				.List<FuelQueryResult>()
				.FirstOrDefault()?.FuelBalance ?? 0;

			var mileageWriteOffQuery = uow.Session.QueryOver(() => mileageWriteOffAlias);

			if(driver != null)
			{
				mileageWriteOffQuery.Where(() => mileageWriteOffAlias.Driver.Id == driver.Id);
			}

			if(car != null)
			{
				mileageWriteOffQuery.Where(() => mileageWriteOffAlias.Car.Id == car.Id);
			}

			if(before.HasValue)
			{
				mileageWriteOffQuery.Where(() => mileageWriteOffAlias.WriteOffDate < before);
			}

			var mileageWriteOffFuelSum =
				mileageWriteOffQuery
				.Select(Projections.Property(nameof(mileageWriteOffAlias.LitersOutlayed)))
				.List<decimal>()
				.Sum();

			return operationsSum - mileageWriteOffFuelSum;
		}

		public decimal GetFuelBalanceForSubdivision(IUnitOfWork uow, Subdivision subdivision, FuelType fuelType)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(subdivision == null)
			{
				throw new ArgumentNullException(nameof(subdivision));
			}

			if(fuelType == null)
			{
				throw new ArgumentNullException(nameof(fuelType));
			}

			FuelExpenseOperation fuelExpenseOperationAlias = null;
			FuelType fuelTypeAlias = null;
			Subdivision cashSubdivision = null;

			var fuelExpenseSubquery = QueryOver.Of(() => fuelExpenseOperationAlias)
				.Left.JoinAlias(() => fuelExpenseOperationAlias.RelatedToSubdivision, () => cashSubdivision)
				.Left.JoinAlias(() => fuelExpenseOperationAlias.FuelType, () => fuelTypeAlias)
				.Where(() => cashSubdivision.Id == subdivision.Id)
				.Where(() => fuelTypeAlias.Id == fuelType.Id)
				.Select(Projections.Sum(Projections.Property<FuelExpenseOperation>(x => x.FuelLiters)))
				.DetachedCriteria;

			FuelIncomeOperation fuelIncomeOperationAlias = null;
			var balance = uow.Session.QueryOver(() => fuelIncomeOperationAlias)
				.Left.JoinAlias(() => fuelIncomeOperationAlias.RelatedToSubdivision, () => cashSubdivision)
				.Left.JoinAlias(() => fuelIncomeOperationAlias.FuelType, () => fuelTypeAlias)
				.Where(() => cashSubdivision.Id == subdivision.Id)
				.Where(() => fuelTypeAlias.Id == fuelType.Id)
				.Select(Projections.SqlFunction(
					new SQLFunctionTemplate(
						NHibernateUtil.Decimal, "( IFNULL(?1, 0) - IFNULL(?2, 0) )"),
						NHibernateUtil.Decimal,
						Projections.Sum(Projections.Property<FuelIncomeOperation>(x => x.FuelLiters)),
						Projections.SubQuery(fuelExpenseSubquery)
					)
				).SingleOrDefault<decimal>();
			return balance;
		}

		public IEnumerable<FuelType> GetFuelTypes(IUnitOfWork uow)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			return uow.GetAll<FuelType>();
		}

		public FuelType GetDefaultFuel(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<FuelType>()
				.Where(x => x.Name == "АИ-92")
				.Take(1)
				.SingleOrDefault();
		}

		public async Task<int> SaveFuelTransactionsIfNeedAsync(IUnitOfWork uow, IEnumerable<FuelTransaction> fuelTransactions)
		{
			var transactionsDates = fuelTransactions.Select(t => t.TransactionDate.Date).Distinct().ToList();

			var existingTransactions =
				(from t in uow.Session.Query<FuelTransaction>()
				 where t.TransactionDate >= transactionsDates.Min() && t.TransactionDate < transactionsDates.Max().AddDays(1)
				 select t.TransactionId)
				.Distinct()
				.ToList();

			var newTransactions = fuelTransactions
				.Where(t => !existingTransactions.Contains(t.TransactionId));

			if(newTransactions.Count() > 0)
			{
				foreach(var transaction in newTransactions)
				{
					await uow.SaveAsync(transaction);
				}

				await uow.CommitAsync();
			}

			return newTransactions.Count();
		}

		public async Task<int> SaveNewAndUpdateExistingFuelTransactions(IUnitOfWork uow, IEnumerable<FuelTransaction> fuelTransactions)
		{
			var transactionsDates = fuelTransactions.Select(t => t.TransactionDate.Date).Distinct().ToList();

			var existingTransactions =
				(from t in uow.Session.Query<FuelTransaction>()
				 where t.TransactionDate >= transactionsDates.Min() && t.TransactionDate < transactionsDates.Max().AddDays(1)
				 select t)
				.Distinct()
				.ToList();

			int newTransactionsCount = default;
			int updatedTransactionsCount = default;

			foreach(var transaction in fuelTransactions)
			{
				var existingTransactionHavingSameId = existingTransactions
					.Where(t => t.TransactionId == transaction.TransactionId)
					.FirstOrDefault();

				if(existingTransactionHavingSameId is null)
				{
					await uow.SaveAsync(transaction);
					newTransactionsCount++;
					continue;
				}

				if(existingTransactionHavingSameId.TotalSum != transaction.TotalSum)
				{
					existingTransactionHavingSameId.TotalSum = transaction.TotalSum;

					await uow.SaveAsync(existingTransactionHavingSameId);
					updatedTransactionsCount++;
				}
			}

			var savedTransactionsCount = newTransactionsCount + updatedTransactionsCount;

			if(savedTransactionsCount > 0)
			{
				await uow.CommitAsync();
			}

			return savedTransactionsCount;
		}

		public IEnumerable<FuelCard> GetFuelCardsByCardId(IUnitOfWork uow, string cardId)
		{
			return uow.Session.Query<FuelCard>().Where(c => c.CardId == cardId);
		}

		public IEnumerable<FuelCard> GetFuelCardsByCardNumber(IUnitOfWork uow, string cardNumber)
		{
			return uow.Session.Query<FuelCard>().Where(c => c.CardNumber == cardNumber);
		}

		public async Task SaveFuelApiRequest(IUnitOfWork uow, FuelApiRequest request)
		{
			if(request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			await uow.SaveAsync(request);
			await uow.CommitAsync();
		}

		public IEnumerable<FuelCardVersion> GetActiveVersionsOnDateHavingFuelCard(IUnitOfWork unitOfWork, DateTime date, int fuelCardId) =>
			unitOfWork.Session.Query<FuelCardVersion>()
			.Where(v => v.FuelCard.Id == fuelCardId
				&& v.StartDate <= date
				&& (v.EndDate == null || v.EndDate >= date));

		public string GetFuelCardIdByNumber(IUnitOfWork unitOfWork, string cardNumber) =>
			unitOfWork.Session.Query<FuelCard>()
			.Where(c => c.CardNumber == cardNumber)
			.Select(c => c.CardId)
			.FirstOrDefault();

		public FuelDocument GetFuelDocumentByFuelLimitId(IUnitOfWork unitOfWork, string fuelLimitId) =>
			unitOfWork.Session.Query<FuelDocument>()
			.Where(d => d.FuelLimit.LimitId == fuelLimitId)
			.FirstOrDefault();

		public decimal GetGivedFuelInLitersOnDate(IUnitOfWork unitOfWork, int carId, DateTime date)
		{
			var carFuelOperations =
				(from fuelOperation in unitOfWork.Session.Query<FuelOperation>()
				 join ce in unitOfWork.Session.Query<CarEvent>() on fuelOperation.Id equals ce.CalibrationFuelOperation.Id into events
				 from carEvent in events.DefaultIfEmpty()
				 where
					fuelOperation.Car.Id == carId
					&& fuelOperation.LitersGived > 0
					&& fuelOperation.OperationTime >= date.Date
					&& fuelOperation.OperationTime < date.Date.AddDays(1)
					&& carEvent.Id == null
				select fuelOperation)
				.ToList();

			var givedLitersSum = carFuelOperations.Sum(o => o.LitersGived);

			return givedLitersSum;
		}

		public async Task<IDictionary<int, decimal>> GetAverageFuelPricesByLastWeekTransactionsData(
			IUnitOfWork uow,
			CancellationToken cancellationToken)
		{
			var lastWeekStartDate = DateTime.Today.AddDays(-7).FirstDayOfWeek();
			var lastWeekEndDate = DateTime.Today.AddDays(-7).LastDayOfWeek();

			return await GetFuelTypesAverageFuelPricesByTransactionsDataForPeriod(uow, lastWeekStartDate, lastWeekEndDate, cancellationToken);
		}

		private async Task<IDictionary<int, decimal>> GetFuelTypesAverageFuelPricesByTransactionsDataForPeriod(
			IUnitOfWork uow,
			DateTime periodStart,
			DateTime periodEnd,
			CancellationToken cancellationToken)
		{
			var startDate = periodStart.Date;
			var endDate = periodEnd.LatestDayTime();

			var query =
				from transaction in uow.Session.Query<FuelTransaction>()
				join fuelProduct in uow.Session.Query<GazpromFuelProduct>() on transaction.ProductId equals fuelProduct.GazpromFuelProductId
				join productGroup in uow.Session.Query<GazpromFuelProductsGroup>() on fuelProduct.GazpromProductsGroupId equals productGroup.Id
				join fuelType in uow.Session.Query<FuelType>() on productGroup.FuelTypeId equals fuelType.Id
				where
					transaction.TransactionDate >= startDate
					&& transaction.TransactionDate <= endDate
					&& !fuelProduct.IsArchived
				select new { FuelTypeId = fuelType.Id, transaction.PricePerItem };

			var fuelPrices = await query.ToListAsync(cancellationToken);

			var groupedPrices =
				fuelPrices.GroupBy(
					x => x.FuelTypeId,
					x => x.PricePerItem)
				.ToDictionary(
					x => x.Key,
					x => x.Average(price => price));

			return groupedPrices;
		}

		public async Task<IEnumerable<FuelType>> GetFuelTypesByIds(
			IUnitOfWork uow,
			IEnumerable<int> fuelTypeIds,
			CancellationToken cancellationToken)
		{
			var fuleTypes =
				await uow.Session.Query<FuelType>()
				.Where(ft => fuelTypeIds.Contains(ft.Id))
				.ToListAsync(cancellationToken);

			return fuleTypes;
		}

		public IEnumerable<GazpromFuelProduct> GetFuelProductsByFuelTypeId(IUnitOfWork uow, int  fuelTypeId)
		{
			var products =
				(from product in uow.Session.Query<GazpromFuelProduct>()
				 join productGroup in uow.Session.Query<GazpromFuelProductsGroup>() on product.GazpromProductsGroupId equals productGroup.Id
				 where
					productGroup.FuelTypeId == fuelTypeId
					&& !product.IsArchived
				 select product)
				 .ToList();

			return products;
		}

		public IEnumerable<GazpromFuelProductsGroup> GetGazpromFuelProductsGroupsByFuelTypeId(IUnitOfWork uow, int fuelTypeId)
		{
			var productGroups = uow.Session.Query<GazpromFuelProductsGroup>()
				.Where(x => x.FuelTypeId == fuelTypeId && !x.IsArchived)
				.ToList();

			return productGroups;
		}
	}
}
