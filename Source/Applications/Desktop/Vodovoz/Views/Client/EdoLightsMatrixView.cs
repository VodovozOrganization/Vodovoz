﻿using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Widgets.EdoLightsMatrix;
using LightsMatrixRow = Vodovoz.ViewModels.Widgets.EdoLightsMatrix.LightsMatrixRow;

namespace Vodovoz.Views.Client
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EdoLightsMatrixView : WidgetViewBase<EdoLightsMatrixViewModel>
	{
		private static readonly Color _greenColor = GdkColors.Green;
		private static readonly Color _redColor = GdkColors.Red;
		private static readonly Color _yellowColor = GdkColors.Yellow;
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
				case EdoLightsColorizeType.Allowed:
					return "V";
				case EdoLightsColorizeType.Forbidden:
					return "X";
				case EdoLightsColorizeType.Unknown:
					return "?";
			}

			return "";
		}
		private static Color GetColor(LightsMatrixRow row, EdoLightsMatrixPaymentType edoLightsMatrixPaymentType)
		{
			switch(row.GetColorizeType(edoLightsMatrixPaymentType))
			{
				case EdoLightsColorizeType.Allowed:
					return _greenColor;
				case EdoLightsColorizeType.Forbidden:
					return _redColor;
				case EdoLightsColorizeType.Unknown:
					return _yellowColor;
			}

			return _primaryBaseColor;
		}
	}
}
