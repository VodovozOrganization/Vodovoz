using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Presentation.ViewModels.Logistic
{
	public class DeliveryScheduleSelectionViewModel : WindowDialogViewModelBase
	{
		public event EventHandler<DeliveryScheduleSelectedEventArgs> DeliveryScheduleSelected;
		public event EventHandler SelectEntityFromJournalSelected;

		public DeliveryScheduleSelectionViewModel(
			INavigationManager navigation,
			IList<DeliverySchedule> deliverySchedules,
			DateTime deliveryDate,
			bool isUserCanOpenJournal = false
			) : base(navigation)
		{
			DeliverySchedules = deliverySchedules;
			DeliveryDate = deliveryDate;
			IsUserCanOpenJournal = isUserCanOpenJournal;
			DeliveryDay = GetDeliveryDay();

			SelectDeliveryScheduleCommand = new DelegateCommand<DeliverySchedule>(SelectDeliverySchedule);
			SelectEntityFromJournalCommand = new DelegateCommand(SelectEntityFromJournal, () => IsUserCanOpenJournal);
		}

		public DateTime DeliveryDate { get; }
		public string DeliveryDay { get; }
		public bool IsUserCanOpenJournal { get; }
		public IList<DeliverySchedule> DeliverySchedules { get; }
		public string NoDeliverySchedulesMessage =>
			"На данный день\nинтервалы\nдоставки\nотсутствуют";

		public DelegateCommand<DeliverySchedule> SelectDeliveryScheduleCommand { get; }
		public DelegateCommand SelectEntityFromJournalCommand { get; }

		private string GetDeliveryDay()
		{
			return DeliveryDate.ToString();
		}

		private void SelectDeliverySchedule(DeliverySchedule deliverySchedule)
		{
			DeliveryScheduleSelected?.Invoke(this, new DeliveryScheduleSelectedEventArgs(deliverySchedule));
			CloseWindow();
		}

		private void SelectEntityFromJournal()
		{
			SelectEntityFromJournalSelected?.Invoke(this, EventArgs.Empty);
			CloseWindow();
		}

		private void CloseWindow()
		{
			Close(false, CloseSource.Self);
		}

		public class DeliveryScheduleSelectedEventArgs : EventArgs
		{
			public DeliveryScheduleSelectedEventArgs(DeliverySchedule deliverySchedule)
			{
				DeliverySchedule = deliverySchedule;
			}

			public DeliverySchedule DeliverySchedule { get; }
		}
	}
}
