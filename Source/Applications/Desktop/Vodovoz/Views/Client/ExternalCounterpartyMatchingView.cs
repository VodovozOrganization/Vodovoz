using System;
using Gamma.Binding;
using Gamma.Binding.Core.LevelTreeConfig;
using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using QS.Journal.GtkUI;
using QS.Navigation;
using QS.Tdi;
using QS.Views;
using Vodovoz.ViewModels.ViewModels.Counterparty;
using Vodovoz.Infrastructure;

namespace Vodovoz.Views.Client
{
	public partial class ExternalCounterpartyMatchingView : ViewBase<ExternalCounterpartyMatchingViewModel>
	{
		private Menu _discrepanciesPopUpMenu;
		
		public ExternalCounterpartyMatchingView(ExternalCounterpartyMatchingViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnSave.Clicked += OnSaveClicked;
			btnCancel.Clicked += OnCancelClicked;
			
			btnSave.Binding
				.AddBinding(ViewModel, vm => vm.HasAssignedCounterparty, w => w.Sensitive)
				.InitializeFromSource();
			
			lblDateValue.Binding
				.AddBinding(ViewModel, vm => vm.EntityDate, w => w.LabelProp)
				.InitializeFromSource();
			lblPhoneValue.Binding
				.AddBinding(ViewModel, vm => vm.PhoneNumber, w => w.LabelProp)
				.InitializeFromSource();
			lblCounterpartyFromValue.Binding
				.AddBinding(ViewModel, vm => vm.CounterpartyFrom, w => w.LabelProp)
				.InitializeFromSource();
			lblExternalIdValue.Binding
				.AddBinding(ViewModel, vm => vm.ExternalCounterpartyId, w => w.LabelProp)
				.InitializeFromSource();
			lblCounterpartyValue.Binding
				.AddBinding(ViewModel, vm => vm.CounterpartyName, w => w.LabelProp)
				.InitializeFromSource();

			expanderDiscrepancies.Expanded = ViewModel.HasDiscrepancies;

			ConfigureTreeViews();
		}

		private void ConfigureTreeViews()
		{
			ConfigureTreeMatchesAndHandleButtons();
			ConfigureTreeDiscrepancies();
		}

		private void ConfigureTreeMatchesAndHandleButtons()
		{
			treeViewMatches.ColumnsConfig = FluentColumnsConfig<IExternalCounterpartyMatchingNode>.Create()
				.AddColumn("Тип КА").AddTextRenderer(n => n is CounterpartyMatchingNode ? n.PersonTypeShort : string.Empty)
				.AddColumn("Код КА/ТД").AddNumericRenderer(n => n.EntityId)
				.AddColumn("КА/ТД").AddTextRenderer(n => n.Title)
				.AddColumn("Совпадение").AddToggleRenderer(n => n.Matching).Editing(false)
				.AddColumn("Дата последнего заказа").AddTextRenderer(n => n.LastOrderDateString)
				.AddColumn("")
				.RowCells()
					.AddSetter<CellRendererText>((c, n) =>
						c.ForegroundGdk = n.HasOtherExternalCounterparty ? GdkColors.OrangeColor : GdkColors.BlackColor)
					.AddSetter<CellRenderer>(
						(c, n) =>
						{
							var color = GdkColors.WhiteColor;
									
							if(n.ExternalCounterpartyId.HasValue && !n.HasOtherExternalCounterparty)
							{
								color = GdkColors.LightGreenColor;
							}

							c.CellBackgroundGdk = color;
						})
				.Finish();

			var levels = LevelConfigFactory.FirstLevel<CounterpartyMatchingNode, DeliveryPointMatchingNode>(
				x => x.DeliveryPoints).LastLevel(c => c.CounterpartyMatchingNode).EndConfig();
			treeViewMatches.YTreeModel = new LevelTreeModel<CounterpartyMatchingNode>(ViewModel.ContactMatches, levels);
			treeViewMatches.ExpandAll();
			treeViewMatches.RowActivated += OnTreeViewMatchesRowActivated;
			treeViewMatches.Binding
				.AddBinding(ViewModel, vm => vm.SelectedMatch, w => w.SelectedRow)
				.InitializeFromSource();

			btnNewCounterparty.Clicked += OnNewCounterpartyClicked;
			btnAssignCounterparty.Clicked += OnAssignCounterpartyClicked;
			btnOrdersJournal.Clicked += OnOrdersJournalClicked;
			btnLegalCounterparty.Clicked += OnLegalCounterpartyClicked;

			btnAssignCounterparty.Binding
				.AddBinding(ViewModel, vm => vm.HasSelectedNotAssignedCounterpartyMatchingNode, w => w.Sensitive)
				.InitializeFromSource();
			btnOrdersJournal.Binding
				.AddBinding(ViewModel, vm => vm.HasSelectedMatch, w => w.Sensitive)
				.InitializeFromSource();
			btnLegalCounterparty.Binding
				.AddFuncBinding(ViewModel, vm => !vm.HasAssignedCounterparty, w => w.Sensitive)
				.InitializeFromSource();
		}

