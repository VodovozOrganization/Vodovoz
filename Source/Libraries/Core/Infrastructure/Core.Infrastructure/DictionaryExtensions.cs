﻿using System;
using System.Collections.Generic;

namespace Core.Infrastructure
{
	public static class DictionaryExtensions
	{
		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
		{
			if (dictionary.TryGetValue(key, out var value))
			{
				return value;
			}

			value = valueFactory(key);
			dictionary.Add(key, value);
			return value;
		}
	}
}
