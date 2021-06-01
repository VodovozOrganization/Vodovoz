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
	public class PremiumRaskatGAZelleWageModel
	{
		private readonly IEmployeeRepository employeeRepository;
		private readonly IWageParametersProvider wageParametersProvider;
		private readonly IPremiumRaskatGAZelleParametersProvider premiumRaskatGAZelleParametersProvider;
		private readonly RouteList routeList;

		public PremiumRaskatGAZelleWageModel(IEmployeeRepository employeeRepository, IWageParametersProvider wageParametersProvider,
			IPremiumRaskatGAZelleParametersProvider premiumRaskatGaZelleParametersProvider, RouteList routeList)
		{
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			this.wageParametersProvider = wageParametersProvider ?? throw new ArgumentNullException(nameof(wageParametersProvider));
			this.premiumRaskatGAZelleParametersProvider = premiumRaskatGaZelleParametersProvider ?? throw new ArgumentNullException(nameof(premiumRaskatGaZelleParametersProvider));
			this.routeList = routeList ?? throw new ArgumentNullException(nameof(routeList));
		}

		public void UpdatePremiumRaskatGAZelle(IUnitOfWork uow)
		{
			if (!NeedPremiumRaskatGAZelleInRouteListDate(uow))
			{
				return;
			}

			PremiumRaskatGAZelle premiumRaskatGAZelle = new PremiumRaskatGAZelle()
			{
				PremiumReasonString = "Автопремия для раскатных газелей",
				Author = employeeRepository.GetEmployeeForCurrentUser(uow),
				Date = DateTime.Now.Date,
				TotalMoney = premiumRaskatGAZelleParametersProvider.PremiumRaskatGAZelleMoney
			};

			uow.Save(premiumRaskatGAZelle);

			WagesMovementOperations operation = new WagesMovementOperations
			{
				OperationType = WagesType.PremiumWage,
				Employee = routeList.Driver,
				Money = premiumRaskatGAZelleParametersProvider.PremiumRaskatGAZelleMoney,
				OperationTime = DateTime.Now.Date
			};

			uow.Save(operation);

			PremiumItem premiumItem = new PremiumItem()
			{
				Premium = premiumRaskatGAZelle,
				Employee = routeList.Driver,
				Money = premiumRaskatGAZelleParametersProvider.PremiumRaskatGAZelleMoney,
				WageOperation = operation
			};

			uow.Save(premiumItem);
		}

		private bool NeedPremiumRaskatGAZelleInRouteListDate(IUnitOfWork uow)
		{
			if(routeList.RecalculatedDistance >= premiumRaskatGAZelleParametersProvider.MinRecalculatedDistanceForPremiumRaskatGAZelle &&
				routeList.Car.IsRaskat &&
				routeList.Car.TypeOfUse == CarTypeOfUse.DriverCar &&
				routeList.Car.RaskatType == RaskatType.RaskatGazelle)
			{
				RouteListItem routeListAdressesAlias = null;
				Order orderAlias = null;
				DeliveryPoint deliveryPointAlias = null;
				District districtAlias = null;
				PremiumItem premiumItemAlias = null;
				PremiumRaskatGAZelle premiumRaskatGAZelleAlias = null;

				// Ищем премию на дату МЛ
				var premiumRaskatGAZelleQuery = uow.Session.QueryOver(() => premiumItemAlias)
					.JoinAlias(() => premiumItemAlias.Premium, () => premiumRaskatGAZelleAlias)
					.Where(() => premiumRaskatGAZelleAlias.Date.Date == routeList.Date.Date &&
								 premiumItemAlias.Employee == routeList.Driver &&
								 premiumRaskatGAZelleAlias.GetType() == typeof(PremiumRaskatGAZelle))
					.Take(1).SingleOrDefault();

				// Ищем заказ в пригороде
				var wageDistrictQuery = uow.Session.QueryOver(() => routeListAdressesAlias)
						.JoinAlias(() => routeListAdressesAlias.Order, () => orderAlias)
						.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
						.JoinAlias(() => deliveryPointAlias.District, () => districtAlias)
						.Where(() => districtAlias.WageDistrict.Id == wageParametersProvider.GetSuburbWageDistrictId &&
									 routeListAdressesAlias.RouteList.Id == routeList.Id)
						.Take(1).SingleOrDefault();

				return premiumRaskatGAZelleQuery == null && wageDistrictQuery != null;
			}
			else
			{
				return false;
			}
		}
	}
}
