using Autofac;
using QS.DomainModel.UoW;
using QS.Validation;
using Vodovoz.Domain.Store;
using Vodovoz.TempAdapters;

namespace Vodovoz
{

	public partial class ProductSpecificationDlg : QS.Dialog.Gtk.EntityDialogBase<ProductSpecification>
	{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();

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

			var nomenclatureSelectorFactory = new NomenclatureJournalFactory(_lifetimeScope);
			entryProduct.SetEntityAutocompleteSelectorFactory(nomenclatureSelectorFactory.GetDefaultNomenclatureSelectorFactory(_lifetimeScope));
			entryProduct.Binding.AddBinding(Entity, e => e.Product, w => w.Subject).InitializeFromSource();

			productspecificationmaterialsview1.SpecificationUoW = UoWGeneric;
		}

		public override bool Save ()
		{
			var validator = new ObjectValidator(new GtkValidationViewFactory());
			if(!validator.Validate(Entity))
			{
				return false;
			}

			UoWGeneric.Save ();
			return true;
		}

		public override void Destroy()
		{
			base.Destroy();
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
		}

	}
}

