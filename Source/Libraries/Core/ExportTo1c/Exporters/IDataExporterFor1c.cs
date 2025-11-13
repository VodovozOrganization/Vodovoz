using QS.Dialog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace ExportTo1c.Library.Exporters
{
	/// <summary>
	/// Экспорт данных для 1С
	/// </summary>
	public interface IDataExporterFor1c
	{
		XElement CreateXml(IList<Order> orders, DateTime startOfYesterday, DateTime endOfYesterday, Organization organization,
			CancellationToken cancellationToken, IProgressBarDisplayable progressBarDisplayable = null);
	}
}
