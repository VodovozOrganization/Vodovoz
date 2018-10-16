using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using Gamma.GtkWidgets;
using Gtk;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSTDI;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ProductSpecificationMaterialsView : Gtk.Bin
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		GenericObservableList<ProductSpecificationMaterial> items;

		private IUnitOfWorkGeneric<ProductSpecification> specificationUoW;

		public IUnitOfWorkGeneric<ProductSpecification> SpecificationUoW {
			get {
				return specificationUoW;
			}
			set {if (specificationUoW == value)
					return;
				specificationUoW = value;
				if (specificationUoW.Root.Materials == null)
					specificationUoW.Root.Materials = new List<ProductSpecificationMaterial> ();
				items = new GenericObservableList<ProductSpecificationMaterial> (specificationUoW.Root.Materials);
				items.ElementChanged += Items_ElementChanged;
				items.ElementAdded += Items_ElementAdded;

				treeMaterialsList.ColumnsConfig = ColumnsConfigFactory.Create<ProductSpecificationMaterial>()
					.AddColumn ("Наименование").AddTextRenderer(p => p.NomenclatureName)
					.AddColumn ("Количество").AddNumericRenderer (p => p.Amount).Editing ()
					.AddSetter((c, p) => c.Digits = (uint)p.Material.Unit.Digits)
					.Adjustment (new Adjustment(0, 0, 1000000, 1, 100,0))
					.AddTextRenderer (p => p.Material.Unit.Name, false)
					.Finish();
				
				treeMaterialsList.ItemsDataSource = items;
				CalculateTotal ();
			}
		}

		void Items_ElementAdded (object aList, int[] aIdx)
		{
			CalculateTotal ();
		}

		public ProductSpecificationMaterialsView ()
		{
			this.Build ();
			treeMaterialsList.Selection.Changed += OnSelectionChanged;
		}

		void Items_ElementChanged (object aList, int[] aIdx)
		{
			CalculateTotal ();
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			bool selected = treeMaterialsList.Selection.CountSelectedRows () > 0;
			buttonDelete.Sensitive = selected;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null) {
				logger.Warn ("Родительская вкладка не найдена.");
				return;
			}

			OrmReference SelectDialog = new OrmReference (typeof(Nomenclature),
				SpecificationUoW,
				Repository.NomenclatureRepository.NomenclatureForProductMaterialsQuery ()
				.GetExecutableQueryOver (SpecificationUoW.Session).RootCriteria
			);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ObjectSelected += NomenclatureSelected;

			mytab.TabParent.AddSlaveTab (mytab, SelectDialog);
		}

		void NomenclatureSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			items.Add (new ProductSpecificationMaterial { 
				Material = e.Subject as Nomenclature, 
				Amount = 1,
				ProductSpec = specificationUoW.Root
			});
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			items.Remove (treeMaterialsList.GetSelectedObjects () [0] as ProductSpecificationMaterial);
			CalculateTotal ();
		}

		void CalculateTotal()
		{
			decimal totalAmount = 0;
			foreach(var item in SpecificationUoW.Root.Materials)
			{
				totalAmount += item.Amount;
			}

			labelSum.LabelProp = String.Format ("Всего: {0}", (totalAmount));
		}
	}
}

