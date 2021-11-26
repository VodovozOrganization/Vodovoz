using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using QS.Models;
using Vodovoz.EntityRepositories.Store;

namespace WhereIsTheBottle.Models
{
	public class BottleAnalyticsModel : UoWFactoryModelBase
	{
		private readonly IWarehouseRepository _warehouseRepository;

		public BottleAnalyticsModel(IUnitOfWorkFactory unitOfWorkFactory, IWarehouseRepository warehouseRepository)
			: base(unitOfWorkFactory)
		{
			_warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
		}

		public async Task<IList<WarehouseNode>> GetActiveWarehouseNodesAsync()
		{
			using var uow = UnitOfWorkFactory.CreateWithoutRoot();
			return await Task.Run(() => _warehouseRepository.GetActiveWarehouseNodes(uow));
		}
	}
}
