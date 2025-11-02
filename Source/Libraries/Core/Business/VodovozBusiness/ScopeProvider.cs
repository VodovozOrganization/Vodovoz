using Autofac;
using System;

namespace Vodovoz
{
	[Obsolete("НЕ использовать на слое ViewModels. Можно использовать только в старых View(Dialogs).")]
	public static class ScopeProvider
	{
		public static ILifetimeScope Scope { get; set; }
	}
}
