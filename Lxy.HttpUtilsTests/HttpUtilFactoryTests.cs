using Lxy.HttpUtils.App;
using Lxy.HttpUtilsTests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Lxy.HttpUtils.Tests
{
    public class HttpUtilFactoryTests
    {
        private IServiceProvider Services { get; }
        private static readonly Uri _uri = new("https://localhost:5200");

        public HttpUtilFactoryTests()
        {
            Services = new WebApplicationFactory<Program>().WithWebHostBuilder(configuration =>
            {
                configuration.UseKestrelHttpsConfiguration();
                configuration.UseUrls(_uri.AbsoluteUri);
                configuration.ConfigureServices(services =>
                {
                    services.AddHttpUtil(config =>
                    {
                        config.BaseAddress = _uri;
                    });

                    services.AddHttpUtil("TestApi", config =>
                    {
                        config.BaseAddress = _uri;
                    });

                    services.AddHttpUtil(_uri.Host, config =>
                    {
                        config.BaseAddress = _uri;
                    });

                    services.AddHttpUtil("github", config =>
                    {
                        config.BaseAddress = new Uri("https://github.com");
                    });
                });
            }).Services;
        }

        static HttpUtilFactoryTests()
        {
            Startup.Run();
        }

        [Fact]
        public async Task GetTest()
        {
            var httpUtilFactory = Services.GetRequiredService<IHttpUtilFactory>();
            using var testApi = httpUtilFactory.Get("TestApi");
            var getQuery = await testApi.Get("test/get-query?id=1&name=2").ReadAsAsync<string[]>();

            Assert.Equal(2, getQuery.Length);
            Assert.Equal<string[]>(getQuery, ["1", "2"]);

            using var localhostApi = httpUtilFactory.Get("localhost");
            getQuery = await localhostApi.Get("test/get-query?id=2&name=3").ReadAsAsync<string[]>();

            Assert.Equal(2, getQuery.Length);
            Assert.Equal<string[]>(getQuery, ["2", "3"]);

            using var defaultApi = httpUtilFactory.Get();
            getQuery = await defaultApi.Get("test/get-query?id=2&name=3").ReadAsAsync<string[]>();

            Assert.Equal(2, getQuery.Length);
            Assert.Equal<string[]>(getQuery, ["2", "3"]);

            var github = httpUtilFactory.Get("github");
            var gitHubResult = await github.Get("LI-XIAOYAO")
                .AddQuery("tab", "repositories")
                .AddCookies("xxx=xxx")
                .ReadAsStringAsync();

            Assert.Contains("LI-XIAOYAO", gitHubResult);
        }
    }
}