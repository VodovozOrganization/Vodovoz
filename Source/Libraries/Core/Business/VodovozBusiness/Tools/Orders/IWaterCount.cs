namespace Vodovoz.Tools.Orders
{
	public interface IWaterCount
	{
		/// <summary>
		/// 19л в неодноразовой таре
		/// </summary>
		decimal NotDisposableWater19LCount { get; }

		/// <summary>
		/// 19л в одноразовой таре
		/// </summary>
		decimal DisposableWater19LCount { get; }

		/// <summary>
		/// 6л в одноразовой таре
		/// </summary>
		decimal DisposableWater6LCount { get; }

		/// <summary>
		/// 1.5л в одноразовой таре
		/// </summary>
		decimal DisposableWater1500mlCount { get; }
		
		/// <summary>
		/// 0.6л в одноразовой таре
		/// </summary>
		decimal DisposableWater600mlCount { get; }

		/// <summary>
		/// 0.5л в одноразовой таре
		/// </summary>
		decimal DisposableWater500mlCount { get; }
	}
}
