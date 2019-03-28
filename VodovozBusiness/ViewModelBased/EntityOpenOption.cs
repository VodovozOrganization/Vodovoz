using System;
namespace Vodovoz.ViewModelBased
{
	public interface IEntityOpenOption
	{
		/// <summary>
		/// Необходимо создание новой сущности
		/// </summary>
		/// <value><c>true</c> if create new; otherwise, <c>false</c>.</value>
		bool NeedCreateNew { get; }

		/// <summary>
		/// Id существующей сущности
		/// </summary>
		int EntityId { get; }
	}

	public class EntityOpenOption : IEntityOpenOption
	{
		private EntityOpenOption()
		{
			NeedCreateNew = false;
			EntityId = 0;
		}

		public static EntityOpenOption Create()
		{
			return new EntityOpenOption { NeedCreateNew = true };
		}

		public static EntityOpenOption Open(int entityId)
		{
			return new EntityOpenOption { EntityId = entityId };
		}

		public bool NeedCreateNew { get; private set; }

		public int EntityId { get; private set; }
	}
}
