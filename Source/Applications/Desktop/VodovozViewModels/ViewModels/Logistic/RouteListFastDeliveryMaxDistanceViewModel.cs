using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class RouteListFastDeliveryMaxDistanceViewModel : EntityTabViewModelBase<RouteList>
	{
		private readonly ILogger<RouteListFastDeliveryMaxDistanceViewModel> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IDeliveryRepository _deliveryRepository;

		public RouteListFastDeliveryMaxDistanceViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IRouteListItemRepository routeListItemRepository,
			IDeliveryRepository deliveryRepository,
			ILogger<RouteListFastDeliveryMaxDistanceViewModel> logger) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			_fastDeliveryMaxDistance = _deliveryRepository.GetFastDeliveryMaxDistanceValue(UoW, Entity.Id);
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			ValidationContext.Items.Add(nameof(IRouteListItemRepository), routeListItemRepository);
			//Для возможности изменения пустых МЛ
			ValidationContext.Items.Add(
				Core.Domain.Permissions.LogisticPermissions.RouteList.CanCreateRouteListWithoutOrders,
				true);

			TabName = $"Изменение радиуса быстрой доставки для маршрутного листа №{Entity.Id}";
		}

		private decimal _fastDeliveryMaxDistance;
		public decimal FastDeliveryMaxDistance
		{
			get => _fastDeliveryMaxDistance;
			set => SetField(ref _fastDeliveryMaxDistance, value);
		}

		public override bool Save(bool close)
		{
			if(FastDeliveryMaxDistance != _deliveryRepository.GetFastDeliveryMaxDistanceValue(UoW, Entity.Id))
			{
				Entity.UpdateFastDeliveryMaxDistanceValue(FastDeliveryMaxDistance);
			}

			_logger.LogInformation("Добавляем новое значения радиуса быстрой доставки...");
			return base.Save(close);
		}
	}
}
