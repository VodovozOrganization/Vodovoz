using System;
using System.Linq;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Goods;

namespace Vodovoz.JournalFilters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NomenclatureRepFilter : RepresentationFilterBase<NomenclatureRepFilter>
	{
		protected override void ConfigureWithUow()
		{
			enumcomboCategory.ItemsEnum = typeof(NomenclatureCategory);
			cmbEquipmentSubtype.ItemsEnum = typeof(SubtypeOfEquipmentCategory);
			cmbEquipmentSubtype.Visible = DefaultSelectedCategory == NomenclatureCategory.equipment;
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

		SubtypeOfEquipmentCategory defaultSelectedSubCategory;

		public SubtypeOfEquipmentCategory DefaultSelectedSubCategory {
			get { return defaultSelectedSubCategory; }
			set {
				defaultSelectedSubCategory = value;
				cmbEquipmentSubtype.SelectedItem = value;
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

		public SubtypeOfEquipmentCategory[] SelectedSubCategories {
			get {
				var selected = cmbEquipmentSubtype.SelectedItem as SubtypeOfEquipmentCategory?;
				if(selected == null) {
					return Enum.GetValues(typeof(SubtypeOfEquipmentCategory)).OfType<SubtypeOfEquipmentCategory>().ToArray();
				} else {
					return new SubtypeOfEquipmentCategory[] { (SubtypeOfEquipmentCategory)cmbEquipmentSubtype.SelectedItem };
				}
			}
		}

		private void SetAvailableCategories()
		{
			enumcomboCategory.ClearEnumHideList();
			var hidingCategories = Enum.GetValues(typeof(NomenclatureCategory)).OfType<NomenclatureCategory>().ToList();
			hidingCategories.RemoveAll(x => AvailableCategories.Contains(x));
			enumcomboCategory.AddEnumToHideList(hidingCategories.Cast<object>().ToArray());
		}

		void SetVisibilityOfSubcategory()
		{
			cmbEquipmentSubtype.Visible = enumcomboCategory.SelectedItem != null
				&& (NomenclatureCategory)enumcomboCategory.SelectedItem == NomenclatureCategory.equipment;
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

		protected void OnCmbEquipmentSubtypeChangedByUser(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnChkOnlyDisposableTareToggled(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}
