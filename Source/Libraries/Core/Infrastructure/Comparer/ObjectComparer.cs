namespace Comparer
{
	/// <summary>
	/// Точка входа для настройки сравнения объектов типа <typeparamref name="T"/> по свойствам.
	/// Выбирает режим по умолчанию и возвращает <see cref="ObjectComparerInstanceBuilder{T}"/>:
	/// <code>
	/// var comparer = new ObjectPropertyComparer&lt;Document&gt;()
	/// 	.DefaultInclude()
	/// 	.Exclude(x => x.Id)
	/// 	.CompareObject(x => x.Address, addressComparer)
	/// 	.CompareObject(x => x.Lines, lineComparer)
	/// 	.Build();
	/// var differences = comparer.Compare(current, sent, formatter);
	/// </code>
	/// </summary>
	public class ObjectPropertyComparer<T>
	{
		/// <summary>
		/// Начинает настройку компаратора, в котором по умолчанию сравниваются все простые свойства.
		/// Лишние свойства убираются через <see cref="ObjectComparerInstanceBuilder{T}.Exclude"/>.
		/// </summary>
		public ObjectComparerInstanceBuilder<T> DefaultInclude()
		{
			return new ObjectComparerInstanceBuilder<T>(DefaultComparisonMode.Include);
		}

		/// <summary>
		/// Начинает настройку компаратора, в котором по умолчанию не сравнивается ни одно свойство.
		/// Нужные свойства добавляются через <see cref="ObjectComparerInstanceBuilder{T}.Include"/>
		/// или CompareObject.
		/// </summary>
		public ObjectComparerInstanceBuilder<T> DefaultExclude()
		{
			return new ObjectComparerInstanceBuilder<T>(DefaultComparisonMode.Exclude);
		}
	}
}
