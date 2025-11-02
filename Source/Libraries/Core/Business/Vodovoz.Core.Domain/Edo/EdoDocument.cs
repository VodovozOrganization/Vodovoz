using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public class EdoDocument : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int _edoTaskId;
		private EdoDocumentType _type;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}
		
		[Display(Name = "Код ЭДО задачи")]
		public virtual int EdoTaskId
		{
			get => _edoTaskId;
			set => SetField(ref _edoTaskId, value);
		}

		[Display(Name = "Тип документа")]
		public virtual EdoDocumentType Type
		{
			get => _type;
			set => SetField(ref _type, value);
		}

	}
}
