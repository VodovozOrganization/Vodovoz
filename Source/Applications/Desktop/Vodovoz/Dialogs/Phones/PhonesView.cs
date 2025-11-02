using Gamma.GtkWidgets;
using Gamma.Widgets;
using Gtk;
using QS.Widgets.GtkUI;
using QSWidgetLib;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Contacts;
using Vodovoz.ViewModels.ViewModels.Contacts;
using Vodovoz.ViewWidgets.Mango;
using VodovozBusiness.Domain.Contacts;

namespace Vodovoz.Dialogs.Phones
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PhonesView : Bin
	{
		private PhonesViewModel _viewModel;

		private IList<HBox> _hBoxList;

		public PhonesViewModel ViewModel
		{
			get { return _viewModel; }
			set
			{
				_viewModel = value;
				ConfigureDlg();
			}
		}

		public PhonesView()
		{
			Build();
		}

		private void ConfigureDlg()
		{
			ViewModel.PhonesListReplaced += ConfigureDlg;

			if(ViewModel.PhonesList == null)
			{
				return;
			}

			buttonAddPhone.Clicked += (sender, e) => ViewModel.AddItemCommand.Execute();
			buttonAddPhone.Binding.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.Sensitive).InitializeFromSource();

			ViewModel.PhonesList.PropertyChanged += (sender, e) => Redraw();
			Redraw();
		}


		private void DrawNewRow(Phone newPhone)
		{
			PhoneViewModel phoneViewModel = ViewModel.GetPhoneViewModel(newPhone);

			if(_hBoxList?.FirstOrDefault() == null)
			{
				_hBoxList = new List<HBox>();
			}

			HBox hBox = new HBox();

			var phoneDataCombo = new yListComboBox();
			phoneDataCombo.WidthRequest = 100;
			phoneDataCombo.SetRenderTextFunc((PhoneType x) => x.Name);
			phoneDataCombo.ItemsList = ViewModel.PhoneTypes;
			phoneDataCombo.Binding.AddBinding(phoneViewModel, pvm => pvm.SelectedPhoneType, w => w.SelectedItem).InitializeFromSource();
			phoneDataCombo.Binding.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.Sensitive).InitializeFromSource();
			hBox.Add(phoneDataCombo);
			hBox.SetChildPacking(phoneDataCombo, true, true, 0, PackType.Start);

			Label textPhoneLabel = new Label("+7");
			hBox.Add(textPhoneLabel);
			hBox.SetChildPacking(textPhoneLabel, false, false, 0, PackType.Start);

			var phoneDataEntry = new yValidatedEntry();
			phoneDataEntry.ValidationMode = ValidationType.phone;
			phoneDataEntry.Tag = newPhone;
			phoneDataEntry.WidthRequest = 110;
			phoneDataEntry.WidthChars = 19;
			phoneDataEntry.Binding.AddBinding(newPhone, e => e.Number, w => w.Text).InitializeFromSource();
			phoneDataEntry.Binding.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.IsEditable).InitializeFromSource();
			hBox.Add(phoneDataEntry);
			hBox.SetChildPacking(phoneDataEntry, false, false, 0, PackType.Start);

			HandsetView handset = new HandsetView(newPhone.DigitsNumber);
			handset.Binding.AddFuncBinding(newPhone, e => !e.IsArchive, w => w.Sensitive).InitializeFromSource();
			hBox.Add(handset);
			hBox.SetChildPacking(handset, false, false, 0, PackType.Start);

			var textAdditionalLabel = new Label("доб.");
			hBox.Add(textAdditionalLabel);
			hBox.SetChildPacking(textAdditionalLabel, false, false, 0, PackType.Start);

			var additionalDataEntry = new yEntry();
			additionalDataEntry.WidthRequest = 40;
			additionalDataEntry.MaxLength = 10;
			additionalDataEntry.Binding.AddBinding(newPhone, e => e.Additional, w => w.Text).InitializeFromSource();
			additionalDataEntry.Binding.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.IsEditable).InitializeFromSource();
			hBox.Add(additionalDataEntry);
			hBox.SetChildPacking(additionalDataEntry, false, false, 0, PackType.Start);

			var labelComment = new Label("коммент.:");
			hBox.Add(labelComment);
			hBox.SetChildPacking(labelComment, false, false, 0, PackType.Start);

			var entryComment = new yEntry();
			entryComment.MaxLength = 150;
			entryComment.Binding.AddBinding(newPhone, e => e.Comment, w => w.Text).InitializeFromSource();
			entryComment.Binding.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.IsEditable).InitializeFromSource();
			hBox.Add(entryComment);
			hBox.SetChildPacking(entryComment, true, true, 0, PackType.Start);

			if(ViewModel.RoboAtsCounterpartyNameSelectorFactory != null)
			{
				var labelName = new Label("имя:");
				hBox.PackStart(labelName, false, false, 0);

				var entityviewmodelentryName = new EntityViewModelEntry();
				entityviewmodelentryName.CanDisposeEntitySelectorFactory = false;
				entityviewmodelentryName.Binding.AddBinding(ViewModel, vm => vm.CanEditCounterpartyName, w => w.CanEditReference)
					.InitializeFromSource();
				entityviewmodelentryName.Binding.AddBinding(newPhone, e => e.RoboAtsCounterpartyName, w => w.Subject)
					.InitializeFromSource();
				entityviewmodelentryName.Binding
					.AddFuncBinding(ViewModel, vm => !vm.ReadOnly && vm.CanReadCounterpartyName, w => w.IsEditable).InitializeFromSource();
				entityviewmodelentryName.SetEntityAutocompleteSelectorFactory(ViewModel.RoboAtsCounterpartyNameSelectorFactory);
				entityviewmodelentryName.WidthRequest = 170;
				hBox.PackStart(entityviewmodelentryName, true, true, 0);
			}

			if(ViewModel.RoboAtsCounterpartyPatronymicSelectorFactory != null)
			{

				var labelPatronymic = new Label("отч.:");
				hBox.PackStart(labelPatronymic, false, false, 0);

				var entityviewmodelentryPatronymic = new EntityViewModelEntry();
				entityviewmodelentryPatronymic.CanDisposeEntitySelectorFactory = false;
				entityviewmodelentryPatronymic.Binding
					.AddBinding(ViewModel, vm => vm.CanEditCounterpartyPatronymic, w => w.CanEditReference).InitializeFromSource();
				entityviewmodelentryPatronymic.Binding.AddBinding(newPhone, e => e.RoboAtsCounterpartyPatronymic, w => w.Subject)
					.InitializeFromSource();
				entityviewmodelentryPatronymic.Binding
					.AddFuncBinding(ViewModel, vm => !vm.ReadOnly && vm.CanReadCounterpartyPatronymic, w => w.IsEditable)
					.InitializeFromSource();
				entityviewmodelentryPatronymic.SetEntityAutocompleteSelectorFactory(ViewModel.RoboAtsCounterpartyPatronymicSelectorFactory);
				entityviewmodelentryPatronymic.WidthRequest = 170;
				hBox.PackStart(entityviewmodelentryPatronymic, true, true, 0);
			}

			yButton deleteButton = new yButton();
			Image image = new Image();
			image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-delete", IconSize.Menu);
			deleteButton.Image = image;
			deleteButton.Clicked += (sender, e) => ViewModel.DeleteItemCommand.Execute(hBox.Data["phone"] as Phone);
			deleteButton.Binding.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.Sensitive).InitializeFromSource();
			hBox.Add(deleteButton);
			hBox.SetChildPacking(deleteButton, false, false, 0, PackType.Start);

			hBox.Data.Add("phone", newPhone); //Для свзяки виджета и телефона
			hBox.ShowAll();

			vboxPhones.Add(hBox);
			vboxPhones.ShowAll();
			_hBoxList.Add(hBox);
		}

		private void Redraw()
		{
			buttonAddPhone.Visible = !ViewModel.ReadOnly;

			foreach(var child in vboxPhones.Children.ToList())
			{
				child.Destroy();
				vboxPhones.Remove(child);
			}

			foreach(Phone phone in ViewModel.PhonesList)
			{
				DrawNewRow(phone);
			}
		}
	}
}
