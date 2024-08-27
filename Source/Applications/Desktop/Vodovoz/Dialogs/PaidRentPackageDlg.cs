using System.ComponentModel.DataAnnotations;
using Autofac;
using NHibernate.Criterion;
using NLog;
using QS.Project.Services;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.EntityRepositories.RentPackages;
using Vodovoz.Factories;

namespace Vodovoz
{
	public partial class PaidRentPackageDlg : QS.Dialog.Gtk.EntityDialogBase<PaidRentPackage>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private readonly IRentPackageRepository _rentPackageRepository = ScopeProvider.Scope.Resolve<IRentPackageRepository>();
		private readonly IValidationContextFactory _validationContextFactory = new ValidationContextFactory();

		private ValidationContext _validationContext;

		public PaidRentPackageDlg ()
		{
			this.Build ();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<PaidRentPackage>();
			ConfigureDlg ();
		}

		public PaidRentPackageDlg (int id)
		{
			this.Build ();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<PaidRentPackage> (id);
			ConfigureDlg ();
		}

		public PaidRentPackageDlg (PaidRentPackage sub) : this(sub.Id)
		{
		}

		private void ConfigureDlg ()
		{
			dataentryName.Binding.AddBinding (Entity, e => e.Name, w => w.Text).InitializeFromSource ();
			spinDeposit.Binding.AddBinding (Entity, e => e.Deposit, w => w.ValueAsDecimal).InitializeFromSource ();
			spinPriceDaily.Binding.AddBinding (Entity, e => e.PriceDaily, w => w.ValueAsDecimal).InitializeFromSource ();
			spinPriceMonthly.Binding.AddBinding (Entity, e => e.PriceMonthly, w => w.ValueAsDecimal).InitializeFromSource ();

			referenceDepositService.SubjectType = typeof(Nomenclature);
			referenceDepositService.ItemsCriteria = UoW.Session.CreateCriteria<Nomenclature> ()
				.Add (Restrictions.Eq ("Category", NomenclatureCategory.deposit));
			referenceDepositService.Binding.AddBinding (Entity, e => e.DepositService, w => w.Subject).InitializeFromSource ();

			referenceRentServiceDaily.SubjectType = typeof(Nomenclature);
			referenceRentServiceDaily.ItemsCriteria = UoW.Session.CreateCriteria<Nomenclature>();
			referenceRentServiceDaily.Binding.AddBinding (Entity, e => e.RentServiceDaily, w => w.Subject).InitializeFromSource ();

			referenceRentServiceMonthly.SubjectType = typeof(Nomenclature);
			referenceRentServiceMonthly.ItemsCriteria = UoW.Session.CreateCriteria<Nomenclature>();
			referenceRentServiceMonthly.Binding.AddBinding (Entity, e => e.RentServiceMonthly, w => w.Subject).InitializeFromSource ();

			referenceEquipmentKind.SubjectType = typeof(EquipmentKind);
			referenceEquipmentKind.Binding.AddBinding (Entity, e => e.EquipmentKind, w => w.Subject).InitializeFromSource ();

			ConfigureValidateContext();
		}

		private void ConfigureValidateContext()
		{
			_validationContext = _validationContextFactory.CreateNewValidationContext(Entity);
			
			_validationContext.ServiceContainer.AddService(typeof(IRentPackageRepository), _rentPackageRepository);
		}

		public override bool Save()
		{
			if(!ServicesConfig.ValidationService.Validate(Entity, _validationContext))
			{
				return false;
			}

			logger.Info ("Сохраняем пакет платных услуг...");
			UoWGeneric.Save();
			return true;
		}
	}
}
