﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Orchestrate.Io.Utility;

namespace Orchestrate.Io
{
    public class Collection
    {
        string host;
        JsonSerializer serializer;
        string apiKey;
        RestClient restClient;

        public string CollectionName { get; private set; }

        public Collection(string collectionName, 
                          string apiKey,
                          string host,
                          JsonSerializer serializer)
        {
            this.apiKey = apiKey;
            this.host = host;
            this.serializer = serializer;
            CollectionName = collectionName;
            restClient = new RestClient(collectionName, apiKey, serializer);
        }

        public Task<SearchResults<T>> SearchAsync<T>(string query, SearchOptions opts = null)
        {
            Guard.ArgumentNotNullOrEmpty("query", query);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                                   .AppendPath(CollectionName)
                                                   .AddQuery("query", query);

            if (opts != null)
            {
                if (!String.IsNullOrEmpty(opts.Sort))
                    uri.AddQuery("sort", opts.Sort);

                uri.AddQuery("limit", opts.Limit.ToString());
                uri.AddQuery("offset", opts.Offset.ToString());
            }

            return restClient.GetAsync<SearchResults<T>>(uri);
        }

        public Task<ListResults<T>> GetLinkAsync<T>(string key, string kind, LinkOptions opts = null)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);
            Guard.ArgumentNotNullOrEmpty("kind", kind);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                                    .AppendPath(CollectionName)
                                                    .AppendPath(key)
                                                    .AppendPath("relations")
                                                    .AppendPath(kind);

            if (opts != null)
            {
                uri.AddQuery("limit", opts.Limit.ToString());
                uri.AddQuery("offset", opts.Offset.ToString());
            }

