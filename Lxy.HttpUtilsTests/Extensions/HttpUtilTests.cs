using Lxy.HttpUtilsTests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Lxy.HttpUtils.Tests
{
    public class HttpUtilTests
    {
        static HttpUtilTests()
        {
            Startup.Run();
        }

        [Fact()]
        public async Task SetConfigTest()
        {
            HttpUtil.SetConfig(config =>
            {
                config.BaseAddress = new Uri("https://localhost:5200");
            });

            using var responseContext = await "test/check".Get().SendAsync();

            Assert.True(responseContext.IsSuccessStatusCode);
        }

        [Fact()]
        public async Task SetConfigTest1()
        {
            HttpUtil.SetConfig("TestApi", config =>
            {
                config.BaseAddress = new Uri("https://localhost:5200");
            });

            using var httpUtil = "TestApi".GetHttpUtil();
            using var responseContext = await httpUtil.Get("test/check").SendAsync();

            Assert.True(responseContext.IsSuccessStatusCode);
        }

        [Fact()]
        public async Task SetConfigTest2()
        {
            HttpUtil.SetConfig(new Uri("https://localhost:5200"), config =>
            {
                config.BaseAddress = new Uri("https://localhost:5200");
            });

            using var httpUtil = "localhost".GetHttpUtil();
            using var responseContext = await httpUtil.Get("test/check").SendAsync();

            Assert.True(responseContext.IsSuccessStatusCode);
        }

        [Theory]
        [InlineData("2", "22")]
        [InlineData("1", "11")]
        public async Task GetHttpUtilTest(string id, string name)
        {
            var getQuery = await $"https://localhost:5200/test/get-query?id={id}&name={name}".Get().ReadAsAsync<string[]>();

            void Assert_GetQuery()
            {
                Assert.Equal(2, getQuery.Length);
                Assert.Equal<string[]>([id, name], getQuery);
            }

            Assert_GetQuery();

            getQuery = await $"https://localhost:5200/test/get-query".Get()
                .AddQuery("id", id)
                .AddQuery("name", name)
                .ReadAsAsync<string[]>();

            Assert_GetQuery();

            using var httpUtil = "TestApi".GetHttpUtil(config =>
            {
                config.BaseAddress = new Uri("https://localhost:5200");
                config.SetRetry(3, d => (int)(Math.Pow(2, d - 1) * 1000)); // 1 2 4 8 16s...
            });

            using var getQuery400ResponseContext = await httpUtil.Get("test/get-query")
                .AddQuery($"id={id}&name={name}")
                .AddQuery("reqId", Guid.NewGuid().ToString())
                .SendAsync();

            Assert.Equal(HttpStatusCode.BadRequest, getQuery400ResponseContext.StatusCode);

            using var getQueryFailResponseContext = await httpUtil.Get("test/get-query")
                .AddQuery($"id={id}&name={name}")
                .AddQuery("reqId", Guid.NewGuid().ToString())
                .SetRetry(3, retryOptions: RetryOptions.FailStatusCode)
                .SendAsync();

            Assert.Equal(HttpStatusCode.OK, getQueryFailResponseContext.StatusCode);
            getQuery = await getQueryFailResponseContext.ReadAsAsync<string[]>();

            Assert_GetQuery();

            getQuery = await httpUtil.Get("test/get-query")
                .AddQuery(new Dictionary<string, object> { { "id", id }, { "name", name } })
                .ReadAsAsync<string[]>();

            Assert_GetQuery();

            var requestContext = httpUtil.Get("test/get-query")
               .AddQuery(new Dictionary<string, object> { { "id", id }, { "name", name } });

            var expected = new[] { id, name };
            var expectedString = JsonConvert.SerializeObject(expected);
            Assert.Equal(expectedString, await requestContext.ReadAsStringAsync());

            var expectedBytes = Encoding.UTF8.GetBytes(expectedString);
            Assert.Equal(Convert.ToBase64String(expectedBytes), await requestContext.ReadAsBase64StringAsync());
            Assert.Equal(expectedBytes, await requestContext.ReadAsByteArrayAsync());

            using var getQueryStream = await requestContext.ReadAsStreamAsync();
            using var streamReader = new StreamReader(getQueryStream);

            Assert.Equal(expectedString, await streamReader.ReadToEndAsync());

            using var responseContext = await requestContext.SendAsync();

            Assert.Equal(HttpStatusCode.OK, responseContext.StatusCode);
            Assert.Equal(expectedString, await responseContext.ReadAsStringAsync());
        }

        [Fact()]
        public async Task GetHttpUtilTest1()
        {
            HttpUtil.SetConfig(config =>
            {
                config.BaseAddress = new Uri("https://localhost:5200");
            });

            using var httpUtil = HttpUtil.GetHttpUtil();
            using var responseContext = await httpUtil.Get("test/check").SendAsync();

            Assert.True(responseContext.IsSuccessStatusCode);
        }

        [Fact()]
        public async Task GetHttpUtilTest2()
        {
            await SetConfigTest1();
        }

        [Fact()]
        public async Task GetHttpUtilTest3()
        {
            HttpUtil.SetConfig(new Uri("https://localhost:5200"), config =>
            {
                config.BaseAddress = new Uri("https://localhost:5200");
            });

            using var httpUtil = new Uri("https://localhost:5200").GetHttpUtil();
            using var responseContext = await httpUtil.Get("test/check").SendAsync();

            Assert.True(responseContext.IsSuccessStatusCode);
        }

        [Fact()]
        public async Task GetHttpUtilTest4()
        {
            new Uri("https://localhost:5200").Dispose();

            HttpUtil.SetConfig(new Uri("https://localhost:5200"), config =>
            {
                config.BaseAddress = new Uri("https://localhost:5200");
            });

            var token = Guid.NewGuid().ToString();
            using var httpUtil = "localhost".GetHttpUtil(config =>
            {
                config.SetDefaultRequestHeaders(headers =>
                {
                    headers.Remove("Token");
                    headers.TryAddWithoutValidation("Token", token);
                });
            });

            Assert.Equal(token, await httpUtil.Get("test/token").ReadAsStringAsync());
        }

        [Fact()]
        public async Task GetHttpUtilTest5()
        {
            new Uri("https://localhost:5200").Dispose();

            HttpUtil.SetConfig(new Uri("https://localhost:5200"), config =>
            {
                config.BaseAddress = new Uri("https://localhost:5200");
            });

            var token = Guid.NewGuid().ToString();
            using var httpUtil = new Uri("https://localhost:5200").GetHttpUtil(config =>
            {
                config.SetDefaultRequestHeaders(headers =>
                {
                    headers.Remove("Token");
                    headers.TryAddWithoutValidation("Token", token);
                });
            });

            Assert.Equal(token, await httpUtil.Get("test/token").ReadAsStringAsync());
        }

        [InlineData("Get")]
        [InlineData("Post")]
        [InlineData("PostFormData")]
        [InlineData("Delete")]
        [InlineData("Put")]
        [InlineData("Head")]
        [InlineData("Options")]
        [InlineData("Trace")]
        [InlineData("Patch")]
        [Theory()]
        public async Task MetohdTest(string method)
        {
            var requestContext = "https://localhost:5200/test/method".Method(HttpMethod.Parse(method));

            if ("Head" == method)
            {
                using var responseContext = await requestContext.SendAsync();
                Assert.True(responseContext.IsSuccessStatusCode);
            }
            else
            {
                Assert.Equal(method.ToUpper(), (await requestContext.ReadAsStringAsync()).ToUpper());
            }
        }

        [InlineData("Get")]
        [InlineData("Post")]
        [InlineData("PostFormData")]
        [InlineData("Delete")]
        [InlineData("Put")]
        [InlineData("Head")]
        [InlineData("Options")]
        [InlineData("Trace")]
        [InlineData("Patch")]
        [Theory()]
        public async Task MetohdTest1(string method)
        {
            var requestContext = new Uri("https://localhost:5200/test/method").Method(HttpMethod.Parse(method));

            if ("Head" == method)
            {
                using var responseContext = await requestContext.SendAsync();
                Assert.True(responseContext.IsSuccessStatusCode);
            }
            else
            {
                Assert.Equal(method.ToUpper(), (await requestContext.ReadAsStringAsync()).ToUpper());
            }
        }

        [Fact()]
        public async Task PostTest()
        {
            Assert.Equal("POST_6", await "https://localhost:5200/test/data".Post()
                .AddQuery("P3", 3)
                .AddHeader("header", "header")
                .SetContent(new { P1 = 1, P2 = 2 })
                .ReadAsStringAsync());
        }

        [Fact()]
        public async Task PostTest1()
        {
            Assert.Equal("POST_6", await new Uri("https://localhost:5200/test/data").Post()
                .AddQuery("P3", 3)
                .SetContent(new { P1 = 1, P2 = 2 })
                .ReadAsStringAsync());

            var asyncStream = new Uri("https://localhost:5200/test/async-stream").Post()
                .AddQuery("s", 1)
                .AddQuery("e", 10)
                .UseHttpCompletionOption(HttpCompletionOption.ResponseHeadersRead)
                .ReadAsAsyncEnumerable<int>();

            var i = 0;
            await foreach (var item in asyncStream)
            {
                Assert.Equal(++i, item);
            }

            var objectAsyncStream = new Uri("https://localhost:5200/test/async-stream").Post()
                .AddQuery("s", 1)
                .AddQuery("e", 10)
                .UseHttpCompletionOption(HttpCompletionOption.ResponseHeadersRead)
                .ReadStreamAsAsyncEnumerable<string>(BlockAsyncEnumerableOptions.Default);

            i = 0;
            var memory = JsonConvert.SerializeObject(Enumerable.Range(1, 10)).AsMemory();
            await foreach (var item in objectAsyncStream)
            {
                Assert.Equal(memory.Slice(i++ * BlockAsyncEnumerableOptions.Default.Size, Math.Min(BlockAsyncEnumerableOptions.Default.Size, item.Length)).ToString(), item);
            }

            var or = "https://localhost:5200/test/async-stream-line".Post()
                .AddQuery("s", 1)
                .AddQuery("e", 10)
                .UseHttpCompletionOption(HttpCompletionOption.ResponseHeadersRead)
                .ReadStreamAsAsyncEnumerable<object>();

            i = 0;
            await foreach (JToken item in or)
            {
                Assert.Equal(++i, item["C"]);
                Assert.Equal(1, item["S"]);
                Assert.Equal(10, item["E"]);
            }
        }

        [Fact()]
        public async Task GetTest()
        {
            Assert.Equal("GET_6", await "https://localhost:5200/test/data".Get()
                .AddQuery("P3", 3)
                .SetContent(new { P1 = 1, P2 = 2 })
                .ReadAsStringAsync());
        }

        [Fact()]
        public async Task GetTest1()
        {
            Assert.Equal("GET_6", await new Uri("https://localhost:5200/test/data").Get()
                .AddQuery("P3", 3)
                .SetContent(new { P1 = 1, P2 = 2 })
                .ReadAsStringAsync());
        }

        [Fact()]
        public async Task PostFormDataTest()
        {
            Assert.Equal("POST_6", await "https://localhost:5200/test/form-data".PostFormData()
                .AddQuery("P3", 3)
                .SetContent("P1=1&P2=2")
                .ReadAsStringAsync());
        }

        [Fact()]
        public async Task PostFormDataTest1()
        {
            Assert.Equal("POST_6", await new Uri("https://localhost:5200/test/form-data").PostFormData()
                .AddQuery("P3", 3)
                .SetContent(new Dictionary<string, string>
                {
                    { "P1", "1" },
                    { "P2", "2" },
                })
                .ReadAsStringAsync());
        }
    }
}