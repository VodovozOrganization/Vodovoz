using System;
using System.Linq;
using Gamma.GtkWidgets;
using Gdk;
using Gtk;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using QS.Validation;
using Vodovoz.Domain.Sale;
using Vodovoz.Repositories.Sale;

namespace Vodovoz.Dialogs.Sale
{
	public partial class CommonDistrictRuleItemsDlg : EntityDialogBase<District>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		CommonDistrictRuleItem[] selectedItems;
		public CommonDistrictRuleItemsDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<District>();
			ConfigureDlg();
		}

		public CommonDistrictRuleItemsDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<District>(id);
			ConfigureDlg();
		}

		public CommonDistrictRuleItemsDlg(District sub) : this(sub.Id) { }

		void ConfigureDlg()
		{
			HasChanges = true;
			var colorRed = new Color(255, 0, 0);
			var colorWhite = new Color(255, 255, 255);
			TabName = "Правила доставки и цены";
			treeRules.ColumnsConfig = ColumnsConfigFactory.Create<CommonDistrictRuleItem>()
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

			treeRules.ItemsDataSource = Entity.ObservableCommonDistrictRuleItems;
			treeRules.Selection.Mode = SelectionMode.Multiple;
			treeRules.Selection.Changed += (s, e) => selectedItems = treeRules.GetSelectedObjects<CommonDistrictRuleItem>();
		}

		protected void OnBtnAddRulesClicked(object sender, EventArgs e)
		{
			var SelectRules = new OrmReference(
				UoW,
				DistrictRuleRepository.GetQueryOverWithAllDeliveryPriceRules()
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
				ruleToAdd => Entity.CommonDistrictRuleItems.All(
					addedRule => addedRule.DeliveryPriceRule.Id != ruleToAdd.Id
				)
			).ToList();
			MainClass.progressBarWin.ProgressAdd();

			onlyNew.ForEach(
				r => Entity.ObservableCommonDistrictRuleItems.Add(
					new CommonDistrictRuleItem {
						DeliveryPrice = 0,
						DeliveryPriceRule = r,
						District = Entity
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
					Entity.ObservableCommonDistrictRuleItems.Remove(i);
			}
		}

		public override bool Save()
		{
			var valid = new QSValidator<District>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			logger.Info("Сохраняем правила и цены доставки...");
			UoWGeneric.Save();
			return true;
		}
	}
}
