using Gtk;
using QS.Views.Dialog;
using Vodovoz.Domain.Logistic;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.Presentation.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	public partial class DeliveryScheduleSelectionView : DialogViewBase<DeliveryScheduleSelectionViewModel>
	{
		private readonly string _dangerTextHtmlColor = GdkColors.DangerText.ToHtmlColor();
		public DeliveryScheduleSelectionView(DeliveryScheduleSelectionViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			SetLabelsContent();
			SetContainersVisibitity();
			ConfigureManualScheduleSelectionEvent();
			AddButtons();
		}

		private void SetLabelsContent()
		{
			ylabelDayOfWeek.Markup = $"<b>На {ViewModel.DeliveryDay}<b>";
			ylabelDate.Markup = $"{ViewModel.DeliveryDate.ToShortDateString()}";

			ylabelNoSchedules.Markup = $"<span foreground='{_dangerTextHtmlColor}'><b>{ViewModel.NoDeliverySchedulesMessage}</b></span>";
		}

		private void SetContainersVisibitity()
		{
			var isViewModelHasDeliverySchedules = ViewModel.DeliverySchedules.Count > 0;

			yvboxButtonsLeftColumn.Visible = isViewModelHasDeliverySchedules;
			yvboxButtonsRightColumn.Visible = isViewModelHasDeliverySchedules;
			yvboxNoSchedulesInfo.Visible = !isViewModelHasDeliverySchedules;
		}

		private void ConfigureManualScheduleSelectionEvent()
		{
			ybuttonManualScheduleSelection.Visible = ViewModel.IsUserCanSelectAnyDeliveryScheduleFromJournal;
			ybuttonManualScheduleSelection.Clicked += (s, e) => ViewModel.SelectEntityFromJournalCommand.Execute();
		}

		private void AddButtons()
		{
			for(int i = 0; i < ViewModel.DeliverySchedules.Count; i++)
			{
				var deliverySchedule = ViewModel.DeliverySchedules[i];

				if(i % 2 == 0)
				{
					yvboxButtonsLeftColumn.Add(GetButton(deliverySchedule));

					continue;
				}

				yvboxButtonsRightColumn.Add(GetButton(deliverySchedule));
			}
		}

		private Button GetButton(DeliverySchedule deliverySchedule)
		{
			var button = new Button
			{
				Label = deliverySchedule.Name
			};

			button.Clicked += (s, e) => ViewModel.SelectDeliveryScheduleCommand.Execute(deliverySchedule);

			return button;
		}
	}
}
