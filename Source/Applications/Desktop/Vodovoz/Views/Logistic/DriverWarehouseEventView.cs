using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Gdk;
using QS.Helpers;
using QS.Navigation;
using QS.Print;
using QS.Views.GtkUI;
using QS.Widgets.GtkUI;
using Vodovoz.Core.Domain.Logistics.Drivers;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.ViewModels.Logistic;
using ZXing;
using ZXing.QrCode;
using Color = System.Drawing.Color;

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
				.AddBinding(ViewModel, vm => vm.CanEditByPermission, w => w.Sensitive)
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
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.InitializeFromSource();

			entryEvent.WidthRequest = 300;
			entryEvent.Binding
				.AddBinding(ViewModel.Entity, e => e.EventName, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.IsEditable)
				.InitializeFromSource();

			entryUriForQr.WidthRequest = 300;
			entryUriForQr.Binding
				.AddBinding(ViewModel.Entity, e => e.UriForQr, w => w.Text)
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
			enumCmbDocumentType.Changed += OnDocumentTypeChanged;
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

		private void OnDocumentTypeChanged(object sender, EventArgs e)
		{
			enumCmbQrPositionOnDocument.ClearEnumHideList();

			if(ViewModel.Entity.DocumentType == EventQrDocumentType.CarLoadDocument)
			{
				enumCmbQrPositionOnDocument.AddEnumToHideList(EventQrPositionOnDocument.Bottom, EventQrPositionOnDocument.Top);
			}
			else
			{
				enumCmbQrPositionOnDocument.AddEnumToHideList(EventQrPositionOnDocument.Left, EventQrPositionOnDocument.Right);
			}
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
			if(ViewModel.HasChanges)
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

			var hints = new Dictionary<EncodeHintType, object> { { EncodeHintType.CHARACTER_SET, "utf-8" } };

			const int qrWidth = 1500;
			const int qrHeight = 1500;
			
			var qrMatrix = qrEncode.encode(
				ViewModel.Entity.GenerateQrData(),
				BarcodeFormat.QR_CODE,
				qrWidth,
				qrHeight,
				hints);

			var qrWriter = new BarcodeWriter();
			var qrImage = qrWriter.Write(qrMatrix);
			var path = AddTextToQr(qrImage);
			var array = ImageHelper.LoadImageToJpgBytes(path);
			
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

		private string AddTextToQr(Bitmap qrImage)
		{
			var text = ViewModel.Entity.EventName;
			const float leftTextPadding = 75f;
			const int heightAreaForText = 250;
			var textArea = new RectangleF(leftTextPadding, qrImage.Height, qrImage.Width - leftTextPadding, heightAreaForText);
			var imageFilePath = System.IO.Path.GetTempFileName();
			
			using(qrImage)
			{
				using(var qrWithText = new Bitmap(qrImage.Width, qrImage.Height + heightAreaForText))
				using(var canvas = Graphics.FromImage(qrWithText))
				{
					using(var arialFont = new System.Drawing.Font("Arial", 50, FontStyle.Bold))
					{
						canvas.Clear(Color.White);
						canvas.DrawImage(qrImage, new PointF(0f, 0f));
						canvas.DrawString(text, arialFont, Brushes.Black, textArea);
						canvas.Save();
					}
					
					qrWithText.Save(imageFilePath, ImageFormat.Jpeg);
				}
			}
			
			return imageFilePath;
		}
		
		private void OnCopyFromClipboard(object sender, EventArgs e)
		{
			ViewModel.SetCoordinatesFromBufferCommand.Execute(GetClipboard(null).WaitForText());
		}
	}
}
