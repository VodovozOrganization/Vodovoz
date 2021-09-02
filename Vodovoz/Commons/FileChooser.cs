using Gtk;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz
{
    public class FileChooser: Gtk.FileChooserDialog, IFileChooserProvider
    {
        private FileChooserDialog _fileChooser;
        private readonly string _fileName;
        public FileChooser(string fileName = null)
        {
	        _fileName = fileName ?? string.Empty;
        }
        
        public string GetExportFilePath(string fileName = null)
        {
            //Создается здесь а не в конструкторе, потому что единственный способ
            //закрыть это destroy
            _fileChooser =
                new Gtk.FileChooserDialog("Выберите где сохранить файл",
                    this,
                    FileChooserAction.Save,
                    "Отмена", ResponseType.Cancel,
                    "Сохранить", ResponseType.Accept);
            _fileChooser.CurrentName = string.IsNullOrWhiteSpace(fileName) ? _fileName : fileName;
            
            var result = _fileChooser.Run();
            if (result == (int)ResponseType.Accept)
                return _fileChooser.Filename;
            else
            {
                CloseWindow();
                return "";
            }
        }

        public string GetExportFolderPath()
        {
            //Создается здесь а не в конструкторе, потому что единственный способ
            //закрыть это destroy
            _fileChooser =
                new Gtk.FileChooserDialog("Выберите где сохранить файл",
                    this,
                    FileChooserAction.SelectFolder,
                    "Отмена", ResponseType.Cancel,
                    "Сохранить", ResponseType.Accept);
            _fileChooser.CurrentName = string.Empty;

            var result = _fileChooser.Run();
            if (result == (int)ResponseType.Accept)
            {
                var path = _fileChooser.Filename;
                CloseWindow();
                return path;
            }
            else
            {
                CloseWindow();
                return "";
            }
        }

        public void CloseWindow()
        {
            _fileChooser.Destroy();
        }
    }
}
