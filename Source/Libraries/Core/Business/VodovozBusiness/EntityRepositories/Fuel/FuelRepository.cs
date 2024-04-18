using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Fuel;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.EntityRepositories.Fuel
{
	public class FuelRepository : IFuelRepository
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

			var fuelExpenseSubquery = QueryOver.Of<FuelExpenseOperation>(() => fuelExpenseOperationAlias)
				.Where(Restrictions.Where(() => fuelExpenseOperationAlias.FuelType.Id == fuelTypeAlias.Id))
				.Select(Projections.Sum(Projections.Property(() => fuelExpenseOperationAlias.FuelLiters)))
				.DetachedCriteria;

			var fuelIncomeSubquery = QueryOver.Of<FuelIncomeOperation>(() => fuelIncomeOperationAlias)
				.Where(Restrictions.Where(() => fuelIncomeOperationAlias.FuelType.Id == fuelTypeAlias.Id))
				.Select(Projections.Sum(Projections.Property(() => fuelIncomeOperationAlias.FuelLiters)))
				.DetachedCriteria;

			var resultList = uow.Session.QueryOver<FuelType>(() => fuelTypeAlias)
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

			var fuelExpenseSubquery = QueryOver.Of<FuelExpenseOperation>(() => fuelExpenseOperationAlias)
				.Left.JoinAlias(() => fuelExpenseOperationAlias.RelatedToSubdivision, () => expenseSubdivisionAlias)
				.Where(() => fuelExpenseOperationAlias.FuelType.Id == fuelTypeAlias.Id)
				.Where(() => expenseSubdivisionAlias.Id == subdivision.Id)
				.Select(Projections.Sum(Projections.Property(() => fuelExpenseOperationAlias.FuelLiters)))
				.DetachedCriteria;

			var fuelIncomeSubquery = QueryOver.Of<FuelIncomeOperation>(() => fuelIncomeOperationAlias)
				.Left.JoinAlias(() => fuelIncomeOperationAlias.RelatedToSubdivision, () => incomeSubdivisionAlias)
				.Where(Restrictions.Where(() => fuelIncomeOperationAlias.FuelType.Id == fuelTypeAlias.Id))
				.Where(() => incomeSubdivisionAlias.Id == subdivision.Id)
				.Select(Projections.Sum(Projections.Property(() => fuelIncomeOperationAlias.FuelLiters)))
				.DetachedCriteria;

			var resultList = uow.Session.QueryOver<FuelType>(() => fuelTypeAlias)
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

			var fuelExpenseSubquery = QueryOver.Of<FuelExpenseOperation>(() => fuelExpenseOperationAlias)
				.Left.JoinAlias(() => fuelExpenseOperationAlias.FuelType, () => fuelTypeAlias)
				.Where(() => fuelTypeAlias.Id == fuelType.Id)
				.Select(Projections.Sum(Projections.Property<FuelExpenseOperation>(x => x.FuelLiters)))
				.DetachedCriteria;

			FuelIncomeOperation fuelIncomeOperationAlias = null;
			var balance = uow.Session.QueryOver<FuelIncomeOperation>(() => fuelIncomeOperationAlias)
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

			var queryResult = uow.Session.QueryOver<FuelOperation>(() => operationAlias);

			if(driver != null)
			{
				queryResult.Where(() => operationAlias.Driver.Id == driver.Id);
			}

			if(car != null)
			{
				queryResult.Where(() => operationAlias.Car.Id == car.Id);
			}

			if(before.HasValue)
			{
				queryResult.Where(() => operationAlias.OperationTime < before);
			}

			if(excludeOperationsIds != null)
			{
				queryResult.Where(() => !operationAlias.Id.IsIn(excludeOperationsIds));
			}

			return queryResult.SelectList(list => list
					.SelectSum(() => operationAlias.LitersGived).WithAlias(() => result.Gived)
					.SelectSum(() => operationAlias.LitersOutlayed).WithAlias(() => result.Outlayed))
				.TransformUsing(Transformers.AliasToBean<FuelQueryResult>())
				.List<FuelQueryResult>()
				.FirstOrDefault()?.FuelBalance ?? 0;
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

			var fuelExpenseSubquery = QueryOver.Of<FuelExpenseOperation>(() => fuelExpenseOperationAlias)
				.Left.JoinAlias(() => fuelExpenseOperationAlias.RelatedToSubdivision, () => cashSubdivision)
				.Left.JoinAlias(() => fuelExpenseOperationAlias.FuelType, () => fuelTypeAlias)
				.Where(() => cashSubdivision.Id == subdivision.Id)
				.Where(() => fuelTypeAlias.Id == fuelType.Id)
				.Select(Projections.Sum(Projections.Property<FuelExpenseOperation>(x => x.FuelLiters)))
				.DetachedCriteria;

			FuelIncomeOperation fuelIncomeOperationAlias = null;
			var balance = uow.Session.QueryOver<FuelIncomeOperation>(() => fuelIncomeOperationAlias)
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
	}
}
