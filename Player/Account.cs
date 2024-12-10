using Blockgame_OpenTK.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.PlayerUtil
{

    public struct AbstractAccountInfo
    {

        [JsonPropertyName("id")]
        public ulong Id { get; set; }
        [JsonPropertyName("hashedid")]
        public string HashedId { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("displayname")]
        public string DisplayName { get; set; }
        [JsonPropertyName("profilepicture")]
        public string ProfilePictureBase64 { get; set; }
    }

    public struct AccountInfo
    {

        public ulong Id;
        public string HashedId;
        public string Username;
        public string DisplayName;
        internal Texture ProfilePicture;

    }
    internal class Account
    {



    }
}
