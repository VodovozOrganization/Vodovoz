using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Domain
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "сертификаты продукции",
		Nominative = "сертификат продукции")]
	[HistoryTrace]
	[EntityPermission]
	public class CertificateEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private string _name;
		private CertificateType? _typeOfCertificate;
		private byte[] _imageFile;
		private bool _isArchive;
		private DateTime? _expirationDate;
		private DateTime? _startDate = DateTime.Today;
		private IList<NomenclatureEntity> _nomenclatures = new List<NomenclatureEntity>();
		private ObservableList<NomenclatureEntity> _observableNomenclatures;

		public CertificateEntity() 
		{
		}

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Идентификатор")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value, () => Id);
		}

		/// <summary>
		/// Название
		/// </summary>
		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value, () => Name);
		}

		/// <summary>
		/// Тип
		/// </summary>
		[Display(Name = "Тип")]
		public virtual CertificateType? TypeOfCertificate
		{
			get => _typeOfCertificate;
			set => SetField(ref _typeOfCertificate, value, () => TypeOfCertificate);
		}

		/// <summary>
		/// Изображение
		/// </summary>
		[Display(Name = "Изображение")]
		public virtual byte[] ImageFile
		{
			get => _imageFile;
			set => SetField(ref _imageFile, value, () => ImageFile);
		}

		/// <summary>
		/// Архивный
		/// </summary>
		[Display(Name = "Архивный")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value, () => IsArchive);
		}

		/// <summary>
		/// Дата окончания срока действия
		/// </summary>
		[Display(Name = "Дата окончания срока действия")]
		public virtual DateTime? ExpirationDate
		{
			get => _expirationDate;
			set => SetField(ref _expirationDate, value, () => ExpirationDate);
		}

		/// <summary>
		/// Дата начала срока действия либо выдачи
		/// </summary>
		[Display(Name = "Дата начала срока действия либо выдачи")]
		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value, () => StartDate);
		}

		/// <summary>
		/// Отгружаемые номенклатуры
		/// </summary>
		[Display(Name = "Отгружаемые номенклатуры")]
		public virtual IList<NomenclatureEntity> Nomenclatures
		{
			get => _nomenclatures;
			set => SetField(ref _nomenclatures, value, () => Nomenclatures);
		}


		/// <summary>
		/// Наблюдаемый список отгружаемых номенклатур
		/// </summary>
		public virtual ObservableList<NomenclatureEntity> ObservableNomenclatures
		{
			get
			{
				if(_observableNomenclatures == null)
				{
					_observableNomenclatures = new ObservableList<NomenclatureEntity>(Nomenclatures);
				}

				return _observableNomenclatures;
			}
		}
	}
}
