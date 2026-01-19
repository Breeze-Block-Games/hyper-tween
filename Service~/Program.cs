using System.Net;
using BreezeBlockGames.PackageBase.Service.Extensions;
using BreezeBlockGames.PackageBase.Service.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Routing.Constraints;

var builder = WebApplication.CreateSlimBuilder(args);

#if DEBUG
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endif

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, ServiceJsonSerializerContext.Default);
});

builder.Services.Configure<RouteOptions>(options => options.SetParameterPolicy<RegexInlineRouteConstraint>("regex"));

builder.Services.AddHelloWorldServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor,
        ForwardLimit = 1, // trust a single proxy hop
        KnownProxies =
        {
            IPAddress.Loopback,        // 127.0.0.1
            IPAddress.IPv6Loopback,    // ::1
        }
    });
}

app.MapHelloWorldEndpoints();

#if DEBUG
app.UseSwagger();
app.UseReDoc(c =>
{
    c.RoutePrefix = "docs";
    c.SpecUrl = "/swagger/v1/swagger.json";
});
#endif

app.Run();

public partial class Program;