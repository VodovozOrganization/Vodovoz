using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Vodovoz.RDL.Elements.Base
{
	[Serializable]
	public abstract class BaseElementWithEnumedItems<TElementTypeEnum> : BaseElementWithItems
	{
		private Dictionary<string, TElementTypeEnum> _elementTypes;

		public BaseElementWithEnumedItems()
		{
			_elementTypes = Enum.GetValues(typeof(TElementTypeEnum))
				.Cast<TElementTypeEnum>()
				.ToDictionary(x => x.ToString(), x => x);
		}

		[XmlIgnore]
		public abstract List<TElementTypeEnum> ItemsElementNameList { get; set; }

		/// <summary>
		/// Используется когда все поля перечисленны в массиве, 
		/// с дополнительным массивом енамов типов полей
		/// </summary>
		public TElement GetEnamedItemsValue<TElement>([CallerMemberName] string propertyName = null)
		{
			if(!IsInitialized(propertyName))
			{
				TElement result = default;
				int? fieldIndex = null;

				var fieldType = _elementTypes[propertyName];
				var fieldTypeIndex = ItemsElementNameList.IndexOf(fieldType);
				if(fieldTypeIndex > -1)
				{
					result = (TElement)ItemsList[fieldTypeIndex];
					fieldIndex = fieldTypeIndex;
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
				return (TElement)ItemsList[index.Value];
			}
		}

		/// <summary>
		/// Используется когда все поля перечисленны в массиве, 
		/// с дополнительным массивом енамов типов полей
		/// </summary>
		public void SetEnamedItemsValue<TElement>(TElement value, [CallerMemberName] string propertyName = null)
		{
			var fieldType = _elementTypes[propertyName];

			if(!IsInitialized(propertyName))
			{
				var typeIndex = ItemsElementNameList.IndexOf(fieldType);
				if(typeIndex > -1)
				{
					Initialize(propertyName, typeIndex);
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
				ItemsElementNameList.Add(fieldType);
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
					ItemsElementNameList.RemoveAt(index.Value);
					UpdateIndex(propertyName, null);
				}
			}
		}

		/// <summary>
		/// Используется когда необходимо получить лист елементов,
		/// которые содержатся в промежуточном классе
		/// </summary>
		public IList<TElement> GetEnumedItemsList<TElement, TListHolder>(Func<TListHolder, IList<TElement>> listSelector, [CallerMemberName] string propertyName = null)
			where TListHolder : new()
		{
			TListHolder listHolder = default;
			IList<TElement> result;

			if(!IsInitialized(propertyName))
			{
				var fieldType = _elementTypes[propertyName];
				var fieldTypeIndex = ItemsElementNameList.IndexOf(fieldType);
				if(fieldTypeIndex > -1)
				{
					listHolder = (TListHolder)ItemsList[fieldTypeIndex];
					Initialize(propertyName, fieldTypeIndex);
				}
				else
				{
					listHolder = new TListHolder();
					SetEnamedItemsValue(listHolder, propertyName);
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
