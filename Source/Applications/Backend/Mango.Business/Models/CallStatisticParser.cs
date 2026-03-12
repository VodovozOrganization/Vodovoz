using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Mango.Business.Interfaces;
using Mango.Domain.Entity;
using Mango.Domain.Enums;

namespace Mango.Business.Models
{
    public class CallStatisticParser : ICallStatisticParser
    {
        public List<CallEntity> Parse(string json)
        {
            var result = new List<CallEntity>();

            using var doc = JsonDocument.Parse(json);
            var data = doc.RootElement.GetProperty("data");

            foreach (var day in data.EnumerateArray())
            {
                foreach (var entry in day.GetProperty("list").EnumerateArray())
                {
                    var analyticsRecord = ParseEntry(entry);
                    if (analyticsRecord != null)
                    {
                        result.Add(analyticsRecord);
                    }
                }
            }

            return result;
        }

        private static CallEntity? ParseEntry(JsonElement entry)
        {
            var entryId = GetString(entry, "entry_id") ?? string.Empty;
            var entryStart = GetNullableInt64(entry, "context_start_time");
            var entryDuration = GetNullableInt32(entry, "duration");
            var contextStatus = GetNullableInt32(entry, "context_status");
            var entryDirection = GetDirectionFromEntry(entry);

            if (!entry.TryGetProperty("context_calls", out var contextCalls) ||
                contextCalls.ValueKind != JsonValueKind.Array ||
                contextCalls.GetArrayLength() == 0)
            {
                if (entryStart == null)
                    return null;

                var start = ToLocalDateTime(entryStart.Value);
                var end = start.AddSeconds(entryDuration ?? 0);

                return new CallEntity
                {
                    EntryId = entryId,
                    GroupName = null,
                    StartTime = start,
                    EndTime = end,
                    AnswerTime = null,
                    CallDirect = entryDirection,
                    IsMissed = entryDirection == CallDirect.Inbound && contextStatus == 0
                };
            }

            var candidates = new List<CallCandidate>();

            foreach (var call in contextCalls.EnumerateArray())
            {
                var parentGroupName = GetString(call, "call_abonent_info");

                if (call.TryGetProperty("members", out var members) &&
                    members.ValueKind == JsonValueKind.Array &&
                    members.GetArrayLength() > 0)
                {
                    foreach (var member in members.EnumerateArray())
                    {
                        if (GetString(member, "call_type") == "user")
                        {
                            candidates.Add(new CallCandidate
                            {
                                Call = member,
                                ParentGroupName = parentGroupName
                            });
                        }
                    }
                }
                else
                {
                    var callType = GetString(call, "call_type");
                    if (callType == "number" || callType == "user")
                    {
                        candidates.Add(new CallCandidate
                        {
                            Call = call,
                            ParentGroupName = parentGroupName
                        });
                    }
                }
            }

            if (candidates.Count == 0)
            {
                if (entryStart == null)
                    return null;

                var start = ToLocalDateTime(entryStart.Value);
                var end = start.AddSeconds(entryDuration ?? 0);

                return new CallEntity
                {
                    EntryId = entryId,
                    GroupName = null,
                    StartTime = start,
                    EndTime = end,
                    AnswerTime = null,
                    CallDirect = entryDirection,
                    IsMissed = entryDirection == CallDirect.Inbound && contextStatus == 0
                };
            }

            var answered = candidates
                .Where(x => HasAnswerTime(x.Call))
                .OrderBy(x => GetAnswerTimeOrMax(x.Call))
                .FirstOrDefault();

            CallCandidate selected;

            if (answered != null)
            {
                selected = answered;
            }
            else
            {
                selected = candidates
                    .OrderBy(x => GetStartTimeOrMax(x.Call))
                    .ThenBy(x => GetEndTimeOrMax(x.Call))
                    .ThenBy(x => GetAbonentIdOrMax(x.Call))
                    .First();
            }

            var directionFromLeg = GetDirectionFromLeg(selected.Call);
            var direction = directionFromLeg == CallDirect.None ? entryDirection : directionFromLeg;

            var startTime = entryStart.HasValue
                ? ToLocalDateTime(entryStart.Value)
                : ToLocalDateTime(GetStartTimeOrMax(selected.Call));

            var endUnix = GetNullableInt64(selected.Call, "call_end_time");
            var endTime = endUnix.HasValue
                ? ToLocalDateTime(endUnix.Value)
                : startTime.AddSeconds(entryDuration ?? 0);

            var answerUnix = GetNullableInt64(selected.Call, "call_answer_time");
            var answerTime = answerUnix.HasValue
                ? ToLocalDateTime(answerUnix.Value)
                : (DateTime?)null;

            return new CallEntity
            {
                EntryId = entryId,
                GroupName = selected.ParentGroupName,
                StartTime = startTime,
                EndTime = endTime,
                AnswerTime = answerTime,
                CallDirect = direction,
                IsMissed = direction == CallDirect.Inbound && contextStatus == 0
            };
        }

