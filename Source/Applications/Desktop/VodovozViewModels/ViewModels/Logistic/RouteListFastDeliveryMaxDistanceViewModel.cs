using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class RouteListFastDeliveryMaxDistanceViewModel : EntityTabViewModelBase<RouteList>
	{
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
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
			get
			{
				return _fastDeliveryMaxDistance;
			}
			set
			{
				_fastDeliveryMaxDistance = value;
				if (_fastDeliveryMaxDistance != Entity.CurrentFastDeliveryMaxDistanceValue) 
				{
					Entity.UpdateFastDeliveryMaxDistanceValue(_fastDeliveryMaxDistance);
				}
			}
		}

		public override bool Save(bool close)
		{
			_logger.Info("Добавляем новое значения радиуса быстрой доставки...");
			return base.Save(close);
		}
	}
}
