using System;
using QS.Views.Dialog;
using Vodovoz.ViewModels.MangoViewModel;

namespace Vodovoz.Views.Mango
{
	public partial class InternalCallView : DialogViewBase<InternalCallViewModel>
	{
		public InternalCallView(InternalCallViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}
	}
}
