using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Common;

namespace Vodovoz.Core.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "номенклатуры",
		Nominative = "номенклатура")]
	[EntityPermission]
	[HistoryTrace]
	public class NomenclatureEntity : PropertyChangedBase, IDomainObject, IBusinessObject, INamed, IHasAttachedFilesInformations<NomenclatureFileInformation>
	{
		private int _id;
		private string _name;
		private NomenclatureCategory _category;
		private bool _isAccountableInTrueMark;
		private string _gtin;
		private IObservableList<NomenclatureFileInformation> _attachedFileInformations = new ObservableList<NomenclatureFileInformation>();
		private IObservableList<Gtin> _gtins = new ObservableList<Gtin>();

		public NomenclatureEntity()
		{
			Category = NomenclatureCategory.water;
		}

		public virtual IUnitOfWork UoW { set; get; }

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set
			{
				if(SetField(ref _id, value))
				{
					UpdateFileInformations();
				}
			}
		}

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Категория")]
		public virtual NomenclatureCategory Category
		{
			get => _category;
			//Нельзя устанавливать, см. логику в Nomenclature.cs
			protected set => SetField(ref _category, value);
		}

		[Display(Name = "Подлежит учету в Честном Знаке")]
		public virtual bool IsAccountableInTrueMark
		{
			get => _isAccountableInTrueMark;
			set => SetField(ref _isAccountableInTrueMark, value);
		}

		[Display(Name = "Номер товарной продукции GTIN")]
		public virtual string Gtin
		{
			get => _gtin;
			set => SetField(ref _gtin, value);
		}

		/// <summary>
		/// Gtin
		/// </summary>
		[Display(Name = "Gtin")]
		public virtual IObservableList<Gtin> Gtins
		{
			get => _gtins;
			set => SetField(ref _gtins, value);
		}

		[Display(Name = "Информация о прикрепленных файлах")]
		public virtual IObservableList<NomenclatureFileInformation> AttachedFileInformations
		{
			get => _attachedFileInformations;
			set => SetField(ref _attachedFileInformations, value);
		}

		/// <summary>
		/// Проверка на то, что номенклатура подлежит учету в Честном Знаке и имеет GTIN
		/// </summary>
		public virtual bool IsAccountableInTrueMarkAndHasGtin =>
			IsAccountableInTrueMark
			&& !string.IsNullOrWhiteSpace(Gtin);

		public virtual void AddFileInformation(string fileName)
		{
			if(AttachedFileInformations.Any(a => a.FileName == fileName))
			{
				return;
			}

			AttachedFileInformations.Add(new NomenclatureFileInformation
			{
				NomenclatureId = Id,
				FileName = fileName
			});
		}

		public virtual void RemoveFileInformation(string filename)
		{
			if(!AttachedFileInformations.Any(fi => fi.FileName == filename))
			{
				return;
			}

			AttachedFileInformations.Remove(AttachedFileInformations.First(x => x.FileName == filename));
		}

		private void UpdateFileInformations()
		{
			foreach(var fileInformation in AttachedFileInformations)
			{
				fileInformation.NomenclatureId = Id;
			}
		}
	}
}
