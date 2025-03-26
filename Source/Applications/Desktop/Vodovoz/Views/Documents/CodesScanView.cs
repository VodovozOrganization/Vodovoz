using System;
using System.Linq;
using Gamma.Binding;
using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using QS.Views.Dialog;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.ViewModels.Documents.SelfDeliveryCodesScan;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.Views.Documents
{
	[WindowSize(1280, 1024)]
	public partial class CodesScanView : DialogViewBase<CodesScanViewModel>
	{
		private readonly Color _colorGreen = GdkColors.SuccessBase;
		private readonly Color _colorLightRed = GdkColors.DangerBase;
		private readonly Color _colorBase = GdkColors.PrimaryBase;
		private readonly Color _colorAggregate = GdkColors.InsensitiveBase;
		private readonly Color _colorCurrencCodeInProcess = GdkColors.LightYellow2;

		public CodesScanView(CodesScanViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureView();
		}

		private void ConfigureView()
		{
			yentryCode.FocusOutEvent += OnYentryCodeOnFocusOutEvent;

			yentryCode.Activated += OnYentryCodeOnActivated;

			treeViewCodes.ColumnsConfig = FluentColumnsConfig<CodesScanViewModel.CodeScanRow>.Create()
				.AddColumn("№")
				.AddTextRenderer(n =>
					n.Parent == null && n.Children.Any()
						? $"{n.RowNumber} ╗   "
						: n.Parent != null
							? $"╠ {n.RowNumber}."
							: $"{n.RowNumber}.      ")
				.XAlign(1)
				.AddColumn("Код")
				.AddTextRenderer(n => n.RawCode)
				.AddSetter((c, n) =>
					c.CellBackgroundGdk = n.Children.Any() ? _colorAggregate : _colorBase)
				.AddColumn("Номенклатура")
				.AddTextRenderer(n => n.NomenclatureName)
				.WrapMode(WrapMode.Word).WrapWidth(400)
				.AddColumn("Наличие в заказе")
				.AddTextRenderer(n => n.HasInOrder.HasValue ? (n.HasInOrder.Value ? "Да" : "Нет") : "")
				.AddSetter((c, n) =>
					c.CellBackgroundGdk = n.Children.Any() ? _colorAggregate :
						n.HasInOrder.HasValue ? n.HasInOrder.Value ? _colorGreen : _colorLightRed : _colorBase)
				.AddColumn("Валиден в ЧЗ")
				.AddTextRenderer(n => n.IsTrueMarkValid.HasValue ? (n.IsTrueMarkValid.Value ? "Да" : "Нет") : "")
				.AddSetter((c, n) =>
					c.CellBackgroundGdk = n.Children.Any() ? _colorAggregate :
						n.IsTrueMarkValid.HasValue ? n.IsTrueMarkValid.Value ? _colorGreen : _colorLightRed : _colorBase)
				.AddSetter((c, n) =>
					c.CellBackgroundGdk = n.Children.Any() ? _colorAggregate :
						n.IsTrueMarkValid.HasValue ? n.IsTrueMarkValid.Value ? _colorGreen : _colorLightRed : _colorBase)
				.AddColumn("Доп.информация")
				.AddTextRenderer(n => n.AdditionalInformation)
				.AddSetter((c, n) =>
					c.CellBackgroundGdk = n.RawCode == ViewModel.CurrentCodeInProcess ? _colorCurrencCodeInProcess : _colorBase)
				.WrapMode(WrapMode.Word).WrapWidth(400)
				.AddColumn("")
				.Finish();

			treeViewCodes.YTreeModel =
				new RecursiveTreeModel<CodesScanViewModel.CodeScanRow>(ViewModel.CodeScanRows, x => x.Parent,
					x => x.Children);

			treeViewCodes.Binding.AddBinding(ViewModel, vm => vm.SelectedRow, w => w.SelectedRow).InitializeFromSource();

			ytreeviewProgress.ColumnsConfig = FluentColumnsConfig<CodesScanViewModel.CodesScanProgressRow>.Create()
				.AddColumn("GTIN")
				.AddTextRenderer(n => n.Gtin)
				.AddColumn("Номенклатура")
				.AddTextRenderer(n => n.NomenclatureName)
				.WrapMode(WrapMode.Word).WrapWidth(400)
				.AddColumn("Осталось отсканировать")
				.AddNumericRenderer(n => n.LeftToScan)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.LeftToScan == 0 ? _colorGreen : _colorBase)
				.AddColumn("В самовывозе")
				.AddNumericRenderer(n => n.InSelfDelivery)
				.AddColumn("")
				.Finish();

			ytreeviewProgress.ItemsDataSource = ViewModel.CodesScanProgressRows;

			ybuttonOk.BindCommand(ViewModel.CloseCommand);

			ybuttonDeleteCode.BindCommand(ViewModel.DeleteCodeCommand);

			ybuttonCopyCodes.Clicked += OnYbuttonCopyCodesClicked;

			ViewModel.RefreshScanningNomenclaturesAction = OnRefreshScanningNomenclatures;
		}

		private void OnYbuttonCopyCodesClicked(object sender, EventArgs e)
		{
			if(ViewModel.CodeScanRows.Any())
			{
				GetClipboard(Gdk.Selection.Clipboard).Text = ViewModel.GetCodesForClipboardCopy();
			}
		}

		private void OnYentryCodeOnActivated(object sender, EventArgs args)
		{
			ViewModel.CheckCode(yentryCode.Text);

			yentryCode.Text = string.Empty;
		}

		private void OnRefreshScanningNomenclatures()
		{
			treeViewCodes.YTreeModel?.EmitModelChanged();
			treeViewCodes.ExpandAll();
			ytreeviewProgress.YTreeModel?.EmitModelChanged();
		}

		private void OnYentryCodeOnFocusOutEvent(object o, FocusOutEventArgs args)
		{
			yentryCode.GrabFocus();
		}

		public override void Destroy()
		{
			yentryCode.FocusOutEvent -= OnYentryCodeOnFocusOutEvent;
			yentryCode.Activated -= OnYentryCodeOnActivated;
			ybuttonCopyCodes.Clicked -= OnYbuttonCopyCodesClicked;

			base.Destroy();
		}
	}
}
