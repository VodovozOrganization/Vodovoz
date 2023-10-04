using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mango.Api.Dto
{
	public abstract class EventRequestBase<TEvent> : EventRequestBase
		where TEvent : class
	{
		private string _json;

		public override string Json
		{
			get => _json;
			set
			{
				_json = value;
				if(!string.IsNullOrWhiteSpace(_json))
				{
					Event = JsonSerializer.Deserialize<TEvent>(_json);
				}
			}
		}

		[JsonIgnore]
		public TEvent Event { get; private set; }
	}
}
