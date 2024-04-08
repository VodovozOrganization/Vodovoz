using QS.DomainModel.Entity;
using System;

namespace Vodovoz.Domain.Fuel
{
	public class FuelTransaction : PropertyChangedBase, IDomainObject
	{
		private string _transactionId;
		private DateTime _transactionDate;
		private string _cardId;
		private string _salePointId;
		private string _productId;
		private string _productCategoryId;
		private int _itemsCount;
		private decimal _pricePerItem;
		private decimal _totalSum;
		private string _cardNumber;

		public virtual int Id { get; set; }

		public virtual string TransactionId
		{
			get => _transactionId;
			set => SetField(ref _transactionId, value);
		}

		public virtual DateTime TransactionDate
		{
			get => _transactionDate;
			set => SetField(ref _transactionDate, value);
		}

		public virtual string CardId
		{
			get => _cardId;
			set => SetField(ref _cardId, value);
		}

		public virtual string SalePointId
		{
			get => _salePointId;
			set => SetField(ref _salePointId, value);
		}

		public virtual string ProductId
		{
			get => _productId;
			set => SetField(ref _productId, value);
		}

		public virtual string ProductCategoryId
		{
			get => _productCategoryId;
			set => SetField(ref _productCategoryId, value);
		}

		public virtual int ItemsCount
		{
			get => _itemsCount;
			set => SetField(ref _itemsCount, value);
		}

		public virtual decimal PricePerItem
		{
			get => _pricePerItem;
			set => SetField(ref _pricePerItem, value);
		}

		public virtual decimal TotalSum
		{
			get => _totalSum;
			set => SetField(ref _totalSum, value);
		}

		public virtual string CardNumber
		{
			get => _cardNumber;
			set => SetField(ref _cardNumber, value);
		}
	}
}
