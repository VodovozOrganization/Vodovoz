using Gamma.ColumnConfig;
using Gdk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Widgets.EdoLightsMatrix;
using LightsMatrixRow = Vodovoz.ViewModels.Widgets.EdoLightsMatrix.LightsMatrixRow;

namespace Vodovoz.Views.Client
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EdoLightsMatrixView : WidgetViewBase<EdoLightsMatrixViewModel>
	{
		private static readonly Color _greenColor = new Color(0, 255, 0);
		private static readonly Color _redColor = new Color(255, 0, 0);

		public EdoLightsMatrixView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			ytreeviewLightsMatrix.ColumnsConfig = FluentColumnsConfig<LightsMatrixRow>.Create()
			.AddColumn("").AddTextRenderer(x => x.Title)
			.AddColumn("Безнал.").AddTextRenderer(x => x.IsAllowed(EdoLightsMatrixPaymentType.Cashless) ? "V" : "X")
				.XAlign(0.5f)
				.WidthChars(7)
				.AddSetter((c, n) =>
				{
					c.BackgroundGdk = n.IsAllowed(EdoLightsMatrixPaymentType.Cashless) ? _greenColor : _redColor;
				})
			.AddColumn("Наличная,\nТерминал,\nQR-код,\nСайт").AddTextRenderer(x => x.IsAllowed(EdoLightsMatrixPaymentType.Receipt) ? "V" : "X")
				.XAlign(0.5f)
				.WidthChars(7)
				.AddSetter((c, n) =>
				{
					c.BackgroundGdk = n.IsAllowed(EdoLightsMatrixPaymentType.Receipt) ? _greenColor : _redColor;
				})
			.AddColumn("")
			.Finish();

			ytreeviewLightsMatrix.ItemsDataSource = ViewModel.ObservableLightsMatrixRows;
		}

	}
}
