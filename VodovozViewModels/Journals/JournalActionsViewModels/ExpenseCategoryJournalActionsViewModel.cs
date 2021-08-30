using System;
using System.IO;
using System.Text;
using QS.Commands;
using QS.Dialog;
using QS.Project.Journal.Actions.ViewModels;
using QS.Project.Journal.EntitySelector;
using QS.ViewModels;
using Vodovoz.ViewModels.Journals.JournalNodes;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.Journals.JournalActionsViewModels
{
	public class ExpenseCategoryJournalActionsViewModel : EntitiesJournalActionsViewModel
	{
		private readonly IFileChooserProvider _fileChooserProvider;
		private DelegateCommand _exportDataCommand;
		
		public ExpenseCategoryJournalActionsViewModel(
			IInteractiveService interactiveService,
			IFileChooserProvider fileChooserProvider) : base(interactiveService)
		{
			_fileChooserProvider = fileChooserProvider ?? throw new ArgumentNullException(nameof(fileChooserProvider));
		}
		
		public DelegateCommand ExportDataCommand => _exportDataCommand ?? (_exportDataCommand = new DelegateCommand(
					() =>
					{
						var CSVbuilder = new StringBuilder();
						var items = (JournalTab as IEntityAutocompleteSelector)?.Items;

						if(items == null)
						{
							return;
						}
						
						foreach (ExpenseCategoryJournalNode expenseCategoryJournalNode in items)
						{
							CSVbuilder.Append(expenseCategoryJournalNode.Level1 + ", ");
							CSVbuilder.Append(expenseCategoryJournalNode.Level2 + ", ");
							CSVbuilder.Append(expenseCategoryJournalNode.Level3 + ", ");
							CSVbuilder.Append(expenseCategoryJournalNode.Level4 + ", ");
							CSVbuilder.Append(expenseCategoryJournalNode.Level5 + ", ");
							CSVbuilder.Append(expenseCategoryJournalNode.Subdivision + "\n");
						}

						var fileChooserPath = _fileChooserProvider.GetExportFilePath();
						var res = CSVbuilder.ToString();

						if (fileChooserPath == "")
						{
							return;
						}

						Stream fileStream = new FileStream(fileChooserPath, FileMode.Create);
						
						using (StreamWriter writer = new StreamWriter(fileStream, Encoding.GetEncoding("Windows-1251")))  
						{  
							writer.Write("\"sep=,\"\n");
							writer.Write(res);
						}                 
						_fileChooserProvider.CloseWindow();
					},
				() => true
			)
		);
	}
}