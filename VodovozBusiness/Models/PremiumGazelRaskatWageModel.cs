using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Services;

namespace Vodovoz.Models
{
	public class PremiumGazelRaskatWageModel
	{
		private readonly IUnitOfWork uow;
		private readonly IEmployeeRepository employeeRepository;
		private readonly IWageParametersProvider wageParametersProvider;
		private readonly IGazelRaskatPremiumParametersProvider gazelRaskatPremiumParametersProvider;
		private readonly RouteList routeList;

		public PremiumGazelRaskatWageModel(IUnitOfWork uow, IEmployeeRepository employeeRepository, IWageParametersProvider wageParametersProvider,
			IGazelRaskatPremiumParametersProvider gazelRaskatPremiumParametersProvider, RouteList routeList)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			this.wageParametersProvider = wageParametersProvider ?? throw new ArgumentNullException(nameof(wageParametersProvider));
			this.gazelRaskatPremiumParametersProvider = gazelRaskatPremiumParametersProvider ?? throw new ArgumentNullException(nameof(gazelRaskatPremiumParametersProvider));
			this.routeList = routeList ?? throw new ArgumentNullException(nameof(routeList));
		}

		public void UpdateGazelRaskatPremium()
		{
			if (HasGazelRaskatPremiumInRouteListDate || !NeedGazelRaskatPremiumInRouteListDate)
				return;

			PremiumGazelRaskat premium = new PremiumGazelRaskat()
			{
				PremiumReasonString = "Автопремия для раскатных газелей",
				Author = employeeRepository.GetEmployeeForCurrentUser(uow),
				Date = DateTime.Now.Date,
				TotalMoney = gazelRaskatPremiumParametersProvider.GazelRaskatPremiumMoney,
				RouteListDate = routeList.Date.Date
			};

			uow.Save(premium);

			WagesMovementOperations operation = new WagesMovementOperations
			{
				OperationType = WagesType.PremiumWage,
				Employee = routeList.Driver,
				Money = gazelRaskatPremiumParametersProvider.GazelRaskatPremiumMoney,
				OperationTime = DateTime.Now.Date
			};

			uow.Save(operation);

			PremiumItem premiumItem = new PremiumItem()
			{
				Premium = premium,
				Employee = routeList.Driver,
				Money = gazelRaskatPremiumParametersProvider.GazelRaskatPremiumMoney,
				WageOperation = operation
			};

			uow.Save(premiumItem);

			uow.Commit();
		}

		private bool HasGazelRaskatPremiumInRouteListDate => uow.GetAll<PremiumItem>()
			.Any(
				p => p.Employee == routeList.Driver &&
					 (p.Premium as PremiumGazelRaskat).RouteListDate == routeList.Date.Date
			);

		private bool NeedGazelRaskatPremiumInRouteListDate => uow.GetAll<RouteList>()
			.Any(
				rl => rl.Driver == routeList.Driver &&
					  rl.Addresses.Any(a => a.Order.DeliveryPoint.District.WageDistrict.Id ==
											wageParametersProvider.GetSuburbWageDistrictId) &&
					  rl.Date.Date == routeList.Date.Date &&
					  rl.Car.IsRaskat &&
					  rl.RecalculatedDistance >= gazelRaskatPremiumParametersProvider.MinRecalculatedDistanceForGazelRaskatPremium
			);
	}
}
