using Gtk;
using QS.DomainModel.Entity;
using QS.Views.Dialog;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.Presentation.ViewModels.Controls.EntitySelection;

namespace Vodovoz.Views
{
	public partial class EntityButtonsSelectionView : DialogViewBase<EntityButtonsSelectionViewModel>
	{
		private readonly string _dangerTextHtmlColor = GdkColors.DangerText.ToHtmlColor();

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
			ylabelTopLabel.Markup = ViewModel.DialogSettings.TopLabelText;

			ylabelNoButtons.Markup = $"<span foreground='{_dangerTextHtmlColor}'><b>{ViewModel.DialogSettings.NoEntitiesMessage}</b></span>";
		}

		private void SetContainersVisibitity()
		{
			var isViewModelHasDeliverySchedules = ViewModel.Entities.Count > 0;

			yvboxLeftButtons.Visible = isViewModelHasDeliverySchedules;
			yvboxRightButtons.Visible = isViewModelHasDeliverySchedules;
			yvboxNoButtons.Visible = !isViewModelHasDeliverySchedules;
		}

		private void ConfigureManualScheduleSelectionEvent()
		{
			ybuttonSelectFromJournal.Visible = ViewModel.DialogSettings.IsCanOpenJournal;
			ybuttonSelectFromJournal.Clicked += (s, e) => ViewModel.SelectEntityFromJournalCommand.Execute();
		}

		private void AddButtons()
		{
			for(var i = 0; i < ViewModel.Entities.Count; i++)
			{
				var entity = ViewModel.Entities[i];

				if(i % 2 == 0)
				{
					yvboxLeftButtons.Add(GetButton(entity));

					continue;
				}

				yvboxRightButtons.Add(GetButton(entity));
			}

			yvboxLeftButtons.ShowAll();
			yvboxRightButtons.ShowAll();
		}

		private Button GetButton(object entity)
		{
			var button = new Button
			{
				Label = entity.GetTitle()
			};

			button.Clicked += (s, e) => ViewModel.SelectEntityCommand.Execute(entity);

			return button;
		}
	}
}
