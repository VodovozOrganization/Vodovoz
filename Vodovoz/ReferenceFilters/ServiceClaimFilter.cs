using System;
using QSOrmProject.RepresentationModel;
using QSOrmProject;
using Vodovoz.Domain.Service;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ServiceClaimFilter : Gtk.Bin, IRepresentationFilter
	{
		public ServiceClaimFilter (IUnitOfWork uow) : this ()
		{
			UoW = uow;
			
		}

		public ServiceClaimFilter ()
		{
			this.Build ();
			IsFiltred = false;
		}

		#region IRepresentationFilter implementation

		public event EventHandler Refiltered;

		void OnRefiltered ()
		{
			if (Refiltered != null)
				Refiltered (this, new EventArgs ());
		}

		public bool IsFiltred { get; private set; }

		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
				comboStatus.ItemsEnum = typeof(ServiceClaimStatus);
			}
		}

		#endregion

		public ServiceClaimStatus? RestrictServiceClaimStatus {
			get { return comboStatus.SelectedItem as ServiceClaimStatus?; }
			set {
				comboStatus.SelectedItem = value;
				comboStatus.Sensitive = false;
			}
		}

		protected void OnComboStatusEnumItemSelected (object sender, EnumItemClickedEventArgs e)
		{
			OnRefiltered ();
		}
	}
}

