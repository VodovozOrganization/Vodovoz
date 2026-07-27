using EdoNotifications.Application.Factories;
using EdoNotifications.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using TransactionalOutbox.Serialization;
using VodovozInfrastructure.Cryptography;

public class EdoNotificationMessageFactory : IEdoNotificationMessageFactory
{
	private readonly IMD5HexHashFromString _hasher;

	public EdoNotificationMessageFactory(IMD5HexHashFromString hasher)
	{
		_hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
	}

	public EdoNotificationMessage Create(
		EdoNotificationType edoNotificationType,
		params (string Key, string Value)[] templateParams)
	{
		var dict = new Dictionary<string, string>();
		foreach(var (key, value) in templateParams)
		{
			dict[key] = value;
		}

		var sortedParams = dict
			.OrderBy(p => p.Key)
			.ToDictionary(p => p.Key, p => p.Value);

		var jsonString = JsonSerializer.Serialize(sortedParams, OutboxJsonSerializerOptions.Instance);
		var stringToHash = $"Type:{(int)edoNotificationType};Params:{jsonString}";
		var hash = _hasher.GetMD5HexHashFromString(stringToHash);
		var deduplicationKey = $"Event={nameof(EdoNotificationMessage)}:Hash={hash}";

		return new EdoNotificationMessage(edoNotificationType, dict, deduplicationKey);
	}
}
