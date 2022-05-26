using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Tools.Orders;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class FastDeliveryAvailabilityHistoryViewModel : EntityTabViewModelBase<FastDeliveryAvailabilityHistory>
	{
		private DelegateCommand _saveLogisticiaCommentCommand;
		private readonly Employee _logistician;

		public FastDeliveryAvailabilityHistoryViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			INavigationManager navigation = null) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			FastDeliveryVerificationViewModel = new FastDeliveryVerificationViewModel(GetFastDeliveryVerification());
			_logistician = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
		}

		private FastDeliveryVerificationDTO GetFastDeliveryVerification()
		{
			//var nodes = new GenericObservableList<FastDeliveryVerificationDetailsNode>();

			//foreach(var item in Entity.Items)
			//{
			//	var node = new FastDeliveryVerificationDetailsNode
			//	{
			//		RouteList = item.RouteList,
			//		IsValidRLToFastDelivery = item.IsValidToFastDelivery,

			//		RemainingTimeForShipmentNewOrder = new FastDeliveryVerificationParameter<TimeSpan>
			//		{
			//			IsValidParameter = item.IsValidRemainingTimeForShipmentNewOrder,
			//			ParameterValue = item.RemainingTimeForShipmentNewOrder
			//		},
			//		DistanceByLineToClient = new FastDeliveryVerificationParameter<decimal>
			//		{
			//			IsValidParameter = item.IsValidDistanceByLineToClient,
			//			ParameterValue = item.DistanceByLineToClient
			//		},
			//		DistanceByRoadToClient = new FastDeliveryVerificationParameter<decimal>
			//		{
			//			IsValidParameter = item.IsValidDistanceByRoadToClient,
			//			ParameterValue = item.DistanceByRoadToClient
			//		},
			//		IsGoodsEnough = new FastDeliveryVerificationParameter<bool>
			//		{
			//			IsValidParameter = item.IsValidIsGoodsEnough,
			//			ParameterValue = item.IsGoodsEnough
			//		},
			//		LastCoordinateTime = new FastDeliveryVerificationParameter<TimeSpan>
			//		{
			//			IsValidParameter = item.IsValidLastCoordinateTime,
			//			ParameterValue = item.LastCoordinateTime
			//		},
			//		UnClosedFastDeliveries = new FastDeliveryVerificationParameter<int>
			//		{
			//			IsValidParameter = item.IsValidUnclosedFastDeliveries,
			//			ParameterValue = item.UnclosedFastDeliveries
			//		}
			//	};
			//	nodes.Add(node);
			//}

			var fastDeliveryHistoryItemConverter = new FastDeliveryHistoryItemsConverter();

			return new FastDeliveryVerificationDTO
			{
				FastDeliveryVerificationDetailsNodes = fastDeliveryHistoryItemConverter.ConvertAvailabilityHistoryItemsToVerificationDetailsNodes(Entity.Items),
				FastDeliveryAvailabilityHistory = new FastDeliveryAvailabilityHistory
				{
					Order = Entity.Order
				}
			};
		}


		public DelegateCommand SaveLogisticiaCommentCommand =>
			_saveLogisticiaCommentCommand ?? (_saveLogisticiaCommentCommand = new DelegateCommand(() =>
				{
					if(Entity.Logistician == null)
					{
						Entity.Logistician = _logistician;
					}

					Entity.LogisticianCommentVersion = DateTime.Now;

					UoW.Save();
					UoW.Commit();
				},
				() => true
			));

		public override bool HasChanges => false;

		public FastDeliveryVerificationViewModel FastDeliveryVerificationViewModel { get; }
	}
}
