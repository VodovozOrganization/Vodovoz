using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "документ запаса",
		NominativePlural = "документы запаса")]
	[HistoryTrace]
	public class AdditionalLoadingDocument : PropertyChangedBase, IDomainObject
	{
		private DateTime _creationDate;
		private Employee _author;
		private IList<AdditionalLoadingDocumentItem> _items = new List<AdditionalLoadingDocumentItem>();

		public virtual int Id { get; set; }

		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}

		[Display(Name = "Автор")]
		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		[Display(Name = "Строки документа запаса")]
		public virtual IList<AdditionalLoadingDocumentItem> Items
		{
			get => _items;
			set => SetField(ref _items, value);
		}

		private GenericObservableList<AdditionalLoadingDocumentItem> _observableItems;
		public virtual GenericObservableList<AdditionalLoadingDocumentItem> ObservableItems =>
			_observableItems ?? (_observableItems = new GenericObservableList<AdditionalLoadingDocumentItem>(Items));

		public virtual bool HasItemsNeededToLoad => Items.Any(item =>
			!Nomenclature.GetCategoriesNotNeededToLoad().Contains(item.Nomenclature.Category)
			&& !item.Nomenclature.NoDelivery);
	}
}
