using System;
using QSOrmProject;
using NHibernate;
using NHibernate.Criterion;

namespace Vodovoz
{
	[OrmDefaultIsFiltered (true)]
	public partial class EquipmentFilter : Gtk.Bin, IReferenceFilter
	{
		public EquipmentFilter (ISession session)
		{
			this.Build ();
			IsFiltred = false;
		}

		#region IReferenceFilter implementation

		public event EventHandler Refiltered;

		public ISession Session { get; set; }

		public ICriteria BaseCriteria { get; set; }

		ICriteria filtredCriteria;

		public ICriteria FiltredCriteria {
			private set {
				filtredCriteria = value;
			}
			get {
				if (filtredCriteria == null)
					UpdateCreteria ();
				return filtredCriteria;
			}		
		}

		void OnRefiltered ()
		{
			if (Refiltered != null)
				Refiltered (this, new EventArgs ());
		}

		public bool IsFiltred { get; private set; }

		#endregion

		void UpdateCreteria ()
		{
			IsFiltred = false;
			if (BaseCriteria == null)
				return;
			FiltredCriteria = (ICriteria)BaseCriteria.Clone ();
			if (checkSelectOutdated.Active) {
				FiltredCriteria.Add (Restrictions.Lt ("LastServiceDate", DateTime.Now.AddMonths (-5).AddDays (-14)));
				IsFiltred = true;
			}
			OnRefiltered ();
		}

		protected void OnCheckSelectOutdatedToggled (object sender, EventArgs e)
		{
			UpdateCreteria ();
		}
	}
}

