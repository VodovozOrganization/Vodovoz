using System;
using QSTDI;
using NLog;
using Vodovoz.ViewModel;

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
			tableAccountingOperations.RepresentationModel = new AccountingVM ();
			tableAccountingOperations.RepresentationModel.UpdateNodes ();
		}
	}
}

