﻿using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Позиция товара в фискальном документе
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "товар в фискальном документе",
		NominativePlural = "товар в фискальном документе"
	)]
	public class FiscalInventPosition : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private OrderItemEntity _orderitem;
		private string _name;
		private decimal _quantity;
		private decimal _price;
		private decimal _discountSum;
		private FiscalVat _vat;
		private FiscalIndustryRequisiteRegulatoryDocument _regulatoryDocument;
		private string _industryRequisiteData;

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Строка заказа
		/// </summary>
		[Display(Name = "Строка заказа")]
		public virtual OrderItemEntity OrderItem
		{
			get => _orderitem;
			set => SetField(ref _orderitem, value);
		}

		/// <summary>
		/// Наименование товара
		/// </summary>
		[Display(Name = "Наименование")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		/// <summary>
		/// Количество
		/// </summary>
		[Display(Name = "Количество")]
		public virtual decimal Quantity
		{
			get => _quantity;
			set => SetField(ref _quantity, value);
		}

		/// <summary>
		/// Цена
		/// </summary>
		[Display(Name = "Цена")]
		public virtual decimal Price
		{
			get => _price;
			set => SetField(ref _price, value);
		}

		/// <summary>
		/// Сумма скидки
		/// </summary>
		[Display(Name = "Скидка")]
		public virtual decimal DiscountSum
		{
			get => _discountSum;
			set => SetField(ref _discountSum, value);
		}

		/// <summary>
		/// НДС
		/// </summary>
		[Display(Name = "НДС")]
		public virtual FiscalVat Vat
		{
			get => _vat;
			set => SetField(ref _vat, value);
		}

		///// <summary>
		///// Маркировка
		///// </summary>
		//[Display(Name = "Маркировка")]
		//public virtual string ProductMark
		//{
		//	get => _productMark;
		//	set => SetField(ref _productMark, value);
		//}

		private EdoTaskItem _edoTaskItem;
		[Display(Name = "Строка задачи с кодом")]
		public virtual EdoTaskItem EdoTaskItem
		{
			get => _edoTaskItem;
			set => SetField(ref _edoTaskItem, value);
		}


		/// <summary>
		/// Регламентирующий документ
		/// </summary>
		[Display(Name = "Регламентирующий документ")]
		public virtual FiscalIndustryRequisiteRegulatoryDocument RegulatoryDocument
		{
			get => _regulatoryDocument;
			set => SetField(ref _regulatoryDocument, value);
		}

		/// <summary>
		/// Данные об идентификаторе и времени запроса проверки КМ. (Получает с помощью терминала сбора данных)
		/// </summary>
		[Display(Name = "Данные об идентификаторе и времени запроса проверки КМ")]
		public virtual string IndustryRequisiteData
		{
			get => _industryRequisiteData;
			set => SetField(ref _industryRequisiteData, value);
		}


	}
}
