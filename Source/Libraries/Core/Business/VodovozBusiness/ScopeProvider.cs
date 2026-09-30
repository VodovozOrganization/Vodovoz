using Autofac;
using System;
using System.Threading;

namespace Vodovoz
{
	[Obsolete("НЕ использовать на слое ViewModels. Можно использовать только в старых View(Dialogs).")]
	public static class ScopeProvider
	{
		private static ILifetimeScope _applicationScope;
		private static readonly AsyncLocal<ILifetimeScope> _threadScope = new AsyncLocal<ILifetimeScope>();

		public static ILifetimeScope Scope
		{
			get => _threadScope.Value ?? _applicationScope;
			set => _applicationScope = value;
		}

		/// <summary>
		/// Подменяет ScopeProvider.Scope в текущем потоке (для параллельных фоновых задач).
		/// </summary>
		public static IDisposable BeginThreadScope(ILifetimeScope scope)
		{
			if(scope == null)
			{
				throw new ArgumentNullException(nameof(scope));
			}

			return new ThreadScopeOverride(scope);
		}

		private sealed class ThreadScopeOverride : IDisposable
		{
			private readonly ILifetimeScope _previousScope;
			private bool _disposed;

			public ThreadScopeOverride(ILifetimeScope scope)
			{
				_previousScope = _threadScope.Value;
				_threadScope.Value = scope;
			}

			public void Dispose()
			{
				if(_disposed)
				{
					return;
				}

				_disposed = true;
				_threadScope.Value = _previousScope;
			}
		}
	}
}
