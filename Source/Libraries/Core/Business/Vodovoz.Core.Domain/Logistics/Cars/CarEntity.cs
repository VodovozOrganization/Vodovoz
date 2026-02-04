using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Common;

namespace Vodovoz.Core.Domain.Logistics.Cars
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "автомобили",
		Nominative = "автомобиль",
		GenitivePlural = "автомобилей")]
	[EntityPermission]
	[HistoryTrace]
	public class CarEntity : PropertyChangedBase, IDomainObject, IBusinessObject, IHasAttachedFilesInformations<CarFileInformation>
	{
		private int _id;
		private string _registrationNumber = string.Empty;
		private bool _isUsedInDelivery;
		private IObservableList<CarFileInformation> _attachedFileInformations = new ObservableList<CarFileInformation>();

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

		[Display(Name = "Государственный номер")]
		public virtual string RegistrationNumber
		{
			get => _registrationNumber;
			set => SetField(ref _registrationNumber, value);
		}

		/// <summary>
		/// Участие авто в доставке
		/// </summary>
		[Display(Name = "Участие авто в доставке")]
		public virtual bool IsUsedInDelivery
		{
			get => _isUsedInDelivery;
			set => SetField(ref _isUsedInDelivery, value);
		}

		[Display(Name = "Информация о прикрепленных файлах")]
		public virtual IObservableList<CarFileInformation> AttachedFileInformations
		{
			get => _attachedFileInformations;
			set => SetField(ref _attachedFileInformations, value);
		}

		public virtual void AddFileInformation(string fileName)
		{
			if(AttachedFileInformations.Any(a => a.FileName == fileName))
			{
				return;
			}

			AttachedFileInformations.Add(new CarFileInformation
			{
				CarId = Id,
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
				fileInformation.CarId = Id;
			}
		}
	}
}
