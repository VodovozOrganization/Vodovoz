using Autofac;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain.Store;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz
{
	public partial class ProductSpecificationDlg : QS.Dialog.Gtk.EntityDialogBase<ProductSpecification>
	{
		private ILifetimeScope _lifetimeScope;

		public ProductSpecificationDlg()
		{
			ResolveDependencies();
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<ProductSpecification>();
			ConfigureDlg();
		}

		public ProductSpecificationDlg(ProductSpecification sub) : this(sub.Id)
		{
		}

		public ProductSpecificationDlg(int id)
		{
			ResolveDependencies();
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<ProductSpecification>(id);
			ConfigureDlg();
		}

		public INavigationManager NavigationManager { get; private set; }

		private void ResolveDependencies()
		{
			_lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
			NavigationManager = _lifetimeScope.Resolve<INavigationManager>();
		}

		private void ConfigureDlg()
		{
			entryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();

			entryProduct.ViewModel = new LegacyEEVMBuilderFactory<ProductSpecification>(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(ps => ps.Product)
				.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel>()
				.UseViewModelDialog<NomenclatureViewModel>()
				.Finish();

			productspecificationmaterialsview1.SpecificationUoW = UoWGeneric;
		}

		public override bool Save()
		{
			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			UoWGeneric.Save();
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
