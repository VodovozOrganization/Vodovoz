using QS.Views.GtkUI;
using System.Linq;
using Vodovoz.ViewModels.Widgets.Cars.CarModelSelection;
using static Vodovoz.ViewModels.Widgets.Cars.CarModelSelection.CarModelSelectionFilterViewModel;

namespace Vodovoz.ViewWidgets.Reports
{
	public partial class CarModelSelectionFilterView : WidgetViewBase<CarModelSelectionFilterViewModel>
	{
		public CarModelSelectionFilterView(CarModelSelectionFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			yentrySearch.Binding
				.AddBinding(ViewModel, vm => vm.SearchString, w => w.Text)
				.InitializeFromSource();

			ybuttonSearchClear.Clicked += (s, e) => ViewModel.ClearSearchStringCommand?.Execute();
			ybuttonClearIncludes.Clicked += (s, e) => ViewModel.ClearAllIncludesCommand?.Execute();
			ybuttonClearExcludes.Clicked += (s, e) => ViewModel.ClearAllExcludesCommand?.Execute();

			ycheckbuttonShowArchive.Binding
				.AddBinding(ViewModel, vm => vm.IsShowArchiveCarModels, w => w.Active)
				.InitializeFromSource();

			ylabelSelectedInfo.Binding
				.AddBinding(ViewModel, vm => vm.IncludedExcludesNodesCountInfo, w => w.Text)
				.InitializeFromSource();

			ytreeviewCarModelsList.HeightRequest = 150;

			ytreeviewCarModelsList.CreateFluentColumnsConfig<CarModelSelectableNode>()
					.AddColumn("✔️")
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
							if(ViewModel.IsModelInfoContainsSearchStringCheck(node))
							{
								var modelInfo = node.ModelInfo.ToLower();
								var searchString = ViewModel.SearchString.Trim().ToLower();
								var substringStartIndex = modelInfo.IndexOf(searchString);
								var substringFromModelInfo = node.ModelInfo.Substring(substringStartIndex, searchString.Length);

								cell.Markup = node.ModelInfo.Replace(substringFromModelInfo, $"<b>{substringFromModelInfo}</b>");
							}
						}
					})
					.Finish();

			ytreeviewCarModelsList.Binding
				.AddSource(ViewModel)
				.AddFuncBinding(vm => vm.CarModelNodes.Where(n => n.IsVisible).ToList(), w => w.ItemsDataSource)
				.InitializeFromSource();
		}

		public override void Destroy()
		{
			ytreeviewCarModelsList?.Destroy();

			base.Destroy();
		}
	}
}
