using Grpc.Core.Logging;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class RouteListFastDeliveryMaxDistanceViewModel : EntityTabViewModelBase<RouteList>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly int _routeListId;

		//private readonly ILogger _logger;

		public RouteListFastDeliveryMaxDistanceViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			int routeListId
			//ILogger logger
			)
		: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_routeListId = routeListId;
			//_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			_fastDeliveryMaxDistance = Entity.CurrentFastDeliveryMaxDistanceValue;

			TabName = "Редактирование радиуса быстрой доставки для маршрутного листа";
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
			//_logger.Info("Добавляем в класс маршрутного листа новое значения радиуса быстрой доставки...");
			return base.Save(close);
		}
	}
}
