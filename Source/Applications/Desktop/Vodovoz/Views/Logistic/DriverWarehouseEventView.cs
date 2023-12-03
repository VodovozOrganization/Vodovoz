using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using Gamma.GtkWidgets;
using Gamma.Widgets;
using Gdk;
using QS.Helpers;
using QS.Navigation;
using QS.Print;
using QS.Views.GtkUI;
using QS.Widgets.GtkUI;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.ViewModels.Logistic;
using ZXing;
using ZXing.QrCode;

namespace Vodovoz.Views.Logistic
{
	public partial class DriverWarehouseEventView : TabViewBase<DriverWarehouseEventViewModel>
	{
		public DriverWarehouseEventView(DriverWarehouseEventViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnSave.Clicked += OnSaveClicked;
			btnCancel.Clicked += OnCancelClicked;
			btnPrintQrCode.Clicked += OnPrintQrCodeClicked;
			btnCopyFromClipboard.Clicked += OnCopyFromClipboard;
			
			btnSave.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			
			btnPrintQrCode.Binding
				.AddBinding(ViewModel, vm => vm.CanPrintQrCode, w => w.Sensitive)
				.InitializeFromSource();

			lblIdTitle.Binding
				.AddBinding(ViewModel, vm => vm.IdGtZero, w => w.Visible)
				.InitializeFromSource();

			lblId.Selectable = true;
			lblId.Binding
				.AddBinding(ViewModel, vm => vm.IdGtZero, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.Id, w => w.Text, new IntToStringConverter())
				.InitializeFromSource();
			
			chkIsArchive.Binding
				.AddBinding(ViewModel, vm => vm.CanEditByPermission, w => w.Sensitive)
				.InitializeFromSource();

			entryEvent.WidthRequest = 300;
			entryEvent.Binding
				.AddBinding(ViewModel.Entity, e => e.EventName, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.IsEditable)
				.InitializeFromSource();

			lblLatitude.Binding
				.AddBinding(ViewModel, vm => vm.IsCoordinatesVisible, w => w.Visible)
				.InitializeFromSource();
			
			spinBtnLatitude.Digits = 6;
			spinBtnLatitude.Binding
				.AddBinding(ViewModel.Entity, e => e.Latitude, w => w.ValueAsDecimal)
				.AddBinding(ViewModel, vm => vm.IsCoordinatesVisible, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			
			lblLongitude.Binding
				.AddBinding(ViewModel, vm => vm.IsCoordinatesVisible, w => w.Visible)
				.InitializeFromSource();
			
			spinBtnLongitude.Digits = 6;
			spinBtnLongitude.Binding
				.AddBinding(ViewModel.Entity, e => e.Longitude, w => w.ValueAsDecimal)
				.AddBinding(ViewModel, vm => vm.IsCoordinatesVisible, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			
			btnCopyFromClipboard.Binding
				.AddBinding(ViewModel, vm => vm.IsCoordinatesVisible, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			enumCmbType.ItemsEnum = typeof(DriverWarehouseEventType);
			enumCmbType.DefaultFirst = true;
			enumCmbType.Binding
				.AddBinding(ViewModel, vm => vm.EventType, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			
			lblDocumentType.Binding
				.AddBinding(ViewModel, vm => vm.IsDocumentQrParametersVisible, w => w.Visible)
				.InitializeFromSource();
			
			enumCmbDocumentType.ItemsEnum = typeof(EventQrDocumentType);
			enumCmbDocumentType.ShowSpecialStateNot = true;
			enumCmbDocumentType.Binding
				.AddBinding(ViewModel.Entity, vm => vm.DocumentType, w => w.SelectedItemOrNull)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.IsDocumentQrParametersVisible, w => w.Visible)
				.InitializeFromSource();
			
			lblQrPositionOnDocument.Binding
				.AddBinding(ViewModel, vm => vm.IsDocumentQrParametersVisible, w => w.Visible)
				.InitializeFromSource();
			
			enumCmbQrPositionOnDocument.ItemsEnum = typeof(EventQrPositionOnDocument);
			enumCmbQrPositionOnDocument.ShowSpecialStateNot = true;
			enumCmbQrPositionOnDocument.Binding
				.AddBinding(ViewModel.Entity, vm => vm.QrPositionOnDocument, w => w.SelectedItemOrNull)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.IsDocumentQrParametersVisible, w => w.Visible)
				.InitializeFromSource();
		}

		private void OnSaveClicked(object sender, EventArgs e)
		{
			ViewModel.SaveAndClose();
		}

		private void OnCancelClicked(object sender, EventArgs e)
		{
			ViewModel.Close(false, CloseSource.Cancel);
		}
		
		private void OnPrintQrCodeClicked(object sender, EventArgs e)
		{
			if(!ViewModel.IdGtZero)
			{
				if(!ViewModel.AskUserQuestion("Прежде чем продолжить, нужно сохранить событие. Сохраняем?"))
				{
					return;
				}

				if(!ViewModel.Save(false))
				{
					return;
				}
			}
			
			var array = CreateQrImage();
			PrintImage(array);
		}

		private byte[] CreateQrImage()
		{
			var qrEncode = new QRCodeWriter();
			var qrString = string.Join(
				",",
				new object[]
				{
					"EventQr",
					ViewModel.Entity.Id,
					ViewModel.Entity.DocumentType,
					ViewModel.Entity.Latitude,
					ViewModel.Entity.Longitude
				});

			var hints = new Dictionary<EncodeHintType, object> { { EncodeHintType.CHARACTER_SET, "utf-8" } };

			var qrMatrix = qrEncode.encode(
				qrString,
				BarcodeFormat.QR_CODE,
				300,
				300,
				hints);

			var qrWriter = new BarcodeWriter();
			var qrImage = qrWriter.Write(qrMatrix);
			qrImage.Save("EventQr.jpg", ImageFormat.Jpeg);

			var array = ImageHelper.LoadImageToJpgBytes("EventQr.jpg");
			return array;
		}

		private void PrintImage(byte[] imgArray)
		{
			if(imgArray != null && DocumentPrinters.ImagePrinter != null)
			{
				using(var pixBuf = new Pixbuf(imgArray))
				{
					var img = new PrintableImage {
						CopiesToPrint = 1,
						PixBuf = pixBuf
					};
					DocumentPrinters.ImagePrinter?.Print(new[] { img });
				}
			}
		}
		
		private void OnCopyFromClipboard(object sender, EventArgs e)
		{
			ViewModel.SetCoordinatesFromBufferCommand.Execute(GetClipboard(null).WaitForText());
		}
	}
}
