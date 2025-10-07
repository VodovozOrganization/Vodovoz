using QS.DomainModel.Entity;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Tools.CallTasks;

namespace Vodovoz
{
	public class RouteListKeepingItemNode : PropertyChangedBase
	{
		public bool HasChanged = false;
		public bool PaymentTypeHasChanged = false;
		public bool ChangedDeliverySchedule = false;
		public event EventHandler<StatusChangedEventArgs> StatusChanged;

		private RouteListItem _routeListItem;
		private PaymentType _paymentType;

		public RouteListItemStatus Status
		{
			get => RouteListItem.Status;
			set
			{
				StatusChanged?.Invoke(this, new StatusChangedEventArgs(value));
			}
		}

		public TimeSpan? WaitUntil
		{
			get => RouteListItem.Order.WaitUntilTime;
			set => RouteListItem.Order.WaitUntilTime = value;
		}

		public string Comment
		{
			get => RouteListItem.Comment;
			set
			{
				RouteListItem.Comment = value;
				OnPropertyChanged(() => Comment);
			}
		}

		public string LastUpdate
		{
			get
			{
				var maybeLastUpdate = RouteListItem.StatusLastUpdate;
				if(maybeLastUpdate.HasValue)
				{
					if(maybeLastUpdate.Value.Date == DateTime.Today)
					{
						return maybeLastUpdate.Value.ToShortTimeString();
					}
					else
					{
						return maybeLastUpdate.Value.ToString();
					}
				}
				return string.Empty;
			}
		}

		public PaymentType PaymentType
		{
			get => _paymentType;
			set
			{
				if(_paymentType != value)
				{
					_paymentType = value;
					PaymentTypeHasChanged = true;
					OnPropertyChanged(() => PaymentType);
				}
			}
		}

		public RouteListItem RouteListItem
		{
			get => _routeListItem;
			set
			{
				_routeListItem = value;
				if(RouteListItem != null)
				{
					_paymentType = RouteListItem.Order.PaymentType;
					RouteListItem.PropertyChanged += (sender, e) => OnPropertyChanged(() => RouteListItem);
				}
			}
		}

		public DateTime? RecievedTransferAt => RouteListItem.RecievedTransferAt;

		public string Transferred => RouteListItem.GetTransferText();

		#region Контроль отмены автоотмены автопереноса

		public bool InitialRouteListItemStatusIsInUndeliveryStatuses { get; set; }
		public bool RouteListItemStatusHasChangedToCompeteStatus { get; set; }

		#endregion Контроль отмены автоотмены автопереноса

		public void UpdateStatus(RouteListItemStatus value, ICallTaskWorker callTaskWorker)
		{
			var uow = RouteListItem.RouteList.UoW;
			RouteListItem.RouteList.ChangeAddressStatusAndCreateTask(uow, RouteListItem.Id, value, callTaskWorker);

			if(RouteListItem.Status == RouteListItemStatus.Overdue || RouteListItem.Status == RouteListItemStatus.Canceled)
			{
				RouteListItem.SetOrderActualCountsToZeroOnCanceled();
			}
			
			HasChanged = true;
			OnPropertyChanged(() => Status);
		}
	}
}
