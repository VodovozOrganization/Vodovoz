using System;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Orders;

namespace Mindbox.Library.Mapping
{
	/// <summary>
	/// Формирует уникальный идентификатор товара для Mindbox.
	/// Формат: <c>{префикс}_{erpId}</c>.
	/// Префикс зависит от <see cref="SaleItemType"/>:
	/// <list type="bullet">
	/// <item><description>Water, Equipment, Service, Other — <c>product_{erpId}</c>;</description></item>
	/// <item><description>PromoSet — <c>promoset_{erpId}</c>;</description></item>
	/// <item><description>RentPackage — <c>rentpackage_{erpId}</c>.</description></item>
	/// </list>
	/// Для Water, Equipment, Service и Other используется общий префикс <c>product</c>,
	/// поэтому смена категории внутри этих типов не меняет ID в Mindbox.
	/// </summary>
	public static class MindboxItemIdBuilder
	{
		private const string ProductPrefix = "product";
		private const string PromoSetPrefix = "promoset";
		private const string RentPackagePrefix = "rentpackage";

		/// <summary>
		/// Собирает ID товара для Mindbox по типу позиции и её ERP-идентификатору.
		/// </summary>
		/// <param name="itemType">Тип позиции в терминах Mindbox.</param>
		/// <param name="erpId">Идентификатор позиции в ERP.</param>
		/// <returns>ID в формате <c>{префикс}_{erpId}</c>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="erpId"/> не положительный или <paramref name="itemType"/> неизвестен.
		/// </exception>
		public static string Build(SaleItemType itemType, int erpId)
		{
			if(erpId <= 0)
			{
				throw new ArgumentOutOfRangeException(
					nameof(erpId),
					erpId,
					"Идентификатор позиции ERP должен быть положительным.");
			}

			return $"{GetPrefix(itemType)}_{erpId}";
		}

		/// <summary>
		/// Собирает ID номенклатуры для Mindbox.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="nomenclature"/> равен <c>null</c>.</exception>
		public static string Build(Nomenclature nomenclature)
		{
			if(nomenclature is null)
			{
				throw new ArgumentNullException(nameof(nomenclature));
			}

			return Build(nomenclature.Category.ToSaleItemType(), nomenclature.Id);
		}

		/// <summary>
		/// Собирает ID промонабора для Mindbox.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="promotionalSet"/> равен <c>null</c>.</exception>
		public static string Build(PromotionalSet promotionalSet)
		{
			if(promotionalSet is null)
			{
				throw new ArgumentNullException(nameof(promotionalSet));
			}

			return Build(SaleItemType.PromoSet, promotionalSet.Id);
		}

		/// <summary>
		/// Собирает ID пакета аренды для Mindbox.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="freeRentPackage"/> равен <c>null</c>.</exception>
		public static string Build(FreeRentPackage freeRentPackage)
		{
			if(freeRentPackage is null)
			{
				throw new ArgumentNullException(nameof(freeRentPackage));
			}

			return Build(SaleItemType.RentPackage, freeRentPackage.Id);
		}

		/// <summary>
		/// Возвращает префикс ID товара для типа позиции.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Тип позиции неизвестен.</exception>
		public static string GetPrefix(SaleItemType itemType)
		{
			switch(itemType)
			{
				case SaleItemType.Water:
				case SaleItemType.Equipment:
				case SaleItemType.Service:
				case SaleItemType.Other:
					return ProductPrefix;
				case SaleItemType.PromoSet:
					return PromoSetPrefix;
				case SaleItemType.RentPackage:
					return RentPackagePrefix;
				default:
					throw new ArgumentOutOfRangeException(
						nameof(itemType),
						itemType,
						"Неизвестный тип позиции Mindbox.");
			}
		}
	}
}
