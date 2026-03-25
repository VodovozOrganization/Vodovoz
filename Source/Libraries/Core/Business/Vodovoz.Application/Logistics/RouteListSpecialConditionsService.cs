using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Application.Logistics
{
	public class RouteListSpecialConditionsService : IRouteListSpecialConditionsService
	{
		private readonly IGenericRepository<RouteListSpecialCondition> _routeListSpecialConditionRepository;
		private readonly IGenericRepository<RouteListSpecialConditionType> _routeListSpecialConditionTypeRepository;
		private readonly IGenericRepository<Employee> _employeeGenericRepository;
		private readonly IGenericRepository<ProductGroup> _productGroupRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly ILogger<RouteListSpecialConditionsService> _logger;

		public RouteListSpecialConditionsService(
			IGenericRepository<RouteListSpecialCondition> routeListSpecialConditionRepository,
			IGenericRepository<RouteListSpecialConditionType> routeListSpecialConditionTypeRepository,
			IGenericRepository<Employee> employeeGenericRepository,
			IGenericRepository<ProductGroup> productGroupRepository,
			IRouteListRepository routeListRepository,
			INomenclatureSettings nomenclatureSettings,
			ILogger<RouteListSpecialConditionsService> logger)
		{
			_routeListSpecialConditionRepository = routeListSpecialConditionRepository ??
			                                       throw new ArgumentNullException(nameof(routeListSpecialConditionRepository));
			_routeListSpecialConditionTypeRepository = routeListSpecialConditionTypeRepository ??
			                                           throw new ArgumentNullException(nameof(routeListSpecialConditionTypeRepository));
			_employeeGenericRepository = employeeGenericRepository ?? throw new ArgumentNullException(nameof(employeeGenericRepository));
			_productGroupRepository = productGroupRepository ?? throw new ArgumentNullException(nameof(productGroupRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public IDictionary<int, string> GetSpecialConditionsDictionaryFor(
			IUnitOfWork unitOfWork,
			int routeListId)
		{
			var routeListConditions = _routeListSpecialConditionRepository
				.Get(unitOfWork, x => x.RouteListId == routeListId);

			var conditionTypesIds = routeListConditions
				.Select(x => x.RouteListSpecialConditionTypeId)
				.Distinct();

			var specialConditrionTypes = _routeListSpecialConditionTypeRepository
				.Get(unitOfWork, sct => conditionTypesIds.Contains(sct.Id));

			var result = routeListConditions
				.ToDictionary(x => x.Id, x => specialConditrionTypes
					.First(sct => sct.Id == x.RouteListSpecialConditionTypeId).Name);

			return result;
		}

		public IEnumerable<RouteListSpecialCondition> GetSpecialConditionsFor(
			IUnitOfWork unitOfWork,
			int routeListId)
		{
			var routeList = _routeListSpecialConditionRepository
				.Get(unitOfWork, x => x.RouteListId == routeListId);

			return routeList;
		}

		public void CreateSpecialConditionsFor(
			IUnitOfWork unitOfWork,
			RouteList routeList)
		{
			var existingSpecialConditions = GetSpecialConditionsFor(unitOfWork, routeList.Id);

			if(routeList.Addresses.Any(x => x.Order.HasCommentForDriver)
			   && !existingSpecialConditions.Any(x =>
				   x.RouteListSpecialConditionTypeId == RouteListSpecialConditionType.OrdersHasComments))
			{
				unitOfWork.Save(new RouteListSpecialCondition
				{
					RouteListId = routeList.Id,
					RouteListSpecialConditionTypeId = RouteListSpecialConditionType.OrdersHasComments
				});
			}

			if(routeList.Addresses.Any(x => x.Order.PaymentType == PaymentType.Terminal)
			   && !existingSpecialConditions.Any(x =>
				   x.RouteListSpecialConditionTypeId == RouteListSpecialConditionType.OrdersRequireTerminal))
			{
				unitOfWork.Save(new RouteListSpecialCondition
				{
					RouteListId = routeList.Id,
					RouteListSpecialConditionTypeId = RouteListSpecialConditionType.OrdersRequireTerminal
				});
			}

			if(routeList.Addresses.Any(x => x.Order.Trifle > 0)
			   && !existingSpecialConditions.Any(x =>
				   x.RouteListSpecialConditionTypeId == RouteListSpecialConditionType.OrdersRequireTrifle))
			{
				unitOfWork.Save(new RouteListSpecialCondition
				{
					RouteListId = routeList.Id,
					RouteListSpecialConditionTypeId = RouteListSpecialConditionType.OrdersRequireTrifle
				});
			}

			if(routeList.AdditionalLoadingDocument != null
			   && !existingSpecialConditions.Any(x =>
				   x.RouteListSpecialConditionTypeId == RouteListSpecialConditionType.RouteListRequireAdditionalLoading))
			{
				unitOfWork.Save(new RouteListSpecialCondition
				{
					RouteListId = routeList.Id,
					RouteListSpecialConditionTypeId = RouteListSpecialConditionType.RouteListRequireAdditionalLoading
				});
			}

			var productGroupsIds = GetAllProductGroupChilds(unitOfWork, _nomenclatureSettings.EquipmentForCheckProductGroupsIds);

			if(routeList.Addresses
			   .Any(address =>
				   address.Order.OrderItems.Any(oi =>
					   productGroupsIds.Contains(oi.Nomenclature.ProductGroup.Id))
				   && !existingSpecialConditions.Any(x =>
					   x.RouteListSpecialConditionTypeId == RouteListSpecialConditionType.EquipmentCheckRequired)))
			{
				unitOfWork.Save(new RouteListSpecialCondition
				{
					RouteListId = routeList.Id,
					RouteListSpecialConditionTypeId = RouteListSpecialConditionType.EquipmentCheckRequired
				});
			}
		}

		private IEnumerable<int> GetAllProductGroupChilds(IUnitOfWork unitOfWork, IEnumerable<int> ids)
		{
			var result = new List<int>(ids);

			var buffer = new List<int>(ids);

			while(buffer.Count > 0)
			{
				var current = buffer[0];

				var productGroupsIds = _productGroupRepository
					.Get(unitOfWork, pg => pg.Parent.Id == current)
					.Select(pg => pg.Id)
					.ToList();

				buffer.AddRange(productGroupsIds);

				result = result.Union(productGroupsIds).ToList();

				buffer.Remove(current);
			}

			return result;
		}

		public void AcceptConditions(
			IUnitOfWork unitOfWork,
			int driverId,
			IEnumerable<int> specialConditionsIds)
		{
			var specialConditions = _routeListSpecialConditionRepository.Get(unitOfWork, sc => specialConditionsIds.Contains(sc.Id));

			var firstRouteListId = specialConditions.FirstOrDefault()?.RouteListId;

			if(!specialConditions.All(x => x.RouteListId == firstRouteListId))
			{
				var tooManyRouteListsErrorTemplate = "Нельзя принять сразу условия несколькоих Маршрутных Листов";

				_logger.LogError(
					"Попытка принять условия нескольких МЛ водителем {DriverId}. Идентификаторы специальных условий: {$SpecialConditionsIds}",
					driverId,
					specialConditions.Select(x => x.Id));

				throw new ArgumentException(tooManyRouteListsErrorTemplate, nameof(specialConditions));
			}

			var driver = _employeeGenericRepository.Get(unitOfWork, x => x.Id == driverId).FirstOrDefault();

			if(driver is null)
			{
				var driverNotFoundErrorTemplate = "Не накйден водитель с идентификатором {DriverId}";

				_logger.LogError(driverNotFoundErrorTemplate, driverId);

				throw new ArgumentException(string.Format(driverNotFoundErrorTemplate, driverId), nameof(driverId));
			}

			var routeList = _routeListRepository.GetDriverRouteLists(unitOfWork, driver.Id)
				.FirstOrDefault(x => x.Id == firstRouteListId);

			if(routeList is null)
			{
				var driverRouteListNotFoundErrorTemplate = "Не найден МЛ водителя {DriverId} номер {RouteListId}";

				_logger.LogError(driverRouteListNotFoundErrorTemplate, driverId, firstRouteListId);

				throw new InvalidOperationException(string.Format(driverRouteListNotFoundErrorTemplate, driverId, firstRouteListId));
			}

			var routeListConditions = GetSpecialConditionsFor(unitOfWork, firstRouteListId.Value);

			if(!routeListConditions.All(x => specialConditions.Any(sc => sc.Id == x.Id)))
			{
				throw new ArgumentException("Нельзя принять не все условия МЛ", nameof(specialConditionsIds));
			}

			using(var transaction = unitOfWork.Session.BeginTransaction())
			{
				foreach(var specialCondition in routeListConditions)
				{
					specialCondition.Accepted = true;
					unitOfWork.Save(specialCondition);
				}

				routeList.SpecialConditionsAccepted = true;
				routeList.SpecialConditionsAcceptedAt = DateTime.Now;
				unitOfWork.Save(routeList);

				transaction.Commit();
			}
		}
	}
}
