using Gamma.ColumnConfig;
using Gamma.Widgets;
using Gtk;
using QS.Journal.GtkUI;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using QSProjectsLib;
using System;
using System.Text;
using Vodovoz.Core.Domain.Complaints;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Complaints;
using VodovozBusiness.Domain.Complaints;

namespace Vodovoz.Views.Complaints
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ComplaintView : TabViewBase<ComplaintViewModel>
	{
		private Menu _popupCopyArrangementsMenu;
		private Menu _popupCopyCommentsMenu;
		private ComplaintStatuses _lastStatus;

		public ComplaintView(ComplaintViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			ylabelSubdivisions.Binding
				.AddBinding(ViewModel, vm => vm.SubdivisionsInWork, w => w.LabelProp)
				.InitializeFromSource();
			ylabelCreatedBy.Binding
				.AddBinding(ViewModel, e => e.CreatedByAndDate, w => w.LabelProp)
				.InitializeFromSource();
			ylabelChangedBy.Binding
				.AddBinding(ViewModel, e => e.ChangedByAndDate, w => w.LabelProp)
				.InitializeFromSource();

			yentryName.Binding
				.AddBinding(ViewModel.Entity, e => e.ComplainantName, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible)
				.InitializeFromSource();
			labelName.Binding
				.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible)
				.InitializeFromSource();
			
			yenumcomboStatus.ItemsEnum = typeof(ComplaintStatuses);
			if (!ViewModel.CanClose)
			{
				yenumcomboStatus.AddEnumToHideList(new object[] { ComplaintStatuses.Closed });
			}
			if(ViewModel.Entity.Status != ComplaintStatuses.NotTakenInProcess)
			{
				yenumcomboStatus.AddEnumToHideList(new object[] { ComplaintStatuses.NotTakenInProcess });
			}

			_lastStatus = ViewModel.Status;
			yenumcomboStatus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Status, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			yenumcomboStatus.EnumItemSelected += OnYenumcomboStatusChanged;

			entryDriver.ViewModel = ViewModel.ComplaintDriverEntryViewModel;

			ydatepickerPlannedCompletionDate.Binding
				.AddBinding(ViewModel.Entity, e => e.PlannedCompletionDate, w => w.Date)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			InitializeEntryViewModels();

			labelCounterparty.Binding
				.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible)
				.InitializeFromSource();

			spLstAddress.Binding
				.AddBinding(ViewModel, s => s.CanSelectDeliveryPoint, w => w.Sensitive)
				.AddBinding(ViewModel, s => s.IsClientComplaint, w => w.Visible)
				.AddBinding(ViewModel.Entity, t => t.DeliveryPoint, w => w.SelectedItem)
				.InitializeFromSource();

			lblAddress.Binding.AddBinding(ViewModel, s => s.IsClientComplaint, w => w.Visible).InitializeFromSource();

			labelOrder.Binding
				.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible)
				.InitializeFromSource();

			yentryPhone.Binding
				.AddBinding(ViewModel.Entity, e => e.Phone, w => w.Text)
				.InitializeFromSource();
			yhboxPhone.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible)
				.InitializeFromSource();
			labelNamePhone.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();

			cmbComplaintKind.SetRenderTextFunc<ComplaintKind>(k => k.GetFullName);
			cmbComplaintKind.Binding
				.AddBinding(ViewModel, vm => vm.ComplaintKindSource, w => w.ItemsList)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.ComplaintKind, w => w.SelectedItem)
				.InitializeFromSource();

			entryComplaintDetalization.ViewModel = ViewModel.ComplaintDetalizationEntryViewModel;
			entryComplaintDetalization.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeDetalization, w => w.Sensitive)
				.InitializeFromSource();

			yspeccomboboxComplaintObject.ShowSpecialStateAll = true;
			yspeccomboboxComplaintObject.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ComplaintObjectSource, w => w.ItemsList)
				.AddBinding(vm => vm.ComplaintObject, w => w.SelectedItem)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			comboboxComplaintSource.SetRenderTextFunc<ComplaintSource>(x => x.Name);
			comboboxComplaintSource.Binding
				.AddBinding(ViewModel, vm => vm.ComplaintSources, w => w.ItemsList)
				.AddBinding(ViewModel.Entity, e => e.ComplaintSource, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible)
				.InitializeFromSource();
			labelSource.Binding
				.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible)
				.InitializeFromSource();

			cmbComplaintResultOfCounterparty.SetRenderTextFunc<ComplaintResultOfCounterparty>(x => x.Name);
			cmbComplaintResultOfCounterparty.Binding
				.AddBinding(ViewModel, vm => vm.ComplaintResultsOfCounterparty, w => w.ItemsList)
				.AddBinding(ViewModel.Entity, e => e.ComplaintResultOfCounterparty, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			cmbComplaintResultOfEmployees.SetRenderTextFunc<ComplaintResultOfEmployees>(x => x.Name);
			cmbComplaintResultOfEmployees.Binding
				.AddBinding(ViewModel, vm => vm.ComplaintResultsOfEmployees, w => w.ItemsList)
				.AddBinding(ViewModel.Entity, e => e.ComplaintResultOfEmployees, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			smallfileinformationsview1.ViewModel = ViewModel.AttachedFileInformationsViewModel;

			ytextviewComplaintText.Binding
				.AddBinding(ViewModel.Entity, e => e.ComplaintText, w => w.Buffer.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			comboType.ItemsEnum = typeof(ComplaintType);
			comboType.Binding
				.AddBinding(ViewModel.Entity, e => e.ComplaintType, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			guiltyitemsview.ViewModel = ViewModel.GuiltyItemsViewModel;

			guiltyitemsview.Visible = ViewModel.CanAddGuilty;
			labelNameGuilties.Visible = ViewModel.CanAddGuilty;

			vboxDicussions.Add(new ComplaintDiscussionsView(ViewModel.DiscussionsViewModel));
			vboxDicussions.ShowAll();

			ytreeviewFines.ColumnsConfig = FluentColumnsConfig<FineItem>.Create()
				.AddColumn("№").AddTextRenderer(x => x.Fine.Id.ToString())
				.AddColumn("Сотрудник").AddTextRenderer(x => x.Employee.ShortName)
				.AddColumn("Сумма штрафа").AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.Money))
				.Finish();

			ytreeviewFines.Binding.AddBinding(ViewModel, vm => vm.FineItems, w => w.ItemsDataSource).InitializeFromSource();

			buttonAddFine.Clicked += (sender, e) => ViewModel.AddFineCommand.Execute();
			buttonAddFine.Binding.AddBinding(ViewModel, vm => vm.CanAddFine, w => w.Sensitive).InitializeFromSource();

			buttonAttachFine.Clicked += (sender, e) => ViewModel.AttachFineCommand.Execute();
			buttonAttachFine.Binding.AddBinding(ViewModel, vm => vm.CanAttachFine, w => w.Sensitive).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			buttonSave.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			buttonCancel.Clicked += (sender, e) => ViewModel.Close(ViewModel.CanEdit, QS.Navigation.CloseSource.Cancel);

			orderRatingEntry.Binding
				.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible)
				.InitializeFromSource();

			lblOrderRating.Binding
				.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible)
				.InitializeFromSource();
			
			orderRatingEntry.ViewModel = ViewModel.OrderRatingEntryViewModel;

			if(!string.IsNullOrWhiteSpace(ViewModel.Entity.Phone))
			{
				handsetPhone.SetPhone(ViewModel.Entity.Phone);
			}

			ViewModel.Entity.PropertyChanged += (o, e) =>
			{
				if(e.PropertyName == nameof(ViewModel.Entity.Phone))
				{
					handsetPhone.SetPhone(ViewModel.Entity.Phone);
				}
				if(e.PropertyName == nameof(ViewModel.Entity.Counterparty))
				{
					OnCounterpartyChanged();
				}
			};

			ytreeviewArrangement.Selection.Mode = SelectionMode.Multiple;
			ytreeviewArrangement.ColumnsConfig = FluentColumnsConfig<ComplaintArrangementComment>.Create()
				.AddColumn("Время")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.CreationTime.ToString("dd.MM.yyyy\nHH:mm"))
				.AddColumn("Автор")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Author.FullName)
				.AddColumn("Комментарий")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Comment)
						.WrapWidth(250)
						.WrapMode(Pango.WrapMode.WordChar)
				.RowCells().AddSetter<CellRenderer>(SetColor)
				.Finish();

			ytreeviewArrangement.ItemsDataSource = ViewModel.Entity.ObservableArrangementComments;

			ytreeviewArrangement.ButtonReleaseEvent += OnButtonArrangementsRelease;

			ytextviewNewArrangement.Binding
				.AddBinding(ViewModel, vm => vm.NewArrangementCommentText, w => w.Buffer.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonAddArrangement.Clicked += (sender, e) => ViewModel.AddArrangementCommentCommand.Execute();
			ybuttonAddArrangement.Binding
				.AddBinding(ViewModel, vm => vm.CanAddArrangementComment, w => w.Sensitive)
				.InitializeFromSource();

			ytreeviewResult.Selection.Mode = SelectionMode.Multiple;
			ytreeviewResult.ColumnsConfig = FluentColumnsConfig<ComplaintResultComment>.Create()
				.AddColumn("Время")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.CreationTime.ToString("dd.MM.yyyy\nHH:mm"))
				.AddColumn("Автор")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Author.FullName)
				.AddColumn("Комментарий")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Comment)
						.WrapWidth(250)
						.WrapMode(Pango.WrapMode.WordChar)
				.RowCells().AddSetter<CellRenderer>(SetColor)
				.Finish();

			ytreeviewResult.ItemsDataSource = ViewModel.Entity.ObservableResultComments;

			ytreeviewResult.ButtonReleaseEvent += OnButtonCommentsRelease;

			ytextviewNewResult.Binding
				.AddBinding(ViewModel, vm => vm.NewResultCommentText, w => w.Buffer.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonAddResult.Clicked += (sender, e) => ViewModel.AddResultCommentCommand.Execute();
			ybuttonAddResult.Binding
				.AddBinding(ViewModel, vm => vm.CanAddResultComment, w => w.Sensitive)
				.InitializeFromSource();

			yenumcomboboxWorkWithClientResult.ItemsEnum = typeof(ComplaintWorkWithClientResult);
			yenumcomboboxWorkWithClientResult.ShowSpecialStateNot = false;
			yenumcomboboxWorkWithClientResult.Binding
				.AddBinding(ViewModel.Entity, e => e.WorkWithClientResult, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			_popupCopyArrangementsMenu = new Menu();
			MenuItem copyArrangementsMenuEntry = new MenuItem("Копировать");
			copyArrangementsMenuEntry.ButtonPressEvent += CopyArrangementsMenuEntry_Activated;
			copyArrangementsMenuEntry.Visible = true;
			_popupCopyArrangementsMenu.Add(copyArrangementsMenuEntry);

			_popupCopyCommentsMenu = new Menu();
			MenuItem copyCommentsMenuEntry = new MenuItem("Копировать");
			copyCommentsMenuEntry.ButtonPressEvent += CopyCopyCommentsMenuEntry_Activated;
			copyCommentsMenuEntry.Visible = true;
			_popupCopyCommentsMenu.Add(copyCommentsMenuEntry);
		}

		private void InitializeEntryViewModels()
		{
			var builder = new LegacyEEVMBuilderFactory<Complaint>(
				Tab, ViewModel.Entity, ViewModel.UoW, ViewModel.NavigationManager, ViewModel.LifetimeScope);

			var counterpartyEntryViewModel =
				builder
					.ForProperty(x => x.Counterparty)
					.UseTdiEntityDialog()
					.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
					.Finish();

			counterpartyEntry.ViewModel = counterpartyEntryViewModel;
			counterpartyEntry.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible)
				.InitializeFromSource();
			OnCounterpartyChanged();

			var orderEntryViewModel =
				builder
					.ForProperty(x => x.Order)
					.UseTdiEntityDialog()
					.UseViewModelJournalAndAutocompleter<OrderJournalViewModel, OrderJournalFilterViewModel>(
						f => f.RestrictCounterparty = ViewModel.Entity.Counterparty
					)
					.Finish();
			orderEntryViewModel.BeforeChangeByUser += OnBeforeChangeOrderByUser;

			orderEntry.ViewModel = orderEntryViewModel;
			orderEntry.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible)
				.InitializeFromSource();
			orderEntry.ViewModel.ChangedByUser += OnOrderChangedByUser;
		}

		private void OnBeforeChangeOrderByUser(object sender, BeforeChangeEventArgs e)
		{
			var result = ViewModel.Entity.CanChangeOrder();

			if(!result.CanChange)
			{
				e.CanChange = false;
				ViewModel.ShowMessage(result.Message);
				return;
			}
			
			e.CanChange = true;
		}
		
		private void OnOrderChangedByUser(object sender, EventArgs e)
		{
			ViewModel.ChangeDeliveryPointCommand.Execute();
		}

		private void OnYenumcomboStatusChanged(object sender, ItemSelectedEventArgs e)
		{
			var newStatus = (ComplaintStatuses)e.SelectedItem;
			ViewModel.ChangeComplaintStatus(_lastStatus, newStatus);
			_lastStatus = newStatus;
		}

		private void CopyArrangementsMenuEntry_Activated(object sender, EventArgs e)
		{
			CopyArrangements();
		}

		private void CopyCopyCommentsMenuEntry_Activated(object sender, EventArgs e)
		{
			CopyComments();
		}

		private void CopyArrangements()
		{
			StringBuilder stringBuilder = new StringBuilder();

			foreach(ComplaintArrangementComment selected in ytreeviewArrangement.SelectedRows)
			{
				stringBuilder.AppendLine(selected.Comment);
			}

			GetClipboard(null).Text = stringBuilder.ToString();
		}

		private void CopyComments()
		{
			StringBuilder stringBuilder = new StringBuilder();

			foreach(ComplaintResultComment selected in ytreeviewResult.SelectedRows)
			{
				stringBuilder.AppendLine(selected.Comment);
			}

			GetClipboard(null).Text = stringBuilder.ToString();
		}

		private void OnButtonArrangementsRelease(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button != (uint)GtkMouseButton.Right)
			{
				return;
			}

			_popupCopyArrangementsMenu.Show();

			if(_popupCopyArrangementsMenu.Children.Length == 0)
			{
				return;
			}

			_popupCopyArrangementsMenu.Popup();
		}

		private void OnButtonCommentsRelease(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button != (uint)GtkMouseButton.Right)
			{
				return;
			}

			_popupCopyCommentsMenu.Show();

			if(_popupCopyCommentsMenu.Children.Length == 0)
			{
				return;
			}

			_popupCopyCommentsMenu.Popup();
		}

		void OnCounterpartyChanged()
		{
			if(ViewModel.Entity.Counterparty != null)
			{
				spLstAddress.NameForSpecialStateNot = "Самовывоз";
				spLstAddress.SetRenderTextFunc<DeliveryPoint>(d => $"{d.Id}: {d.ShortAddress}");
				spLstAddress.ItemsList = ViewModel.Entity.Counterparty.DeliveryPoints;

				return;
			}
			
			spLstAddress.NameForSpecialStateNot = null;
			spLstAddress.SelectedItem = SpecialComboState.Not;
			spLstAddress.ItemsList = null;
		}

		private void SetColor(CellRenderer cell, object node)
		{
			if(node is ComplaintArrangementComment || node is ComplaintResultComment)
			{
				cell.CellBackgroundGdk = GdkColors.DiscussionCommentBase;
			}
			else
			{
				cell.CellBackgroundGdk = GdkColors.PrimaryBase;
			}
		}

		public override void Destroy()
		{
			orderEntry.ViewModel.BeforeChangeByUser -= OnBeforeChangeOrderByUser;
			orderEntry.ViewModel.ChangedByUser -= OnOrderChangedByUser;
			base.Destroy();
		}
	}
}
