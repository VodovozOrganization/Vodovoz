using System.Text;

namespace TaxcomEdoApi.Library.Models.Interfaces
{
	public interface IFileData
	{
		string Name { get; }
		string Path { get; }
		byte[] Image { get; }
		Encoding Encoding { get; }
	}
}
