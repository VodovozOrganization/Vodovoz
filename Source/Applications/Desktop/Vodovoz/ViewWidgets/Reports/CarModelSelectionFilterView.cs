using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.ViewModels.Widgets.Cars.CarModelSelection;
using static Vodovoz.ViewModels.Widgets.Cars.CarModelSelection.CarModelSelectionFilterViewModel;

namespace Vodovoz.ViewWidgets.Reports
{
	public partial class CarModelSelectionFilterView : WidgetViewBase<CarModelSelectionFilterViewModel>
	{
		public CarModelSelectionFilterView(CarModelSelectionFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			yentrySearch.Binding
				.AddBinding(ViewModel, vm => vm.SearchString, w => w.Text)
				.InitializeFromSource();

			ybuttonSelectAll.Clicked += (s, e) => ViewModel.SelectAllRowsCommand?.Execute();
			ybuttonRemoveAll.Clicked += (s, e) => ViewModel.RemoveAllRowsSelectionCommand?.Execute();
			ybuttonInversSelected.Clicked += (s, e) => ViewModel.InverseSelectionCommand?.Execute();

			ylabelSelectedInfo.Binding
				.AddBinding(ViewModel, vm => vm.SelectedRowsCountInfo, w => w.Text)
				.InitializeFromSource();

			ytreeviewCarModelsList.CreateFluentColumnsConfig<CarModelRow>()
					.AddColumn("\t✔️")
					.AddToggleRenderer(x => x.IsIncluded)
					.AddColumn("X").AddToggleRenderer(x => x.IsExcluded)
					.AddColumn("Модель").AddTextRenderer(x => x.ModelInfo)
					.AddSetter((cell, node) =>
					{
						if(cell == null)
						{
							return;
						}

						if(!string.IsNullOrWhiteSpace(ViewModel.SearchString))
						{
							cell.Markup = node.ModelInfo.Replace(ViewModel.SearchString, $"<b>{ViewModel.SearchString}</b>");
						}
					})
					.Finish();

		}
	}
}
