using Microsoft.WindowsAzure.Storage.Table;
using SBQWorker;
using System.Runtime.Serialization;

namespace AzureClient
{
    [DataContract]
    public class MessageData {
        [DataMember]
        public string Message { get; private set; }
        [DataMember]
        public string User { get; private set; }
        [DataMember]
        public MessagePurpose Purpose { get; private set; }
        [DataMember]
        public TableQuery<UserEntity> Query;

        public MessageData(string u, string s, MessagePurpose p, TableQuery<UserEntity> q = null) {
            User = (string.IsNullOrEmpty(s)) ? "default" : u;
            Message = (string.IsNullOrEmpty(s)) ? "no message" : s;
            Purpose = p;
            Query = q;
        }
    }

    public enum MessagePurpose {
        Update = 1,
        Connect = 2,
        Disconnect = 3,
        Error = 4
    }
}
