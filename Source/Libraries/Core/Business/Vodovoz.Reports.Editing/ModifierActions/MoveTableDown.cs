using System;
using System.Xml.Linq;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class MoveTableDown : ModifierAction
	{
		private readonly string _tableName;
		private readonly double _offsetInPt;

		public MoveTableDown(string tableName, double offsetInPt)
		{
			if(string.IsNullOrWhiteSpace(tableName))
			{
				throw new ArgumentException($"'{nameof(tableName)}' cannot be null or whitespace.", nameof(tableName));
			}
			_tableName = tableName;
			_offsetInPt = offsetInPt;
		}

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;
			report.MoveTableDown(_tableName, @namespace, _offsetInPt);
		}
	}
}

