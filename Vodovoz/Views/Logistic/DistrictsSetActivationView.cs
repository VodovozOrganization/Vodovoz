using System;
using Gamma.GtkWidgets;
using Gtk;
using QS.Dialog.GtkUI;
using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	public partial class DistrictsSetActivationView : TabViewBase<DistrictsSetActivationViewModel>
	{
		public DistrictsSetActivationView(DistrictsSetActivationViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ylabelCurrentDistrictsSetStr.Text = ViewModel.ActiveDistrictsSet?.Name ?? "-";
			ylabelSelectedDistrictsSetStr.Text = ViewModel.Entity?.Name ?? "";

			ybuttonActivate.Clicked += async (sender, args) =>
			{
				if(!MessageDialogHelper.RunQuestionDialog($"Переключить базу на версию районов \"{ViewModel.Entity.Name}\""))
				{
					return;
				}
				try
				{
					await ViewModel.ActivateAsync();
				}
				catch(Exception ex)
				{
					Application.Invoke((s, e) => throw ex);
				}
			};

			ytreePrioritiesToDelete.ColumnsConfig = ColumnsConfigFactory.Create<DriverDistrictPriority>()
				.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
				.AddColumn("Водитель").AddTextRenderer(x => x.DriverDistrictPrioritySet.Driver.ShortName)
				.AddColumn("Старый район").AddTextRenderer(x => x.District.DistrictName)
				.AddColumn("")
				.Finish();

			ViewModel.PropertyChanged += (sender, args) =>
			{
				Application.Invoke((s, e) =>
				{
					if(args.PropertyName == nameof(ViewModel.ActivationStatus))
					{
						ylabelActivationStatus.Text = ViewModel.ActivationStatus;
					}
					if(args.PropertyName == nameof(ViewModel.ActivationInProgress) || args.PropertyName == nameof(ViewModel.WasActivated))
					{
						ybuttonActivate.Sensitive = !ViewModel.ActivationInProgress && !ViewModel.WasActivated;
					}
					if(args.PropertyName == nameof(ViewModel.ActiveDistrictsSet))
					{
						ylabelCurrentDistrictsSetStr.Text = ViewModel.ActiveDistrictsSet.Name;
					}
					if(args.PropertyName == nameof(ViewModel.NotCopiedPriorities))
					{
						ytreePrioritiesToDelete.ItemsDataSource = ViewModel.NotCopiedPriorities;
					}
				});
			};
		}
	}
}
