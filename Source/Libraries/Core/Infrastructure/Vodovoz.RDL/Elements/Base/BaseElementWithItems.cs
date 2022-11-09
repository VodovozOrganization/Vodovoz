using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Vodovoz.RDL.Elements.Base
{
	[Serializable]
	public abstract class BaseElementWithItems : BaseElement
	{
		[XmlIgnore]
		public abstract List<object> ItemsList { get; set; }

		/// <summary>
		/// Используется когда все поля перечисленны в массиве, 
		/// без дополнительного массива енамов типов полей
		/// </summary>
		public virtual T GetItemsValue<T>([CallerMemberName] string propertyName = null)
		{
			if(!IsInitialized(propertyName))
			{
				T result = default;
				int? fieldIndex = null;

				var field = ItemsList.FirstOrDefault(x => x is T);
				if(field != null)
				{
					result = (T)field;
					fieldIndex = ItemsList.IndexOf(field);
				}

				Initialize(propertyName, fieldIndex);
				return result;
			}

			var index = InitializedPropertyIndexes[propertyName];
			if(index == null)
			{
				return default;
			}
			else
			{
				return (T)ItemsList[index.Value];
			}
		}

		/// <summary>
		/// Используется когда все поля перечисленны в массиве, 
		/// без дополнительного массива енамов типов полей
		/// </summary>
		public virtual void SetItemsValue<T>(T value, [CallerMemberName] string propertyName = null)
		{
			if(!IsInitialized(propertyName))
			{
				var itemIndex = ItemsList.IndexOf(value);
				if(itemIndex > -1)
				{
					Initialize(propertyName, itemIndex);
				}
				else
				{
					Initialize(propertyName, null);
				}
			}

			if(value == null)
			{
				RemoveItemsValue(value, propertyName);
				return;
			}

			var index = InitializedPropertyIndexes[propertyName];
			if(index == null)
			{
				ItemsList.Add(value);
				var newIndex = ItemsList.IndexOf(value);
				UpdateIndex(propertyName, newIndex);
			}
			else
			{
				ItemsList[index.Value] = value;
			}
		}

		private void RemoveItemsValue<T>(T value, string propertyName)
		{
			if(!IsInitialized(propertyName))
			{
				Initialize(propertyName, null);
			}
			else
			{
				var index = InitializedPropertyIndexes[propertyName];
				if(index != null)
				{
					ItemsList.RemoveAt(index.Value);
				}
			}
		}


		/// <summary>
		/// Используется когда необходимо получить лист элементов,
		/// которые содержатся в промежуточном классе
		/// </summary>
		public IList<TElement> GetItemsList<TElement, TListHolder>(Func<TListHolder, IList<TElement>> listSelector, [CallerMemberName] string propertyName = null)
			where TListHolder : new()
		{
			TListHolder listHolder = default;
			IList<TElement> result;

			if(!IsInitialized(propertyName))
			{
				var field = ItemsList.FirstOrDefault(x => x is TListHolder);
				if(field != null)
				{
					listHolder = (TListHolder)field;
					var fieldIndex = ItemsList.IndexOf(field);
					Initialize(propertyName, fieldIndex);
				}
				else
				{
					listHolder = new TListHolder();
					SetItemsValue(listHolder, propertyName);
				}
			}
			else
			{
				var index = InitializedPropertyIndexes[propertyName];
				if(index != null)
				{
					listHolder = (TListHolder)ItemsList[index.Value];
				}
			}

			result = listSelector(listHolder);
			return result;
		}


	}
}
