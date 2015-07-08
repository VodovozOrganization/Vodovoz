using System;
using NLog;
using QSOrmProject;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Documents;
using Gtk;
using System.Collections.Generic;
using System.Linq;
using QSTDI;
using Vodovoz.Domain;
using Gtk.DataBindings;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class MovementDocumentItemsView : Gtk.Bin
	{
		GenericObservableList<MovementDocumentItem> items;

		static Logger logger = LogManager.GetCurrentClassLogger ();

		public MovementDocumentItemsView ()
		{
			this.Build ();
			treeItemsList.Selection.Changed += OnSelectionChanged;
		}

		private IUnitOfWorkGeneric<MovementDocument> documentUoW;

		public IUnitOfWorkGeneric<MovementDocument> DocumentUoW {
			get { return documentUoW; }
			set {
				if (documentUoW == value)
					return;
				documentUoW = value;
				if (DocumentUoW.Root.Items == null)
					DocumentUoW.Root.Items = new List<MovementDocumentItem> ();
				items = DocumentUoW.Root.ObservableItems;

				treeItemsList.ColumnMappingConfig = FluentMappingConfig<MovementDocumentItem>.Create ()
					.AddColumn ("Наименование").AddTextRenderer (i => i.Name)
					.AddColumn ("С/Н оборудования").AddTextRenderer (i => i.EquipmentString)
					.AddColumn ("Количество")
					.AddNumericRenderer (i => i.Amount).Editing ().WidthChars (10)
					.AddSetter ((c, i) => c.Digits = (uint)i.Nomenclature.Unit.Digits)
					.AddSetter ((c, i) => c.Editable = i.CanEditAmount)
					.Adjustment (new Adjustment (0, 0, 1000000, 1, 100, 0))
					.AddTextRenderer (i => i.Nomenclature.Unit.Name, false)
					.Finish ();

				treeItemsList.ItemsDataSource = items;
			}
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			items.Remove (treeItemsList.GetSelectedObjects () [0] as MovementDocumentItem);
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			buttonDelete.Sensitive = treeItemsList.Selection.CountSelectedRows () > 0;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null) {
				logger.Warn ("Родительская вкладка не найдена.");
				return;
			}

			//ICriteria ItemsCriteria = DocumentUoW.Session.CreateCriteria (typeof(Nomenclature))
			//	.Add (Restrictions.In ("Category", new[] { NomenclatureCategory.additional, NomenclatureCategory.equipment }));

			ReferenceRepresentation SelectDialog = new ReferenceRepresentation (new ViewModel.StockBalanceVM ());
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.None;
			SelectDialog.ObjectSelected += NomenclatureSelected;

			mytab.TabParent.AddSlaveTab (mytab, SelectDialog);
		}

		void NomenclatureSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			var nomenctature = DocumentUoW.GetById<Nomenclature> (e.ObjectId);
			DocumentUoW.Root.AddItem (new MovementDocumentItem { 
				Nomenclature = nomenctature,
				Amount = 1
			});
		}
	}
}