		private void ConfigureTreeDiscrepancies()
		{
			treeViewDiscrepancies.ColumnsConfig = FluentColumnsConfig<ExistingExternalCounterpartyNode>.Create()
				.AddColumn("Тип КА").AddTextRenderer(n => n.PersonTypeShort)
				.AddColumn("Код КА").AddNumericRenderer(n => n.EntityId)
				.AddColumn("КА").AddTextRenderer(n => n.CounterpartyName)
				.AddColumn("Код КА в ИПЗ")
					.AddTextRenderer(n => n.ExternalCounterpartyGuid.ToString())
					.AddSetter((c, n) =>
						c.ForegroundGdk = n.ExternalCounterpartyGuid != ViewModel.Entity.ExternalCounterpartyGuid
							? GdkColors.RedColor2
							: GdkColors.BlackColor)
				.AddColumn("Телефон")
					.AddTextRenderer(n => n.PhoneNumber)
					.AddSetter((c, n) =>
						c.ForegroundGdk = n.PhoneNumber != ViewModel.DigitsPhoneNumber
							? GdkColors.RedColor2
							: GdkColors.BlackColor)
				.AddColumn("")
				.Finish();

			treeViewDiscrepancies.ItemsDataSource = ViewModel.Discrepancies;
			treeViewDiscrepancies.RowActivated += OnTreeViewDiscrepanciesRowActivated;
			treeViewDiscrepancies.ButtonReleaseEvent += OnTreeViewDiscrepanciesButtonRelease;
			treeViewDiscrepancies.Binding
				.AddBinding(ViewModel, vm => vm.SelectedDiscrepancy, w => w.SelectedRow)
				.InitializeFromSource();

			CreateDiscrepanciesPopUpMenu();
		}

		private void CreateDiscrepanciesPopUpMenu()
		{
			_discrepanciesPopUpMenu = new Menu();
			var menuItem = new MenuItem("Переприсвоить контрагента");
			menuItem.Activated += ReAssignCounterparty;
			_discrepanciesPopUpMenu.Add(menuItem);
			_discrepanciesPopUpMenu.ShowAll();
		}

		private void ReAssignCounterparty(object sender, EventArgs e)
		{
			ViewModel.ReAssignCounterpartyCommand.Execute();
			treeViewMatches.YTreeModel.EmitModelChanged();
		}

		private void OnTreeViewMatchesRowActivated(object o, RowActivatedArgs args)
		{
			if(ViewModel.SelectedMatch is CounterpartyMatchingNode counterpartyNode)
			{
				OpenCounterpartyDlg(counterpartyNode.EntityId);
			}
		}

		private void OnTreeViewDiscrepanciesRowActivated(object o, RowActivatedArgs args)
		{
			if(ViewModel.SelectedDiscrepancy is ExistingExternalCounterpartyNode existingCounterpartyNode)
			{
				OpenCounterpartyDlg(existingCounterpartyNode.EntityId);
			}
		}
		
		private void OnTreeViewDiscrepanciesButtonRelease(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button != (uint)GtkMouseButton.Right || ViewModel.SelectedDiscrepancy is null)
			{
				return;
			}
			
			_discrepanciesPopUpMenu.Popup();
		}
		
		private void OpenCounterpartyDlg(int counterpartyId)
		{
			ViewModel.Navigation.OpenTdiTab<CounterpartyDlg, int>(ViewModel, counterpartyId, OpenPageOptions.AsSlave);
		}

		private void OnSaveClicked(object sender, EventArgs e)
		{
			ViewModel.Save(false);
			ViewModel.Close(false, CloseSource.Save);
		}
		
		private void OnCancelClicked(object sender, EventArgs e)
		{
			ViewModel.Close(false, CloseSource.Cancel);
		}
		
		private void OnNewCounterpartyClicked(object sender, EventArgs e)
		{
			var page = ViewModel.Navigation.OpenTdiTab<CounterpartyDlg>(ViewModel, OpenPageOptions.AsSlave);
			(page.TdiTab as CounterpartyDlg).EntitySaved += OnNewCounterpartySaved;
		}

		private void OnNewCounterpartySaved(object sender, EntitySavedEventArgs e)
		{
			ViewModel.UpdateMatches();
			treeViewMatches.ExpandAll();
		}

		private void OnAssignCounterpartyClicked(object sender, EventArgs e)
		{
			ViewModel.AssignCounterpartyCommand.Execute();
			treeViewMatches.Selection.UnselectAll();
			treeViewMatches.QueueDraw();
		}

		private void OnOrdersJournalClicked(object sender, EventArgs e)
		{
			ViewModel.OpenOrderJournalCommand.Execute();
		}

		private void OnLegalCounterpartyClicked(object sender, EventArgs e)
		{
			ViewModel.LegalCounterpartyCommand.Execute();
		}
	}
}
