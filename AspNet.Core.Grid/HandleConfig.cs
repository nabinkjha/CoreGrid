public static class ConfigHelper
    {
        public static IConfigurationRoot GetConfig(ExecutionContext context)
        {
            string environmentName;
#if DEBUG
            environmentName = "debug";
#else
            environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
#endif
            return new ConfigurationBuilder()
             .SetBasePath(context.FunctionAppDirectory)
             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
             .AddJsonFile($"appsettings.{environmentName}.json", optional: false, reloadOnChange: true)
             .AddEnvironmentVariables()
             .Build();
        }
        //
        public static ApiClientSettings GetApiClientSettings(ExecutionContext context)
        {
            var config = GetConfig(context);
            var apiClientSettings = new ApiClientSettings();
            apiClientSettings.ODataApiBaseUrl = config[StringConstant.ApiClientSettings.ODataApiBaseUrl];
            apiClientSettings.ApiBaseUrl = config[StringConstant.ApiClientSettings.ApiBaseUrl];
            apiClientSettings.UseProxy = config[StringConstant.ApiClientSettings.UseProxy];
            apiClientSettings.Scope = config[StringConstant.ApiClientSettings.Scope];
            apiClientSettings.ClientId = config[StringConstant.ApiClientSettings.ClientId];
            apiClientSettings.ClientSecret = config[StringConstant.ApiClientSettings.ClientSecret];
            apiClientSettings.Resource = config[StringConstant.ApiClientSettings.Resource];
            apiClientSettings.Authority = config[StringConstant.ApiClientSettings.Authority];
            apiClientSettings.APIMKeyName = config[StringConstant.ApiClientSettings.APIMKeyName];
            apiClientSettings.APIMKeyValue = config[StringConstant.ApiClientSettings.APIMKeyValue];
            return apiClientSettings;
        }
        public static AzureBlobOptions GetAzureBlobOptions(ExecutionContext context)
        {
            var config = GetConfig(context);
            var azureBlobOption = new AzureBlobOptions();
            azureBlobOption.CitrixFileUploadQueueName = config[StringConstant.AzureBlobOptions.CitrixFileUploadQueueName];
            azureBlobOption.VideoContainerName = config[StringConstant.AzureBlobOptions.VideoContainerName];
            azureBlobOption.ImageContainerName = config[StringConstant.AzureBlobOptions.ImageContainerName];
            azureBlobOption.DocumentContainerName = config[StringConstant.AzureBlobOptions.DocumentContainerName];
            azureBlobOption.ConnectionString = config[StringConstant.AzureBlobOptions.ConnectionString];
            azureBlobOption.CitrixFileUploadQueueName = config[StringConstant.AzureBlobOptions.CitrixFileUploadQueueName];
            return azureBlobOption;
        }
        public static CitrixFileScanningOptions GetCitrixFileScanningOptions(ExecutionContext context)
        {
            var config= GetConfig(context);
            var citrixFileScanning = new CitrixFileScanningOptions();
            citrixFileScanning.HostName = config[StringConstant.CitrixFileScanningOptions.HostName];
            citrixFileScanning.ControlPanel = config[StringConstant.CitrixFileScanningOptions.ControlPanel];
            citrixFileScanning.ClientId = config[StringConstant.CitrixFileScanningOptions.ClientId];
            citrixFileScanning.ClientSecret = config[StringConstant.CitrixFileScanningOptions.ClientSecret];
            citrixFileScanning.UserName = config[StringConstant.CitrixFileScanningOptions.UserName];
            citrixFileScanning.Password = config[StringConstant.CitrixFileScanningOptions.Password];
            citrixFileScanning.SubDomain = config[StringConstant.CitrixFileScanningOptions.SubDomain];
            citrixFileScanning.ShareFileClientUrl = config[StringConstant.CitrixFileScanningOptions.ShareFileClientUrl];
            citrixFileScanning.CitrixRootFolder = config[StringConstant.CitrixFileScanningOptions.CitrixRootFolder];
            citrixFileScanning.CitrixUploadFolder = config[StringConstant.CitrixFileScanningOptions.CitrixUploadFolder];
            return citrixFileScanning;
        }
    }
  public async Task<string> GetBearerToken(ExecutionContext context)
        {
            try
            {
                var _apiClientSettings = ConfigHelper.GetApiClientSettings(context);
                //var token = GetToken(context, _apiClientSettings).Result;
                //return token;
#if DEBUG
                HttpClientHandler clientHandler = new HttpClientHandler
                {
                    UseProxy = true,
                    Proxy = new WebProxy
                    {
                        Address = new Uri(proxyurl),
                        BypassProxyOnLocal = true,
                        UseDefaultCredentials = true
                    }
                };
                var client = new HttpClient(handler: clientHandler);
#else
               var client = new HttpClient();
#endif

                JObject payload = null;
                client.BaseAddress = new Uri("https://login.microsoftonline.com/");

                var content = new FormUrlEncodedContent(new[]
                            {
                            new KeyValuePair<string, string>(OpenIdConnectParameterNames.ClientId, _apiClientSettings.ClientId),
                            new KeyValuePair<string, string>(OpenIdConnectParameterNames.ClientSecret,_apiClientSettings.ClientSecret),
                            new KeyValuePair<string, string>(OpenIdConnectParameterNames.Resource, _apiClientSettings.Resource),
                            new KeyValuePair<string, string>(OpenIdConnectParameterNames.Scope, _apiClientSettings.Scope),
                            new KeyValuePair<string, string>(OpenIdConnectParameterNames.GrantType, "client_credentials"),
                         });

                var response = await client.PostAsync("CompanyName/oauth2/token", content);
                string resultContent = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                payload = JObject.Parse(resultContent);
                var bearerToken = payload.Value<string>(OpenIdConnectParameterNames.AccessToken);
                return bearerToken;
            }
            catch (Exception ex)
            {

                throw ex;
            }
           
        }
