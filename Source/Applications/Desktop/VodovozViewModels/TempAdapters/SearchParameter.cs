using System;
using System.Linq.Expressions;

namespace Vodovoz.TempAdapters
{
	public enum SearchParametrType
	{
		Id,
		VodovozInternalId,
		Name,
		INN,
		DigitsNumber,
		CompiledAddress
	}

	public class  SearchParameter //<TRootEntity> пока все только для OCunterparty это не нужно
	{
		public SearchParametrType type;
		public Expression<Func<object>> alias;

		public SearchParameter(Expression<Func<object>> alias, SearchParametrType type)
		{
			this.type = type;
			this.alias = alias;
		}

	}
}
