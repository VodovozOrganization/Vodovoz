using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;

namespace ExportTo1c.Library.Exporters
{
	/// <summary>
	/// Экспорт данных для 1С
	/// </summary>
	public interface IDataExporterFor1c<T>
	{
		XElement CreateXml(
			IList<T> sourceList,
			CancellationToken cancellationToken);
	}
}
