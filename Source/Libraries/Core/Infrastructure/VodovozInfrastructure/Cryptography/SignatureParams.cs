namespace VodovozInfrastructure.Cryptography
{
	/// <summary>
	/// Базовые параметры для генерации итоговой суммы для сущности
	/// Для корректного расчета у потомков класса для всех свойств должен стоять атрибут <see cref="PositionForGenerateSignatureAttribute"/>>
	/// с правильным значением позиции
	/// </summary>
	public abstract class SignatureParams
	{
		public string Sign { get; set; }
		[PositionForGenerateSignature(1)]
		public long ShopId { get; set; }
	}
}
