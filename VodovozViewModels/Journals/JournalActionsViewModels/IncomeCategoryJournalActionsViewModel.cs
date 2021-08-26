using System;
using System.IO;
using System.Text;
using QS.Commands;
using QS.Dialog;
using QS.Project.Journal.EntitySelector;
using QS.ViewModels;
using Vodovoz.ViewModels.Journals.JournalNodes;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.Journals.JournalActionsViewModels
{
	public class IncomeCategoryJournalActionsViewModel : EntitiesJournalActionsViewModel
	{
		private readonly IFileChooserProvider _fileChooserProvider;
		private DelegateCommand _exportDataCommand;
		
		public IncomeCategoryJournalActionsViewModel(
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
						
						foreach (IncomeCategoryJournalNode incomeCategoryJournalNode in items)
						{
							CSVbuilder.Append(incomeCategoryJournalNode.Level1 + ", ");
							CSVbuilder.Append(incomeCategoryJournalNode.Level2 + ", ");
							CSVbuilder.Append(incomeCategoryJournalNode.Level3 + ", ");
							CSVbuilder.Append(incomeCategoryJournalNode.Level4 + ", ");
							CSVbuilder.Append(incomeCategoryJournalNode.Level5 + ", ");
							CSVbuilder.Append(incomeCategoryJournalNode.Subdivision + "\n");
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