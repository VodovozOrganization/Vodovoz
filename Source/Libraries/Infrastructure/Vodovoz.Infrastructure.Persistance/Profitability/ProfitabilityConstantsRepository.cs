using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Profitability;

namespace Vodovoz.Infrastructure.Persistance.Profitability
{
	internal sealed class ProfitabilityConstantsRepository : IProfitabilityConstantsRepository
	{
		public ProfitabilityConstants GetLastProfitabilityConstants(IUnitOfWork uow)
		{
			var lastCalculatedMonth = uow.Session.QueryOver<ProfitabilityConstants>()
				.Select(Projections.Max<ProfitabilityConstants>(pc => pc.CalculatedMonth))
				.SingleOrDefault<DateTime?>();

			return lastCalculatedMonth != null ? GetProfitabilityConstantsByCalculatedMonth(uow, lastCalculatedMonth.Value) : null;
		}

		public ProfitabilityConstants GetProfitabilityConstantsByCalculatedMonth(IUnitOfWork uow, DateTime calculatedMonth)
		{
			return uow.Session.QueryOver<ProfitabilityConstants>()
				.Where(pc => pc.CalculatedMonth == calculatedMonth)
				.SingleOrDefault();
		}

		public bool ProfitabilityConstantsByCalculatedMonthExists(IUnitOfWork uow, DateTime monthFrom, DateTime monthTo)
		{
			return uow.Session.QueryOver<ProfitabilityConstants>()
				.Where(Restrictions.Between(
					Projections.Property<ProfitabilityConstants>(pc => pc.CalculatedMonth),
					monthFrom,
					monthTo))
				.List<ProfitabilityConstants>()
				.Any();
		}

		public IList<AverageMileageCarsByTypeOfUseNode> GetAverageMileageCarsByTypeOfUse(IUnitOfWork uow, DateTime calculatedMonth)
		{
			RouteList routeListAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;
			CarVersion carVersion = null;
			AverageMileageCarsByTypeOfUseNode resultAlias = null;

			var query = uow.Session.QueryOver(() => routeListAlias)
				.JoinAlias(r => r.Car, () => carAlias)
				.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.JoinEntityAlias(
					() => carVersion,
					() => carAlias.Id == carVersion.Car.Id
						&& carVersion.StartDate <= routeListAlias.Date
						&& (carVersion.EndDate == null || carVersion.EndDate > routeListAlias.Date)
						&& carVersion.CarOwnType == CarOwnType.Company)
				.Where(() => routeListAlias.Date >= calculatedMonth && routeListAlias.Date < calculatedMonth.AddMonths(1))
				.SelectList(list => list
					.SelectGroup(() => carModelAlias.CarTypeOfUse).WithAlias(() => resultAlias.CarTypeOfUse)
					.Select(Projections.Sum(
						Projections.Conditional(
							Restrictions.Gt(Projections.Property<RouteList>(r => r.ConfirmedDistance), 0),
							Projections.Property<RouteList>(r => r.ConfirmedDistance),
							Projections.Conditional(
								Restrictions.Gt(Projections.Property<RouteList>(r => r.RecalculatedDistance), 0),
								Projections.Property<RouteList>(r => r.RecalculatedDistance),
								Projections.Conditional(
									Restrictions.Gt(Projections.Property<RouteList>(r => r.PlanedDistance), 0),
									Projections.Property<RouteList>(r => r.PlanedDistance),
									Projections.Constant(0m)))))).WithAlias(() => resultAlias.Distance)
					.SelectCountDistinct(() => carAlias.Id).WithAlias(() => resultAlias.CountCars))
				.TransformUsing(Transformers.AliasToBean<AverageMileageCarsByTypeOfUseNode>())
				.List<AverageMileageCarsByTypeOfUseNode>();

			return query;
		}

		/// <summary>
		/// Ищем ближайшие рассчитанные константы рентабельности относительно даты
		/// Если есть на нужный месяц - берем их, нет - ищем ближайшие ранние, их нет - ищем ближайшие поздние
		/// </summary>
		/// <param name="uow">Unit of work</param>
		/// <param name="date">дата для поиска рассчитанных констант</param>
		/// <returns></returns>
		public ProfitabilityConstants GetNearestProfitabilityConstantsByDate(IUnitOfWork uow, DateTime date)
		{
			var profitabilityConstants = (GetProfitabilityConstantsByCalculatedMonth(uow, date) ??
									  GetEarlyProfitabilityConstantsByCalculatedMonth(uow, date)) ??
									 GetLateProfitabilityConstantsByCalculatedMonth(uow, date);

			return profitabilityConstants;
		}

		private ProfitabilityConstants GetEarlyProfitabilityConstantsByCalculatedMonth(IUnitOfWork uow, DateTime date)
		{
			return uow.Session.QueryOver<ProfitabilityConstants>()
				.Where(pc => pc.CalculatedMonth < date)
				.OrderBy(pc => pc.CalculatedMonth).Desc
				.Take(1)
				.SingleOrDefault();
		}

		private ProfitabilityConstants GetLateProfitabilityConstantsByCalculatedMonth(IUnitOfWork uow, DateTime date)
		{
			return uow.Session.QueryOver<ProfitabilityConstants>()
				.Where(pc => pc.CalculatedMonth > date)
				.OrderBy(pc => pc.CalculatedMonth).Asc
				.Take(1)
				.SingleOrDefault();
		}
	}
}
