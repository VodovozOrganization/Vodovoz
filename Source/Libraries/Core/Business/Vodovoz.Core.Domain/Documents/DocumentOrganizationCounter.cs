using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.Documents;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Domain.Documents
{
	public class DocumentOrganizationCounter: PropertyChangedBase, IDomainObject
	{
		private int _id;
		private OrganizationEntity _organization;
		private int _counter;
		private int? _counterDateYear;
		private string _documentNumber;
		private OrderEntity _order;

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
		/// Организация
		/// </summary>
		[Display(Name = "Организация")]
		public virtual OrganizationEntity Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}
		
		/// <summary>
		/// Заказ 
		/// </summary>
		[Display(Name = "Заказ")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		/// <summary>
		/// Счетчик документов УПД
		/// </summary>
		[Display(Name = "Счетчик документов УПД")]
		public virtual int Counter
		{
			get => _counter;
			set => SetField(ref _counter, value);
		}

		/// <summary>
		/// Дата создания счетчика
		/// </summary>
		[Display(Name = "Дата создания счетчика")]
		public virtual int? CounterDateYear
		{
			get => _counterDateYear;
			set => SetField(ref _counterDateYear, value);
		}
		
		/// <summary>
		/// Номер документа подготовленный
		/// </summary>
		[Display(Name = "Номер документа подготовленный")]
		public virtual string DocumentNumber
		{
			get => _documentNumber;
			set => SetField(ref _documentNumber, value);
		}
	}
}
