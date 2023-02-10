using System.ComponentModel.DataAnnotations;
using Gamma.GtkWidgets;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Factories;

namespace Vodovoz.Dialogs.Sale
{
	public partial class DeliveryPriceRuleDlg : EntityDialogBase<DeliveryPriceRule>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IDistrictRuleRepository _districtRuleRepository = new DistrictRuleRepository();
		private readonly IValidationContextFactory _validationContextFactory = new ValidationContextFactory();

		private ValidationContext _validationContext;

		public DeliveryPriceRuleDlg()
		{
			TabName = "Создание нового правила для цены доставки";
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<DeliveryPriceRule>();
			ConfigureDlg();
		}

		public DeliveryPriceRuleDlg(int id)
		{
			TabName = "Правка правила для цены доставки";
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<DeliveryPriceRule>(id);
			ConfigureDlg();
		}

		public DeliveryPriceRuleDlg(DeliveryPriceRule sub) : this(sub.Id) { }

		void ConfigureDlg()
		{
			ConfigureValidationContext();
			
			spin19LQty.Binding.AddBinding(Entity, e => e.Water19LCount, w => w.ValueAsInt).InitializeFromSource();
			spinOrderMinSumEShopGoods.Binding.AddBinding(Entity, e => e.OrderMinSumEShopGoods, w => w.ValueAsDecimal).InitializeFromSource();
			ylabel6LWater.Binding.AddBinding(Entity, e => e.Water6LCount.ToString(), w => w.LabelProp).InitializeFromSource();
			ylabel1500mlBottles.Binding.AddBinding(Entity, e => e.Water1500mlCount.ToString(), w => w.LabelProp).InitializeFromSource();
			ylabel600mlBottles.Binding.AddBinding(Entity, e => e.Water600mlCount.ToString(), w => w.LabelProp).InitializeFromSource();
			ylabel500mlBottles.Binding.AddBinding(Entity, e => e.Water500mlCount.ToString(), w => w.LabelProp).InitializeFromSource();
			vboxDistricts.Visible = Entity.Id > 0;
			if(Entity.Id > 0) {
				treeDistricts.ColumnsConfig = ColumnsConfigFactory.Create<District>()
					.AddColumn("Правило используется в районах:").AddTextRenderer(d => d.DistrictName)
					.Finish();

				treeDistricts.ItemsDataSource = _districtRuleRepository.GetDistrictsHavingRule(UoW, Entity);
			}
		}

		private void ConfigureValidationContext()
		{
			_validationContext = _validationContextFactory.CreateNewValidationContext(Entity);
			
			_validationContext.ServiceContainer.AddService(typeof(IDistrictRuleRepository), _districtRuleRepository);
		}

		public override bool Save()
		{
			if(!ServicesConfig.ValidationService.Validate(Entity, _validationContext))
			{
				return false;
			}

			logger.Info("Сохраняем правило для цены доставки...");
			UoWGeneric.Save();
			return true;
		}
	}
}
