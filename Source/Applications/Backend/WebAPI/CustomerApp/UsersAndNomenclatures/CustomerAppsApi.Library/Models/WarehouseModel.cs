using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Store;

namespace CustomerAppsApi.Library.Models
{
	public class WarehouseModel : IWarehouseModel
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IWarehouseRepository _warehouseRepository;

		public WarehouseModel(
			IUnitOfWork unitOfWork,
			IWarehouseRepository warehouseRepository)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
		}

		public IEnumerable<SelfDeliveryAddressDto> GetSelfDeliveriesAddresses()
		{
			return _warehouseRepository.GetSelfDeliveriesAddresses(_unitOfWork);
		}
	}
}
