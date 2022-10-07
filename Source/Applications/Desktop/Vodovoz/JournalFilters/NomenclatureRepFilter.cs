using System;
using System.Linq;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Goods;

namespace Vodovoz.JournalFilters
{
	[System.ComponentModel.ToolboxItem(true)]
	[Obsolete("Использовать MVVM фильтр")]
	public partial class NomenclatureRepFilter : RepresentationFilterBase<NomenclatureRepFilter>
	{
		protected override void ConfigureWithUow()
		{
			enumcomboCategory.ItemsEnum = typeof(NomenclatureCategory);
			cmbSaleCategory.ItemsEnum = typeof(SaleCategory);
			cmbSaleCategory.Visible = Nomenclature.GetCategoriesWithSaleCategory().Contains(DefaultSelectedCategory);
			chkShowDilers.Visible = DefaultSelectedCategory == NomenclatureCategory.water;
			OnRefiltered();
			UpdateVisibility();
		}

		public NomenclatureRepFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public NomenclatureRepFilter()
		{
			this.Build();
		}

		private void UpdateVisibility()
		{
			chkOnlyDisposableTare.Visible = chkShowDilers.Visible = (enumcomboCategory.SelectedItem is NomenclatureCategory) && (NomenclatureCategory)enumcomboCategory.SelectedItem == NomenclatureCategory.water;
		}

		NomenclatureCategory[] availableCategories;

		public NomenclatureCategory[] AvailableCategories {
			get { return availableCategories; }
			set {
				availableCategories = value;
				SetAvailableCategories();
			}
		}

		NomenclatureCategory defaultSelectedCategory;

		public NomenclatureCategory DefaultSelectedCategory {
			get { return defaultSelectedCategory; }
			set {
				defaultSelectedCategory = value;
				enumcomboCategory.SelectedItem = value;
			}
		}

		SaleCategory defaultSelectedSaleCategory;

		public SaleCategory DefaultSelectedSaleCategory {
			get { return defaultSelectedSaleCategory; }
			set {
				defaultSelectedSaleCategory = value;
				cmbSaleCategory.SelectedItem = value;
			}
		}

		public bool ShowDilers => chkShowDilers.Active;

		public bool OnlyDisposableTare => chkOnlyDisposableTare.Active && SelectedCategories.Contains(NomenclatureCategory.water);

		public NomenclatureCategory[] SelectedCategories {
			get {
				var selected = enumcomboCategory.SelectedItem as NomenclatureCategory?;
				if(selected == null) {
					if(availableCategories.Any()) {
						return availableCategories;
					} else {
						return Enum.GetValues(typeof(NomenclatureCategory)).OfType<NomenclatureCategory>().ToArray();
					}
				} else {
					return new NomenclatureCategory[] { (NomenclatureCategory)enumcomboCategory.SelectedItem };
				}
			}
		}

		public SaleCategory[] SelectedSubCategories {
			get {
				var selected = cmbSaleCategory.SelectedItem as SaleCategory?;
				if(selected == null)
					return Enum.GetValues(typeof(SaleCategory)).OfType<SaleCategory>().ToArray();
				return new SaleCategory[] { (SaleCategory)cmbSaleCategory.SelectedItem };
			}
		}

		private void SetAvailableCategories()
		{
			enumcomboCategory.ClearEnumHideList();
			var hidingCategories = Enum.GetValues(typeof(NomenclatureCategory)).OfType<NomenclatureCategory>().ToList();
			hidingCategories.RemoveAll(AvailableCategories.Contains);
			enumcomboCategory.AddEnumToHideList(hidingCategories.Cast<object>().ToArray());
		}

		void SetVisibilityOfSubcategory()
		{
			cmbSaleCategory.Visible = enumcomboCategory.SelectedItem != null
				&& Nomenclature.GetCategoriesWithSaleCategory().Contains((NomenclatureCategory)enumcomboCategory.SelectedItem);
		}

		protected void OnEnumcomboCategoryChangedByUser(object sender, EventArgs e)
		{
			SetVisibilityOfSubcategory();

			if(enumcomboCategory.SelectedItem == null) {
				return;
			}


			UpdateVisibility();
			OnRefiltered();
		}

		protected void OnChkShowDilersToggled(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnCmbSaleCategoryChangedByUser(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnChkOnlyDisposableTareToggled(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}
