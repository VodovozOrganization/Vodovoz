namespace VodovozInfrastructure.Interfaces
{
	public interface IFileChooserProvider
	{
		/// <summary>
		/// Метод для выбора места сохранения. Переданное в метод значение имени превалирует над заданным
		/// </summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		string GetExportFilePath(string filename = null);
		/// <summary>
		/// Метод для выбора папки. Использует пустое название файла
		/// </summary>
		/// <returns></returns>
		string GetExportFolderPath();
		void CloseWindow();
	}
}
