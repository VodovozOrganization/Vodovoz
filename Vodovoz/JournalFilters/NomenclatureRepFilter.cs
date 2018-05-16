using System;
using System.Collections;
using System.Linq;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Goods;

namespace Vodovoz.JournalFilters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NomenclatureRepFilter : Gtk.Bin, IRepresentationFilter
	{

		public NomenclatureRepFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public NomenclatureRepFilter()
		{
			this.Build();
			enumcomboCategory.ItemsEnum = typeof(NomenclatureCategory);
			OnRefiltered();
		}

		#region IRepresentationFilter implementation

		public event EventHandler Refiltered;

		void OnRefiltered()
		{
			if(Refiltered != null)
				Refiltered(this, new EventArgs());
			if(enumcomboCategory.SelectedItem != null)
				chkShowDilers.Visible = (NomenclatureCategory)enumcomboCategory.SelectedItem == NomenclatureCategory.water;
		}

		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
			}
		}

		#endregion

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

		public bool ShowDilers {
			get { return chkShowDilers.Active; }
		}

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

		private void SetAvailableCategories()
		{
			enumcomboCategory.ClearEnumHideList();
			var hidingCategories = Enum.GetValues(typeof(NomenclatureCategory)).OfType<NomenclatureCategory>().ToList();
			hidingCategories.RemoveAll(x => AvailableCategories.Contains(x));
			enumcomboCategory.AddEnumToHideList(hidingCategories.Cast<object>().ToArray());
		}

		protected void OnEnumcomboCategoryChangedByUser(object sender, EventArgs e)
		{
			if(enumcomboCategory.SelectedItem == null) {
				return;
			}

			OnRefiltered();
		}

		protected void OnChkShowDilersToggled(object sender, EventArgs e) {
			OnRefiltered();
		}
	}
}
