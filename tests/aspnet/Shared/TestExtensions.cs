using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace biscuit_net.AspNet.Tests.Shared;

public static class TestExtensions
{
    public static IServiceCollection ConfigureAuthTestServices(this IServiceCollection services)
        => services.AddOptions()
                   .AddLogging();
}