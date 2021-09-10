using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Attachments
{
	public class Attachment : PropertyChangedBase, IDomainObject
	{
		private string _fileName;
		private EntityType _entityType;
		private int _entityId;
		private byte[] _byteFile;
		
		public virtual int Id { get; set; }
		
		[Display(Name = "Имя файла")]
		public virtual string FileName
		{
			get => _fileName;
			set => SetField(ref _fileName, value);
		}
		
		[Display(Name = "Тип сущности")]
		public virtual EntityType EntityType
		{
			get => _entityType;
			set => SetField(ref _entityType, value);
		}
		
		[Display(Name = "Id сущности")]
		public virtual int EntityId
		{
			get => _entityId;
			set => SetField(ref _entityId, value);
		}
		
		[Display(Name = "Файл")]
		public virtual byte[] ByteFile
		{
			get => _byteFile;
			set => SetField(ref _byteFile, value);
		}

		public virtual bool Saved => Id > 0;

		public virtual string Title => FileName;
	}

	public enum EntityType
	{
		[Display(Name = "Сотрудник")]
		Employee,
		[Display(Name = "Автомобиль")]
		Car
	}
	
	public class EntityTypeStringType : NHibernate.Type.EnumStringType
	{
		public EntityTypeStringType() : base(typeof(EntityType))
		{
		}
	}
}
