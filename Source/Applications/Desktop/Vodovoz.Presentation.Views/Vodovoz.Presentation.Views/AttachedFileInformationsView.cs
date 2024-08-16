using Gdk;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Presentation.ViewModels.AttachedFiles;

namespace Vodovoz.Presentation.Views
{
	[ToolboxItem(true)]
	public partial class AttachedFileInformationsView : WidgetViewBase<AttachedFileInformationsViewModel>
	{
		private static readonly Pixbuf _emptyImg = new Pixbuf(System.Reflection.Assembly.GetEntryAssembly(), "Vodovoz.icons.common.empty16.png");
		private static readonly Pixbuf _fire = new Pixbuf(System.Reflection.Assembly.GetEntryAssembly(), "Vodovoz.icons.common.fire16.png");

		public AttachedFileInformationsView() : base()
		{
			Build();
		}

		public AttachedFileInformationsView(AttachedFileInformationsViewModel viewModel)
			: base(viewModel)
		{
			Build();
			ConfigureWidget();
		}

		public void InitializeViewModel(AttachedFileInformationsViewModel viewModel)
		{
			if(ViewModel is null)
			{
				ViewModel = viewModel;
			}
			else
			{
				throw new InvalidOperationException("ViewModel уже инициализирована");
			}
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			buttonAdd.Clicked += (sender, args) => ViewModel.AddCommand.Execute();
			buttonScan.Clicked += (sender, args) => ViewModel.ScanCommand.Execute();
			btnOpen.Clicked += (sender, args) => ViewModel.OpenCommand.Execute();
			btnSave.Clicked += (sender, args) => ViewModel.SaveCommand.Execute();
			btnDelete.Clicked += (sender, args) => ViewModel.DeleteCommand.Execute();

			btnOpen.Binding.AddBinding(ViewModel, vm => vm.CanOpen, w => w.Sensitive).InitializeFromSource();
			btnSave.Binding.AddBinding(ViewModel, vm => vm.CanSave, w => w.Sensitive).InitializeFromSource();
			btnDelete.Binding.AddBinding(ViewModel, vm => vm.CanDelete, w => w.Sensitive).InitializeFromSource();

			ConfigureTreeFiles();

			ViewModel.OnFileInformationChanged += OnViewModelFileInformationChanged;
		}

		private void OnViewModelFileInformationChanged(object sender, EventArgs e)
		{
			treeFiles.QueueDraw();
		}

		private void ConfigureTreeFiles()
		{
			treeFiles.CreateFluentColumnsConfig<FileInformation>()
				.AddColumn("").AddPixbufRenderer((node) =>
					ViewModel.FilesMissingOnStorage.Contains(node.FileName)
					? _fire
					: _emptyImg)
				.AddColumn("Файл").AddTextRenderer(n => n.FileName)
				.AddColumn("")
				.Finish();

			treeFiles.Binding.AddBinding(ViewModel, vm => vm.SelectedFile, w => w.SelectedRow).InitializeFromSource();
			treeFiles.ItemsDataSource = ViewModel.FileInformations;
			treeFiles.RowActivated += (sender, args) => ViewModel.OpenCommand.Execute();
		}

		public override void Destroy()
		{
			ViewModel.OnFileInformationChanged -= OnViewModelFileInformationChanged;
			base.Destroy();
		}
	}
}
