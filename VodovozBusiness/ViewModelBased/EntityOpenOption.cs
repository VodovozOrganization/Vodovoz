namespace Vodovoz.ViewModelBased
{
	public interface IEntityOpenOption
	{
		/// <summary>
		/// Необходимо создание новой сущьности
		/// </summary>
		/// <value><c>true</c> if create new; otherwise, <c>false</c>.</value>
		bool NeedCreateNew { get; }

		/// <summary>
		/// Необходимо использовать дочерний UoW
		/// </summary>
		bool UseChildUoW { get; }

		/// <summary>
		/// Id существующей сущьности
		/// </summary>
		int EntityId { get; }
	}

	public class EntityOpenOption : IEntityOpenOption
	{
		EntityOpenOption()
		{
			NeedCreateNew = false;
			UseChildUoW = false;
			EntityId = 0;
		}

		public bool NeedCreateNew { get; private set; }
		public bool UseChildUoW { get; private set; }
		public int EntityId { get; private set; }

		/// <summary>
		/// Вариант создания диалога, при котором диалог будет создан для создания сущьности
		/// </summary>
		/// <returns>Опция</returns>
		/// <param name="useChildUoW">Если <c>true</c>, то при создании диалога будет использован дочерний UoW</param>
		public static EntityOpenOption Create(bool useChildUoW = false) => new EntityOpenOption { NeedCreateNew = true, UseChildUoW = useChildUoW };

		/// <summary>
		/// Вариант создания диалога, при котором диалог будет создан для существующей сущьности
		/// </summary>
		/// <returns>Опция</returns>
		/// <param name="useChildUoW">Если <c>true</c>, то при создании диалога будет использован дочерний UoW</param>
		public static EntityOpenOption Open(int entityId, bool useChildUoW = false) => new EntityOpenOption { EntityId = entityId, UseChildUoW = useChildUoW };
	}
}
