// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

// <AuthProviderSnippet>
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace family_calendar
{
    public class DeviceCodeAuthProvider : BackgroundService, IAuthenticationProvider
    {
        private static IPublicClientApplication _msalClient;
        private static string[] _scopes;
        private static IAccount _userAccount;
        private static int refreshPeriod = -1;
        private static bool firstTime = true;
        private static readonly int before = 2;

        public DeviceCodeAuthProvider(){}

        public DeviceCodeAuthProvider(string appId, string[] scopes)
        {
            _scopes = scopes;

            _msalClient = PublicClientApplicationBuilder
                .Create(appId)
                .WithAuthority(AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount, true)
                .Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if(refreshPeriod == -1)
                {
                    Console.WriteLine("Waiting for authentication...");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                else if(firstTime)
                {
                    firstTime = false;
                    Console.WriteLine("The token will be refreshed in " + refreshPeriod + " minutes for the first time.");
                    await Task.Delay(TimeSpan.FromMinutes(refreshPeriod), stoppingToken);
                }
                else {
                    string date = DateTime.Now.ToString("g");
                    Console.WriteLine($"The token is updated at {date} and will be refreshed in {refreshPeriod} minutes.");
                    var accessToken = await GetAccessToken(); 
                    await Task.Delay(TimeSpan.FromMinutes(refreshPeriod), stoppingToken);
                }
            }
        }

        private void SetRefreshPeriod(DateTimeOffset expiresOn)
        {
            refreshPeriod = (expiresOn - DateTime.UtcNow).Minutes - before;
        }

        public async Task<string> GetAccessToken()
        {
            // If there is no saved user account, the user must sign-in
            if (_userAccount == null)
            {
                try
                {
                    // Invoke device code flow so user can sign-in with a browser
                    var result = await _msalClient.AcquireTokenWithDeviceCode(_scopes, callback => {
                        Console.WriteLine(callback.Message);
                        return Task.FromResult(0);
                    }).ExecuteAsync();

                    _userAccount = result.Account;
                    SetRefreshPeriod(result.ExpiresOn);
                    return result.AccessToken;
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Error getting access token: {exception.Message}");
                    return null;
                }
            }
            else
            {
                // If there is an account, call AcquireTokenSilent
                // By doing this, MSAL will refresh the token automatically if
                // it is expired. Otherwise it returns the cached token.

                    var result = await _msalClient
                        .AcquireTokenSilent(_scopes, _userAccount)
                        .ExecuteAsync();

                    SetRefreshPeriod(result.ExpiresOn);
                    return result.AccessToken;
            }
        }

        // This is the required function to implement IAuthenticationProvider
        // The Graph SDK will call this function each time it makes a Graph
        // call.
        public async Task AuthenticateRequestAsync(HttpRequestMessage requestMessage)
        {
            requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("bearer", await GetAccessToken());
        }
    }
}
// </AuthProviderSnippet>
