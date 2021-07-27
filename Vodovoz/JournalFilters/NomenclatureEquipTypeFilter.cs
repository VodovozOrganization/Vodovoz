//NomenclatureEquipTypeFilter

using System;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
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
			entryrefEquipmentKind.SubjectType = typeof(EquipmentKind);
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

		EquipmentKind nomenEquipmentKind;

		public EquipmentKind NomenEquipmentKind {
			get { return nomenEquipmentKind; }
			set {
				nomenEquipmentKind = value;
				entryrefEquipmentKind.Subject = value;
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
			if(entryrefEquipmentKind.Subject is EquipmentKind) {
				filtredCriteria.Add(Restrictions.Eq("Type", entryrefEquipmentKind.Subject));
			}
			IsFiltred = true;
			OnRefiltered();
		}

		public bool IsFiltred { get; private set; }

		protected void OnEntryrefEquipmentKindChangedByUser(object sender, EventArgs e)
		{
			if(entryrefEquipmentKind.Subject == null) {
				return;
			} else {
				NomenEquipmentKind = (EquipmentKind)entryrefEquipmentKind.Subject;
				OnRefiltered();
			}
		}

		protected void OnButtonClearClicked(object sender, EventArgs e)
		{
			NomenEquipmentKind = null;
			FiltredCriteria = null;
			OnRefiltered();
		}
	}
}
