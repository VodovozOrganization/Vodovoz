using Gamma.ColumnConfig;
using Gamma.Widgets;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;
using Gtk;

namespace Vodovoz.Views.Complaints
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ComplaintView : TabViewBase<ComplaintViewModel>
	{
		public ComplaintView(ComplaintViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			ylabelSubdivisions.Binding.AddBinding(ViewModel, vm => vm.SubdivisionsInWork, w => w.LabelProp).InitializeFromSource();
			ylabelCreatedBy.Binding.AddBinding(ViewModel, e => e.CreatedByAndDate, w => w.LabelProp).InitializeFromSource();
			ylabelChangedBy.Binding.AddBinding(ViewModel, e => e.ChangedByAndDate, w => w.LabelProp).InitializeFromSource();

			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.ComplainantName, w => w.Text).InitializeFromSource();
			yentryName.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			yentryName.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();
			labelName.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();

			yenumcomboStatus.ItemsEnum = typeof(ComplaintStatuses);
			if (!ViewModel.CanClose)
			{
				yenumcomboStatus.AddEnumToHideList(new object[] { ComplaintStatuses.Closed });
			}

			yenumcomboStatus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Status, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			
			yenumcomboStatus.EnumItemSelected += (sender, args) => ViewModel.CloseComplaint((ComplaintStatuses)args.SelectedItem);

			ydatepickerPlannedCompletionDate.Binding.AddBinding(ViewModel.Entity, e => e.PlannedCompletionDate, w => w.Date).InitializeFromSource();
			ydatepickerPlannedCompletionDate.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			entryCounterparty.Changed += EntryCounterparty_Changed;
			entryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartyJournalFactory.CreateCounterpartyAutocompleteSelectorFactory());
			entryCounterparty.Binding.AddBinding(ViewModel.Entity, e => e.Counterparty, w => w.Subject).InitializeFromSource();
			entryCounterparty.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			entryCounterparty.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();
			labelCounterparty.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();

			spLstAddress.Binding.AddBinding(ViewModel, s => s.CanSelectDeliveryPoint, w => w.Sensitive).InitializeFromSource();
			spLstAddress.Binding.AddBinding(ViewModel, s => s.IsClientComplaint, w => w.Visible).InitializeFromSource();
			lblAddress.Binding.AddBinding(ViewModel, s => s.IsClientComplaint, w => w.Visible).InitializeFromSource();

			entryOrder.SetEntityAutocompleteSelectorFactory(ViewModel.OrderAutocompleteSelectorFactory);
			entryOrder.Binding.AddBinding(ViewModel.Entity, e => e.Order, w => w.Subject).InitializeFromSource();
			entryOrder.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			entryOrder.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();
			entryOrder.ChangedByUser += (sender, e) => ViewModel.ChangeDeliveryPointCommand.Execute();
			labelOrder.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();

			yentryPhone.Binding.AddBinding(ViewModel.Entity, e => e.Phone, w => w.Text).InitializeFromSource();
			yhboxPhone.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			yhboxPhone.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();
			labelNamePhone.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();

			arrangementTextView.Binding
				.AddBinding(ViewModel.Entity, e => e.Arrangement, w => w.Buffer.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

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
			comboboxComplaintSource.Binding.AddBinding(ViewModel, vm => vm.ComplaintSources, w => w.ItemsList).InitializeFromSource();
			comboboxComplaintSource.Binding.AddBinding(ViewModel.Entity, e => e.ComplaintSource, w => w.SelectedItem).InitializeFromSource();
			comboboxComplaintSource.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			comboboxComplaintSource.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();
			labelSource.Binding.AddBinding(ViewModel, vm => vm.IsClientComplaint, w => w.Visible).InitializeFromSource();

			cmbComplaintResultOfCounterparty.SetRenderTextFunc<ComplaintResultOfCounterparty>(x => x.Name);
			cmbComplaintResultOfCounterparty.Binding.AddBinding(ViewModel, vm => vm.ComplaintResultsOfCounterparty, w => w.ItemsList).InitializeFromSource();
			cmbComplaintResultOfCounterparty.Binding.AddBinding(ViewModel.Entity, e => e.ComplaintResultOfCounterparty, w => w.SelectedItem).InitializeFromSource();
			cmbComplaintResultOfCounterparty.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			cmbComplaintResultOfEmployees.SetRenderTextFunc<ComplaintResultOfEmployees>(x => x.Name);
			cmbComplaintResultOfEmployees.Binding.AddBinding(ViewModel, vm => vm.ComplaintResultsOfEmployees, w => w.ItemsList).InitializeFromSource();
			cmbComplaintResultOfEmployees.Binding.AddBinding(ViewModel.Entity, e => e.ComplaintResultOfEmployees, w => w.SelectedItem).InitializeFromSource();
			cmbComplaintResultOfEmployees.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ytextviewResultText.Binding.AddBinding(ViewModel.Entity, e => e.ResultText, w => w.Buffer.Text).InitializeFromSource();
			ytextviewResultText.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			complaintfilesview.ViewModel = ViewModel.FilesViewModel;

			ytextviewComplaintText.Binding.AddBinding(ViewModel.Entity, e => e.ComplaintText, w => w.Buffer.Text).InitializeFromSource();
			ytextviewComplaintText.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

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

			buttonAddFine.Clicked += (sender, e) => { ViewModel.AddFineCommand.Execute(this.Tab); };
			buttonAddFine.Binding.AddBinding(ViewModel, vm => vm.CanAddFine, w => w.Sensitive).InitializeFromSource();

			buttonAttachFine.Clicked += (sender, e) => { ViewModel.AttachFineCommand.Execute(); };
			buttonAttachFine.Binding.AddBinding(ViewModel, vm => vm.CanAttachFine, w => w.Sensitive).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonSave.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(ViewModel.CanEdit, QS.Navigation.CloseSource.Cancel); };

			ViewModel.FilesViewModel.ReadOnly = !ViewModel.CanEdit;

			ViewModel.Entity.PropertyChanged += (o, e) =>
			{
				if(e.PropertyName == nameof(ViewModel.Entity.Phone))
				{
					handsetPhone.SetPhone(ViewModel.Entity.Phone);
				}
			};

			ytextviewNewArrangement.Binding.AddBinding(ViewModel, vm => vm.NewArrangementCommentText, w => w.Buffer.Text).InitializeFromSource();
			ytextviewNewArrangement.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ybuttonAddArrangement.Clicked += (sender, e) => ViewModel.AddArrangementCommentCommand.Execute();
			ybuttonAddArrangement.Binding.AddBinding(ViewModel, vm => vm.CanAddArrangementComment, w => w.Sensitive).InitializeFromSource();

			ytreeviewResult.ShowExpanders = false;
			ytreeviewResult.ColumnsConfig = FluentColumnsConfig<object>.Create()
				.AddColumn("Время")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => GetTime(x))
				.AddColumn("Автор")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => GetAuthor(x))
				.AddColumn("Комментарий")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => GetNodeName(x))
						.WrapWidth(300)
						.WrapMode(Pango.WrapMode.WordChar)
				.RowCells().AddSetter<CellRenderer>(SetColor)
				.Finish();
			//var levels = LevelConfigFactory.FirstLevel<ComplaintDiscussionComment, ComplaintFile>(x => x.ComplaintFiles).LastLevel(c => c.ComplaintDiscussionComment).EndConfig();
			//ytreeviewResult.YTreeModel = new LevelTreeModel<ComplaintDiscussionComment>(ViewModel.Entity.Comments, levels);

			ViewModel.Entity.ObservableResultComments.ListContentChanged += (sender, e) =>
			{
				ytreeviewResult.YTreeModel.EmitModelChanged();
				ytreeviewResult.ExpandAll();
			};
			ytreeviewResult.ExpandAll();

			ytextviewNewResult.Binding.AddBinding(ViewModel, vm => vm.NewResultCommentText, w => w.Buffer.Text).InitializeFromSource();
			ytextviewNewResult.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ybuttonAddResult.Clicked += (sender, e) => ViewModel.AddResultCommentCommand.Execute();
			ybuttonAddResult.Binding.AddBinding(ViewModel, vm => vm.CanAddResultComment, w => w.Sensitive).InitializeFromSource();
		}

		void EntryCounterparty_Changed(object sender, System.EventArgs e)
		{
			spLstAddress.Binding.CleanSources();
			
			if(ViewModel.Entity.Counterparty != null) {
				spLstAddress.NameForSpecialStateNot = "Самовывоз";
				spLstAddress.SetRenderTextFunc<DeliveryPoint>(d => string.Format("{0}: {1}", d.Id, d.ShortAddress));
				spLstAddress.Binding.AddBinding(ViewModel.Entity.Counterparty, s => s.DeliveryPoints, w => w.ItemsList).InitializeFromSource();
				spLstAddress.Binding.AddBinding(ViewModel.Entity, t => t.DeliveryPoint, w => w.SelectedItem).InitializeFromSource();
				return;
			}
			spLstAddress.NameForSpecialStateNot = null;
			spLstAddress.SelectedItem = SpecialComboState.Not;
			spLstAddress.ItemsList = null;
		}

		private string GetTime(object node)
		{
			if(node is ComplaintArrangementResultComment)
			{
				return (node as ComplaintArrangementResultComment).CreationTime.ToShortDateString() + "\n" + (node as ComplaintArrangementResultComment).CreationTime.ToShortTimeString();
			}

			return "";
		}

		private string GetAuthor(object node)
		{
			if(node is ComplaintArrangementResultComment)
			{
				var author = (node as ComplaintArrangementResultComment).Author;
				var subdivisionName = author.Subdivision != null && !string.IsNullOrWhiteSpace(author.Subdivision.ShortName) ? "\n" + author.Subdivision.ShortName : "";
				var result = $"{author.GetPersonNameWithInitials()}{subdivisionName}";
				return result;
			}
			return "";
		}

		private string GetNodeName(object node)
		{
			if(node is ComplaintArrangementResultComment _complaintArrangementResultComment)
			{
				return (node as ComplaintArrangementResultComment).Comment;
			}
			return "";
		}

		private void SetColor(CellRenderer cell, object node)
		{
			if(node is ComplaintArrangementResultComment)
			{
				cell.CellBackgroundGdk = new Gdk.Color(230, 230, 245);
			}
			else
			{
				cell.CellBackgroundGdk = new Gdk.Color(255, 255, 255);
			}
		}
	}
}
