using Gamma.GtkWidgets;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Navigation;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class IncomingWaterMaterialView : Gtk.Bin
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		private IUnitOfWorkGeneric<IncomingWater> documentUoW;

		public IUnitOfWorkGeneric<IncomingWater> DocumentUoW {
			get { return documentUoW; }
			set {
				if (documentUoW == value)
					return;
				documentUoW = value;
				if (DocumentUoW.Root.Materials == null)
					DocumentUoW.Root.Materials = new List<IncomingWaterMaterial> ();
				items = DocumentUoW.Root.ObservableMaterials;
				items.ElementChanged += Items_ElementChanged;
				treeMaterialsList.ColumnsConfig = ColumnsConfigFactory.Create<IncomingWaterMaterial> ()
					.AddColumn ("Наименование").AddTextRenderer (i => i.Name)
					.AddColumn ("На продукт")
					.AddNumericRenderer (i => i.OneProductAmountEdited).Editing ().WidthChars (10)
					.AddSetter ((c, i) => c.Digits = (uint)((i.Nomenclature?.Unit?.Digits) ?? 0))
					.Adjustment (new Adjustment (0, 0, 1000000, 1, 100, 0))
					.AddTextRenderer()
					.AddSetter((c, i) => c.Text = i.Nomenclature?.Unit?.Name)
					.AddSetter((c, i) => c.IsExpanded = false)
					.AddColumn ("Всего израсходовано")
					.AddNumericRenderer (i => i.Amount).Editing ().WidthChars (10)
					.AddSetter((c, i) => c.Digits = (uint)((i.Nomenclature?.Unit?.Digits) ?? 0))
					.AddSetter ((c, i) => c.Adjustment = new Adjustment(0, 0, (double)i.AmountOnSource, 1, 100, 0))
					.AddTextRenderer()
					.AddSetter((c, i) => c.Text = i.Nomenclature?.Unit?.Name)
					.AddSetter((c, i) => c.IsExpanded = false)
					.AddColumn("")
					.Finish ();

				treeMaterialsList.ItemsDataSource = items;

				CalculateTotal();
			}
		}

		void Items_ElementChanged (object aList, int[] aIdx)
		{
			CalculateTotal ();
		}

		public IncomingWaterMaterialView ()
		{
			this.Build ();
			treeMaterialsList.Selection.Changed += OnSelectionChanged;
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			buttonDelete.Sensitive = treeMaterialsList.Selection.CountSelectedRows () > 0;
		}

		GenericObservableList<IncomingWaterMaterial> items;

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			items.Remove (treeMaterialsList.GetSelectedObjects () [0] as IncomingWaterMaterial);
			CalculateTotal ();
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			var mytab = DialogHelper.FindParentTab (this);
			if (mytab == null) {
				logger.Warn ("Родительская вкладка не найдена.");
				return;
			}

			Startup.MainWin.NavigationManager
				.OpenViewModelOnTdi<NomenclatureStockBalanceJournalViewModel, Action<NomenclatureStockFilterViewModel>>(
					mytab,
					f => f.RestrictWarehouse = DocumentUoW.Root.WriteOffWarehouse,
					OpenPageOptions.AsSlave,
					vm =>
					{
						vm.SelectionMode = JournalSelectionMode.Single;
						
						vm.OnEntitySelectedResult += (s, ea) =>
						{
							var selectedNode = ea.SelectedNodes.Cast<NomenclatureStockJournalNode>().FirstOrDefault();
							
							if(selectedNode == null)
							{
								return;
							}
							
							var nomenclature = DocumentUoW.GetById<Nomenclature>(selectedNode.Id);
							
							if(DocumentUoW.Root.Materials.Any(x => x.Nomenclature.Id == nomenclature.Id))
							{
								return;
							}
							DocumentUoW.Root.AddMaterial(nomenclature, 1, selectedNode.StockAmount);
						};
					});
		}

		void CalculateTotal ()
		{
			decimal total = 0;
			foreach (var item in documentUoW.Root.Materials) {
				total += item.Amount;
			}
			labelSum.LabelProp = String.Format ("Всего: {0}", total);
		}
	}
}

