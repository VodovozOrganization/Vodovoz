using System;
using System.Collections.Generic;
using QSHistoryLog;
using Gamma.Utilities;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using System.Linq;

namespace ServiceDialogs.LoadFrom1c
{
	public class ChangedItem
	{
		public string Title { get; set;}
		public List<FieldChange> Fields;

		public static ChangedItem CompareAndChange(Counterparty oldCP, Counterparty newCP)
		{
			if (oldCP == null || newCP == null)
				return null;
			
			var result = new List<FieldChange>();

			if (oldCP.Name != newCP.Name)
			{
				result.Add(new FieldChange("Изменено имя", oldCP.Name, newCP.Name));
				oldCP.Name = newCP.Name;
			}
			if (oldCP.PersonType != newCP.PersonType)
			{
				result.Add(new FieldChange("Изменен тип", oldCP.PersonType.GetEnumTitle(), newCP.PersonType.GetEnumTitle()));
				oldCP.PersonType = newCP.PersonType;
			}
			if (oldCP.PaymentMethod != newCP.PaymentMethod)
			{
				result.Add(new FieldChange("Изменен метод оплаты", oldCP.PaymentMethod.GetEnumTitle(), newCP.PaymentMethod.GetEnumTitle()));
				oldCP.PaymentMethod = newCP.PaymentMethod;
			}
			if (oldCP.Comment != newCP.Comment)
			{
				result.Add(new FieldChange("Изменен комментарий", oldCP.Comment, newCP.Comment));
				oldCP.Comment = newCP.Comment;
			}
			if (oldCP.FullName != newCP.FullName)
			{
				result.Add(new FieldChange("Изменено полное имя", oldCP.FullName, newCP.FullName));
				oldCP.FullName = newCP.FullName;
			}
			if (oldCP.INN != newCP.INN)
			{
				result.Add(new FieldChange("Изменен ИНН", oldCP.INN, newCP.INN));
				oldCP.INN = newCP.INN;
			}
			if (oldCP.KPP != newCP.KPP)
			{
				result.Add(new FieldChange("Изменен КПП", oldCP.KPP, newCP.KPP));
				oldCP.KPP = newCP.KPP;
			}
			if (oldCP.TypeOfOwnership != newCP.TypeOfOwnership)
			{
				result.Add(new FieldChange("Изменена форма собственности", oldCP.TypeOfOwnership, newCP.TypeOfOwnership));
				oldCP.TypeOfOwnership = newCP.TypeOfOwnership;
			}

			if (result.Count > 0)
				return new ChangedItem
				{
					Title = string.Format("Контрагент с кодом {0} и именем {1}", oldCP.Code1c, oldCP.Name),
					Fields = result
				};
			else
				return null;
		}

		public static ChangedItem CompareAndChange(Order oldOrder, Order newOrder)
		{
			if (oldOrder == null || newOrder == null)
				return null;
			string noValue = "Нет значения";

			var result = new List<FieldChange>();

			if (oldOrder.Comment != newOrder.Comment)
			{
				result.Add(new FieldChange("Изменен комментарий",
					oldOrder.Comment ?? noValue,
					newOrder.Comment ?? noValue));
				oldOrder.Comment = newOrder.Comment;
			}
			if (oldOrder.Client.Code1c != newOrder.Client.Code1c)
			{
				result.Add(new FieldChange("Изменен клиент", oldOrder.Client.FullName, newOrder.Client.FullName));
				oldOrder.Client = newOrder.Client;
			}
			if (oldOrder.DeliveryDate != newOrder.DeliveryDate)
			{
				result.Add(new FieldChange("Изменена дата доставки",
					oldOrder.DeliveryDate?.ToString() ?? noValue,
					newOrder.DeliveryDate?.ToString() ?? noValue));
				oldOrder.DeliveryDate = newOrder.DeliveryDate;
			}
			if (oldOrder.DeliverySchedule != newOrder.DeliverySchedule)
			{
				result.Add(new FieldChange("Изменено время доставки",
					oldOrder.DeliverySchedule?.Name ?? noValue,
					newOrder.DeliverySchedule?.Name ?? noValue));
				oldOrder.DeliverySchedule = newOrder.DeliverySchedule;
			}
			if (oldOrder.DeliveryPoint != newOrder.DeliveryPoint)
			{
				result.Add(new FieldChange("Изменена точка доставки",
					oldOrder.DeliveryPoint?.CompiledAddress ?? noValue,
					newOrder.DeliveryPoint?.CompiledAddress ?? noValue));
				oldOrder.DeliveryPoint = newOrder.DeliveryPoint;
			}
			if (oldOrder.Address1c != newOrder.Address1c)
			{
				result.Add(new FieldChange("Изменен адрес из 1С", oldOrder.Address1c, newOrder.Address1c));
				oldOrder.Address1c = newOrder.Address1c;
			}

			List<OrderItem> oldOrderItems = new List<OrderItem>();
			oldOrderItems.AddRange(oldOrder.OrderItems);
			List<OrderItem> newOrderItems = new List<OrderItem>();
			newOrderItems.AddRange(newOrder.OrderItems);

			//Сравнение строк заказов
			foreach (var newItem in newOrderItems) {
				var oldItem = oldOrderItems.FirstOrDefault(oi => oi.Nomenclature.Code1c == newItem.Nomenclature.Code1c);
				if(oldItem== null)
				{
					result.Add(new FieldChange("Добавлена новая строка заказа", "",
						string.Format("Номенклатура \"{0}\", количество {1}, цена {2}, скидка {3}%",
							newItem.Nomenclature.Name, newItem.Count, newItem.Price, newItem.Discount)));
					oldOrder.OrderItems.Add(newItem);
					continue;
				}
				if(oldItem.Count != newItem.Count)
				{
					result.Add(new FieldChange(
						string.Format("Номенклатура \"{0}\". Изменено количество", oldItem.Nomenclature.Name),
						oldItem.Count.ToString(), newItem.Count.ToString()));
					oldItem.Count = newItem.Count;
				}
				if(oldItem.Price != newItem.Price)
				{
					result.Add(new FieldChange(
						string.Format("Номенклатура \"{0}\". Изменена цена", oldItem.Nomenclature.Name),
						oldItem.Price.ToString(), newItem.Price.ToString()));
					oldItem.Price = newItem.Price;
				}
				if(oldItem.Discount != newItem.Discount)
				{
					result.Add(new FieldChange(
						string.Format("Номенклатура \"{0}\". Изменена скидка", oldItem.Nomenclature.Name),
						oldItem.Discount.ToString(), newItem.Discount.ToString()));
					oldItem.Discount = newItem.Discount;
				}
				oldOrderItems.Remove(oldItem);
			}
			foreach (var item in oldOrderItems)
			{
				result.Add(new FieldChange("Удалена строка заказа",
					string.Format("Номенклатура \"{0}\", количество {1}, цена {2}, скидка {3}%",
						item.Nomenclature.Name, item.Count, item.Price, item.Discount),
					""));
				oldOrder.OrderItems.Remove(item);
			}

			if (result.Count > 0)
				return new ChangedItem
				{
					Title = string.Format("Заказ с кодом {0} и номером {1}", oldOrder.Code1c, oldOrder.Id),
					Fields = result
				};
			else
				return null;
		}

	}
}

