using System.IO;
using System.Text;
using TaxcomEdoApi.Library.Models.Interfaces;

namespace TaxcomEdoApi.Library.Models
{
	public sealed class FileData : IFileData
	{
		public void LoadByPath(string path)
		{
			Name = System.IO.Path.GetFileName(path);
			Image = File.ReadAllBytes(path);
		}

		public FileData() { }
		
		public FileData(IFileData fileData)
		{
			Name = fileData.Name;
			Image = fileData.Image;
			Encoding = fileData.Encoding;
		}

		private FileData(
			string fileName,
			string filePath,
			byte[] image,
			Encoding encoding = null)
		{
			Name = fileName;
			Path = filePath;
			Image = image;
			Encoding = encoding;
		}

		//[Required("\"Имя файла\"")]
		public string Name { get; set; }
		
		public string Path { get; set; }

		//[Required("\"Содержимое файла\"")]
		public byte[] Image { get; set; }

		public Encoding Encoding { get; set; }

		public static IFileData Create(
			string fileName,
			string filePath,
			byte[] image,
			Encoding encoding = null) => new FileData(fileName, filePath, image, encoding);
	}
}
