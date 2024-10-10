using Autofac;
using NHibernate.Criterion;
using NLog;
using QS.Project.Services;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.EntityRepositories.RentPackages;
using Vodovoz.Factories;

namespace Vodovoz
{
	public partial class FreeRentPackageDlg : QS.Dialog.Gtk.EntityDialogBase<FreeRentPackage>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private readonly IRentPackageRepository _rentPackageRepository = ScopeProvider.Scope.Resolve<IRentPackageRepository>();
		private readonly IValidationContextFactory _validationContextFactory = new ValidationContextFactory();

		private ValidationContext _validationContext;

		public FreeRentPackageDlg()
		{
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<FreeRentPackage>();
			TabName = "Новый пакет бесплатной аренды";
			ConfigureDlg();
		}

		public FreeRentPackageDlg(int id)
		{
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<FreeRentPackage>(id);
			ConfigureDlg();
		}

		public FreeRentPackageDlg(FreeRentPackage sub) : this(sub.Id) { }

		private void ConfigureDlg()
		{
			dataentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			spinDeposit.Binding.AddBinding(Entity, e => e.Deposit, w => w.ValueAsDecimal).InitializeFromSource();
			spinMinWaterAmount.Binding.AddBinding(Entity, e => e.MinWaterAmount, w => w.ValueAsInt).InitializeFromSource();

			referenceDepositService.SubjectType = typeof(Nomenclature);
			referenceDepositService.ItemsCriteria = UoW.Session.CreateCriteria<Nomenclature>()
				.Add(Restrictions.Eq("Category", NomenclatureCategory.deposit));
			referenceDepositService.Binding.AddBinding(Entity, e => e.DepositService, w => w.Subject).InitializeFromSource();
			referenceEquipmentKind.SubjectType = typeof(EquipmentKind);
			referenceEquipmentKind.Binding.AddBinding(Entity, e => e.EquipmentKind, w => w.Subject).InitializeFromSource();

			ycheckbuttonArchive.Binding
				.AddBinding(Entity, e => e.IsArchive, w => w.Active)
				.InitializeFromSource();

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

			logger.Info("Сохраняем пакет бесплатной аренды...");
			UoWGeneric.Save();
			return true;
		}
	}
}
