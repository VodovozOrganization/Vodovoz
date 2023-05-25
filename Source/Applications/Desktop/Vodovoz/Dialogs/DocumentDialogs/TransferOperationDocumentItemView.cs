using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Dialogs;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Tdi;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.JournalNodes;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModel;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.Dialogs.DocumentDialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TransferOperationDocumentItemView : Gtk.Bin
	{
		GenericObservableList<MovementDocumentItem> items;

		static Logger logger = LogManager.GetCurrentClassLogger();

		public TransferOperationDocumentItemView()
		{
			this.Build();
			treeItemsList.Selection.Changed += OnSelectionChanged;
		}

		private IUnitOfWorkGeneric<TransferOperationDocument> documentUoW;

		public IUnitOfWorkGeneric<TransferOperationDocument> DocumentUoW {
			get { return documentUoW; }
			set {
				if(documentUoW == value)
					return;
				documentUoW = value;

				treeItemsList.ColumnsConfig = Gamma.ColumnConfig.FluentColumnsConfig<MovementDocumentItem>.Create()
					.AddColumn("Наименование").AddTextRenderer(i => i.Name)
					.AddColumn("Количество")
					.AddNumericRenderer(i => i.SentAmount).Editing().WidthChars(10)
					.AddSetter((c, i) => c.Digits = (uint)i.Nomenclature.Unit.Digits)
					.AddSetter((c, i) => c.Editable = i.CanEditAmount)
					.AddSetter((c, i) => c.Adjustment = new Adjustment(0, 0, (double)i.AmountOnSource, 1, 100, 0))
					.AddTextRenderer(i => i.Nomenclature.Unit.Name, false)
					.Finish();

				treeItemsList.ItemsDataSource = items;
			}
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			items.Remove(treeItemsList.GetSelectedObjects()[0] as MovementDocumentItem);
		}

		void OnSelectionChanged(object sender, EventArgs e)
		{
			buttonDelete.Sensitive = treeItemsList.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			throw new NotSupportedException("На данный момент не поддерживается добавление номенклатур");

			if(DocumentUoW.Root.FromClient == null) {
				MessageDialogHelper.RunErrorDialog("Не добавлен отправитель.");
				return;
			}

			if(DocumentUoW.Root.FromDeliveryPoint == null) {
				MessageDialogHelper.RunErrorDialog("Не добавлена точка доставки отправителя.");
			}

			ITdiTab mytab = DialogHelper.FindParentTab(this);
			if(mytab == null) {
				logger.Warn("Родительская вкладка не найдена.");
				return;
			}

			var vm = MainClass.MainWin.NavigationManager
				.OpenViewModel<NomenclatureStockBalanceJournalViewModel>(null)
				.ViewModel;

			vm.SelectionMode = JournalSelectionMode.Single;
			vm.OnEntitySelectedResult += (s, ea) => {
				var selectedNode = ea.SelectedNodes.Cast<NomenclatureStockJournalNode>().FirstOrDefault();
				if(selectedNode == null) {
					return;
				}
				var nomenclature = DocumentUoW.GetById<Nomenclature>(selectedNode.Id);
				throw new NotSupportedException("На данный момент не поддерживается добавление номенклатур");
			};
		}
	}
}
