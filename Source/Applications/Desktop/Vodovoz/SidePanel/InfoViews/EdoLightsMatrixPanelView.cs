using QS.Views.GtkUI;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewModels.ViewModels.SidePanels;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EdoLightsMatrixPanelView : WidgetViewBase<EdoLightsMatrixPanelViewModel>, IPanelView
	{
		public EdoLightsMatrixPanelView(EdoLightsMatrixPanelViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureWidget();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			ybuttonOpenCounterparty.Clicked += (s, a) =>
			{
				if(InfoProvider is IEdoLightsMatrixInfoProvider edoLightsMatrixInfoProvider
					&& edoLightsMatrixInfoProvider.Counterparty != null)
				{
					ViewModel.OpenEdoTabInCounterparty.Execute(edoLightsMatrixInfoProvider.Counterparty.Id);
				}
			};

			edoLightsMatrixvVew.ViewModel = ViewModel.EdoLightsMatrixViewModel;
		}

		#region IPanelView implementation

		public IInfoProvider InfoProvider { get; set; }
		public void Refresh()
		{
			if(InfoProvider is IEdoLightsMatrixInfoProvider edoLightsMatrixInfoProvider
				&& edoLightsMatrixInfoProvider.Counterparty != null)
			{
				ViewModel.Refresh(edoLightsMatrixInfoProvider.Counterparty, edoLightsMatrixInfoProvider.EdoLightMatrxiOrganization?.Id);
			}
		}

		public void OnCurrentObjectChanged(object changedObject) => Refresh();

		public bool VisibleOnPanel => true;

		#endregion IPanelView implementation
	}
}
