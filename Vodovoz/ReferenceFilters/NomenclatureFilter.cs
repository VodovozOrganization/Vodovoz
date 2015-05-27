using System;
using NHibernate;
using NHibernate.Criterion;
using QSOrmProject;
using Vodovoz.Domain;

namespace Vodovoz
{
	[OrmDefaultIsFiltered (true)]
	public partial class NomenclatureFilter : Gtk.Bin, IReferenceFilter
	{
		public NomenclatureFilter (ISession session)
		{
			this.Build ();
			IsFiltred = false;
			enumcomboType.ItemsEnum = typeof(NomenclatureCategory);
		}

		#region IReferenceFilter implementation

		public event EventHandler Refiltered;

		public ISession Session { get; set; }

		public ICriteria BaseCriteria { get; set; }

		ICriteria filtredCriteria;

		public ICriteria FiltredCriteria {
			private set { filtredCriteria = value; }
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
			if (enumcomboType.SelectedItem is NomenclatureCategory) {
				FiltredCriteria.Add (Restrictions.Eq ("Category", enumcomboType.SelectedItem));
				IsFiltred = true;
			} else
				FiltredCriteria.AddOrder (NHibernate.Criterion.Order.Asc ("Category"));
			OnRefiltered ();
		}

		protected void OnEnumcomboTypeEnumItemSelected (object sender, EnumItemClickedEventArgs e)
		{
			UpdateCreteria ();
		}
	}
}

