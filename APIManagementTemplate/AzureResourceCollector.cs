﻿using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace APIManagementTemplate
{
    public class AzureResourceCollector : IResourceCollector
    {


        public string token;


        public AzureResourceCollector()  
        {

        }
        public string Login(string tenantName)
        {
            string authstring = Constants.AuthString;
            if (!string.IsNullOrEmpty(tenantName))
            {
                authstring = authstring.Replace("common", tenantName);
            }
            AuthenticationContext ac = new AuthenticationContext(authstring, true);

            var ar = ac.AcquireTokenAsync(Constants.ResourceUrl, Constants.ClientId, new Uri(Constants.RedirectUrl), new PlatformParameters(PromptBehavior.RefreshSession)).GetAwaiter().GetResult();
            token = ar.AccessToken;
            return token;
        }
        private static HttpClient client = new HttpClient() { BaseAddress = new Uri("https://management.azure.com") };

        public async Task<JObject> GetResource(string resourceId, string suffix = "")
        {
            string url = resourceId + "?api-version=2017-03-01" + (string.IsNullOrEmpty(suffix) ? "" : $"&{suffix}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            return JObject.Parse(responseContent);

        }
    }
}
