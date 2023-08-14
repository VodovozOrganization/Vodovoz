using QS.Views.Dialog;
using Vodovoz.Settings.Database;
using Vodovoz.ViewModels.BaseParameters;

namespace Vodovoz.Views.BaseParameters
{
	[WindowSize(600, 600)]
	public partial class BaseParametersView : DialogViewBase<BaseParametersViewModel>
	{
		public BaseParametersView(BaseParametersViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}
		private void Configure()
		{
			if(ViewModel == null)
			{
				return;
			}

			treeParameters.CreateFluentColumnsConfig<Setting>()
				.AddColumn("Название").AddTextRenderer(x => x.Name).Editable()
				.AddColumn("Значение").AddTextRenderer(x => x.StrValue).Editable()
				.AddColumn("")
			.Finish();

			treeParameters.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Settings, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedSetting, v => v.SelectedRow)
				.InitializeFromSource();

			treeParameters.Selection.Changed += (s, e) => {
				buttonDelete.Sensitive = (treeParameters.Selection.CountSelectedRows() > 0);
			};

			buttonAdd.Clicked += (s, e) => ViewModel.AddParameterCommand?.Execute();
			buttonDelete.Clicked += (s, e) => ViewModel.RemoveParameterCommand?.Execute();
			buttonOk.Clicked += (s, e) => ViewModel.SaveParametersCommand?.Execute();
			buttonCancel.Clicked += (s, e) => ViewModel.CancelCommand?.Execute();
		}

		public override void Destroy()
		{
			treeParameters?.Destroy();
			base.Destroy();
		}
	}
}
