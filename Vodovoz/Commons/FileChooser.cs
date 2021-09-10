using Gtk;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz
{
    public class FileChooser: Gtk.FileChooserDialog, IFileChooserProvider
    {
        private Gtk.FileChooserDialog fileChooser;
		private string _predefinedFileName;

		public FileChooser(string fileName = null)
        {
			_predefinedFileName = fileName;
        }
        
        public string GetExportFilePath(string fileName = null)
        {
            //Создается здесь а не в конструкторе, потому что единственный способ
            //закрыть это destroy
            fileChooser =
                new Gtk.FileChooserDialog("Выберите где сохранить файл",
                    this,
                    FileChooserAction.Save,
                    "Отмена", ResponseType.Cancel,
                    "Сохранить", ResponseType.Accept)
            {
	            DoOverwriteConfirmation = true
            };

            fileChooser.CurrentName = string.IsNullOrWhiteSpace(fileName) ? _predefinedFileName : fileName;
            
            var result = fileChooser.Run();
           
			if (result == (int)ResponseType.Accept)
			{
				return fileChooser.Filename;
			}
			else
            {
                CloseWindow();
                return "";
            }
        }
        
        public string GetAttachedFileName()
        {
	        fileChooser = new FileChooserDialog(
		        "Выберите файл для прикрепления...",
		        this,
		        FileChooserAction.Open,
		        "Отмена", ResponseType.Cancel,
		        "Прикрепить", ResponseType.Accept);
	        {
		        DoOverwriteConfirmation = true;
	        }

	        if (fileChooser.Run() == (int)ResponseType.Accept)
	        {
				Hide();
				return fileChooser.Filename;
	        }
	        
	        CloseWindow();
	        return "";
        }

        public string GetExportFolderPath()
        {
            //Создается здесь а не в конструкторе, потому что единственный способ
            //закрыть это destroy
            fileChooser =
                new Gtk.FileChooserDialog("Выберите где сохранить файл",
                    this,
                    FileChooserAction.SelectFolder,
                    "Отмена", ResponseType.Cancel,
                    "Сохранить", ResponseType.Accept)
                {
	                DoOverwriteConfirmation = true
                };
            fileChooser.CurrentName = _predefinedFileName;

            var result = fileChooser.Run();
            if (result == (int)ResponseType.Accept)
            {
                var path = fileChooser.Filename;
                CloseWindow();
                return path;
            }
            else
            {
                CloseWindow();
                return "";
            }
        }

        public void CloseWindow() => fileChooser.Destroy();

        public void Hide() => fileChooser.Hide();
    }
}
