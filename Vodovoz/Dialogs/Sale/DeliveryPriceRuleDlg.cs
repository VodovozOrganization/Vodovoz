using Gamma.GtkWidgets;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QSValidation;
using Vodovoz.Domain.Sale;
using Vodovoz.Repositories.Sale;

namespace Vodovoz.Dialogs.Sale
{
	public partial class DeliveryPriceRuleDlg : EntityDialogBase<DeliveryPriceRule>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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
			spin19LQty.Binding.AddBinding(Entity, e => e.Water19LCount, w => w.ValueAsInt).InitializeFromSource();
			ylabel6LWater.Binding.AddBinding(Entity, e => e.Water6LCount, w => w.LabelProp).InitializeFromSource();
			ylabel600mlBottles.Binding.AddBinding(Entity, e => e.Water600mlCount, w => w.LabelProp).InitializeFromSource();
			vboxDistricts.Visible = Entity.Id > 0;
			if(Entity.Id > 0) {
				treeDistricts.ColumnsConfig = ColumnsConfigFactory.Create<ScheduleRestrictedDistrict>()
					.AddColumn("Правило используется в районах:").AddTextRenderer(d => d.DistrictName)
					.Finish();

				treeDistricts.ItemsDataSource = ScheduleRestrictedDistrictRuleRepository.GetDistrictsHavingRule(UoW, Entity);
			}
		}

		public override bool Save()
		{
			var valid = new QSValidator<DeliveryPriceRule>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			logger.Info("Сохраняем правило для цены доставки...");
			UoWGeneric.Save();
			return true;
		}
	}
}
