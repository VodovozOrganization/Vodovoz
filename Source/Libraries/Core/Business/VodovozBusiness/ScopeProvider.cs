using Autofac;
using System;

namespace Vodovoz
{
	[Obsolete("Без везкой причины не расширять использование этого класса. Необходимо сокращать зависимости в сущностях")]
	public static class ScopeProvider
	{
		public static ILifetimeScope Scope { get; set; }
	}
}
