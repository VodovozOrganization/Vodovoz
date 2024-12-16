using QS.Dialog.Gtk;
using QS.Project.Services;
using Vodovoz.Domain.Store;

namespace Vodovoz.Store
{
	public partial class RegradingOfGoodsTemplateView : EntityDialogBase<RegradingOfGoodsTemplate>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public RegradingOfGoodsTemplateView()
		{
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<RegradingOfGoodsTemplate> ();
			ConfigureDlg ();
		}

		public RegradingOfGoodsTemplateView (int id)
		{
			this.Build ();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<RegradingOfGoodsTemplate> (id);
			ConfigureDlg ();
		}

		public RegradingOfGoodsTemplateView (RegradingOfGoodsTemplate sub) : this (sub.Id)
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
