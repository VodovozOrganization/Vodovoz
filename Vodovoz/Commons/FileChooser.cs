using Gtk;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz
{
    public class FileChooser: Gtk.FileChooserDialog, IFileChooserProvider
    {
        private Gtk.FileChooserDialog fileChooser;
        private string fileName;
        public FileChooser(string fileName)
        {
            this.fileName = fileName;
        }
        
        public string GetExportFilePath()
        {
            //Создается здесь а не в конструкторе, потому что единственный способ
            //закрыть это destroy
            fileChooser =
                new Gtk.FileChooserDialog("Выберите где сохранить файл",
                    this,
                    FileChooserAction.Save,
                    "Отмена", ResponseType.Cancel,
                    "Сохранить", ResponseType.Accept);
            fileChooser.CurrentName = fileName;
            
            var result = fileChooser.Run();
            if (result == (int)ResponseType.Accept)
                return fileChooser.Filename;
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
            fileChooser =
                new Gtk.FileChooserDialog("Выберите где сохранить файл",
                    this,
                    FileChooserAction.SelectFolder,
                    "Отмена", ResponseType.Cancel,
                    "Сохранить", ResponseType.Accept);
            fileChooser.CurrentName = fileName;

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

        public void CloseWindow()
        {
            fileChooser.Destroy();
        }
    }
}