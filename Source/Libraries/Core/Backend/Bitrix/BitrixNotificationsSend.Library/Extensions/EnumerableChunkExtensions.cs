using System.Collections.Generic;
using System.Linq;

namespace BitrixNotificationsSend.Library.Extensions
{
	/// <summary>
	/// Расширения для разбиения последовательностей на части
	/// </summary>
	internal static class EnumerableChunkExtensions
	{
		/// <summary>
		/// Разбиение последовательности на части не более заданного размера
		/// </summary>
		/// <typeparam name="TItem">Тип элементов</typeparam>
		/// <param name="items">Элементы</param>
		/// <param name="chunkSize">Максимальный размер части</param>
		/// <returns>Части последовательности</returns>
		public static IEnumerable<IList<TItem>> SplitToChunks<TItem>(this IEnumerable<TItem> items, int chunkSize)
		{
			var chunk = new List<TItem>(chunkSize);

			foreach(var item in items)
			{
				chunk.Add(item);

				if(chunk.Count == chunkSize)
				{
					yield return chunk;
					chunk = new List<TItem>(chunkSize);
				}
			}

			if(chunk.Any())
			{
				yield return chunk;
			}
		}
	}
}
