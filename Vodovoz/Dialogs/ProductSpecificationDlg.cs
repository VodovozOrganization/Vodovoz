using System;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ProductSpecificationDlg : OrmGtkDialogBase<ProductSpecification>
	{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		protected IContractOwner ContractOwner;

		public ProductSpecificationDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<ProductSpecification> ();
			ConfigureDlg ();
		}

		public ProductSpecificationDlg (ProductSpecification sub) : this (sub.Id) {}

		public ProductSpecificationDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<ProductSpecification> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			datatable5.DataSource = subjectAdaptor;
			referenceProduct.SubjectType = typeof(Organization);
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

