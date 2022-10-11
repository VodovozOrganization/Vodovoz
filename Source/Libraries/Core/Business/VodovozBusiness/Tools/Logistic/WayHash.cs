namespace Vodovoz.Tools.Logistic
{
	/// <summary>
	/// Структура храняшая 2 хеша кординат точки отправления и точки прибытия.
	/// </summary>
	public struct WayHash
	{
		public long FromHash;
		public long ToHash;

		public WayHash(long fromHash, long toHash)
		{
			FromHash = fromHash;
			ToHash = toHash;
		}
	}
}
