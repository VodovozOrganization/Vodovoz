using Gamma.GtkWidgets;
using QS.Utilities;
using QS.Views.GtkUI;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewModels.Cash.DocumentsJournal;
using Vodovoz.ViewModels.ViewModels.SidePanels;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashInfoPanelView : WidgetViewBase<CashInfoPanelViewModel>, IPanelView
	{

		public CashInfoPanelView(CashInfoPanelViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureWidget();
		}

		protected override void ConfigureWidget()
		{
			yTreeView.ColumnsConfig = ColumnsConfigFactory.Create<object>()
				.AddColumn("Ответственный")
					.AddTextRenderer(n => ViewModel.GetNodeText(n))
					.AddSetter((c, n) => c.Alignment = n is SubdivisionBalanceNode ? Pango.Alignment.Left : Pango.Alignment.Right)
					.WrapWidth(110).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Баланс")
					.AddTextRenderer(n => CurrencyWorks.GetShortCurrencyString(ViewModel.GetBalance(n)))
					.WrapWidth(110).WrapMode(Pango.WrapMode.WordChar)
				.Finish();

			#region MainInfo

			ylabelMainInfo.Binding
				.AddBinding(ViewModel, vm => vm.MainInfo, w => w.LabelProp)
				.InitializeFromSource();

			#endregion

			#region Detalization

			yTreeView.Binding
				.AddBinding(ViewModel, vm => vm.LevelTreeModel, w => w.YTreeModel)
				.InitializeFromSource();

			ylabelDetalizationTitle.Binding
				.AddBinding(ViewModel, vm => vm.DetalizationTitle, w => w.LabelProp)
				.InitializeFromSource();

			ylabelInfoTop.Binding
				.AddBinding(ViewModel, vm => vm.DetalizationInfoTop, w => w.LabelProp)
				.InitializeFromSource();

			ylabelInfoBottom.Binding
				.AddBinding(ViewModel, vm => vm.DetalizationInfoBottom, w => w.LabelProp)
				.InitializeFromSource();

			#endregion
		}

		#region IPanelView implementation

		public IInfoProvider InfoProvider { get; set; }

		public bool VisibleOnPanel => true;

		public void OnCurrentObjectChanged(object changedObject) => Refresh();

		public void Refresh()
		{
			if(!(InfoProvider is IDocumentsInfoProvider documentsInfoProvider))
			{
				return;
			}

			var filter = documentsInfoProvider.DocumentsFilterViewModel;

			if(filter is null)
			{
				return;
			}

			ViewModel.RefreshCommand.Execute(filter);
		}
		

		#endregion

		public override void Destroy()
		{
			yTreeView?.Destroy();
			base.Destroy();
		}
	}
}
