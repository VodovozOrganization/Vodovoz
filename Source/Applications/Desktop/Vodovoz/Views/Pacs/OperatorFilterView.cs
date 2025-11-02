using Gamma.GtkWidgets;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Presentation.ViewModels.Pacs.Journals;
using static Vodovoz.Presentation.ViewModels.Pacs.Journals.OperatorFilterViewModel;

namespace Vodovoz.Views.Pacs
{
	[ToolboxItem(true)]
	public partial class OperatorFilterView : FilterViewBase<OperatorFilterViewModel>
	{
		public OperatorFilterView(OperatorFilterViewModel viewModel) : base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			switch(ViewModel.OperatorIsWorkingFilteringMode)
			{
				case OperatorIsWorkingFilteringModeEnum.All:
					yradiobuttonAll.Active = true;
					break;
				case OperatorIsWorkingFilteringModeEnum.Enabled:
					yradiobuttonEnabled.Active = true;
					break;
				case OperatorIsWorkingFilteringModeEnum.Disabled:
					yradiobuttonDisabled.Active = true;
					break;
			}

			foreach(yRadioButton radioButton in yradiobuttonAll.Group)
			{
				radioButton.Toggled += OnOperatorIsWorkingRadioButtonToggled;
			}
		}

		private void OnOperatorIsWorkingRadioButtonToggled(object sender, EventArgs e)
		{
			if(sender is yRadioButton yRadioButton && yRadioButton.Active)
			{
				if(yRadioButton.Name.EndsWith(nameof(OperatorIsWorkingFilteringModeEnum.All)))
				{
					ViewModel.OperatorIsWorkingFilteringMode = OperatorIsWorkingFilteringModeEnum.All;
				}

				if(yRadioButton.Name.EndsWith(nameof(OperatorIsWorkingFilteringModeEnum.Enabled)))
				{
					ViewModel.OperatorIsWorkingFilteringMode = OperatorIsWorkingFilteringModeEnum.Enabled;
				}

				if(yRadioButton.Name.EndsWith(nameof(OperatorIsWorkingFilteringModeEnum.Disabled)))
				{
					ViewModel.OperatorIsWorkingFilteringMode = OperatorIsWorkingFilteringModeEnum.Disabled;
				}
			}
		}

		public override void Destroy()
		{
			foreach(yRadioButton radioButton in yradiobuttonAll.Group)
			{
				radioButton.Toggled -= OnOperatorIsWorkingRadioButtonToggled;
			}

			base.Destroy();
		}
	}
}
