﻿using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Validation;
using Vodovoz.Domain.Store;

namespace Vodovoz
{
	public partial class RegradingOfGoodsTemplateDlg : QS.Dialog.Gtk.EntityDialogBase<RegradingOfGoodsTemplate>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public RegradingOfGoodsTemplateDlg()
		{
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<RegradingOfGoodsTemplate> ();
			ConfigureDlg ();
		}

		public RegradingOfGoodsTemplateDlg (int id)
		{
			this.Build ();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<RegradingOfGoodsTemplate> (id);
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
			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			logger.Info ("Сохраняем шаблон пересортицы...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}
	}
}

