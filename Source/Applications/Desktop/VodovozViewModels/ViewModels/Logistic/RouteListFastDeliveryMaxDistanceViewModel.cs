using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class RouteListFastDeliveryMaxDistanceViewModel : EntityTabViewModelBase<RouteList>
	{
		private readonly ILogger<RouteListFastDeliveryMaxDistanceViewModel> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IRouteListItemRepository _routeListItemRepository;

		public RouteListFastDeliveryMaxDistanceViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IRouteListItemRepository routeListItemRepository,
			ILogger<RouteListFastDeliveryMaxDistanceViewModel> logger) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_fastDeliveryMaxDistance = Entity.CurrentFastDeliveryMaxDistanceValue;
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
			if(FastDeliveryMaxDistance != Entity.CurrentFastDeliveryMaxDistanceValue)
			{
				Entity.UpdateFastDeliveryMaxDistanceValue(FastDeliveryMaxDistance);
			}

			_logger.LogInformation("Добавляем новое значения радиуса быстрой доставки...");
			return base.Save(close);
		}
	}
}
