//NomenclatureEquipTypeFilter

using System;
using NHibernate;
using NHibernate.Criterion;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;

namespace Vodovoz.JournalFilters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NomenclatureEquipTypeFilter : RepresentationFilterBase<NomenclatureEquipTypeFilter>, IReferenceFilter
	{
		protected override void ConfigureWithUow()
		{
			entryrefEquipmentType.SubjectType = typeof(EquipmentType);
			OnRefiltered();
		}

		public NomenclatureEquipTypeFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public NomenclatureEquipTypeFilter()
		{
			this.Build();
		}

		EquipmentType nomenEquipmentType;

		public EquipmentType NomenEquipmentType {
			get { return nomenEquipmentType; }
			set {
				nomenEquipmentType = value;
				entryrefEquipmentType.Subject = value;
			}
		}

		public ICriteria BaseCriteria { get; set; }

		ICriteria filtredCriteria;

		public ICriteria FiltredCriteria {
			private set { filtredCriteria = value; }
			get {
				UpdateCreteria();
				return filtredCriteria;
			}
		}

		void UpdateCreteria()
		{
			IsFiltred = false;
			if(BaseCriteria == null) {
				filtredCriteria = null;
				return;
			}
			filtredCriteria = (ICriteria)BaseCriteria.Clone();
			if(entryrefEquipmentType.Subject is EquipmentType) {
				filtredCriteria.Add(Restrictions.Eq("Type", entryrefEquipmentType.Subject));
			}
			IsFiltred = true;
			OnRefiltered();
		}

		public bool IsFiltred { get; private set; }

		protected void OnEntryrefEquipmentTypeChangedByUser(object sender, EventArgs e)
		{
			if(entryrefEquipmentType.Subject == null) {
				return;
			} else {
				NomenEquipmentType = (EquipmentType)entryrefEquipmentType.Subject;
				OnRefiltered();
			}
		}

		protected void OnButtonClearClicked(object sender, EventArgs e)
		{
			NomenEquipmentType = null;
			FiltredCriteria = null;
			OnRefiltered();
		}
	}
}
