# Lxy.HttpUtils

HttpClient FluentAPI extension

<!--TOC-->
- [Method](#method)
  - [Request Methods](#request-methods)
  - [Request Context Methods](#request-context-methods)
  - [Response Context Methods](#response-context-methods)
- [Use inject](#use-inject)
- [Use extensions](#use-extensions)
<!--/TOC-->

# Method
## Request Methods
```c#
"https://github.com".Get()
    ...Post()
    ...PostFormData()
    ...Put()
    ...Delete()
    ...Head()
    ...Options()
    ...Trace()
    ...Patch()
    ...Method(HttpMethod...)
```

## Request Context Methods
```c#
"https://github.com".Get()
    .AddHeader()
    .AddHeaders()
    .AddQuery()
    .AddCookie()
    .AddCookies()
    .SetContent()
    .SetUserAgent()
    .SetAuthorization()
    .SetContentType()
    .SetRelativeUri()
    .SetTimeout()
    .SetJsonSerializerSettings()
    .SetEncoding()
    .SetRetry()
    .SetVersion()
    .UseCookies()
    .UseHttpCompletionOption()
    .EnsureSuccessStatusCode()
```

```c#
// Read http response content methods
"https://github.com".Get().ReadAsStringAsync()
    ...ReadAsAsync()
    ...ReadAsStreamAsync()
    ...ReadAsAsyncEnumerable()
    ...ReadStreamAsAsyncEnumerable()
    ...ReadAsByteArrayAsync()
    ...ReadAsBase64StringAsync()
    ...SendAsync()
```

## Response Context Methods
```c#
// Read http response content methods
"https://github.com".Get().SendAsync().ReadAsStringAsync()
    ...ReadAsAsync()
    ...ReadAsStreamAsync()
    ...ReadAsAsyncEnumerable()
    ...ReadStreamAsAsyncEnumerable()
    ...ReadAsByteArrayAsync()
    ...ReadAsBase64StringAsync()
```

# Use inject

**Configure the default `HttpUtilConfig`**
```c#
services.AddHttpUtil(config =>
{
    config.BaseAddress = new Uri("https://github.com");
});

var httpUtilFactory = Services.GetRequiredService<IHttpUtilFactory>();
var github = httpUtilFactory.Get();
var result = await github.Get("LI-XIAOYAO")
    .AddQuery("tab","repositories")
    .AddHeader("header", "xx")
    .SetAuthorization("Bearer", "xx")
    .ReadAsStringAsync();

var responseContext = await github.Get("LI-XIAOYAO")
    .SendAsync();

if (responseContext.IsSuccessStatusCode)
{
    var result = await responseContext.ReadAsStringAsync();
}
```

**Configure by name**
```c#
services.AddHttpUtil("github", config =>
{
    config.BaseAddress = new Uri("https://github.com");
});

var httpUtilFactory = Services.GetRequiredService<IHttpUtilFactory>();
var github = httpUtilFactory.Get("github");
var result = await github.Get("LI-XIAOYAO")
    .ReadAsStringAsync();
```

# Use extensions

**Send a request using the default `HttpUtilConfig` and create an `IHttpUtil` named `Uri.Host`**

```c#
var result = await "https://github.com/LI-XIAOYAO".Get()
    .ReadAsStringAsync();

var result = await HttpUtil.Get("https://github.com/LI-XIAOYAO")
    .ReadAsStringAsync();

var result = await "github.com".GetHttpUtil()
    .Get("https://github.com/LI-XIAOYAO")
    .ReadAsStringAsync();
```

**Configure `HttpUtilConfig`**

```c#
HttpUtil.SetConfig(config => {
    config.BaseAddress = new Uri("https://github.com"); 
});

var result = await "LI-XIAOYAO".Get()
    .ReadAsStringAsync();

// Configure by name
HttpUtil.SetConfig("github", config => {
    config.BaseAddress = new Uri("https://github.com"); 
});

var result = await "github".GetHttpUtil()
    .Get("LI-XIAOYAO")
    .ReadAsStringAsync();
```
