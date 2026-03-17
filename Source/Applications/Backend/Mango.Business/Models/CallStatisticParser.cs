using System;
using System.Collections.Generic;
using System.Linq;
using Mango.Business.Interfaces;
using Mango.Contracts.V1.Response;
using Mango.Domain.Entity;
using Mango.Domain.Enums;

namespace Mango.Business.Models
{
    public class CallStatisticParser : ICallStatisticParser
    {
        public List<CallEntity> Parse(CallsResponse response, MangoReferenceData referenceData)
        {
            var result = new List<CallEntity>();

            if (response.Data == null)
            {
	            return result;
            }

            foreach (var day in response.Data.Where(day => day.List != null))
            {
	            result.AddRange(day.List
		            .Select(entry => ParseEntry(entry, referenceData))
		            .Where(analyticsRecord => analyticsRecord != null));
            }

            return result;
        }

        private static CallEntity ParseEntry(CallEntry entry, MangoReferenceData referenceData)
        {
            var entryId = entry.EntryId ?? string.Empty;
            var entryStart = entry.ContextStartTime;
            var entryDuration = entry.Duration;
            var contextStatus = entry.ContextStatus;
            var entryDirection = GetDirectionFromEntry(entry);

            if (entry.ContextCalls == null || entry.ContextCalls.Count == 0)
            {
                return null;
            }

            var candidates = new List<CallCandidate>();

            foreach (var call in entry.ContextCalls)
            {
                CollectCandidates(call, null, candidates);
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            var selected = SelectCandidate(candidates);

            var resolvedGroup = ResolveGroup(selected, referenceData);
            if (resolvedGroup == null)
            {
                return null;
            }

            var directionFromLeg = GetDirectionFromLeg(selected.Call);
            var direction = directionFromLeg == CallDirect.None ? entryDirection : directionFromLeg;

            var startTime = entryStart.HasValue
                ? ToLocalDateTime(entryStart.Value)
                : selected.Call.CallStartTime.HasValue
                    ? ToLocalDateTime(selected.Call.CallStartTime.Value)
                    : (DateTime?)null;

            if (!startTime.HasValue)
            {
	            return null;
            }

            var endTime = selected.Call.CallEndTime.HasValue
                ? ToLocalDateTime(selected.Call.CallEndTime.Value)
                : startTime.Value.AddSeconds(entryDuration ?? 0);

            var answerTime = selected.Call.CallAnswerTime.HasValue
                ? ToLocalDateTime(selected.Call.CallAnswerTime.Value)
                : (DateTime?)null;

            return new CallEntity
            {
                EntryId = entryId,
                GroupName = resolvedGroup.GroupName,
                StartTime = startTime.Value,
                EndTime = endTime,
                AnswerTime = answerTime,
                CallDirect = direction,
                IsMissed = direction == CallDirect.Inbound && contextStatus == 0
            };
        }

        private static CallCandidate SelectCandidate(List<CallCandidate> candidates)
        {
            var answered = candidates
                .Where(x => x.Call.CallAnswerTime.HasValue)
                .OrderBy(x => x.Call.CallAnswerTime ?? long.MaxValue)
                .FirstOrDefault();

            return answered ??
                   candidates
                       .OrderBy(x => x.Call.CallStartTime ?? long.MaxValue)
                       .ThenBy(x => x.Call.CallEndTime ?? long.MaxValue)
                       .ThenBy(x => x.Call.CallAbonentId ?? long.MaxValue)
                       .First();
        }

        private static void CollectCandidates(
            CallNode node,
            string currentGroupName,
            List<CallCandidate> candidates)
        {
            var nodeGroupName = currentGroupName;

            if (string.Equals(node.CallType, "group", StringComparison.OrdinalIgnoreCase))
            {
                nodeGroupName = node.CallAbonentInfo;
            }

            if (node.Members is { Count: > 0 })
            {
                foreach (var member in node.Members)
                {
                    CollectCandidates(member, nodeGroupName, candidates);
                }

                return;
            }

            if (string.Equals(node.CallType, "user", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(node.CallType, "number", StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add(new CallCandidate
                {
                    Call = node,
                    ParentGroupName = nodeGroupName
                });
            }
        }

        private static MangoOperatorReference ResolveGroup(
            CallCandidate selected,
            MangoReferenceData referenceData)
        {
            if (selected.Call.CallAbonentId.HasValue &&
                referenceData.OperatorsById.TryGetValue(selected.Call.CallAbonentId.Value, out var opById))
            {
                return opById;
            }

            var extension = ExtractExtension(selected.Call.CallAbonentNumber);
            if (!string.IsNullOrWhiteSpace(extension) &&
                referenceData.OperatorsByExtension.TryGetValue(extension, out var opByExtension))
            {
                return opByExtension;
            }

            return null;
        }

        private static string ExtractExtension(string abonentNumber)
        {
            if (string.IsNullOrWhiteSpace(abonentNumber))
                return null;

            const string prefix = "sip:";
            if (!abonentNumber.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var value = abonentNumber[prefix.Length..];
            var atIndex = value.IndexOf('@');
            if (atIndex >= 0)
            {
                value = value[..atIndex];
            }

            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static CallDirect GetDirectionFromEntry(CallEntry entry)
        {
            return entry.ContextType switch
            {
                0 => CallDirect.None,
                1 => CallDirect.Inbound,
                2 => CallDirect.Outbound,
                3 => CallDirect.Inner,
                _ => CallDirect.None
            };
        }

        private static CallDirect GetDirectionFromLeg(CallNode leg)
        {
            if (leg.DirectionInbound == true)
                return CallDirect.Inbound;

            if (leg.DirectionOutbound == true)
                return CallDirect.Outbound;

            return CallDirect.None;
        }

        private static DateTime ToLocalDateTime(long unix)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unix)
                .DateTime
                .AddHours(3);
        }

        private sealed class CallCandidate
        {
            public CallNode Call { get; set; } = null!;
            public string ParentGroupName { get; set; }
        }
    }
}
