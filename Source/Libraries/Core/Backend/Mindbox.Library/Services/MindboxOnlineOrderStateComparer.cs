using System;
using System.Collections.Generic;
using System.Linq;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Сравнение снимка и текущего состояния онлайн-заказа
	/// </summary>
	public static class MindboxOnlineOrderStateComparer
	{
		/// <summary>
		/// Проверяет изменение состава, цены или стоимости доставки
		/// </summary>
		/// <param name="snapshot">Ранее отправленный снимок</param>
		/// <param name="current">Текущее состояние заказа</param>
		/// <returns>Признак изменения состава</returns>
		public static bool HasCompositionChanges(MindboxOnlineOrderState snapshot, MindboxOnlineOrderState current)
		{
			if(snapshot is null || current is null)
			{
				return true;
			}

			if(snapshot.DeliveryCost != current.DeliveryCost
				|| snapshot.TotalPrice != current.TotalPrice
				|| (snapshot.Lines?.Count ?? 0) != (current.Lines?.Count ?? 0))
			{
				return true;
			}

			var snapshotLines = (snapshot.Lines ?? new List<MindboxOnlineOrderStateLine>())
				.ToDictionary(line => line.LineId, StringComparer.Ordinal);

			foreach(var currentLine in current.Lines ?? new List<MindboxOnlineOrderStateLine>())
			{
				if(!snapshotLines.TryGetValue(currentLine.LineId, out var snapshotLine))
				{
					return true;
				}

				if(snapshotLine.ProductId != currentLine.ProductId
					|| snapshotLine.Quantity != currentLine.Quantity
					|| snapshotLine.BasePricePerItem != currentLine.BasePricePerItem)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Проверяет изменение статуса заказа
		/// </summary>
		/// <param name="snapshot">Ранее отправленный снимок</param>
		/// <param name="current">Текущее состояние заказа</param>
		/// <returns>Признак изменения статуса</returns>
		public static bool HasStatusChanges(MindboxOnlineOrderState snapshot, MindboxOnlineOrderState current)
		{
			if(snapshot is null || current is null)
			{
				return false;
			}

			return !string.Equals(snapshot.Status, current.Status, StringComparison.Ordinal);
		}

		/// <summary>
		/// Проверяет наличие любых изменений
		/// </summary>
		/// <param name="snapshot">Ранее отправленный снимок</param>
		/// <param name="current">Текущее состояние заказа</param>
		/// <returns>Признак наличия изменений</returns>
		public static bool HasChanges(MindboxOnlineOrderState snapshot, MindboxOnlineOrderState current)
		{
			return HasCompositionChanges(snapshot, current) || HasStatusChanges(snapshot, current);
		}

		/// <summary>
		/// Возвращает позиции текущего состояния, статус которых отличается от снимка
		/// </summary>
		/// <param name="snapshot">Ранее отправленный снимок</param>
		/// <param name="current">Текущее состояние заказа</param>
		/// <returns>Позиции с изменившимся статусом</returns>
		public static IReadOnlyCollection<MindboxOnlineOrderStateLine> GetLinesWithChangedStatus(
			MindboxOnlineOrderState snapshot,
			MindboxOnlineOrderState current)
		{
			var result = new List<MindboxOnlineOrderStateLine>();

			if(snapshot?.Lines is null || current?.Lines is null)
			{
				return result;
			}

			var snapshotLines = snapshot.Lines
				.ToDictionary(line => line.LineId, StringComparer.Ordinal);

			foreach(var currentLine in current.Lines)
			{
				if(!snapshotLines.TryGetValue(currentLine.LineId, out var snapshotLine)
					|| !string.Equals(snapshotLine.Status, currentLine.Status, StringComparison.Ordinal))
				{
					result.Add(currentLine);
				}
			}

			return result;
		}
	}
}
