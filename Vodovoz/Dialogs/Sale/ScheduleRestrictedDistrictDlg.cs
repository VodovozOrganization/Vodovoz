using System;
using System.Linq;
using Gamma.GtkWidgets;
using Gdk;
using Gtk;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Sale;
using Vodovoz.Repositories.Sale;

namespace Vodovoz.Dialogs.Sale
{
	public partial class ScheduleRestrictedDistrictDlg : EntityDialogBase<ScheduleRestrictedDistrict>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		ScheduleRestrictedDistrictRuleItem[] selectedItems;
		public ScheduleRestrictedDistrictDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<ScheduleRestrictedDistrict>();
			ConfigureDlg();
		}

		public ScheduleRestrictedDistrictDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<ScheduleRestrictedDistrict>(id);
			ConfigureDlg();
		}

		public ScheduleRestrictedDistrictDlg(ScheduleRestrictedDistrict sub) : this(sub.Id) { }

		void ConfigureDlg()
		{
			UoW.CanCheckIfDirty = false;
			HasChanges = true;
			var colorRed = new Color(255, 0, 0);
			var colorWhite = new Color(255, 255, 255);
			TabName = "Правила доставки и цены";
			treeRules.ColumnsConfig = ColumnsConfigFactory.Create<ScheduleRestrictedDistrictRuleItem>()
				.AddColumn("Правило").AddTextRenderer(p => p.DeliveryPriceRule.ToString())
				.AddColumn("Цена")
			       	.AddNumericRenderer(p => p.DeliveryPrice)
			       	.Digits(2)
			       	.WidthChars(10)
			       	.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
			       	.Editing(true)
					.AddSetter(
						(c, r) => c.BackgroundGdk = r.DeliveryPrice <= 0 ? colorRed : colorWhite
					)
					.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("")
				.Finish();

			treeRules.ItemsDataSource = Entity.ObservableScheduleRestrictedDistrictRuleItems;
			treeRules.Selection.Mode = SelectionMode.Multiple;
			treeRules.Selection.Changed += (s, e) => selectedItems = treeRules.GetSelectedObjects<ScheduleRestrictedDistrictRuleItem>();
		}

		protected void OnBtnAddRulesClicked(object sender, EventArgs e)
		{
			var SelectRules = new OrmReference(
				UoW,
				ScheduleRestrictedDistrictRuleRepository.GetQueryOverWithAllDeliveryPriceRules()
			) {
				Mode = OrmReferenceMode.MultiSelect,
				ButtonMode = QS.Project.Dialogs.ReferenceButtonMode.None
			};
			SelectRules.ObjectSelected += SelectRules_ObjectSelected;
			TabParent.AddSlaveTab(this, SelectRules);
		}

		void SelectRules_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var rulesToAdd = e.GetEntities<DeliveryPriceRule>().ToList();
			logger.Info("Получаем список выбранных правил...");
			MainClass.progressBarWin.ProgressStart(2);
			var onlyNew = rulesToAdd.Where(
				ruleToAdd => Entity.ScheduleRestrictedDistrictRuleItems.All(
					addedRule => addedRule.DeliveryPriceRule.Id != ruleToAdd.Id
				)
			).ToList();
			MainClass.progressBarWin.ProgressAdd();

			onlyNew.ForEach(
				r => Entity.ObservableScheduleRestrictedDistrictRuleItems.Add(
					new ScheduleRestrictedDistrictRuleItem {
						DeliveryPrice = 0,
						DeliveryPriceRule = r,
						ScheduleRestrictedDistrict = Entity
					}
				)
			);

			MainClass.progressBarWin.ProgressAdd();
			logger.Info("Ок");
			MainClass.progressBarWin.ProgressClose();
		}

		protected void OnBtnDeleteRulesClicked(object sender, EventArgs e)
		{
			if(selectedItems != null && selectedItems.Any()) {
				foreach(var i in selectedItems)
					Entity.ObservableScheduleRestrictedDistrictRuleItems.Remove(i);
			}
		}

		public override bool Save()
		{
			var valid = new QSValidator<ScheduleRestrictedDistrict>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			logger.Info("Сохраняем правила и цены доставки...");
			UoWGeneric.Save();
			return true;
		}
	}
}
