using System;
using System.Linq;
using System.Xml.Linq;
using Vodovoz.RDL.Elements;
using Vodovoz.RDL.Utilities;

namespace Vodovoz.Reports.Editing.Providers
{
	public class HeaderExpressionRowProvider : ExpressionRowProvider
	{
		private readonly int _serialNumber;

		public HeaderExpressionRowProvider(int serialNumber = 1)
		{
			if(_serialNumber < 1)
			{
				throw new ArgumentException("Порядковый номер строки должен быть больше 1");
			}

			_serialNumber = serialNumber;
		}

		public override TableRow GetExpressionRow(XDocument report, string tableName)
		{
			var ns = report.Root.Attribute("xmlns").Value;
			var rows = report
				.GetTable(tableName, ns)
				.GetHeader(ns)
				.GetTableRows(ns);

			if(!rows.Any())
			{
				throw new InvalidOperationException($"В таблице {tableName} в разделе Header отсутствуют строки");
			}

			if(rows.Elements().Count() < _serialNumber)
			{
				throw new InvalidOperationException($"В таблице {tableName} в разделе Header должно быть требуемое количество строк: {_serialNumber}.");
			}

			var row = rows.Elements().Skip(_serialNumber-1).First();
			var result = row.FromXElement<TableRow>();
			return result;
		}
	}
}
