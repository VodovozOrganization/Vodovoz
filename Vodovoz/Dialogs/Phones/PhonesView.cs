using System.Collections.Generic;
using Gamma.Widgets;
using Gtk;
using System.Linq;
using Gamma.GtkWidgets;
using QSWidgetLib;
using Vodovoz.Domain.Contacts;
using Vodovoz.ViewModels.ViewModels;
using Vodovoz.ViewWidgets.Mango;
using Vodovoz.ViewModels.ViewModels.Contacts;

namespace Vodovoz.Dialogs.Phones
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PhonesView : Gtk.Bin
	{
		private PhonesViewModel viewModel;

		private IList<HBox> hBoxList;

		public PhonesViewModel ViewModel {
			get { return viewModel; }
			set {
				viewModel = value;
				ConfigureDlg();
			}
		}

		public PhonesView()
		{
			this.Build();
		}

		private void ConfigureDlg()
		{
			viewModel.PhonesListReplaced += ConfigureDlg;

			if(viewModel.PhonesList == null)
				return;

			buttonAddPhone.Clicked += (sender, e) => viewModel.AddItemCommand.Execute();
			buttonAddPhone.Binding.AddFuncBinding(viewModel, e => !e.ReadOnly, w => w.Sensitive).InitializeFromSource();

			viewModel.PhonesList.PropertyChanged += (sender, e) => Redraw();
			Redraw();
		}


		private void DrawNewRow(Phone newPhone)
		{
			if(hBoxList?.FirstOrDefault() == null) 
				hBoxList = new List<HBox>();

			HBox hBox = new HBox();

			var phoneDataCombo = new yListComboBox();
			phoneDataCombo.WidthRequest = 100;
			phoneDataCombo.SetRenderTextFunc((PhoneType x) => x.Name);
			phoneDataCombo.ItemsList = viewModel.PhoneTypes;
			phoneDataCombo.Binding.AddBinding(newPhone, e => e.PhoneType, w => w.SelectedItem).InitializeFromSource();
			phoneDataCombo.Binding.AddFuncBinding(viewModel, e => !e.ReadOnly, w => w.Sensitive).InitializeFromSource();
			hBox.Add(phoneDataCombo);
			hBox.SetChildPacking(phoneDataCombo, true, true, 0, PackType.Start);

			Label textPhoneLabel = new Label("+7");
			hBox.Add(textPhoneLabel);
			hBox.SetChildPacking(textPhoneLabel, false, false, 0, PackType.Start);

			var phoneDataEntry = new yValidatedEntry();
			phoneDataEntry.ValidationMode = ValidationType.phone;
			phoneDataEntry.Tag = newPhone;
			phoneDataEntry.WidthRequest = 125;
			phoneDataEntry.WidthChars = 19;
			phoneDataEntry.Binding.AddBinding(newPhone, e => e.Number, w => w.Text).InitializeFromSource();
			phoneDataEntry.Binding.AddFuncBinding(viewModel, e => !e.ReadOnly, w => w.IsEditable).InitializeFromSource();
			hBox.Add(phoneDataEntry);
			hBox.SetChildPacking(phoneDataEntry, false, false, 0, PackType.Start);

			HandsetView handset = new HandsetView(newPhone.DigitsNumber);
			hBox.Add(handset);
			hBox.SetChildPacking(handset, false, false, 0, PackType.Start);

			var textAdditionalLabel = new Gtk.Label("доб.");
			hBox.Add(textAdditionalLabel);
			hBox.SetChildPacking(textAdditionalLabel, false, false, 0, PackType.Start);

			var additionalDataEntry = new yEntry();
			additionalDataEntry.WidthRequest = 50;
			additionalDataEntry.MaxLength = 10;
			additionalDataEntry.Binding.AddBinding(newPhone, e => e.Additional, w => w.Text).InitializeFromSource();
			additionalDataEntry.Binding.AddFuncBinding(viewModel, e => !e.ReadOnly, w => w.IsEditable).InitializeFromSource();
			hBox.Add(additionalDataEntry);
			hBox.SetChildPacking(additionalDataEntry, false, false, 0, PackType.Start);

			var labelName = new Label("имя:");
			hBox.Add(labelName);
			hBox.SetChildPacking(labelName, false, false, 0, PackType.Start);

			var entryName = new yEntry();
			entryName.MaxLength = 150;
			entryName.Binding.AddBinding(newPhone, e => e.Name, w => w.Text).InitializeFromSource();
			entryName.Binding.AddFuncBinding(viewModel, e => !e.ReadOnly, w => w.IsEditable).InitializeFromSource();
			hBox.Add(entryName);
			hBox.SetChildPacking(entryName, true, true, 0, PackType.Start);

			yButton deleteButton = new yButton();
			Image image = new Image();
			image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-delete", IconSize.Menu);
			deleteButton.Image = image;
			deleteButton.Clicked += (sender, e) => viewModel.DeleteItemCommand.Execute(hBox.Data["phone"] as Phone);
			deleteButton.Binding.AddFuncBinding(viewModel, e => !e.ReadOnly, w => w.Sensitive).InitializeFromSource();
			hBox.Add(deleteButton);
			hBox.SetChildPacking(deleteButton, false, false, 0, PackType.Start);

			hBox.Data.Add("phone", newPhone); //Для свзяки виджета и телефона
			hBox.ShowAll();

			vboxPhones.Add(hBox);
			vboxPhones.ShowAll();
			hBoxList.Add(hBox);

		}

		private void Redraw()
		{
			buttonAddPhone.Visible = !ViewModel.ReadOnly;

			foreach(var child in vboxPhones.Children) 
			{
				vboxPhones.Remove(child);
			}

			foreach(Phone phone in viewModel.PhonesList) 
			{
				DrawNewRow(phone);
			}
		}

		private void DeleteRow(int rowNumber)
		{
			hBoxList.RemoveAt(rowNumber);
		}

		private void DeleteRow(HBox widget)
		{
			hBoxList.Remove(widget);
		}
	}
}
