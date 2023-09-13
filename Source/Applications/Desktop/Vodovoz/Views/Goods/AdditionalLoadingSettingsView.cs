using System.ComponentModel;
using Gamma.GtkWidgets;
using Gtk;
using QS.Navigation;
using QS.Views.Dialog;
using QS.Widgets;
using Vodovoz.Domain.Goods;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Goods;

namespace Vodovoz.Views.Goods
{
	public partial class AdditionalLoadingSettingsView : DialogViewBase<AdditionalLoadingSettingsViewModel>
	{
		public AdditionalLoadingSettingsView(AdditionalLoadingSettingsViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ybuttonCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
			ybuttonCancel.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ybuttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			ybuttonSave.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			entryFastDeliveryMaxDistance.ValidationMode = ValidationType.Numeric;
			entryFastDeliveryMaxDistance.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(vm => vm.FastDeliveryMaxDistance, w => w.Text, new DoubleToStringConverter())
				.InitializeFromSource();

			entryMaxOrdersCount.ValidationMode = ValidationType.Numeric;
			entryMaxOrdersCount.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(vm => vm.MaxFastOrdersPerSpecificTime, w => w.Text, new IntToStringConverter())
				.InitializeFromSource();

			ycheckFlyerAdditionEnabled.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(vm => vm.FlyerAdditionEnabled, w => w.Active)
				.InitializeFromSource();

			entryBottlesCount.ValidationMode = ValidationType.Numeric;
			entryBottlesCount.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(vm => vm.BottlesCount, w => w.Text, new IntToStringConverter())
				.InitializeFromSource();

			ycheckFlyerAdditionForNewCounterpartyEnabled.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(vm => vm.FlyerForNewCounterpartyEnabled, w => w.Active)
				.InitializeFromSource();

			entryBottlesForNewCounterpartyCount.ValidationMode = ValidationType.Numeric;
			entryBottlesForNewCounterpartyCount.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(vm => vm.FlyerForNewCounterpartyBottlesCount, w => w.Text, new IntToStringConverter())
				.InitializeFromSource();

			ybuttonAddNomenclature.Clicked += (sender, args) => ViewModel.AddNomenclatureDistributionCommand.Execute();
			ybuttonAddNomenclature.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ybuttonRemoveNomenclature.Clicked += (sender, args) =>
				ViewModel.RemoveNomenclatureDistributionCommand.Execute(
					ytreeNomenclatures.GetSelectedObjects<AdditionalLoadingNomenclatureDistribution>());
			ybuttonRemoveNomenclature.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ytreeNomenclatures.ColumnsConfig = ColumnsConfigFactory.Create<AdditionalLoadingNomenclatureDistribution>()
				.AddColumn("ТМЦ")
					.HeaderAlignment(0.5f)
					.MinWidth(320)
					.AddTextRenderer(x => x.Nomenclature.Name)
				.AddColumn("Распределение %")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.Percent)
					.Adjustment(new Adjustment(1, 0.01, 100, 1, 1, 0))
					.Digits(2)
					.AddSetter((c, n) => { c.Editable = ViewModel.CanEdit; })
					.XAlign(0.5f)
				.Finish();
			ytreeNomenclatures.Reorderable = true;
			ytreeNomenclatures.Selection.Mode = SelectionMode.Multiple;
			ytreeNomenclatures.ItemsDataSource = ViewModel.ObservableNomenclatureDistributions;

			ybuttonFlyerInfo.Clicked += (sender, args) => ViewModel.ShowFlyerInfoCommand.Execute();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
			OnViewModelPropertyChanged(this, new PropertyChangedEventArgs(nameof(ViewModel.PercentSum)));
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.PercentSum))
			{
				ylabelPercentSum.Markup = "Сумма процентов: " +
					(ViewModel.PercentSum == 100m ? "100" : $"<span foreground = \"{GdkColors.Red.ToHtmlColor()}\">{ViewModel.PercentSum}</span>");
			}
		}

		protected override void OnDestroyed()
		{
			ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
			base.OnDestroyed();
		}
	}
}
