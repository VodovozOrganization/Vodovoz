using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Заказ по перемещению подотчетных в ЧЗ товаров между организациями
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "заказы по перемещению подотчетных в ЧЗ товаров между организациями",
		Nominative = "заказ по перемещению подотчетного в ЧЗ товара между организациями")]
	public class TransferOrder : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _date = DateTime.Now;
		private OrganizationEntity _seller;
		private OrganizationEntity _customer;
		private IObservableList<TransferOrderTrueMarkCode> _items = new ObservableList<TransferOrderTrueMarkCode>();
		private DocumentOrganizationCounter _transferDocument;

		protected TransferOrder()
		{
		}

		protected TransferOrder(DateTime date, OrganizationEntity seller, OrganizationEntity customer, DocumentOrganizationCounter transferDocument)
		{
			Date = date;
			Seller = seller;
			Customer = customer;
			TransferDocument = transferDocument;
		}

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
		/// Дата
		/// </summary>
		[Display(Name = "Дата")]
		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		/// <summary>
		/// Продавец
		/// </summary>
		[Display(Name = "Продавец")]
		public virtual OrganizationEntity Seller
		{
			get => _seller;
			set => SetField(ref _seller, value);
		}

		/// <summary>
		/// Покупатель
		/// </summary>
		[Display(Name = "Покупатель")]
		public virtual OrganizationEntity Customer
		{
			get => _customer;
			set => SetField(ref _customer, value);
		}

		[Display(Name = "Description")]
		public virtual IObservableList<TransferOrderTrueMarkCode> Items
		{
			get => _items;
			set => SetField(ref _items, value);
		}
		
		/// <summary>
		/// Счетчик документа трансфера
		/// </summary>
		[Display(Name = "Счетчик документа трансфера")]
		public virtual DocumentOrganizationCounter TransferDocument
		{
			get => _transferDocument;
			set => SetField(ref _transferDocument, value);
		}

		public static Result<TransferOrder> Create(DateTime date, OrganizationEntity seller, OrganizationEntity customer, DocumentOrganizationCounter transferDocument)
		{
			if(date == default)
			{
				return Errors.Edo.TransferOrder.TransferOrderCreateDateMissing;
			}

			if(seller == null)
			{
				return Errors.Edo.TransferOrder.TransferOrderCreateSellerMissing;
			}

			if(customer == null)
			{
				return Errors.Edo.TransferOrder.TransferOrderCreateCustomerMissing;
			}
			
			if(transferDocument == null)
			{
				return Errors.Edo.TransferOrder.TransferOrderDocumentOrganizationCounterMissing;
			}

			return new TransferOrder(date, seller, customer, transferDocument);
		}
	}
}
