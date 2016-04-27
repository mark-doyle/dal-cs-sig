using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Protocol;
using RWS.Lib.Configuration;
using RWS.Lib.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWS.Lib.Storage
{
    public class TableStorageService<T> where T : class, ITableEntity, new()
    {
        #region Declarations

        private string _tableName;

        #endregion // Declarations

        #region Constructors

        public TableStorageService()
        {
            this._tableName = typeof(T).Name;

            CloudTable table = CloudTableClient.GetTableReference(this._tableName);
            table.CreateIfNotExists();
        }

        public TableStorageService(string tableName)
        {
            this._tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;

            CloudTable table = CloudTableClient.GetTableReference(this._tableName);
            table.CreateIfNotExists();
        }

        #endregion // Constructors

        #region Private Methods

        private void _BatchUpsert(IEnumerable<T> entities)
        {
            if (entities != null)
            {
                var partitions = entities.GroupBy(entity => entity.PartitionKey);
                foreach (var partitionGroup in partitions)
                {
                    IEnumerable<T> batch = null;
                    int pass = 0;
                    int actualResults = 0;
                    int batchSize = 100;
                    var cloudTable = CloudTableClient.GetTableReference(_tableName);

                    do
                    {
                        actualResults = 0;
                        int skip = pass * batchSize;
                        batch = partitionGroup.Skip(skip).Take(batchSize);
                        if (batch.Any())
                        {
                            TableBatchOperation batchOperation = new TableBatchOperation();

                            foreach (var item in batch)
                            {
                                if (item != null)
                                {
                                    item.ETag = "*";
                                    batchOperation.InsertOrReplace(item);
                                    actualResults++;
                                }
                            }

                            cloudTable.ExecuteBatch(batchOperation);
                        }
                        pass++;
                    }
                    while (actualResults == batchSize);
                }
            }
        }

        private async Task _BatchUpsertAsync(IEnumerable<T> entities)
        {
            if (entities != null)
            {
                await Task.Run(() =>
                {
                    var partitions = entities.GroupBy(entity => entity.PartitionKey);
                    foreach (var partitionGroup in partitions)
                    {
                        IEnumerable<T> batch = null;
                        int pass = 0;
                        int actualResults = 0;
                        int batchSize = 100;
                        var cloudTable = CloudTableClient.GetTableReference(_tableName);

                        do
                        {
                            actualResults = 0;
                            int skip = pass * batchSize;
                            batch = partitionGroup.Skip(skip).Take(batchSize);
                            if (batch.Any())
                            {
                                TableBatchOperation batchOperation = new TableBatchOperation();

                                foreach (var item in batch)
                                {
                                    if (item != null)
                                    {
                                        item.ETag = "*";
                                        batchOperation.InsertOrReplace(item);
                                        actualResults++;
                                    }
                                }

                                cloudTable.ExecuteBatch(batchOperation);
                            }
                            pass++;
                        }
                        while (actualResults == batchSize);
                    }
                });
            }
        }

        private List<T> GetEntities(CloudTable cloudTable, TableQuery<T> query)
        {
            TableContinuationToken token = null;
            List<T> results = null;
            do
            {
                var segment = cloudTable.ExecuteQuerySegmented(query, token);
                if (segment != null && segment.Results != null)
                {
                    token = segment.ContinuationToken;
                    if (results == null)
                    {
                        results = new List<T>();
                    }
                    results.AddRange(segment.Results);
                }
                else
                {
                    token = null;
                }
            }
            while (token != null && !string.IsNullOrEmpty(token.NextPartitionKey) && !string.IsNullOrEmpty(token.NextRowKey));
            return results;
        }

        private async Task<List<T>> GetEntitiesAsync(CloudTable cloudTable, TableQuery<T> query)
        {
            TableContinuationToken token = null;
            List<T> results = null;
            do
            {
                var segment = await cloudTable.ExecuteQuerySegmentedAsync(query, token);
                if (segment != null && segment.Results != null)
                {
                    token = segment.ContinuationToken;
                    if (results == null)
                    {
                        results = new List<T>();
                    }
                    results.AddRange(segment.Results);
                }
                else
                {
                    token = null;
                }
            }
            while (token != null && !string.IsNullOrEmpty(token.NextPartitionKey) && !string.IsNullOrEmpty(token.NextRowKey));
            return results;
        }

        #endregion // Private Methods

        #region Protected Static Methods

        protected static CloudStorageAccount StorageAccount
        {
            get
            {
                string config = ConfigurationReader.GetInstance().GetStringProperty(ConfigurationKeys.ConnectionStrings.AzureStorage);
                return (string.IsNullOrEmpty(config) ? CloudStorageAccount.DevelopmentStorageAccount : CloudStorageAccount.Parse(config));
            }
        }

        protected static CloudBlobClient CloudBlobClient
        {
            get
            {
                return new CloudBlobClient(StorageAccount.BlobEndpoint, StorageAccount.Credentials);
            }
        }

        protected static CloudTableClient CloudTableClient
        {
            get
            {
                return StorageAccount.CreateCloudTableClient();
            }
        }

        protected static CloudQueueClient CloudQueueClient
        {
            get
            {
                return StorageAccount.CreateCloudQueueClient();
            }
        }

        #endregion // Protected Static Methods

        #region Public Methods

        public virtual T GetEntity(string partitionKey, string rowKey)
        {
            var cloudTable = CloudTableClient.GetTableReference(_tableName);
            var query = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var result = cloudTable.Execute(query);

            return result.Result as T;
        }

        public virtual async Task<T> GetEntityAsync(string partitionKey, string rowKey)
        {
            var cloudTable = CloudTableClient.GetTableReference(_tableName);
            var query = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var result = await cloudTable.ExecuteAsync(query);

            return result.Result as T;
        }

        public virtual IEnumerable<T> GetEntities(string partitionKey)
        {
            var cloudTable = CloudTableClient.GetTableReference(_tableName);
            var partitionKeyFilter = TableQuery.GenerateFilterCondition(TableConstants.PartitionKey, QueryComparisons.Equal, partitionKey);
            var query = new TableQuery<T>().Where(partitionKeyFilter);
            var entities = this.GetEntities(cloudTable, query);

            return entities;

        }

        public virtual async Task<IEnumerable<T>> GetEntitiesAsync(string partitionKey)
        {
            var cloudTable = CloudTableClient.GetTableReference(_tableName);
            var partitionKeyFilter = TableQuery.GenerateFilterCondition(TableConstants.PartitionKey, QueryComparisons.Equal, partitionKey);
            var query = new TableQuery<T>().Where(partitionKeyFilter);
            var entities = await this.GetEntitiesAsync(cloudTable, query);

            return entities;
        }

        public virtual IEnumerable<T> GetEntities(string partitionKey, IEnumerable<string> rowKeys)
        {
            return rowKeys.Select(rowKey => GetEntity(partitionKey, rowKey));
        }

        public virtual async Task<IEnumerable<T>> GetEntitiesAsync(string partitionKey, IEnumerable<string> rowKeys)
        {
            return await Task.WhenAll(
                rowKeys.Select(rowKey => GetEntityAsync(partitionKey, rowKey))
                );
        }

        public virtual IEnumerable<T> GetEntities(string partitionKey, string minRowKey, string maxRowKey)
        {
            var cloudTable = CloudTableClient.GetTableReference(_tableName);
            var partitionFilter = TableQuery.GenerateFilterCondition(TableConstants.PartitionKey, QueryComparisons.Equal, partitionKey);
            var minFilter = TableQuery.GenerateFilterCondition(TableConstants.RowKey, QueryComparisons.GreaterThanOrEqual, minRowKey);
            var maxFilter = TableQuery.GenerateFilterCondition(TableConstants.RowKey, QueryComparisons.LessThanOrEqual, maxRowKey);

            var combinedRowFilter = TableQuery.CombineFilters(minFilter, TableOperators.And, maxFilter);
            var combinedFilter = TableQuery.CombineFilters(partitionFilter, TableOperators.And, combinedRowFilter);

            var query = new TableQuery<T>().Where(combinedFilter);
            var entities = this.GetEntities(cloudTable, query);

            return entities;
        }

        public virtual async Task<IEnumerable<T>> GetEntitiesAsync(string partitionKey, string minRowKey, string maxRowKey)
        {
            var cloudTable = CloudTableClient.GetTableReference(_tableName);
            var partitionFilter = TableQuery.GenerateFilterCondition(TableConstants.PartitionKey, QueryComparisons.Equal, partitionKey);
            var minFilter = TableQuery.GenerateFilterCondition(TableConstants.RowKey, QueryComparisons.GreaterThanOrEqual, minRowKey);
            var maxFilter = TableQuery.GenerateFilterCondition(TableConstants.RowKey, QueryComparisons.LessThanOrEqual, maxRowKey);

            var combinedRowFilter = TableQuery.CombineFilters(minFilter, TableOperators.And, maxFilter);
            var combinedFilter = TableQuery.CombineFilters(partitionFilter, TableOperators.And, combinedRowFilter);

            var query = new TableQuery<T>().Where(combinedFilter);
            var entities = await this.GetEntitiesAsync(cloudTable, query);

            return entities;
        }

        public virtual IEnumerable<T> GetAllEntities()
        {
            var cloudTable = CloudTableClient.GetTableReference(_tableName);
            var partitionFilter = TableQuery.GenerateFilterCondition(TableConstants.PartitionKey, QueryComparisons.GreaterThan, "0");
            var query = new TableQuery<T>().Where(partitionFilter);
            var entities = this.GetEntities(cloudTable, query);

            return entities;
        }

        public virtual async Task<IEnumerable<T>> GetAllEntitiesAsync()
        {
            var cloudTable = CloudTableClient.GetTableReference(_tableName);
            var partitionFilter = TableQuery.GenerateFilterCondition(TableConstants.PartitionKey, QueryComparisons.GreaterThan, "0");
            var query = new TableQuery<T>().Where(partitionFilter);
            var entities = await this.GetEntitiesAsync(cloudTable, query);

            return entities;
        }

        public virtual void UpsertEntity(T entity)
        {
            if (entity != null)
            {
                var cloudTable = CloudTableClient.GetTableReference(_tableName);
                entity.ETag = "*";
                TableOperation upsertOperation = TableOperation.InsertOrReplace(entity);
                cloudTable.Execute(upsertOperation);
            }
        }

        public virtual async Task UpsertEntityAsync(T entity)
        {
            if (entity != null)
            {
                var cloudTable = CloudTableClient.GetTableReference(_tableName);
                entity.ETag = "*";
                TableOperation upsertOperation = TableOperation.InsertOrReplace(entity);
                await cloudTable.ExecuteAsync(upsertOperation);
            }
        }

        public virtual void UpsertBatch(IEnumerable<T> entities)
        {
            if (entities != null)
            {
                _BatchUpsert(entities);
            }
        }

        public virtual async Task UpsertBatchAsync(IEnumerable<T> entities)
        {
            if (entities != null)
            {
                await _BatchUpsertAsync(entities);
            }
        }

        #endregion // Public Methods

    }
}
