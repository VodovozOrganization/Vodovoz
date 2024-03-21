using Gtk;
using QS.Views.Dialog;
using Vodovoz.Presentation.ViewModels.Controls.EntitySelection;

namespace Vodovoz.Views
{
	public partial class EntityButtonsSelectionView : DialogViewBase<EntityButtonsSelectionViewModel>
	{
		public EntityButtonsSelectionView(EntityButtonsSelectionViewModel viewModel) : base(viewModel)
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
			ylabelTopLabel.Markup = ViewModel.TopMessageString;

			ylabelNoButtons.Markup = ViewModel.NoEntitiesMessage;
		}

		private void SetContainersVisibitity()
		{
			var isViewModelHasDeliverySchedules = ViewModel.Entities.Count > 0;

			yvboxLeftButtons.Visible = isViewModelHasDeliverySchedules;
			yvboxRightButtons.Visible = isViewModelHasDeliverySchedules;
			yvbox4.Visible = !isViewModelHasDeliverySchedules;
		}

		private void ConfigureManualScheduleSelectionEvent()
		{
			ybuttonSelectFromJournal.Visible = ViewModel.IsCanOpenJournal;
			ybuttonSelectFromJournal.Clicked += (s, e) => ViewModel.SelectEntityFromJournalCommand.Execute();
		}

		private void AddButtons()
		{
			var counter = 0;

			foreach(var entity in ViewModel.Entities)
			{
				if(counter % 2 == 0)
				{
					yvboxLeftButtons.Add(GetButton(entity.Key, entity.Value));

					continue;
				}

				yvboxRightButtons.Add(GetButton(entity.Key, entity.Value));

				counter++;
			}

			yvboxLeftButtons.ShowAll();
			yvboxRightButtons.ShowAll();
		}

		private Button GetButton(object entity, string buttonLabel)
		{
			var button = new Button
			{
				Label = buttonLabel
			};

			button.Clicked += (s, e) => ViewModel.SelectEntityCommand.Execute(entity);

			return button;
		}
	}
}
