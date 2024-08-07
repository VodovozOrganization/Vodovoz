using System;
namespace Vodovoz.Presentation.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AttachedFileInformationsView : Gtk.Bin
	{
		public AttachedFileInformationsView()
		{
			this.Build();
		}

		//protected override void ConfigureWidget()
		//{
		//	buttonAdd.Clicked += (sender, args) => ViewModel.AddCommand.Execute();
		//	buttonScan.Clicked += (sender, args) => ViewModel.ScanCommand.Execute();
		//	btnOpen.Clicked += (sender, args) => ViewModel.OpenCommand.Execute();
		//	btnSave.Clicked += (sender, args) => ViewModel.SaveCommand.Execute();
		//	btnDelete.Clicked += (sender, args) => ViewModel.DeleteCommand.Execute();

		//	btnOpen.Binding.AddBinding(ViewModel, vm => vm.CanOpen, w => w.Sensitive).InitializeFromSource();
		//	btnSave.Binding.AddBinding(ViewModel, vm => vm.CanSave, w => w.Sensitive).InitializeFromSource();
		//	btnDelete.Binding.AddBinding(ViewModel, vm => vm.CanDelete, w => w.Sensitive).InitializeFromSource();

		//	ConfigureTreeFiles();
		//}

		//private void ConfigureTreeFiles()
		//{
		//	treeFiles.ColumnsConfig = new FluentColumnsConfig<QS.Attachments.Domain.Attachment>()
		//		.AddColumn("Файл").AddTextRenderer(n => n.FileName)
		//		.AddColumn("")
		//		.Finish();

		//	treeFiles.Binding.AddBinding(ViewModel, vm => vm.SelectedAttachment, w => w.SelectedRow).InitializeFromSource();
		//	treeFiles.ItemsDataSource = ViewModel.Attachments;
		//	treeFiles.RowActivated += (sender, args) => ViewModel.OpenCommand.Execute();
		//}
	}
}
