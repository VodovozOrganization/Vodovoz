//NomenclatureEquipTypeFilter

using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Linq;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;

namespace Vodovoz.JournalFilters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NomenclatureEquipTypeFilter : Gtk.Bin, IRepresentationFilter
	{

		public NomenclatureEquipTypeFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public NomenclatureEquipTypeFilter()
		{
			this.Build();
			UoW = uow;
			entryrefEquipmentType.SubjectType = typeof(EquipmentType);
			OnRefiltered();
		}

		#region IRepresentationFilter implementation

		public event EventHandler Refiltered;

		void OnRefiltered()
		{
			if(Refiltered != null)
				Refiltered(this, new EventArgs());
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

		EquipmentType nomenEquipmentType;

		public EquipmentType NomenEquipmentType {
			get { return nomenEquipmentType; }
			set {
				nomenEquipmentType = value;
				entryrefEquipmentType.Subject = value;
			}
		}

		protected void OnEntryrefEquipmentTypeChangedByUser(object sender, EventArgs e)
		{
			if(entryrefEquipmentType.Subject == null) {
				return;
			} else {
				NomenEquipmentType = (EquipmentType)entryrefEquipmentType.Subject;
				OnRefiltered();
			}
		}

	}
}
