using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Domain.Service;
using VodovozBusiness.Services;

namespace Vodovoz.Application.Goods
{
	public sealed class NomenclatureService : INomenclatureService
	{
		private readonly IGenericRepository<Nomenclature> _nomenclatureRepository;

		private readonly IGenericRepository<WarehouseBulkGoodsAccountingOperation> _warehouseBulkGoodsAccountingOperationRepository;
		private readonly IGenericRepository<WarehouseInstanceGoodsAccountingOperation> _warehouseInstanceGoodsAccountingOperationRepository;

		private readonly IGenericRepository<EmployeeBulkGoodsAccountingOperation> _employeeBulkGoodsAccountingOperationRepository;
		private readonly IGenericRepository<EmployeeInstanceGoodsAccountingOperation> _employeeInstanceGoodsAccountingOperationRepository;

		private readonly IGenericRepository<CarBulkGoodsAccountingOperation> _carBulkGoodsAccountingOperationRepository;
		private readonly IGenericRepository<CarInstanceGoodsAccountingOperation> _carInstanceGoodsAccountingOperationRepository;

		private readonly IGenericRepository<MovementDocument> _movementDocumentRepository;
		private readonly IDeliveryRepository _deliveryRepository;
		private readonly int _masterCallNomenclatureId;

		public NomenclatureService(
			IGenericRepository<Nomenclature> nomenclatureRepository,
			IGenericRepository<WarehouseBulkGoodsAccountingOperation> warehouseBulkGoodsAccountingOperationRepository,
			IGenericRepository<WarehouseInstanceGoodsAccountingOperation> warehouseInstanceGoodsAccountingOperationRepository,
			IGenericRepository<EmployeeBulkGoodsAccountingOperation> employeeBulkGoodsAccountingOperationRepository,
			IGenericRepository<EmployeeInstanceGoodsAccountingOperation> employeeInstanceGoodsAccountingOperationRepository,
			IGenericRepository<CarBulkGoodsAccountingOperation> carBulkGoodsAccountingOperationRepository,
			IGenericRepository<CarInstanceGoodsAccountingOperation> carInstanceGoodsAccountingOperationRepository,
			IGenericRepository<MovementDocument> movementDocumentRepository,
			INomenclatureSettings nomenclatureSettings,
			IDeliveryRepository deliveryRepository)
		{
			_nomenclatureRepository = nomenclatureRepository
				?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_warehouseBulkGoodsAccountingOperationRepository = warehouseBulkGoodsAccountingOperationRepository
				?? throw new ArgumentNullException(nameof(warehouseBulkGoodsAccountingOperationRepository));
			_warehouseInstanceGoodsAccountingOperationRepository = warehouseInstanceGoodsAccountingOperationRepository
				?? throw new ArgumentNullException(nameof(warehouseInstanceGoodsAccountingOperationRepository));
			_employeeBulkGoodsAccountingOperationRepository = employeeBulkGoodsAccountingOperationRepository
				?? throw new ArgumentNullException(nameof(employeeBulkGoodsAccountingOperationRepository));
			_employeeInstanceGoodsAccountingOperationRepository = employeeInstanceGoodsAccountingOperationRepository
				?? throw new ArgumentNullException(nameof(employeeInstanceGoodsAccountingOperationRepository));
			_carBulkGoodsAccountingOperationRepository = carBulkGoodsAccountingOperationRepository
				?? throw new ArgumentNullException(nameof(carBulkGoodsAccountingOperationRepository));
			_carInstanceGoodsAccountingOperationRepository = carInstanceGoodsAccountingOperationRepository
				?? throw new ArgumentNullException(nameof(carInstanceGoodsAccountingOperationRepository));
			_movementDocumentRepository = movementDocumentRepository
				?? throw new ArgumentNullException(nameof(movementDocumentRepository));
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));

			if(nomenclatureSettings is null)
			{
				throw new ArgumentNullException(nameof(nomenclatureSettings));
			}

