using System;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Widgets
{
	public class UndeliveredOrdersViewModel : EntityWidgetViewModelBase<UndeliveredOrder>
	{
		private DateTime? _selectedDate;
		private UndeliveredOrder _selectedUndeliveredOrder;
		//private DelegateCommand _addNewUndeliveredOrderCommand;
		//private DelegateCommand _changeUndeliveredOrderStartDateCommand;

		public UndeliveredOrdersViewModel(UndeliveredOrder entity, ICommonServices commonServices)
			: base(entity, commonServices)
		{

			//CanRead = PermissionResult.CanRead;
			//CanCreate = PermissionResult.CanCreate && Entity.Id == 0
			//	|| commonServices.CurrentPermissionService.ValidatePresetPermission("can_change_odometer_reading");
			//CanEdit = commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_odometer_reading");

			if(IsNewCar)
			{
				SelectedDate = DateTime.Now.Date;
			}
		}

		public DateTime? SelectedDate
		{
			get => _selectedDate;
			set
			{
				if(SetField(ref _selectedDate, value))
				{
					//OnPropertyChanged(nameof(CanAddNewUndeliveredOrder));
					//OnPropertyChanged(nameof(CanChangeUndeliveredOrderDate));
				}
			}
		}

		public UndeliveredOrder SelectedUndeliveredOrder
		{
			get => _selectedUndeliveredOrder;
			set
			{
				if(SetField(ref _selectedUndeliveredOrder, value))
				{
					//OnPropertyChanged(nameof(CanChangeUndeliveredOrderDate));
				}
			}
		}

		public bool CanRead { get; }
		public bool CanCreate { get; }
		public bool CanEdit { get; }

		public bool IsNewCar => Entity.Id == 0;

		//public bool CanAddNewUndeliveredOrder =>
		//	CanCreate
		//	&& SelectedDate.HasValue
		//	&& Entity.UndeliveredOrders.All(x => x.Id != 0)
		//	&& _undeliveredOrderController.IsValidDateForNewUndeliveredOrder(SelectedDate.Value);

		//public bool CanChangeUndeliveredOrderDate =>
		//	SelectedDate.HasValue
		//	&& SelectedUndeliveredOrder != null
		//	&& (CanEdit || SelectedUndeliveredOrder.Id == 0)
		//	&& _undeliveredOrderController.IsValidDateForUndeliveredOrderStartDateChange(SelectedUndeliveredOrder, SelectedDate.Value);


		#region Commands

		//public DelegateCommand AddNewUndeliveredOrderCommand =>
		//	_addNewUndeliveredOrderCommand ?? (_addNewUndeliveredOrderCommand = new DelegateCommand(() =>
		//	{
		//		if(SelectedDate == null)
		//		{
		//			return;
		//		}
		//		_undeliveredOrderController.CreateAndAddUndeliveredOrder(SelectedDate);

		//		OnPropertyChanged(nameof(CanAddNewUndeliveredOrder));
		//		OnPropertyChanged(nameof(CanChangeUndeliveredOrderDate));
		//	}
		//	));

		//public DelegateCommand ChangeUndeliveredOrderStartDateCommand =>
		//	_changeUndeliveredOrderStartDateCommand ?? (_changeUndeliveredOrderStartDateCommand = new DelegateCommand(() =>
		//	{
		//		if(SelectedDate == null || SelectedUndeliveredOrder == null)
		//		{
		//			return;
		//		}
		//		_undeliveredOrderController.ChangeUndeliveredOrderStartDate(SelectedUndeliveredOrder, SelectedDate.Value);

		//		OnPropertyChanged(nameof(CanAddNewUndeliveredOrder));
		//		OnPropertyChanged(nameof(CanChangeUndeliveredOrderDate));
		//	}
		//	));

		#endregion
	}
}
