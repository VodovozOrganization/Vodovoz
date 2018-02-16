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

		public NomenclatureRepFilter(IUnitOfWork uow): this()
		{
			UoW = uow;
		}

		public NomenclatureRepFilter()
		{
			this.Build();
			UoW = uow;
			enumcomboCategory.ItemsEnum = typeof(NomenclatureCategory);
			OnRefiltered();
		}

		#region IRepresentationFilter implementation

		public event EventHandler Refiltered;

		void OnRefiltered()
		{
			if (Refiltered != null)
				Refiltered(this, new EventArgs());
		}

		IUnitOfWork uow;

		public IUnitOfWork UoW
		{
			get
			{
				return uow;
			}
			set
			{
				uow = value;
			}
		}

		#endregion

		NomenclatureCategory nomenCategory;

		public NomenclatureCategory NomenCategory
		{
			get { return nomenCategory; }
			set { 
				nomenCategory = value;
				AllSelected = false;
				enumcomboCategory.SelectedItem = value;
			}
		}

		bool allSelected = true;

		public bool AllSelected
		{
			get { return allSelected; }
			set { allSelected = value; }
		}

		private object[] HideList(NomenclatureCategory[] nomCat)
		{
			ArrayList ncList = new ArrayList(), objList = new ArrayList();
			ncList.AddRange(Enum.GetValues(typeof(NomenclatureCategory)));
			foreach (NomenclatureCategory nc in ncList)
			{
				if(!nomCat.Contains(nc))
				{
					objList.Add(nc);
				}
			}

			return objList.Cast<object>().ToArray();
		}

		protected void OnEnumcomboCategoryChangedByUser(object sender, EventArgs e)
		{
			if(enumcomboCategory.SelectedItem == null) {
				return;
			}

			if((SpecialComboState)enumcomboCategory.SelectedItem == SpecialComboState.All) {
				AllSelected = true;
			} else {
				NomenCategory = (NomenclatureCategory)enumcomboCategory.SelectedItem;
			}
			OnRefiltered();
		}
	}
}
