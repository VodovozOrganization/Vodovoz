using System;
using System.Xml.Linq;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class AddTableFilter : ModifierAction
	{
		private readonly string _tableName;
		private readonly string _expression;
		private readonly string _operator;
		private readonly string _value;

		public AddTableFilter(string tableName, string expression, string @operator, string value)
		{
			if(string.IsNullOrWhiteSpace(tableName))
			{
				throw new ArgumentException($"'{nameof(tableName)}' cannot be null or whitespace.", nameof(tableName));
			}

			if(string.IsNullOrWhiteSpace(expression))
			{
				throw new ArgumentException($"'{nameof(expression)}' cannot be null or whitespace.", nameof(expression));
			}

			if(string.IsNullOrWhiteSpace(@operator))
			{
				throw new ArgumentException($"'{nameof(@operator)}' cannot be null or whitespace.", nameof(@operator));
			}

			if(string.IsNullOrWhiteSpace(value))
			{
				throw new ArgumentException($"'{nameof(value)}' cannot be null or whitespace.", nameof(value));
			}

			_tableName = tableName;
			_expression = expression;
			_operator = @operator;
			_value = value;
		}

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;
			report.AddTableFilter(_tableName, _expression, _operator, _value, @namespace);
		}
	}
}



