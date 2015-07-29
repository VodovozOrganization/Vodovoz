using System;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Orders;

namespace Vodovoz
{
	[OrmDefaultIsFiltered (true)]
	public partial class OrdersFilter : Gtk.Bin, IRepresentationFilter
	{
		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
				enumcomboStatus.ItemsEnum = typeof(OrderStatus);
			}
		}

		public OrdersFilter (IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public OrdersFilter ()
		{
			this.Build ();
			IsFiltred = false;
		}

		#region IReferenceFilter implementation

		public event EventHandler Refiltered;

		void OnRefiltered ()
		{
			if (Refiltered != null)
				Refiltered (this, new EventArgs ());
		}

		public bool IsFiltred { get; private set; }

		#endregion

		void UpdateCreteria ()
		{
			OnRefiltered ();
		}

		public OrderStatus? RestrictStatus {
			get { return enumcomboStatus.SelectedItem as OrderStatus?;}
			set { enumcomboStatus.SelectedItem = value;
				enumcomboStatus.Sensitive = false;
			}
		}

		protected void OnEnumcomboStatusEnumItemSelected (object sender, EnumItemClickedEventArgs e)
		{
			OnRefiltered ();
		}
	}
}

