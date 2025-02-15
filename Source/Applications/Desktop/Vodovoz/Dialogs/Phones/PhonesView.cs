using Gamma.GtkWidgets;
using Gamma.Widgets;
using Gtk;
using QS.Extensions;
using Vodovoz.Domain.Contacts;
using Vodovoz.ViewModels.ViewModels.Contacts;
using Vodovoz.ViewWidgets.Mango;
using System;
using System.Linq.Expressions;
using QS.ViewModels.Control.EEVM;
using QS.Views.Control;
using QSWidgetLib;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Dialogs.Counterparties;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;

namespace Vodovoz.Dialogs.Phones
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PhonesView : Bin
	{
		private PhonesViewModel _viewModel;

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

			for(var i = 0; i < ViewModel.PhonesList.Count; i++)
			{
				DrawNewRow(ViewModel.PhonesList[i], i);
			}

			ViewModel.AddedPhoneAction += AddPhone;
			ViewModel.DeletedPhoneAction += RemovePhone;
		}

		private void DrawNewRow(Phone newPhone, int index)
		{
			var hBox = new HBox();
			var phoneViewModel = ViewModel.PhoneViewModels[index];

			var phoneDataCombo = CreateWithConfigurePhoneTypeCombo(phoneViewModel);
			hBox.PackFromStart(phoneDataCombo, true, true);

			var textPhoneLabel = new Label("+7");
			hBox.PackFromStart(textPhoneLabel);

			var phoneDataEntry = CreateWithConfigurePhoneNumberEntry(newPhone);
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

			if(phoneViewModel.ParentTab != null || phoneViewModel.RoboatsCounterpartyNameViewModel != null)
			{
				var labelName = new Label("имя:");
				hBox.PackFromStart(labelName);

				var entityviewmodelentryName = CreateWithConfigureRoboAtsCounterpartyNameEntry(phoneViewModel);
				hBox.PackFromStart(entityviewmodelentryName, true, true);
			}

			if(phoneViewModel.ParentTab != null || phoneViewModel.RoboatsCounterpartyPatronymicViewModel != null)
			{
				var labelPatronymic = new Label("отч.:");
				hBox.PackFromStart(labelPatronymic);

				var entityviewmodelentryPatronymic = CreateWithConfigureRoboAtsCounterpartyPatronymicEntry(phoneViewModel);
				hBox.PackFromStart(entityviewmodelentryPatronymic, true, true);
			}

			var deleteButton = CreateWithConfigureDeleteButton();
			hBox.PackFromStart(deleteButton);

			hBox.ShowAll();

			vboxPhones.Add(hBox);
			vboxPhones.ShowAll();
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

		private Widget CreateWithConfigurePhoneNumberEntry(Phone newPhone)
		{
			var widget = new yValidatedEntry();
			widget.ValidationMode = ValidationType.phone;
			widget.Tag = newPhone;
			widget.WidthRequest = 110;
			widget.WidthChars = 19;
			widget.Binding
				.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.IsEditable)
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

		#region Roboats widgets

		private Widget CreateWithConfigureRoboAtsCounterpartyNameEntry(PhoneViewModel phoneViewModel)
		{
			var widget = new EntityEntry();
			IEntityEntryViewModel viewModel = null;
			
			if(phoneViewModel.ParentTab != null)
			{
				var builder = GetLegacyEevmBuilderFactory(phoneViewModel);

				var legacyViewModel = builder.ForProperty(x => x.RoboAtsCounterpartyName)
					.UseViewModelJournalAndAutocompleter<RoboAtsCounterpartyNameJournalViewModel>()
					.UseViewModelDialog<RoboAtsCounterpartyNameViewModel>()
					.Finish();
				
				legacyViewModel.CanViewEntity = ViewModel.CanEditCounterpartyName;
				viewModel = legacyViewModel;
			}
			else
			{
				viewModel = phoneViewModel.RoboatsCounterpartyNameViewModel;
				(viewModel as EntityEntryViewModel<RoboAtsCounterpartyName>).CanViewEntity = ViewModel.CanEditCounterpartyName;
			}
			
			ConfigureRoboatsEntityEntry(widget, viewModel, vm => !vm.ReadOnly && vm.CanReadCounterpartyName);

			return widget;
		}

		private Widget CreateWithConfigureRoboAtsCounterpartyPatronymicEntry(PhoneViewModel phoneViewModel)
		{
			var widget = new EntityEntry();
			IEntityEntryViewModel viewModel = null;
			
			if(phoneViewModel.ParentTab != null)
			{
				var builder = GetLegacyEevmBuilderFactory(phoneViewModel);

				var legacyViewModel = builder.ForProperty(x => x.RoboAtsCounterpartyPatronymic)
					.UseViewModelJournalAndAutocompleter<RoboAtsCounterpartyPatronymicJournalViewModel>()
					.UseViewModelDialog<RoboAtsCounterpartyPatronymicViewModel>()
					.Finish();
				
				legacyViewModel.CanViewEntity = ViewModel.CanEditCounterpartyPatronymic;
				viewModel = legacyViewModel;
			}
			else
			{
				viewModel = phoneViewModel.RoboatsCounterpartyPatronymicViewModel;
				(viewModel as EntityEntryViewModel<RoboAtsCounterpartyPatronymic>).CanViewEntity = ViewModel.CanEditCounterpartyPatronymic;
			}

			ConfigureRoboatsEntityEntry(widget, viewModel, vm => !vm.ReadOnly && vm.CanReadCounterpartyPatronymic);

			return widget;
		}

		private static LegacyEEVMBuilderFactory<Phone> GetLegacyEevmBuilderFactory(PhoneViewModel phoneViewModel)
		{
			var builder = new LegacyEEVMBuilderFactory<Phone>(
				phoneViewModel.ParentTab,
				phoneViewModel.Phone,
				phoneViewModel.UoW,
				phoneViewModel.NavigationManager,
				phoneViewModel.LifetimeScope);
			return builder;
		}
		
		private void ConfigureRoboatsEntityEntry(
			EntityEntry widget,
			IEntityEntryViewModel viewModel,
			Expression<Func<PhonesViewModel, object>> predicate)
		{
			widget.ViewModel = viewModel;
			widget.WidthRequest = 170;
			
			widget.Binding
				.AddFuncBinding(ViewModel, predicate, w => w.ViewModel.IsEditable)
				.InitializeFromSource();
		}

		#endregion

		private Widget CreateWithConfigureDeleteButton()
		{
			var widget = new yButton();
			var image = new Image();
			image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-delete", IconSize.Menu);
			widget.Image = image;
			widget.Clicked += OnDeletePhoneClicked;
			widget.Binding
				.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.Sensitive)
				.InitializeFromSource();

			return widget;
		}

		private void OnAddPhoneClicked(object sender, EventArgs e)
		{
			ViewModel.AddItemCommand.Execute();
		}

		private void OnDeletePhoneClicked(object sender, EventArgs e)
		{
			var hbox = (sender as Widget).Parent;
			var position = ((Box.BoxChild)vboxPhones[hbox]).Position;
			ViewModel.DeleteItemCommand.Execute(position);
		}

		private void AddPhone(Phone phone, int phoneIndex)
		{
			DrawNewRow(phone, phoneIndex);
		}

		private void RemovePhone(int index)
		{
			var widget = vboxPhones.Children[index];
			vboxPhones.Remove(widget);
		}

		public override void Destroy()
		{
			ViewModel.AddedPhoneAction -= AddPhone;
			ViewModel.DeletedPhoneAction -= RemovePhone;
			
			base.Destroy();
		}
	}
}
