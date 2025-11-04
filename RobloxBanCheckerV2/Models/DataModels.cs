using System;
using Newtonsoft.Json;

namespace RobloxBanCheckerV2.Models
{
    public class UserInfo
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string UserName { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("robux")]
        public decimal Robux { get; set; }
    }

    public class VoiceStatusResponse
    {
        [JsonProperty("isVoiceEnabled")]
        public bool IsVoiceEnabled { get; set; }

        [JsonProperty("isUserOptIn")]
        public bool IsUserOptIn { get; set; }

        [JsonProperty("isUserEligible")]
        public bool IsUserEligible { get; set; }

        [JsonProperty("isBanned")]
        public bool IsBanned { get; set; }

        [JsonProperty("banReason")]
        public int BanReason { get; set; }

        [JsonProperty("bannedUntil")]
        public RobloxDateTime? BannedUntil { get; set; }

        [JsonProperty("isVerifiedForVoice")]
        public bool IsVerifiedForVoice { get; set; }

        [JsonProperty("denialReason")]
        public int DenialReason { get; set; }
    }

    public class RobloxDateTime
    {
        [JsonProperty("Seconds")]
        public long Seconds { get; set; }

        [JsonProperty("Nanos")]
        public int Nanos { get; set; }

        public DateTime? ToDateTime()
        {
            if (Seconds == 0) return null;
            return DateTimeOffset.FromUnixTimeSeconds(Seconds).DateTime.ToLocalTime();
        }
    }
}