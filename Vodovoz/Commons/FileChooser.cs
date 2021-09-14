using Gtk;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz
{
    public class FileChooser: FileChooserDialog, IFileChooserProvider
    {
        private FileChooserDialog fileChooser;
		private string _predefinedFileName;

		public FileChooser(string fileName = null)
        {
			_predefinedFileName = fileName;
        }
		
		private void CreateNewFileChooser(string title, FileChooserAction fileChooserAction, params object[] buttonData)
		{
			fileChooser = new FileChooserDialog(title, this, fileChooserAction, buttonData);
			{
				DoOverwriteConfirmation = true;
			}
		}
        
        public string GetExportFilePath(string fileName = null)
        {
            //Создается здесь а не в конструкторе, потому что единственный способ
            //закрыть это destroy
            object[] buttonData = {
	            "Отмена",
	            ResponseType.Cancel,
	            "Сохранить",
	            ResponseType.Accept
            };
            
            CreateNewFileChooser("Выберите где сохранить файл", FileChooserAction.Save, buttonData);

            fileChooser.CurrentName = string.IsNullOrWhiteSpace(fileName) ? _predefinedFileName : fileName;
            
            var result = fileChooser.Run();
           
			if(result == (int)ResponseType.Accept)
			{
				return fileChooser.Filename;
			}
			
			CloseWindow();
	        return "";
        }
        
        public string GetAttachedFileName()
        {
	        object[] buttonData = {
		        "Отмена",
		        ResponseType.Cancel,
		        "Прикрепить",
		        ResponseType.Accept
	        };
	        
	        CreateNewFileChooser("Выберите файл для прикрепления...", FileChooserAction.Open, buttonData);

	        if(fileChooser.Run() == (int)ResponseType.Accept)
	        {
				HideWindow();
				return fileChooser.Filename;
	        }
	        
	        CloseWindow();
	        return "";
        }

        public string GetExportFolderPath()
        {
            //Создается здесь а не в конструкторе, потому что единственный способ
            //закрыть это destroy
            object[] buttonData = {
	            "Отмена",
	            ResponseType.Cancel,
	            "Сохранить",
	            ResponseType.Accept
            };
	        
            CreateNewFileChooser("Выберите где сохранить файл", FileChooserAction.SelectFolder, buttonData);
            
            fileChooser.CurrentName = _predefinedFileName;

            var result = fileChooser.Run();
            if(result == (int)ResponseType.Accept)
            {
                var path = fileChooser.Filename;
                CloseWindow();
                return path;
            }
            
            CloseWindow();
	        return "";
        }

        public void CloseWindow() => fileChooser.Destroy();

        public void HideWindow() => fileChooser.Hide();
    }
}
