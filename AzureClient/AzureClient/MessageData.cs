using SBQWorker;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AzureClient {
    [DataContract]
    public class MessageData {
        [DataMember]
        public string Message { get; private set; }
        [DataMember]
        public UserEntity User { get; private set; }
        [DataMember]
        public MessagePurpose Purpose { get; private set; }
        [DataMember]
        public IEnumerable<UserEntity> Query;

        public MessageData(string u, string s, MessagePurpose p, IEnumerable<UserEntity> q = null) {
            User.PartitionKey = "Player";
            User.RowKey = (string.IsNullOrEmpty(s)) ? "default" : u;
            User.Score = 0;
            Message = (string.IsNullOrEmpty(s)) ? "no message" : s;
            Purpose = p;
            Query = q;
        }
    }

    public enum MessagePurpose {
        Update = 1,
        Connect = 2,
        Disconnect = 3,
        Error = 4,
        Initialize = 5
    }
}
