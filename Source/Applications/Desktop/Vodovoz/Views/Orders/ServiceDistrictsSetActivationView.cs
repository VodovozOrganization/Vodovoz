using QS.Dialog.GtkUI;
using QS.Views.GtkUI;
using System;
using Vodovoz.ViewModels.Logistic;
namespace Vodovoz.Views.Orders
{
	// Art8m Переделать
	public partial class ServiceDistrictsSetActivationView : TabViewBase<ServiceDistrictsSetActivationViewModel>
	{
		public ServiceDistrictsSetActivationView(ServiceDistrictsSetActivationViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ylabelCurrentDistrictsSetStr.Text = ViewModel.ActiveServiceDistrictsSet?.Name ?? "-";
			ylabelSelectedDistrictsSetStr.Text = ViewModel.Entity?.Name ?? "";
			ylabelPriorities.Text =
				"На новую версию не были перенесены следующие приоритеты районов водителей (необходимо вручную проконтролировать настройку приоритетов районов указанных водителей):";

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
					Gtk.Application.Invoke((s, e) => throw ex);
				}
			};

			ViewModel.PropertyChanged += (sender, args) =>
			{
				Gtk.Application.Invoke((s, e) =>
				{
					if(args.PropertyName == nameof(ViewModel.ActivationStatus))
					{
						ylabelActivationStatus.Text = ViewModel.ActivationStatus;
					}
					if(args.PropertyName == nameof(ViewModel.ActivationInProgress) || args.PropertyName == nameof(ViewModel.WasActivated))
					{
						ybuttonActivate.Sensitive = !ViewModel.ActivationInProgress && !ViewModel.WasActivated;
					}
					if(args.PropertyName == nameof(ViewModel.ActiveServiceDistrictsSet))
					{
						ylabelCurrentDistrictsSetStr.Text = ViewModel.ActiveServiceDistrictsSet.Name;
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
