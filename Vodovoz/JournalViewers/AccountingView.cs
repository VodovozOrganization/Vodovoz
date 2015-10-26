using System;
using QSTDI;
using NLog;
using Vodovoz.ViewModel;
using QSOrmProject;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AccountingView : TdiTabBase, ITdiJournal
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		public AccountingView ()
		{
			this.Build ();
			this.TabName = "Журнал операций по счету";
			accountingFilter.UoW = UnitOfWorkFactory.CreateWithoutRoot ();
			tableAccountingOperations.RepresentationModel = new AccountingVM (accountingFilter);
			tableAccountingOperations.RepresentationModel.UpdateNodes ();
		}
	}
}

