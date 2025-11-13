using System;
using Gamma.GtkWidgets;
using Gamma.Widgets;
using Gtk;
using QS.Widgets.GtkUI;
using QSWidgetLib;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QS.Extensions;
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
			get => _viewModel;
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
			if(ViewModel.PhonesList == null)
			{
				return;
			}

			buttonAddPhone.Clicked += OnAddPhoneClicked;
			buttonAddPhone.Binding
				.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.Sensitive)
				.InitializeFromSource();

			ViewModel.PhonesList.PropertyChanged += OnPhonesListPropertyChanged;
			Redraw();
		}

		private void OnAddPhoneClicked(object sender, EventArgs e)
		{
			ViewModel.AddItemCommand.Execute();
		}
		
		private void OnPhonesListPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Redraw();
		}

		private void DrawNewRow(Phone newPhone)
		{
			var phoneViewModel = ViewModel.GetPhoneViewModel(newPhone);

			if(_hBoxList?.FirstOrDefault() == null)
			{
				_hBoxList = new List<HBox>();
			}

			var hBox = new HBox();

			var phoneDataCombo = CreateWithConfigurePhoneTypeCombo(phoneViewModel);
			hBox.PackFromStart(phoneDataCombo, true, true);

			var textPhoneLabel = new Label("+7");
			hBox.PackFromStart(textPhoneLabel);

			var phoneDataEntry = CreateWithConfigurePhoneNumberEntry(phoneViewModel, newPhone);
			hBox.PackFromStart(phoneDataEntry);

			var handset = CreateWithConfigureHandset(newPhone);
			hBox.PackFromStart(handset);

			var textAdditionalLabel = new Label("доб.");
			hBox.PackFromStart(textAdditionalLabel);

			var additionalDataEntry = CreateWithConfigureAdditionalPhoneNumberEntry(newPhone);
			hBox.PackFromStart(additionalDataEntry);

			var labelComment = new Label("коммент.:");
			
			hBox.PackFromStart(labelComment);

			var entryComment = CreateWithConfigureCommentEntry(newPhone);
			hBox.PackFromStart(entryComment, true, true);

			if(ViewModel.RoboAtsCounterpartyNameSelectorFactory != null)
			{
				var labelName = new Label("имя:");
				hBox.PackFromStart(labelName);

				var entityviewmodelentryName = CreateWithConfigureRoboAtsCounterpartyNameEntry(newPhone);
				hBox.PackFromStart(entityviewmodelentryName, true, true);
			}

			if(ViewModel.RoboAtsCounterpartyPatronymicSelectorFactory != null)
			{
				var labelPatronymic = new Label("отч.:");
				hBox.PackFromStart(labelPatronymic);

				var entityviewmodelentryPatronymic = CreateWithConfigureRoboAtsCounterpartyPatronymicEntry(newPhone);
				hBox.PackFromStart(entityviewmodelentryPatronymic, true, true);
			}

			var deleteButton = CreateWithConfigureDeleteButton(hBox);
			hBox.PackFromStart(deleteButton);

			hBox.Data.Add("phone", newPhone); //Для свзяки виджета и телефона
			hBox.ShowAll();

			vboxPhones.Add(hBox);
			vboxPhones.ShowAll();
			_hBoxList.Add(hBox);
		}

		private Widget CreateWithConfigurePhoneTypeCombo(PhoneViewModel phoneViewModel)
		{
			var widget = new yListComboBox();
			widget.WidthRequest = 100;
			widget.SetRenderTextFunc((PhoneType x) => x.Name);
			widget.ItemsList = ViewModel.PhoneTypes;
			widget.Binding
				.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.Sensitive)
				.AddBinding(phoneViewModel, pvm => pvm.SelectedPhoneType, w => w.SelectedItem)
				.InitializeFromSource();

			return widget;
		}

		private Widget CreateWithConfigurePhoneNumberEntry(PhoneViewModel phoneViewModel, Phone newPhone)
		{
			var widget = new yValidatedEntry();
			widget.ValidationMode = ValidationType.phone;
			widget.Tag = newPhone;
			widget.WidthRequest = 110;
			widget.WidthChars = 19;
			widget.Binding
				.AddFuncBinding(phoneViewModel, pvm => pvm.IsPhoneNumberEditable, w => w.IsEditable)
				.AddBinding(newPhone, e => e.Number, w => w.Text)
				.InitializeFromSource();

			return widget;
		}
		
		private Widget CreateWithConfigureHandset(Phone newPhone)
		{
			var widget = new HandsetView(newPhone.DigitsNumber);
			widget.Binding
				.AddFuncBinding(newPhone, e => !e.IsArchive, w => w.Sensitive)
				.InitializeFromSource();

			return widget;
		}
		
		private Widget CreateWithConfigureAdditionalPhoneNumberEntry(Phone newPhone)
		{
			var widget = new yEntry();
			widget.WidthRequest = 40;
			widget.MaxLength = 10;
			widget.Binding
				.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.IsEditable)
				.AddBinding(newPhone, e => e.Additional, w => w.Text)
				.InitializeFromSource();

			return widget;
		}
		
		private Widget CreateWithConfigureCommentEntry(Phone newPhone)
		{
			var widget = new yEntry();
			widget.MaxLength = 150;
			widget.Binding
				.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.IsEditable)
				.AddBinding(newPhone, e => e.Comment, w => w.Text)
				.InitializeFromSource();

			return widget;
		}
		
		private Widget CreateWithConfigureRoboAtsCounterpartyNameEntry(Phone newPhone)
		{
			var widget = new EntityViewModelEntry();
			widget.Binding
				.AddFuncBinding(ViewModel, vm => !vm.ReadOnly && vm.CanReadCounterpartyName, w => w.IsEditable)
				.AddBinding(ViewModel, vm => vm.CanEditCounterpartyName, w => w.CanEditReference)
				.AddBinding(newPhone, e => e.RoboAtsCounterpartyName, w => w.Subject)
				.InitializeFromSource();
				
			widget.SetEntityAutocompleteSelectorFactory(ViewModel.RoboAtsCounterpartyNameSelectorFactory);
			widget.WidthRequest = 170;

			return widget;
		}
		
		private Widget CreateWithConfigureRoboAtsCounterpartyPatronymicEntry(Phone newPhone)
		{
			var widget = new EntityViewModelEntry();
			widget.Binding
				.AddFuncBinding(ViewModel, vm => !vm.ReadOnly && vm.CanReadCounterpartyPatronymic, w => w.IsEditable)
				.AddBinding(ViewModel, vm => vm.CanEditCounterpartyPatronymic, w => w.CanEditReference)
				.AddBinding(newPhone, e => e.RoboAtsCounterpartyPatronymic, w => w.Subject)
				.InitializeFromSource();
				
			widget.SetEntityAutocompleteSelectorFactory(ViewModel.RoboAtsCounterpartyPatronymicSelectorFactory);
			widget.WidthRequest = 170;

			return widget;
		}
		
		private Widget CreateWithConfigureDeleteButton(HBox hBox)
		{
			var widget = new yButton();
			var image = new Image();
			image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-delete", IconSize.Menu);
			widget.Image = image;
			widget.Clicked += (sender, e) => ViewModel.DeleteItemCommand.Execute(hBox.Data["phone"] as Phone);
			widget.Binding
				.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.Sensitive)
				.InitializeFromSource();

			return widget;
		}

		private void Redraw()
		{
			buttonAddPhone.Visible = !ViewModel.ReadOnly;

			foreach(var child in vboxPhones.Children.ToList())
			{
				child.Destroy();
				vboxPhones.Remove(child);
			}

			foreach(var phone in ViewModel.PhonesList)
			{
				DrawNewRow(phone);
			}
		}

		public override void Destroy()
		{
			ViewModel.PhonesList.PropertyChanged -= OnPhonesListPropertyChanged;
			base.Destroy();
		}
	}
}
