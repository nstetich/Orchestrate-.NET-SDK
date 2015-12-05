﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Orchestrate.Io
{
    public static class HttpRequestMessageExtensions
    {
        public static void AddIfMatch(this HttpRequestMessage message, string reference)
        {
            var quotedRef = String.Format("\"{0}\"", reference);
            message.Headers.IfMatch.Add(new EntityTagHeaderValue(quotedRef));
        }

        public static void AddIfNoneMatch(this HttpRequestMessage message)
        {
            message.Headers.IfNoneMatch.Add(new EntityTagHeaderValue("\"*\""));
        }

        public static void AddContent<T>(this HttpRequestMessage message, T item, JsonSerializer serializer)
        {
            var json = serializer.SerializeObject(item);
            message.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        public static void AddUserAgent(this HttpRequestMessage message)
        {
            string productName = "OrchestrateDotNetClient";
            var productVersion = typeof(KvMetadata).GetTypeInfo().Assembly.GetName().Version;
            ProductInfoHeaderValue infoHeader = new ProductInfoHeaderValue(productName, productVersion.ToString());

            message.Headers.UserAgent.Add(infoHeader);
        }
    }
}
