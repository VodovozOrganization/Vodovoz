using FluentNHibernate.Data;
using NHibernate.Criterion;
using QS.Navigation;
using QS.ViewModels.Control.EEVM;
using QS.Views.Dialog;
using System;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Edo;
using Vodovoz.ViewModels.TrueMark;
namespace Vodovoz.Views.Edo
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EdoView : DialogViewBase<EdoViewModel>
	{
		public EdoView(EdoViewModel viewModel) : base(viewModel)
		{
			this.Build();

			ViewModel.EdoInOrderViewModel.OrderCodesViewModel.ConfigureReuseTargetOrderEntry(ViewModel);

			edoinorderview.ViewModel = viewModel.EdoInOrderViewModel;
		}
	}
}
