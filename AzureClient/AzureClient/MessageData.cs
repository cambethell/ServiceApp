using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AzureClient {
    [DataContract]
    public class MessageData {
        [DataMember]
        public string Message { get; private set; }
        [DataMember]
        public MessagePurpose Purpose { get; private set; }
        
        public MessageData(string s, MessagePurpose p) {
            Message = (string.IsNullOrEmpty(s)) ? "no message" : s;
            Purpose = p;
        }
    }

    public enum MessagePurpose {
        Update = 1,
        Connect = 2,
        Disconnect = 3,
        Request = 4,
        Initialize = 5
    }
}
