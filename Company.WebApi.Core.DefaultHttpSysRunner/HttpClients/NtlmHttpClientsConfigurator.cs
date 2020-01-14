﻿using System;
using System.Net.Http;
using Company.HttpClient;
using Company.Logging;
using Company.WebApi.Core.DefaultConfiguration.HttpClients;
using Company.WebApi.Core.DefaultConfiguration.HttpClients.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Company.WebApi.Core.DefaultHttpSysRunner.HttpClients
{
  /// <summary>
  /// Конфигуратор для регистрации Http-клиентов со специальной настройкой для поддержки NTLM
  /// </summary>
  public static class NtlmHttpClientsConfigurator
  {
    public static void RegisterHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
      try
      {
        var endpoints = configuration.GetSection("webApiEndpoints")
                                     .Get<WebapiEndpoint[]>();
        if (endpoints == null)
        {
          services.AddHttpClient();
          return;
        }

        foreach (var endpoint in endpoints)
        {
          ConfigureNtlmClient(endpoint)
            .Register(services);
        }
      }
      catch (Exception ex)
      {
        Logger.LogErrorFormat(ex, "Error while register HttpClients");
        throw;
      }
    }

    private static HttpClientRegistrator ConfigureBaseClient(WebapiEndpoint endpoint)
    {
      return HttpClientRegistrator.Create()
                                  .WithName(endpoint.Name)
                                  .WithUrl(endpoint.Url)
                                  .WithTimeout(endpoint.Timeout)
                                  .WithHandler<BaseCompanyRequestHeadersHandler>();
    }

    private static HttpClientRegistrator ConfigureNtlmClient(WebapiEndpoint endpoint)
    {
      return ConfigureBaseClient(endpoint)
        .ConfigureBuilder(x =>
        {
          x.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
          {
            AllowAutoRedirect = false,
            UseDefaultCredentials = true,
            PreAuthenticate = true
          });
        });
    }
  }
}
