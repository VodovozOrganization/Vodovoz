using System;
using System.Data;

namespace WhereIsTheBottle.Controls
{
	public class TaggedDataColumn<T> : DataColumn
		where T : Enum
	{
		public TaggedDataColumn(T tag, string columnName) : base(columnName)
		{
			Tag = tag;
		}

		public TaggedDataColumn(T tag, string columnName, Type dataType) : base(columnName, dataType)
		{
			Tag = tag;
		}

		public TaggedDataColumn(T tag, string columnName, Type dataType, string expr) : base(columnName, dataType, expr)
		{
			Tag = tag;
		}

		public TaggedDataColumn(T tag, string columnName, Type dataType, string expr, MappingType type) : base(columnName, dataType, expr,
			type)
		{
			Tag = tag;
		}

		public T Tag { get; set; }
	}
}