            return restClient.GetAsync<ListResults<T>>(uri);
        }

        public Task<T> GetLinkAsync<T>(string key, string kind, GraphNode destinationNode)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);
            Guard.ArgumentNotNullOrEmpty("kind", kind);
            Guard.ArgumentNotNull("destination node", destinationNode);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                                    .AppendPath(CollectionName)
                                                    .AppendPath(key)
                                                    .AppendPath("relation")
                                                    .AppendPath(kind)
                                                    .AppendPath(destinationNode.CollectionName)
                                                    .AppendPath(destinationNode.Key);

            return restClient.GetAsync<T>(uri);
        }

        public Task<ListResults<T>> HistoryAsync<T>(string productKey, HistoryOptions opts = null)
        {
            Guard.ArgumentNotNullOrEmpty("key", productKey);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                       .AppendPath(CollectionName)
                                       .AppendPath(productKey)
                                       .AppendPath("refs");

            if(opts != null)
            {
                if (opts.Values == true)
                    uri.AddQuery("values", "true");

                uri.AddQuery("limit", opts.Limit.ToString());
                uri.AddQuery("offset", opts.Offset.ToString());
            }

            return restClient.GetAsync<ListResults<T>>(uri);
        }

        public Task<SearchResults<T>> SearchAsync<T>(string field, decimal latitude, decimal longitude, string distance)
        {
            string luceneQuery = string.Format("{0}:NEAR:{4}lat:{1} lon:{2} dist:{3}{5}", field, 
                                                                                          latitude.ToString(), 
                                                                                          longitude.ToString(), 
                                                                                          distance,
                                                                                          "{",
                                                                                          "}");
            return SearchAsync<T>(luceneQuery);
        }

        public Task<ListResults<T>> ListAsync<T>(int limit = 100)
        {
            if (limit < 1 || limit > 100)
                throw new ArgumentOutOfRangeException("limit", "limit must be between 1 and 100");

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                                   .AppendPath(CollectionName)
                                                   .AddQuery("limit", limit.ToString());

            return restClient.GetAsync<ListResults<T>>(uri);
        }

        public Task<KvMetadata> AddAsync<T>(T item)
        {
            Guard.ArgumentNotNull("item", item);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                        .AppendPath(CollectionName);

            return restClient.SendAsync(uri, HttpMethod.Post, item);
        }

        public Task<ListResults<T>> ExclusiveListAsync<T>(int limit = 100,
                                                                string afterKey = null,
                                                                string beforeKey = null)
        {
            if (limit < 1 || limit > 100)
                throw new ArgumentOutOfRangeException("limit", "limit must be between 1 and 100");

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                                   .AppendPath(CollectionName)
                                                   .AddQuery("limit", limit.ToString());

            if (!string.IsNullOrEmpty(beforeKey))
                uri.AddQuery("beforeKey", beforeKey);

            if (!string.IsNullOrEmpty(afterKey))
                uri.AddQuery("afterKey", afterKey);

            return restClient.GetAsync<ListResults<T>>(uri);
        }


        public Task<ListResults<T>> InclusiveListAsync<T>(int limit = 100, 
                                                                string startKey = null, 
                                                                string endKey = null)
        {
            if (limit < 1 || limit > 100)
                throw new ArgumentOutOfRangeException("limit", "limit must be between 1 and 100");

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                                   .AppendPath(CollectionName)
                                                   .AddQuery("limit", limit.ToString());

            if (!string.IsNullOrEmpty(startKey))
                uri.AddQuery("startKey", startKey);

            if (!string.IsNullOrEmpty(endKey))
                uri.AddQuery("endKey", endKey);

            return restClient.GetAsync<ListResults<T>>(uri);
        }

        public Task<KvMetadata> AddOrUpdateAsync<T>(string key,
                                                          T item, 
                                                          string reference = null)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);
            Guard.ArgumentNotNull("item", item);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                        .AppendPath(CollectionName)
                                        .AppendPath(key);

            return restClient.SendIfMatchAsync(uri, HttpMethod.Put, item, reference);
        }

        public Task<KvMetadata> TryAddAsync<T>(string key,
                                                     T item)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);
            Guard.ArgumentNotNull("item", item);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                        .AppendPath(CollectionName)
                                        .AppendPath(key);

            return restClient.SendIfNoneMatchAsync(uri, HttpMethod.Put, item);
        }


        public Task DeleteAsync(string key, bool purge = true, string reference = null)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                        .AppendPath(CollectionName)
                                        .AppendPath(key)
                                        .AddQuery("purge", purge ? "true" : "false");

            return restClient.SendIfMatchAsync(uri, HttpMethod.Delete, (object) null, reference);
        }

        public Task<KvObject<T>> GetAsync<T>(string key, string versionReference = null)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                        .AppendPath(CollectionName)
                                        .AppendPath(key);

            if (versionReference != null)
            {
                uri.AppendPath("refs")
                   .AppendPath(versionReference);
            }

            return restClient.GetAsync<T>(key, uri);
        }

        public Task<KvMetadata> MergeAsync<T>(string key,
                                                    T item, 
                                                    string reference = null)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);
            Guard.ArgumentNotNull("item", item);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                        .AppendPath(CollectionName)
                                        .AppendPath(key);

            return restClient.SendIfMatchAsync(uri, new HttpMethod("PATCH"), item, reference);
        }


        public Task<KvMetadata> PatchAsync(string key,
                                                 IEnumerable<PatchOperation> patchOperations, 
                                                 string reference = null)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);
            Guard.ArgumentNotNull("operations", patchOperations);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                        .AppendPath(CollectionName)
                                        .AppendPath(key);

            return restClient.SendIfMatchAsync(uri, new HttpMethod("PATCH"), patchOperations.ToArray(), reference);
        }

        public async Task<KvMetadata> UpdateAsync<T>(string key,
                                                          T item,
                                                          string reference = null)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);
            Guard.ArgumentNotNull("item", item);

            try
            {
                await GetAsync<T>(key);
            }
            catch(NotFoundException exception)
            {
                throw exception;
            }

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                        .AppendPath(CollectionName)
                                        .AppendPath(key);

            return await restClient.SendIfMatchAsync(uri, HttpMethod.Put, item, reference);
        }
    }
}