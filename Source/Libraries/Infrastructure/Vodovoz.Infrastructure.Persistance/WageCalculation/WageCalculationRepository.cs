using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.EntityRepositories.WageCalculation;

namespace Vodovoz.Infrastructure.Persistance.WageCalculation
{
	internal sealed class WageCalculationRepository : IWageCalculationRepository
	{
		public IEnumerable<WageDistrict> AllWageDistricts(IUnitOfWork uow, bool hideArchive = true)
		{
			var baseQuery = uow.Session.QueryOver<WageDistrict>();
			return hideArchive ? baseQuery.Where(d => !d.IsArchive).List() : baseQuery.List();
		}

		public IEnumerable<WageDistrictLevelRates> AllLevelRates(IUnitOfWork uow, bool hideArchive = true)
		{
			var baseQuery = uow.Session.QueryOver<WageDistrictLevelRates>();
			return hideArchive
				? baseQuery.Where(d => !d.IsArchive).OrderBy(r => r.Name).Asc.List()
				: baseQuery.OrderBy(r => r.Name).Asc.List();
		}

		public IEnumerable<SalesPlan> AllSalesPlans(IUnitOfWork uow, bool hideArchive = true)
		{
			var baseQuery = uow.Session.QueryOver<SalesPlan>();
			return hideArchive ? baseQuery.Where(d => !d.IsArchive).List() : baseQuery.List();
		}

		public WageDistrictLevelRates DefaultLevelForNewEmployees(IUnitOfWork uow)
		{
			var query = uow.Session.QueryOver<WageDistrictLevelRates>()
				.Where(x => x.IsDefaultLevel);

			return query.Take(1).SingleOrDefault();
		}

		public WageDistrictLevelRates DefaultLevelForNewEmployeesOnOurCars(IUnitOfWork uow)
		{
			var query = uow.Session.QueryOver<WageDistrictLevelRates>()
				.Where(x => x.IsDefaultLevelForOurCars);

			return query.Take(1).SingleOrDefault();
		}

		public WageDistrictLevelRates DefaultLevelForNewEmployeesOnRaskatCars(IUnitOfWork uow)
		{
			var query = uow.Session.QueryOver<WageDistrictLevelRates>()
				.Where(x => x.IsDefaultLevelForRaskatCars);

			return query.Take(1).SingleOrDefault();
		}

		public IEnumerable<DateTime> GetDaysWorkedWithRouteLists(IUnitOfWork uow, Employee employee)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(employee == null)
			{
				throw new ArgumentNullException(nameof(employee));
			}

			return uow.Session.QueryOver<RouteList>()
				.Where(x => x.Driver.Id == employee.Id)
				.Select(Projections.Distinct(Projections.Property<RouteList>(x => x.Date)))
				.List<DateTime>();
		}

		/// <inheritdoc/>
		public IEnumerable<WageDistrictLevelRates> AllDefaultLevelForNewEmployees(IUnitOfWork uow) =>
			uow.Session.Query<WageDistrictLevelRates>()
			.Where(x => x.IsDefaultLevel)
			.ToList();

		/// <inheritdoc/>
		public IEnumerable<WageDistrictLevelRates> AllDefaultLevelForNewEmployeesOnOurCars(IUnitOfWork uow) =>
			uow.Session.Query<WageDistrictLevelRates>()
			.Where(x => x.IsDefaultLevelForOurCars)
			.ToList();

		/// <inheritdoc/>
		public IEnumerable<WageDistrictLevelRates> AllDefaultLevelForNewEmployeesOnRaskatCars(IUnitOfWork uow) =>
			uow.Session.Query<WageDistrictLevelRates>()
			.Where(x => x.IsDefaultLevelForRaskatCars)
			.ToList();

		/// <inheritdoc/>
		public void ResetExistinDefaultLevelsForNewEmployees(IUnitOfWork uow)
		{
			var defaultLevelForNewEmployees = AllDefaultLevelForNewEmployees(uow);

			foreach(var defaultLevel in defaultLevelForNewEmployees)
			{
				defaultLevel.IsDefaultLevel = false;
				uow.Save(defaultLevel);
			}
		}

		/// <inheritdoc/>
		public void ResetExistinDefaultLevelsForNewEmployeesOnOurCars(IUnitOfWork uow)
		{
			var defaultLevelForOurCars = AllDefaultLevelForNewEmployeesOnOurCars(uow);

			foreach(var defaultLevel in defaultLevelForOurCars)
			{
				defaultLevel.IsDefaultLevelForOurCars = false;
				uow.Save(defaultLevel);
			}
		}

		/// <inheritdoc/>
		public void ResetExistinDefaultLevelsForNewEmployeesOnRaskatCars(IUnitOfWork uow)
		{
			var defaultLevelForRaskatCars = AllDefaultLevelForNewEmployeesOnRaskatCars(uow);

			foreach(var defaultLevel in defaultLevelForRaskatCars)
			{
				defaultLevel.IsDefaultLevelForRaskatCars = false;
				uow.Save(defaultLevel);
			}
		}
	}
}
