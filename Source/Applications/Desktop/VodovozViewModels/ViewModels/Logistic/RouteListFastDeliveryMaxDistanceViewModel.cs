using NLog;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class RouteListFastDeliveryMaxDistanceViewModel : EntityTabViewModelBase<RouteList>
	{
		private static ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IRouteListItemRepository _routeListItemRepository;

		public RouteListFastDeliveryMaxDistanceViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IRouteListItemRepository routeListItemRepository) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_fastDeliveryMaxDistance = Entity.CurrentFastDeliveryMaxDistanceValue;

			ValidationContext.Items.Add(nameof(IRouteListItemRepository), routeListItemRepository);

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

			_logger.Info("Добавляем новое значения радиуса быстрой доставки...");
			return base.Save(close);
		}
	}
}
