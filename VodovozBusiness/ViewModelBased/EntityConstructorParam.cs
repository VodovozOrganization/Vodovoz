using QS.DomainModel.UoW;
namespace Vodovoz.ViewModelBased
{
	public interface IEntityConstructorParam
	{
		/// <summary>
		/// Необходимо создание новой сущьности
		/// </summary>
		bool IsNewEntity { get; }

		/// <summary>
		/// Id существующей сущьности
		/// </summary>
		int EntityOpenId { get; }

		/// <summary>
		/// Родительский UoW
		/// </summary>
		IUnitOfWork RootUoW { get; }
	}

	public class EntityConstructorParam : IEntityConstructorParam
	{
		EntityConstructorParam()
		{
			IsNewEntity = false;
			RootUoW = null;
			EntityOpenId = 0;
		}

		public bool IsNewEntity { get; private set; }
		public int EntityOpenId { get; private set; }
		public IUnitOfWork RootUoW { get; private set; }

		/// <summary>
		/// Для создания новой сущности
		/// </summary>
		public static EntityConstructorParam ForCreate() => new EntityConstructorParam { IsNewEntity = true };

		/// <summary>
		/// Для открытия новой сущности
		/// </summary>
		public static EntityConstructorParam ForOpen(int entityId) => new EntityConstructorParam { EntityOpenId = entityId };

		/// <summary>
		/// Для создания новой сущности, в дочернем UoW
		/// </summary>
		/// <param name="rootUoW">Родительский UoW</param>
		public static EntityConstructorParam ForCreateInChildUoW(IUnitOfWork rootUoW) => new EntityConstructorParam { IsNewEntity = true, RootUoW = rootUoW };

		/// <summary>
		/// Для открытия новой сущности, в дочернем UoW
		/// </summary>
		/// <param name="rootUoW">Родительский UoW</param>
		public static EntityConstructorParam ForOpenInChildUoW(int entityId, IUnitOfWork rootUoW) => new EntityConstructorParam { EntityOpenId = entityId, RootUoW = rootUoW };
	}
}
