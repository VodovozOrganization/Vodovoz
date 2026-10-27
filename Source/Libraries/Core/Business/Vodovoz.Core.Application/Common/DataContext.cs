using System;
using Vodovoz.Core.Domain.Common;

namespace Vodovoz.Core.Application.Common
{
	/// <inheritdoc/>
	public class DataContext : IDataContext
	{
		private DataContext(object data) =>
			Data = data ?? throw new ArgumentNullException(nameof(data), "Данные для контекста не могут быть пустыми");
		
		/// <inheritdoc/>
		public object Data { get; }

		public static DataContext Create(object data) =>
			new DataContext(data);
	}
}
