using System;
using Gamma.GtkWidgets;
using Gamma.Widgets;
using Gtk;
using QSWidgetLib;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using QS.Extensions.Observable.Collections.List;
using QS.ViewModels.Control.EEVM;
using QS.Views.Control;
using Vodovoz.Domain.Contacts;
using Vodovoz.ViewModels.Dialogs.Counterparties;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using Vodovoz.ViewModels.ViewModels.Contacts;
using Vodovoz.ViewWidgets.Mango;
using VodovozBusiness.Domain.Contacts;

namespace Vodovoz.Dialogs.Phones
{
	[ToolboxItem(true)]
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
			ViewModel.PhonesListReplaced += ConfigureDlg;

			if(ViewModel.PhonesList == null)
			{
				return;
			}

			buttonAddPhone.BindCommand(ViewModel.AddItemCommand);
			buttonAddPhone.Binding
				.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.Sensitive)
				.InitializeFromSource();
			
			ViewModel.PhonesListChangedAction += ViewModelOnPhonesListChangedAction;
			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
			ViewModel.PhonesList.CollectionChanged += OnPhonesCollectionChanged;
			Redraw();
		}

		private void ViewModelOnPhonesListChangedAction(IObservableList<Phone> oldList)
		{
			if(oldList is null)
			{
				return;
			}
			
			oldList.CollectionChanged -= OnPhonesCollectionChanged;
		}

		private void OnPhonesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Redraw();
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.PhonesList))
			{
				Redraw();
			}
		}

		private void DrawNewRow(Phone newPhone)
		{
			var phoneViewModel = ViewModel.GetPhoneViewModel(newPhone);

			if(_hBoxList?.FirstOrDefault() == null)
			{
				_hBoxList = new List<HBox>();
			}

			var hasExternalUsersForThisPhone = false;

			if(newPhone.Id > 0)
			{
				hasExternalUsersForThisPhone = ViewModel.ExternalCounterpartyHandler.HasExternalCounterparties(ViewModel.UoW, newPhone);
			}

			var hBox = new HBox();

			var phoneDataCombo = new yListComboBox();
			phoneDataCombo.WidthRequest = 100;
			phoneDataCombo.SetRenderTextFunc((PhoneType x) => x.Name);
			phoneDataCombo.ItemsList = ViewModel.PhoneTypes;
			phoneDataCombo.Binding
				.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.Sensitive)
				.AddBinding(phoneViewModel, pvm => pvm.SelectedPhoneType, w => w.SelectedItem)
				.InitializeFromSource();
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

			if(hasExternalUsersForThisPhone)
			{
				phoneDataEntry.TooltipText =
					"По этому телефону привязан пользователь ИПЗ(МП или сайта). Для удаления или архивации обратитесь в отдель разработки";
			}

			phoneDataEntry.Binding
				.AddFuncBinding(ViewModel, e => !e.ReadOnly && !hasExternalUsersForThisPhone, w => w.IsEditable)
				.AddBinding(newPhone, e => e.Number, w => w.Text)
				.InitializeFromSource();

			hBox.Add(phoneDataEntry);
			hBox.SetChildPacking(phoneDataEntry, false, false, 0, PackType.Start);

			var handset = new HandsetView(newPhone.DigitsNumber);
			handset.Binding.AddFuncBinding(newPhone, e => !e.IsArchive, w => w.Sensitive).InitializeFromSource();
			hBox.Add(handset);
			hBox.SetChildPacking(handset, false, false, 0, PackType.Start);

			var textAdditionalLabel = new Label("доб.");
			hBox.Add(textAdditionalLabel);
			hBox.SetChildPacking(textAdditionalLabel, false, false, 0, PackType.Start);

			var additionalDataEntry = new yEntry();
			additionalDataEntry.WidthRequest = 40;
			additionalDataEntry.MaxLength = 10;
			
			additionalDataEntry.Binding
				.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.IsEditable)
				.AddBinding(newPhone, e => e.Additional, w => w.Text)
				.InitializeFromSource();
			
			hBox.Add(additionalDataEntry);
			hBox.SetChildPacking(additionalDataEntry, false, false, 0, PackType.Start);

			var labelComment = new Label("коммент.:");
			hBox.Add(labelComment);
			hBox.SetChildPacking(labelComment, false, false, 0, PackType.Start);
			
			var entryComment = new yEntry();
			entryComment.MaxLength = 150;
			
			entryComment.Binding
				.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.IsEditable)
				.AddBinding(newPhone, e => e.Comment, w => w.Text)
				.InitializeFromSource();

			hBox.Add(entryComment);
			hBox.SetChildPacking(entryComment, true, true, 0, PackType.Start);

			if(ViewModel.WithRoboatsWidgets)
			{
				CreateRoboatsClientNameEntry(newPhone, hBox);
				CreateRoboatsClientPatronymicEntry(newPhone, hBox);
			}

			var deleteButton = new yButton();
			var image = new Image();
			image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-delete", IconSize.Menu);
			deleteButton.Image = image;
			deleteButton.Clicked += OnRemovePhoneClicked;
			deleteButton.Binding
				.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.Sensitive)
				.InitializeFromSource();
			hBox.Add(deleteButton);
			hBox.SetChildPacking(deleteButton, false, false, 0, PackType.Start);

			hBox.Data.Add("phone", newPhone); //Для свзяки виджета и телефона
			hBox.ShowAll();

			vboxPhones.Add(hBox);
			vboxPhones.ShowAll();
			_hBoxList.Add(hBox);
		}

		private void CreateRoboatsClientNameEntry(Phone newPhone, HBox hBox)
		{
			var labelName = new Label("имя:");
			hBox.PackStart(labelName, false, false, 0);

			var entityEntryName = new EntityEntry();
			var builder =
				new LegacyEEVMBuilderFactory<Phone>(ViewModel.ParentTab, newPhone, ViewModel.UoW, ViewModel.NavigationManager, ViewModel.Scope);

			var viewModel = builder.ForProperty(x => x.RoboAtsCounterpartyName)
				.UseViewModelJournalAndAutocompleter<RoboAtsCounterpartyNameJournalViewModel>()
				.UseViewModelDialog<RoboAtsCounterpartyNameViewModel>()
				.Finish();

			viewModel.IsEditable = !ViewModel.ReadOnly && ViewModel.CanReadCounterpartyName;
			viewModel.CanViewEntity = ViewModel.CanEditCounterpartyName;
			entityEntryName.ViewModel = viewModel;
			
			entityEntryName.WidthRequest = 170;
			hBox.PackStart(entityEntryName, true, true, 0);
		}
		
		private void CreateRoboatsClientPatronymicEntry(Phone newPhone, HBox hBox)
		{
			var labelPatronymic = new Label("отч.:");
			hBox.PackStart(labelPatronymic, false, false, 0);

			var entityEntryPatronymic = new EntityEntry();
			var builder =
				new LegacyEEVMBuilderFactory<Phone>(ViewModel.ParentTab, newPhone, ViewModel.UoW, ViewModel.NavigationManager, ViewModel.Scope);

			var viewModel = builder.ForProperty(x => x.RoboAtsCounterpartyPatronymic)
				.UseViewModelJournalAndAutocompleter<RoboAtsCounterpartyPatronymicJournalViewModel>()
				.UseViewModelDialog<RoboAtsCounterpartyPatronymicViewModel>()
				.Finish();
			
			viewModel.IsEditable = !ViewModel.ReadOnly && ViewModel.CanReadCounterpartyPatronymic;
			viewModel.CanViewEntity = ViewModel.CanEditCounterpartyPatronymic;
			entityEntryPatronymic.ViewModel = viewModel;
			
			entityEntryPatronymic.WidthRequest = 170;
			hBox.PackStart(entityEntryPatronymic, true, true, 0);
		}
		
		private void OnRemovePhoneClicked(object sender, EventArgs e)
		{
			var deleteBtn =  sender as yButton;
			var hbox = deleteBtn.Parent as HBox;

			ViewModel.DeleteItemCommand.Execute(hbox.Data["phone"]);
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

		protected override void OnDestroyed()
		{
			ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
			ViewModel.PhonesList.CollectionChanged -= OnPhonesCollectionChanged;
			base.OnDestroyed();
		}
	}
}
