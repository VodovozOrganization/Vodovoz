using System;
using QS.DomainModel.UoW;
using QSOrmProject;
using Vodovoz.Domain.Store;

namespace Vodovoz
{
	public partial class RegradingOfGoodsTemplateDlg : OrmGtkDialogBase<RegradingOfGoodsTemplate>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public RegradingOfGoodsTemplateDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<RegradingOfGoodsTemplate> ();
			ConfigureDlg ();
		}

		public RegradingOfGoodsTemplateDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RegradingOfGoodsTemplate> (id);
			ConfigureDlg ();
		}

		public RegradingOfGoodsTemplateDlg (RegradingOfGoodsTemplate sub) : this (sub.Id)
		{
		}

		void ConfigureDlg ()
		{
			regradingofgoodstemplateitemsview1.TemplateUoW = UoWGeneric;
			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
		}

		public override bool Save ()
		{
			var valid = new QSValidation.QSValidator<RegradingOfGoodsTemplate> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем шаблон пересортицы...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}
	}
}

