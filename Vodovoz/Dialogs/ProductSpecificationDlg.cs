using System;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Domain.Store;

namespace Vodovoz
{
	
	public partial class ProductSpecificationDlg : OrmGtkDialogBase<ProductSpecification>
	{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public ProductSpecificationDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<ProductSpecification> ();
			ConfigureDlg ();
		}

		public ProductSpecificationDlg (ProductSpecification sub) : this (sub.Id)
		{
		}

		public ProductSpecificationDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<ProductSpecification> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			referenceProduct.SubjectType = typeof (Organization);
			referenceProduct.Binding.AddBinding (Entity, e => e.Product, w => w.Subject).InitializeFromSource ();
			productspecificationmaterialsview1.SpecificationUoW = UoWGeneric;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<ProductSpecification> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			UoWGeneric.Save ();
			return true;
		}

	}
}

