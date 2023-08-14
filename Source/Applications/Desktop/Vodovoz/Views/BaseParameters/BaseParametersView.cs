using QS.Views.Dialog;
using System;
using Vodovoz.Settings.Database;
using Vodovoz.ViewModels.BaseParameters;

namespace Vodovoz.Views.BaseParameters
{
	[WindowSize(400, 400)]
	public partial class BaseParametersView : DialogViewBase<BaseParametersViewModel>
	{
		BaseParametersViewModel _viewModel;
		public BaseParametersView(BaseParametersViewModel viewModel) : base(viewModel)
		{
			this.Build();

			_viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

			Configure();
		}
		private void Configure()
		{
			treeParameters.CreateFluentColumnsConfig<Setting>()
				.AddColumn("Название").AddTextRenderer(x => x.Name).Editable()
				.AddColumn("Значение").AddTextRenderer(x => x.StrValue).Editable()
			.Finish();

			treeParameters.ItemsDataSource = _viewModel.Settings;

			buttonDelete.Sensitive = false;

			treeParameters.Selection.Changed += (sender, e) => {
				buttonDelete.Sensitive = (treeParameters.Selection.CountSelectedRows() > 0);
			};

			buttonAdd.Clicked += (sender, e) => _viewModel.AddParameterCommand?.Execute();
			buttonDelete.Clicked += (s, e) => _viewModel.RemoveParameterCommand?.Execute();
			buttonOk.Clicked += (s, e) => _viewModel.SaveParametersCommand?.Execute();
		}
	}
}
