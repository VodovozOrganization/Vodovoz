using Autofac;
using Gamma.GtkWidgets;
using Gtk;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Tdi;
using QSOrmProject;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ProductSpecificationMaterialsView : Gtk.Bin
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly INomenclatureRepository _nomenclatureRepository =
			new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));

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
					.AddColumn("")
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
			ITdiTab mytab = DialogHelper.FindParentTab (this);
			if (mytab == null) {
				logger.Warn ("Родительская вкладка не найдена.");
				return;
			}

			var filter = new NomenclatureFilterViewModel();
			filter.AvailableCategories = Nomenclature.GetCategoriesForProductMaterial();

			NomenclatureJournalFactory nomenclatureJournalFactory = new NomenclatureJournalFactory(_lifetimeScope);
			var journal = nomenclatureJournalFactory.CreateNomenclaturesJournalViewModel();
			journal.FilterViewModel = filter;
			journal.OnEntitySelectedResult += Journal_OnEntitySelectedResult;

			mytab.TabParent.AddSlaveTab(mytab, journal);
		}

		private void Journal_OnEntitySelectedResult(object sender, JournalSelectedNodesEventArgs e)
		{
			var selectedNode = e.SelectedNodes.FirstOrDefault();
			if(selectedNode == null)
			{
				return;
			}

			var selectedNomenclature = SpecificationUoW.GetById<Nomenclature>(selectedNode.Id);
			items.Add(new ProductSpecificationMaterial
			{
				Material = selectedNomenclature,
				Amount = 1,
				ProductSpec = specificationUoW.Root
			});
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

		public override void Destroy()
		{
			base.Destroy();
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
		}
	}
}