        private static CallDirect GetDirectionFromEntry(JsonElement entry)
        {
            var initType = GetNullableInt32(entry, "context_type");

            return initType switch
            {
                0 => CallDirect.None,
                1 => CallDirect.Inbound,
                2 => CallDirect.Outbound,
                3 => CallDirect.Inner,
                _ => CallDirect.None
            };
        }

        private static CallDirect GetDirectionFromLeg(JsonElement leg)
        {
            if (leg.TryGetProperty("DirectionInbound", out var inboundProp) &&
                inboundProp.ValueKind == JsonValueKind.True)
                return CallDirect.Inbound;

            if (leg.TryGetProperty("DirectionOutbound", out var outboundProp) &&
                outboundProp.ValueKind == JsonValueKind.True)
                return CallDirect.Outbound;

            return CallDirect.None;
        }

        private static bool HasAnswerTime(JsonElement member)
        {
            return member.TryGetProperty("call_answer_time", out var prop) &&
                   prop.ValueKind != JsonValueKind.Null;
        }

        private static long GetAnswerTimeOrMax(JsonElement member)
        {
            return GetNullableInt64(member, "call_answer_time") ?? long.MaxValue;
        }

        private static long GetStartTimeOrMax(JsonElement member)
        {
            return GetNullableInt64(member, "call_start_time") ?? long.MaxValue;
        }

        private static long GetEndTimeOrMax(JsonElement member)
        {
            return GetNullableInt64(member, "call_end_time") ?? long.MaxValue;
        }

        private static long GetAbonentIdOrMax(JsonElement member)
        {
            return GetNullableInt64(member, "call_abonent_id") ?? long.MaxValue;
        }

        private static long? GetNullableInt64(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var prop))
                return null;

            if (prop.ValueKind == JsonValueKind.Null)
                return null;

            if (prop.ValueKind == JsonValueKind.Number)
                return prop.GetInt64();

            if (prop.ValueKind == JsonValueKind.String &&
                long.TryParse(prop.GetString(), out var value))
                return value;

            return null;
        }

        private static int? GetNullableInt32(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var prop))
                return null;

            if (prop.ValueKind == JsonValueKind.Null)
                return null;

            if (prop.ValueKind == JsonValueKind.Number)
                return prop.GetInt32();

            if (prop.ValueKind == JsonValueKind.String &&
                int.TryParse(prop.GetString(), out var value))
                return value;

            return null;
        }

        private static string? GetString(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var prop))
                return null;

            return prop.ValueKind == JsonValueKind.Null ? null : prop.GetString();
        }

        private static DateTime ToLocalDateTime(long unix)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unix)
                .DateTime.AddHours(3);
        }

        private sealed class CallCandidate
        {
            public JsonElement Call { get; set; }
            public string? ParentGroupName { get; set; }
        }
    }
}
