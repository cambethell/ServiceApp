using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBQWorker
{
    public class UserEntity : TableEntity
    {
        public UserEntity(string userType, string userName)
        {
            this.PartitionKey = userType;
            this.RowKey = userName;
        }

        public UserEntity() { }

        public string Message { get; set; }

    }
}
