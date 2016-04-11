using Microsoft.WindowsAzure.Storage.Table;

namespace SBQWorker {
    public class UserEntity : TableEntity {
        public string Message { get; private set; }

        public UserEntity() { }
        public UserEntity(string t, string n) {
            PartitionKey = t;
            RowKey = n;
        }
    }
}
