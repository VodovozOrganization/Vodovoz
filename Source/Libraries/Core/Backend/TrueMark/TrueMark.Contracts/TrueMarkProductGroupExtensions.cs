using System;

namespace TrueMark.Contracts
{
	/// <summary>
	/// Преобразование товарной группы в значение, которое ожидает API Честного Знака.
	/// </summary>
	public static class TrueMarkProductGroupExtensions
	{
		/// <summary>
		/// Идентификатор товарной группы для API ЧЗ (параметр pg=).
		/// </summary>
		/// <param name="productGroup">Товарная группа</param>
		/// <returns>Значение для API ЧЗ: water или petfood</returns>
		public static string ToApiValue(this TrueMarkProductGroup productGroup)
		{
			switch(productGroup)
			{
				case TrueMarkProductGroup.Water:
					return "water";
				case TrueMarkProductGroup.PetFood:
					return "petfood";
				default:
					throw new ArgumentOutOfRangeException(
						nameof(productGroup),
						productGroup,
						"Неизвестная товарная группа маркировки");
			}
		}
	}
}
