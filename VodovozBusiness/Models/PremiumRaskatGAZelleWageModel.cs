﻿using QS.DomainModel.UoW;
using System;
using NHibernate.Criterion;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
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
			if(!NeedPremiumRaskatGAZelleInRouteListDate(uow))
			{
				return;
			}

			PremiumRaskatGAZelle premiumRaskatGAZelle = new PremiumRaskatGAZelle()
			{
				PremiumReasonString = $"Автопремия для раскатных газелей МЛ №{routeList.Id.ToString()}",
				Author = employeeRepository.GetEmployeeForCurrentUser(uow),
				Date = DateTime.Today,
				TotalMoney = premiumRaskatGAZelleParametersProvider.PremiumRaskatGAZelleMoney,
				RouteList = routeList
			};

			uow.Save(premiumRaskatGAZelle);

			WagesMovementOperations operation = new WagesMovementOperations
			{
				OperationType = WagesType.PremiumWage,
				Employee = routeList.Driver,
				Money = premiumRaskatGAZelleParametersProvider.PremiumRaskatGAZelleMoney,
				OperationTime = DateTime.Today
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
			   routeList.CarVersion.OwnershipCar == OwnershipCar.RaskatCar &&
			   routeList.CarVersion.Car.Model.CarTypeOfUse == CarTypeOfUse.GAZelle)
			{
				RouteListItem routeListAdressesAlias = null;
				Order orderAlias = null;
				DeliveryPoint deliveryPointAlias = null;
				District districtAlias = null;
				PremiumItem premiumItemAlias = null;
				PremiumRaskatGAZelle premiumRaskatGAZelleAlias = null;

				// Ищем премию
				var premiumRaskatGAZelleQuery = uow.Session.QueryOver(() => premiumItemAlias)
					.JoinAlias(() => premiumItemAlias.Premium, () => premiumRaskatGAZelleAlias)
					.Where(() =>
						(	// Если МЛ переоткрыли в другой день и повторно его закрывают
							(premiumRaskatGAZelleAlias.RouteList.Id == routeList.Id) ||
							// Если на дату закрытия у водителя уже есть премии
							(premiumRaskatGAZelleAlias.Date == DateTime.Today && premiumItemAlias.Employee == routeList.Driver)
						) &&
						premiumRaskatGAZelleAlias.GetType() == typeof(PremiumRaskatGAZelle)
					)
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
