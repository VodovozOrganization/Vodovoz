using QS.DomainModel.UoW;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;

namespace Vodovoz
{

	public partial class ProductSpecificationDlg : QS.Dialog.Gtk.EntityDialogBase<ProductSpecification>
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
			entryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			referenceProduct.SubjectType = typeof (Nomenclature);
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

