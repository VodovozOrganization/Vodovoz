using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Widgets.EdoLightsMatrix;
using LightsMatrixRow = Vodovoz.ViewModels.Widgets.EdoLightsMatrix.LightsMatrixRow;

namespace Vodovoz.Views.Client
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EdoLightsMatrixView : WidgetViewBase<EdoLightsMatrixViewModel>
	{
		private static readonly Color _greenColor = GdkColors.SuccessText;
		private static readonly Color _redColor = GdkColors.DangerText;
		private static readonly Color _yellowColor = GdkColors.WarningBase;
		private static readonly Color _primaryBaseColor = GdkColors.PrimaryBase;

		public EdoLightsMatrixView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			ytreeviewLightsMatrix.ColumnsConfig = FluentColumnsConfig<LightsMatrixRow>.Create()
			.AddColumn("Виды оплат").AddTextRenderer(x => x.Title)
			.AddColumn("Безнал.").AddTextRenderer(x => GetColotrizeString(x, EdoLightsMatrixPaymentType.Cashless))
				.XAlign(0.5f)
				.AddSetter((c, n) =>
				{
					c.BackgroundGdk = GetColor(n, EdoLightsMatrixPaymentType.Cashless);
				})
			.AddColumn("Наличная,\nТерминал,\nQR-код,\nСайт").AddTextRenderer(x => GetColotrizeString(x, EdoLightsMatrixPaymentType.Receipt))
				.XAlign(0.5f)
				.AddSetter((c, n) =>
				{
					c.BackgroundGdk = GetColor(n, EdoLightsMatrixPaymentType.Receipt);
				})
			.Finish();

			ytreeviewLightsMatrix.ItemsDataSource = ViewModel.ObservableLightsMatrixRows;

			ytreeviewLightsMatrix.Selection.Mode = SelectionMode.None;
		}

		private static string GetColotrizeString(LightsMatrixRow row, EdoLightsMatrixPaymentType edoLightsMatrixPaymentType)
		{
			switch(row.GetColorizeType(edoLightsMatrixPaymentType))
			{
				case PossibleAccessState.Allowed:
					return "V";
				case PossibleAccessState.Forbidden:
					return "X";
				case PossibleAccessState.Unknown:
					return "?";
			}

			return "";
		}
		private static Color GetColor(LightsMatrixRow row, EdoLightsMatrixPaymentType edoLightsMatrixPaymentType)
		{
			switch(row.GetColorizeType(edoLightsMatrixPaymentType))
			{
				case PossibleAccessState.Allowed:
					return _greenColor;
				case PossibleAccessState.Forbidden:
					return _redColor;
				case PossibleAccessState.Unknown:
					return _yellowColor;
			}

			return _primaryBaseColor;
		}
	}
}
