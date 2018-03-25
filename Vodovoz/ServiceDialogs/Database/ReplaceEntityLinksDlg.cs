using System;
using System.ComponentModel;
using QSOrmProject;
using QSOrmProject.Deletion;
using QSTDI;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ServiceDialogs.Database
{
	[DisplayName("Замена ссылок на объекты")]
	public partial class ReplaceEntityLinksDlg : TdiTabBase
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot();

		int totalLinks = 0;

		public ReplaceEntityLinksDlg()
		{
			this.Build();
			ConfigureDlg();
		}

		public ReplaceEntityLinksDlg(Nomenclature nomenclatureFrom)
		{
			this.Build();
			ConfigureDlg();
			var nom = UoW.GetById<Nomenclature>(nomenclatureFrom.Id);
			entryreference1.Subject = nom;
		}

		private void ConfigureDlg()
		{
			entryreference1.SubjectType = typeof(Nomenclature);
			entryreference2.SubjectType = typeof(Nomenclature);
		}

		void CanRelace()
		{
			buttonReplace.Sensitive = entryreference1.Subject != null && entryreference2.Subject != null && totalLinks > 0;
		}

		protected void OnEntryreference1Changed(object sender, EventArgs e)
		{
			if(entryreference1.Subject != null)
			{
				totalLinks = ReplaceEntity.CalculateTotalLinks(UoW, entryreference1.Subject as Nomenclature);
				labelTotalLinks.LabelProp = String.Format("Найдено {0} ссылок", totalLinks);
			}
			else
			{
				totalLinks = 0;
				labelTotalLinks.LabelProp = String.Empty;
			}
			CanRelace();
		}

		protected void OnEntryreference2Changed(object sender, EventArgs e)
		{
			CanRelace();
		}

		protected void OnButtonReplaceClicked(object sender, EventArgs e)
		{
			var result = ReplaceEntity.ReplaceEverywhere(UoW, entryreference1.Subject as Nomenclature, entryreference2.Subject as Nomenclature);
			UoW.Commit();
			logger.Info("Заменено {0} ссылок.", result);
			entryreference1.Subject = null;
			entryreference2.Subject = null;
		}
	}
}
