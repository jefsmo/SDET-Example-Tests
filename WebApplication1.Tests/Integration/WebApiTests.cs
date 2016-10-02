using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using WebApplication1.Web.Models;

namespace WebApplication1.Tests.Integration
{
    /// <summary>
    /// Represents an instance of ApiTransactionsControllerTests.
    /// </summary>
    [TestClass]
    public class WebApiTests : IDisposable
    {
        private readonly HttpServer _server;
        private readonly string _jsonMediaType = JsonMediaTypeFormatter.DefaultMediaType.MediaType;  // i.e. "application/json"
        private const string BaseAddress = @"http://localhost:58733/";

        public WebApiTests()
        {
            var config = new HttpConfiguration();

            config.Routes.MapHttpRoute(
                "DefaultApi",
                "api/{controller}/{id}",
                new { routetemplate = "Version", id = RouteParameter.Optional });

            _server = new HttpServer(config);
        }

        public TestContext TestContext { get; set; }

        // Using HttpClient (uses more resources)
        //[TestMethod]
        //public void GetTransactions_ReturnsAllTransactions_UsingHttpClient()
        //{
        //    // arrange
        //    using (var server = new HttpServer(config))
        //    using (var client = new HttpClient(server))
        //    {
        //        // act
        //        var response = client.GetAsync("http://localhost:58733/api/Transactions").Result;

        //        // assert
        //        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        //    }
        //}

        // *** Prefer this syntax: Using HttpMessageInvoker (lightweight)

        [TestMethod]
        public void GetTransactions_ReturnsAllTransactions()
        {
            // arrange
            using (var client = new HttpMessageInvoker(_server))
            {
                var request = CreateRequest("api/Transactions", _jsonMediaType, HttpMethod.Get);

                // act
                using (var response = client.SendAsync(request, CancellationToken.None).Result)
                {
                    // assert
                    Assert.IsNotNull(response.Content);
                    Assert.AreEqual(_jsonMediaType, response.Content.Headers.ContentType.MediaType);
                    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                    Assert.AreEqual(5, response.Content.ReadAsAsync<IEnumerable<Transaction>>().Result.Count());
                }

                request.Dispose();
            }
        }

        [TestMethod]
        public void Get_ReturnsTransaction_WhenTransactionMatchesId()
        {
            // arrange
            var expectedJson = JsonConvert.SerializeObject(new { Id = 1, Description = "DEF", Amount = 50 });

            using (var client = new HttpMessageInvoker(_server))
            {
                var request = CreateRequest(
                    "api/Transactions/1",
                    _jsonMediaType, 
                    HttpMethod.Get);

                // act
                using (var response = client.SendAsync(request, CancellationToken.None).Result)
                {
                    // assert
                    Assert.IsNotNull(response.Content);
                    Assert.AreEqual(_jsonMediaType, response.Content.Headers.ContentType.MediaType);
                    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                    Assert.IsInstanceOfType(response.Content, typeof(ObjectContent));
                    var answer = response.Content.ReadAsStringAsync().Result;
                    Assert.AreEqual(expectedJson, answer);
                }

                request.Dispose();
            }
        }

        [TestMethod]
        public void Get_ReturnsNotFound_WhenTransactionNotFound()
        {
            // arrange
            using (var client = new HttpMessageInvoker(_server))
            {
                var request = CreateRequest(
                    "api/Transactions/5",
                    _jsonMediaType, 
                    HttpMethod.Get);

                // act
                using (var response = client.SendAsync(request, CancellationToken.None).Result)
                {
                    // assert
                    Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
                }

                request.Dispose();
            }
        }

        [TestMethod]
        public void Post_NewTransaction_ReturnsAllTransactions()
        {
            // arrange
            var content = new Transaction { Description = "Test Post 01", Amount = 42 };

            using (var client = new HttpMessageInvoker(_server))
            {
                var request = CreateRequest(
                    "api/Transactions",
                    _jsonMediaType,  
                    HttpMethod.Post, 
                    content, 
                    new JsonMediaTypeFormatter());

                // act
                using (var response = client.SendAsync(request, CancellationToken.None).Result)
                {
                    // assert
                    Assert.IsNotNull(response.Content);
                    Assert.AreEqual(_jsonMediaType, response.Content.Headers.ContentType.MediaType);
                    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                    Assert.AreEqual(6, response.Content.ReadAsAsync<IEnumerable<Transaction>>().Result.Count());
                }

                request.Dispose();
            }
        }

        public void Dispose()
        {
            _server?.Dispose();
        }

        /// <summary>
        /// Creates an HTTP request.
        /// mthv = Media Type Header Value.  "application/json"

        /// </summary>
        /// <param name="relativeUri"></param>
        /// <param name="mthv"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private HttpRequestMessage CreateRequest(string relativeUri, string mthv, HttpMethod method)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(BaseAddress + relativeUri)
            };

            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(mthv));
            request.Method = method;

            return request;
        }

        /// <summary>
        /// Creates an HTTP request with content of type T.
        /// mthv = Media Type Header Value. "application/json"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="relativeUri"></param>
        /// <param name="mthv"></param>
        /// <param name="method"></param>
        /// <param name="content"></param>
        /// <param name="formatter"></param>
        /// <returns></returns>
        private HttpRequestMessage CreateRequest<T>(string relativeUri, 
            string mthv, 
            HttpMethod method, 
            T content, 
            MediaTypeFormatter formatter) where T : class
        {
            var request = CreateRequest(relativeUri, mthv, method);
            request.Content = new ObjectContent<T>(content, formatter);

            return request;
        }
    }
}
