﻿using Microsoft.WindowsAzure.Storage.Table;

namespace SBQWorker {
    public class UserEntity : TableEntity {

        public UserEntity() { }
        public UserEntity(string t, string n) {
            PartitionKey = t;
            RowKey = n;
            Score = 0;
        }

        public int Score { get; set; }
    }
}
