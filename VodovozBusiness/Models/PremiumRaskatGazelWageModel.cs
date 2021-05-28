using QS.DomainModel.UoW;
using System;
using NHibernate.Criterion;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Services;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Models
{
	public class PremiumRaskatGAZelWageModel
	{
		private readonly IEmployeeRepository employeeRepository;
		private readonly IWageParametersProvider wageParametersProvider;
		private readonly IPremiumRaskatGAZelParametersProvider premiumRaskatGAZelParametersProvider;
		private readonly RouteList routeList;

		public PremiumRaskatGAZelWageModel(IEmployeeRepository employeeRepository, IWageParametersProvider wageParametersProvider,
			IPremiumRaskatGAZelParametersProvider premiumRaskatGAZelParametersProvider, RouteList routeList)
		{
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			this.wageParametersProvider = wageParametersProvider ?? throw new ArgumentNullException(nameof(wageParametersProvider));
			this.premiumRaskatGAZelParametersProvider = premiumRaskatGAZelParametersProvider ?? throw new ArgumentNullException(nameof(premiumRaskatGAZelParametersProvider));
			this.routeList = routeList ?? throw new ArgumentNullException(nameof(routeList));
		}

		public void UpdatePremiumRaskatGAZel(IUnitOfWork uow)
		{
			if (!NeedPremiumRaskatGAZelInRouteListDate(uow))
			{
				return;
			}

			PremiumRaskatGAZel premiumRaskat = new PremiumRaskatGAZel()
			{
				PremiumReasonString = "Автопремия для раскатных газелей",
				Author = employeeRepository.GetEmployeeForCurrentUser(uow),
				Date = DateTime.Now.Date,
				TotalMoney = premiumRaskatGAZelParametersProvider.PremiumRaskatGAZelMoney,
				RouteListDate = routeList.Date.Date
			};

			uow.Save(premiumRaskat);

			WagesMovementOperations operation = new WagesMovementOperations
			{
				OperationType = WagesType.PremiumWage,
				Employee = routeList.Driver,
				Money = premiumRaskatGAZelParametersProvider.PremiumRaskatGAZelMoney,
				OperationTime = DateTime.Now.Date
			};

			uow.Save(operation);

			PremiumItem premiumItem = new PremiumItem()
			{
				Premium = premiumRaskat,
				Employee = routeList.Driver,
				Money = premiumRaskatGAZelParametersProvider.PremiumRaskatGAZelMoney,
				WageOperation = operation
			};

			uow.Save(premiumItem);
		}

		private bool NeedPremiumRaskatGAZelInRouteListDate(IUnitOfWork uow)
		{
			RouteList routeListAlias = null;
			RouteListItem routeListAdressesAlias = null;
			Order orderAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			District districtAlias = null;
			Car carAlias = null;
			PremiumItem premiumItemAlias = null;
			PremiumRaskatGAZel premiumRaskatGaZelAlias = null;

			var premiumRaskatGAZelSubquery = QueryOver.Of(() => premiumItemAlias)
				.JoinAlias(() => premiumItemAlias.Premium, () => premiumRaskatGaZelAlias)

				.Where(() => premiumRaskatGaZelAlias.RouteListDate == routeList.Date.Date &&
							 premiumItemAlias.Employee == routeList.Driver &&
							 premiumRaskatGaZelAlias.GetType() == typeof(PremiumRaskatGAZel))
				.Select(p => p.Id);

			var wageDistrictSubquery = QueryOver.Of(() => routeListAdressesAlias)
					.JoinAlias(() => routeListAdressesAlias.Order, () => orderAlias)
					.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
					.JoinAlias(() => deliveryPointAlias.District, () => districtAlias)
					.Where(() => districtAlias.WageDistrict.Id == wageParametersProvider.GetSuburbWageDistrictId &&
								 routeListAdressesAlias.RouteList.Id == routeList.Id)
					.Select(r => r.Id);

			var checkQuery = uow.Session.QueryOver(() => routeListAlias)
				.JoinAlias(() => routeListAlias.Car, () => carAlias)
				.WithSubquery.WhereNotExists(premiumRaskatGAZelSubquery)
				.WithSubquery.WhereExists(wageDistrictSubquery)
				.Where(() => routeListAlias.Driver == routeList.Driver &&
							 routeListAlias.Date.Date == routeList.Date.Date &&
							 routeListAlias.RecalculatedDistance >= premiumRaskatGAZelParametersProvider.MinRecalculatedDistanceForPremiumRaskatGAZel &&
							 carAlias.IsRaskat
				)
				.Take(1).SingleOrDefault();

			return checkQuery != null;
		}
	}
}
