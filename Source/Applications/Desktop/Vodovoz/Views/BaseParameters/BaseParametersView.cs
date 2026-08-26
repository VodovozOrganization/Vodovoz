using QS.Views.Dialog;
using Vodovoz.Settings.Database;
using Vodovoz.ViewModels.BaseParameters;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.Views.BaseParameters
{
	[WindowSize(800, 600)]
	public partial class BaseParametersView : DialogViewBase<BaseParametersViewModel>
	{
		public BaseParametersView(BaseParametersViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}
		private void Configure()
		{
			if(ViewModel == null)
			{
				return;
			}

			treeParameters.CreateFluentColumnsConfig<Setting>()
				.AddColumn("Название").AddTextRenderer(x => x.Name).WrapWidth(250).WrapMode(WrapMode.WordChar).Editable()
				.AddColumn("Значение").AddTextRenderer(x => x.StrValue).WrapWidth(250).WrapMode(WrapMode.WordChar).Editable()
				.AddColumn("Описание").AddTextRenderer(x => x.Description).WrapWidth(250).WrapMode(WrapMode.WordChar).Editable()
				.AddColumn("")
			.Finish();

			treeParameters.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Settings, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedSetting, v => v.SelectedRow)
				.InitializeFromSource();

			buttonDelete.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedSetting != null, v => v.Sensitive)
				.InitializeFromSource();

			buttonAdd.BindCommand(ViewModel.AddParameterCommand);
			buttonDelete.BindCommand(ViewModel.RemoveParameterCommand);
			buttonOk.BindCommand(ViewModel.SaveParametersCommand);
			buttonCancel.BindCommand(ViewModel.CancelCommand);
		}
	}
}
