using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Configuration;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Net;
using B2BPortal.Models;
using B2BPortal.Infrastructure;

namespace B2BPortal.Data
{
    public static class DocDBSettings
    {
        public static string DocDBUri;
        public static string DocDBAuthKey;
        public static string DocDBName;
        public static string DocDBCollection;
    }

    public static class DocDBRepo<T> where T : class
    {
        
        private static DocumentClient client;

        public static async Task<Document> CreateItemAsync(T item)
        {
            (item as DocModelBase).DocType = (DocTypes)Enum.Parse(typeof(DocTypes), typeof(T).Name);
            return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DocDBSettings.DocDBName, DocDBSettings.DocDBCollection), item);
        }

        public static async Task<Document> UpdateItemAsync(string id, T item)
        {
            return await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(DocDBSettings.DocDBName, DocDBSettings.DocDBCollection, id), item);
        }

        public static async Task<Document> DeleteItemAsync(string id, T item)
        {
            return await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DocDBSettings.DocDBName, DocDBSettings.DocDBCollection, id));
        }

        public static async Task<T> GetItemAsync(string id)
        {
            try
            {
                Document document = await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(DocDBSettings.DocDBName, DocDBSettings.DocDBCollection, id));
                return (T)(dynamic)document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public static async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
        {
            var query = client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(DocDBSettings.DocDBName, DocDBSettings.DocDBCollection))
                .Where(predicate)
                .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }
        public static async Task<IEnumerable<T>> GetItemsAsyncOrg<T>(Expression<Func<T, bool>> predicate = null) where T : DocModelBase
        {
            var docType = (DocTypes)Enum.Parse(typeof(DocTypes), typeof(T).Name);

            var queryable = client.CreateDocumentQuery<T>(UriFactory.CreateDocumentCollectionUri(DocDBSettings.DocDBName, DocDBSettings.DocDBCollection));

            Expression<Func<T, bool>> docTypeFilter = (q => q.DocType == docType);

            if (predicate != null)
            {
                docTypeFilter = docTypeFilter.CombineWithAndAlso(predicate);
            }

            docTypeFilter.Compile();

            queryable.Where(docTypeFilter);
            var query = queryable.AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }

        public static void Initialize()
        {
            client = new DocumentClient(new Uri(DocDBSettings.DocDBUri), DocDBSettings.DocDBAuthKey);
            CreateDatabaseIfNotExistsAsync().Wait();
            CreateCollectionIfNotExistsAsync().Wait();
        }

        private static async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DocDBSettings.DocDBName));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(new Database { Id = DocDBSettings.DocDBName });
                }
                else
                {
                    throw;
                }
            }
        }

        private static async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DocDBSettings.DocDBName, DocDBSettings.DocDBCollection));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(DocDBSettings.DocDBName),
                        new DocumentCollection { Id = DocDBSettings.DocDBCollection },
                        new RequestOptions { OfferThroughput = 1000 });
                }
                else
                {
                    throw;
                }
            }
        }
    }

}