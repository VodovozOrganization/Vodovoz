using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Controllers
{
	public class OrderDiscountsController : IOrderDiscountsController
	{
		private readonly INomenclatureFixedPriceProvider _fixedPriceProvider;

		public OrderDiscountsController(INomenclatureFixedPriceProvider fixedPriceProvider)
		{
			_fixedPriceProvider = fixedPriceProvider ?? throw new ArgumentNullException(nameof(fixedPriceProvider));
		}

		/// <summary>
		/// Возможность установки скидки на строку заказа
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="orderItem">Строка заказа</param>
		/// <returns>true/false</returns>
		private bool CanSetDiscount(DiscountReason reason, OrderItem orderItem) =>
			IsApplicableDiscount(reason, orderItem.Nomenclature)
			&& orderItem.Price * orderItem.CurrentCount != default(decimal);
		
		/// <summary>
		/// Возможность установки скидки на строку счета без отгрузки на предоплату
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="orderItem">Строка счета без отгрузки на предоплату</param>
		/// <returns>true/false</returns>
		private bool CanSetDiscountForOrderWithoutShipment(DiscountReason reason, OrderWithoutShipmentForAdvancePaymentItem orderItem) =>
			IsApplicableDiscount(reason, orderItem.Nomenclature)
			&& orderItem.Price * orderItem.Count != default(decimal);

		/// <summary>
		/// Содержит ли основание скидки соответствующую категорию номенклатуры 
		/// </summary>
		/// <param name="nomenclatureCategory">Категория номенклатуры</param>
		/// <param name="discountNomenclatureCategories">Список категорий номенклатур у основания скидки</param>
		/// <returns>true/false</returns>
		private bool ContainsNomenclatureCategory(
			NomenclatureCategory nomenclatureCategory, IList<DiscountReasonNomenclatureCategory> discountNomenclatureCategories)
		{
			return discountNomenclatureCategories.Any(x => x.NomenclatureCategory == nomenclatureCategory);
		}
		
		/// <summary>
		/// Содержит ли основание скидки ссылку на указанную номенкалтуру
		/// </summary>
		/// <param name="nomenclatureId">Id номенклатуры</param>
		/// <param name="discountNomenclatures">Список номенклатур основания скидки</param>
		/// <returns>ture/false</returns>
		private bool ContainsNomenclature(int nomenclatureId, IList<Nomenclature> discountNomenclatures) =>
			discountNomenclatures.Any(n => n.Id == nomenclatureId);

		/// <summary>
		/// Содержит ли основание скидки в списке товарную группу строки заказа 
		/// </summary>
		/// <param name="itemProductGroup">Товарная группа строки заказа</param>
		/// <param name="discountProductGroups">Товарные группы основания скидки</param>
		/// <returns>true/false</returns>
		private bool ContainsProductGroup(ProductGroup itemProductGroup, IList<ProductGroup> discountProductGroups) =>
			itemProductGroup != null
			&& discountProductGroups.Any(discountProductGroup => ContainsProductGroup(itemProductGroup, discountProductGroup));
		
		/// <summary>
		/// Проверяет соответствие товарных групп у основания скидки и строки заказа,
		/// с обходом всех ее родительских групп
		/// </summary>
		/// <param name="itemProductGroup">Товарная группа строки заказа</param>
		/// <param name="discountProductGroup">Товарная группа основания скидки</param>
		/// <returns>true/false</returns>
		private bool ContainsProductGroup(ProductGroup itemProductGroup, ProductGroup discountProductGroup)
		{
			while(true)
			{
				if(itemProductGroup == discountProductGroup)
				{
					return true;
				}

				if(itemProductGroup.Parent != null)
				{
					itemProductGroup = itemProductGroup.Parent;
					continue;
				}

				return false;
			}
		}
		
		/// <summary>
		/// Содержит ли строка заказа промонабор или есть фикса
		/// </summary>
		/// <param name="orderItem">Строка заказа</param>
		/// <returns>true/false</returns>
		private bool OrderItemContainsPromoSetOrFixedPrice(OrderItem orderItem)
		{
			if(orderItem == null)
			{
				throw new ArgumentNullException(nameof(orderItem));
			}
			
			if(orderItem.PromoSet != null)
			{
				return true;
			}

			if(orderItem.Order.SelfDelivery)
			{
				if(orderItem.Order.Client != null)
				{
					return _fixedPriceProvider.ContainsFixedPrice(orderItem.Order.Client, orderItem.Nomenclature, orderItem.TotalCountInOrder);
				}
			}
			else
			{
				if(orderItem.Order.DeliveryPoint != null)
				{
					return _fixedPriceProvider.ContainsFixedPrice(orderItem.Order.DeliveryPoint, orderItem.Nomenclature, orderItem.TotalCountInOrder);
				}
			}

			return false;
		}

		/// <summary>
		/// Установка определенной скидки на строку заказа с прикреплением указанного основания скидки,
		/// после проверки возможности этого действия
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="discount">Скидка</param>
		/// <param name="unit">Скидка в процентах или рублях</param>
		/// <param name="orderItem">Строка заказа</param>
		private void SetCustomDiscountForOrderItem(DiscountReason reason, decimal discount, DiscountUnits unit, OrderItem orderItem)
		{
			if(!CanSetDiscount(reason, orderItem))
			{
				return;
			}

			SetCustomDiscount(reason, discount, unit, orderItem);
		}
		
		/// <summary>
		/// Установка определенной скидки на строку заказа с прикреплением указанного основания скидки
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="discount">Скидка</param>
		/// <param name="unit">Скидка в процентах или рублях</param>
		/// <param name="orderItem">Строка заказа</param>
		private void SetCustomDiscount(DiscountReason reason, decimal discount, DiscountUnits unit, OrderItem orderItem)
		{
			orderItem.SetDiscount(unit == DiscountUnits.money, discount, reason);
		}
		
		/// <summary>
		/// Установка скидки из основания скидки на конкретную позицию
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="item">Элемент, к которому применяется скидка</param>
		private void SetDiscount(DiscountReason reason, IDiscount item)
		{
			item.SetDiscount(reason.ValueType == DiscountUnits.money, reason.Value, reason);
		}

		/// <summary>
		/// Удаление скидки из строки заказа
		/// </summary>
		/// <param name="orderItem">Строка заказа</param>
		private void RemoveDiscountFromOrderItem(OrderItem orderItem)
		{
			orderItem.RemoveDiscount();
		}

		/// <summary>
		/// Удаление скидки из заказа
		/// </summary>
		public void RemoveDiscountFromOrder(IList<OrderItem> orderItems)
		{
			foreach(var item in orderItems)
			{
				RemoveDiscountFromOrderItem(item);
			}
		}

		/// <summary>
		/// Устанавливает основание скидки с введенными значениями в рублях или процентах для всего заказа
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="discount">Скидки</param>
		/// <param name="unit">Скидка в процентах или рублях</param>
		/// <param name="orderItems">Список строк заказа</param>
		public void SetCustomDiscountForOrder(DiscountReason reason, decimal discount, DiscountUnits unit, IList<OrderItem> orderItems)
		{
			foreach(var item in orderItems)
			{
				SetCustomDiscountForOrderItem(reason, discount, unit, item);
			}
		}

		/// <summary>
		/// Установка скидки исходя из выбранного основания скидки для всего заказа
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="orderItems">Список строк заказа</param>
		/// <param name="canChangeDiscountValue">Может ли пользователь менять скидку</param>
		/// <param name="messages">Описание позиций на которые не применилась скидка</param>
		public void SetDiscountFromDiscountReasonForOrder(
			DiscountReason reason, IList<OrderItem> orderItems, bool canChangeDiscountValue, out string messages)
		{
			messages = null;
			
			for(var i = 0; i < orderItems.Count; i++)
			{
				SetDiscountFromDiscountReasonForOrderItem(reason, orderItems[i], canChangeDiscountValue, out string message);

				if(message != null)
				{
					messages += $"№{i + 1} {message}";
				}
			}
		}

		/// <summary>
		/// Установка скидки исходя из выбранного основания скидки для строки заказа
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="orderItem">Строка заказа</param>
		/// <param name="canChangeDiscountValue">Может ли пользователь менять скидку</param>
		/// <param name="message">Описание позици на которую не применилась скидка</param>
		/// <returns>true/false - установилась скидка или нет</returns>
		public bool SetDiscountFromDiscountReasonForOrderItem(
			DiscountReason reason, OrderItem orderItem, bool canChangeDiscountValue, out string message)
		{
			message = null;
			
			if(!CanSetDiscount(reason, orderItem))
			{
				return false;
			}
			
			if(!canChangeDiscountValue && OrderItemContainsPromoSetOrFixedPrice(orderItem))
			{
				message = $"{orderItem.Nomenclature.Name}\n";
				return false;
			}

			SetDiscount(reason, orderItem);
			return true;
		}
		
		/// <summary>
		/// Установка скидки исходя из выбранного основания скидки для строки счета без отгрузки на предоплату
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="orderItem">Строка счета без отгрузки на предоплату</param>
		public void SetDiscountFromDiscountReasonForOrderItemWithoutShipment(
			DiscountReason reason, OrderWithoutShipmentForAdvancePaymentItem orderItem)
		{
			if(!CanSetDiscountForOrderWithoutShipment(reason, orderItem))
			{
				return;
			}

			SetDiscount(reason, orderItem);
		}

		/// <summary>
		/// Проверка применимости скидки к номенклатуре, т.е. если выбранное основание скидки содержит номенклатуру,
		/// которая указана в основании скидки, либо основание содержит категорию номенклатуры, либо основание содержит товарную группу
		/// с такой номенклатурой, то возвращаем true
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="nomenclature">Номенклатура</param>
		/// <returns>true/false</returns>
		/// <exception cref="ArgumentNullException">Кидаем ошибку, если основание скидки null</exception>
		public bool IsApplicableDiscount(DiscountReason reason, Nomenclature nomenclature)
		{
			if(reason == null)
			{
				throw new ArgumentNullException(nameof(reason));
			}

			return ContainsNomenclature(nomenclature.Id, reason.Nomenclatures)
				|| ContainsNomenclatureCategory(nomenclature.Category, reason.NomenclatureCategories)
				|| ContainsProductGroup(nomenclature.ProductGroup, reason.ProductGroups);
		}
	}
}
