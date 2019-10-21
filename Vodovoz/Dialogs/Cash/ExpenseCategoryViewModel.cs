using System;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Dialogs.Cash
{
	public class ExpenseCategoryViewModel : EntityTabViewModelBase<ExpenseCategory>
	{
		public ExpenseCategoryViewModel(IEntityUoWBuilder ctorParam, ICommonServices commonServices) : base(ctorParam, commonServices)
		{
		}
	}
}