			_masterCallNomenclatureId = nomenclatureSettings.MasterCallNomenclatureId;
		}

		public Result Archive(IUnitOfWork unitOfWork, int nomenclatureId)
		{
			var nomenclature = _nomenclatureRepository.Get(unitOfWork, n => n.Id == nomenclatureId).FirstOrDefault();

			if(nomenclature is null)
			{
				return Result.Failure(Vodovoz.Errors.Goods.NomenclatureErrors.NotFound(nomenclatureId));
			}

			return Archive(unitOfWork, nomenclature);
		}

		public Result Archive(IUnitOfWork unitOfWork, Nomenclature nomenclature)
		{
			if(nomenclature.Id == 0)
			{
				nomenclature.IsArchive = true;
				return Result.Success();
			}

			var errors = new List<Error>();

			decimal warehouseBalance = 0m;
			decimal employeeBalance = 0m;
			decimal carBalance = 0m;

			if(nomenclature.HasInventoryAccounting)
			{
				warehouseBalance = _warehouseInstanceGoodsAccountingOperationRepository
					.GetValue(
						unitOfWork,
						wigao => wigao.Amount,
						wigao => wigao.Nomenclature.Id == nomenclature.Id)
					.Sum();

				employeeBalance = _employeeInstanceGoodsAccountingOperationRepository
					.GetValue(
						unitOfWork,
						eigao => eigao.Amount,
						eigao => eigao.Nomenclature.Id == nomenclature.Id)
					.Sum();

				carBalance = _carInstanceGoodsAccountingOperationRepository
					.GetValue(
						unitOfWork,
						cigao => cigao.Amount,
						cigao => cigao.Nomenclature.Id == nomenclature.Id)
					.Sum();
			}
			else
			{
				warehouseBalance = _warehouseBulkGoodsAccountingOperationRepository
					.GetValue(
						unitOfWork,
						wbgao => wbgao.Amount,
						wbgao => wbgao.Nomenclature.Id == nomenclature.Id)
					.Sum();

				employeeBalance = _employeeBulkGoodsAccountingOperationRepository
					.GetValue(
						unitOfWork,
						ebgao => ebgao.Amount,
						ebgao => ebgao.Nomenclature.Id == nomenclature.Id)
					.Sum();

				carBalance = _carBulkGoodsAccountingOperationRepository
					.GetValue(
						unitOfWork,
						cbgao => cbgao.Amount,
						cbgao => cbgao.Nomenclature.Id == nomenclature.Id)
					.Sum();
			}

			var sendedButNotRecievedBalance = _movementDocumentRepository
				.GetValue(
					unitOfWork,
					md => md.Items
						.Where(mi => mi.Nomenclature.Id == nomenclature.Id)
						.Sum(mi => mi.SentAmount),
					md => md.Status == MovementDocumentStatus.Sended
						&& md.Items.Any(mi => mi.Nomenclature.Id == nomenclature.Id))
				.Sum();

			if(warehouseBalance != 0)
			{
				errors.Add(Vodovoz.Errors.Goods.NomenclatureErrors.HasResiduesInWarhouses);
			}

			if(employeeBalance != 0)
			{
				errors.Add(Vodovoz.Errors.Goods.NomenclatureErrors.HasResiduesOnEmployees);
			}

			if(carBalance != 0)
			{
				errors.Add(Vodovoz.Errors.Goods.NomenclatureErrors.HasResiduesOnCars);
			}

			if(sendedButNotRecievedBalance != 0)
			{
				errors.Add(Vodovoz.Errors.Goods.NomenclatureErrors.HasNotAcceptedTransfers);
			}

			if(!errors.Any())
			{
				nomenclature.IsArchive = true;
				return Result.Success();
			}
			else
			{
				return Result.Failure(errors);
			}
		}

		public void CalculateMasterCallNomenclaturePriceIfNeeded(IUnitOfWork unitOfWork, Order order)
		{
			var masterCallOrerItem = order.OrderItems.FirstOrDefault(x => x.Nomenclature.Id == _masterCallNomenclatureId);

			if(masterCallOrerItem is null)
			{
				return;
			}
			
			var deliveryPoint = order.DeliveryPoint;

			if(deliveryPoint is null || order.DeliveryDate is null)
			{
				masterCallOrerItem.SetPrice(masterCallOrerItem.Nomenclature.GetPrice(1));
				
				return;
			}

			var serviceDistrict = _deliveryRepository.GetServiceDistrictByCoordinates(unitOfWork, deliveryPoint.Latitude.Value, deliveryPoint.Longitude.Value);

			if(serviceDistrict is null)
			{
				masterCallOrerItem.SetPrice(masterCallOrerItem.Nomenclature.GetPrice(1));

				return;
			}

			decimal price = 0;

			if(order.OrderItems.Any(x => x.Nomenclature.MasterServiceType ==  MasterServiceType.Cleaning))
			{
				price = GetMasterServiceTypePrice(serviceDistrict, MasterServiceType.Cleaning, order.DeliveryDate.Value);
			}
			else if(order.OrderItems.Any(x => x.Nomenclature.MasterServiceType == MasterServiceType.Repair))
			{
				price = GetMasterServiceTypePrice(serviceDistrict, MasterServiceType.Repair, order.DeliveryDate.Value);
			}

			masterCallOrerItem.SetPrice(price);
		}

		private decimal GetMasterServiceTypePrice(ServiceDistrict serviceDistrict, MasterServiceType masterServiceType, DateTime deliveryDate)
		{
			var serviceDistrictRuleByWeekDay = serviceDistrict.GetWeekDayServiceDistrictRuleByDeliveryDate(deliveryDate)
				.Where(x => x.ServiceType == masterServiceType);

			if(serviceDistrictRuleByWeekDay.Any())
			{
				return serviceDistrictRuleByWeekDay.Single().Price;
			}

			var commonServiceDistrictRule = serviceDistrict.GetCommonServiceDistrictRules()
				.Where(x => x.ServiceType == masterServiceType);

			if(commonServiceDistrictRule.Any())
			{
				return commonServiceDistrictRule.Single().Price;
			}

			return 0;
		}
	}
}
