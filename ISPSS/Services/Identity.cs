using System.Net;
using System;
using static System.Net.WebRequestMethods;

namespace ISPSS.Services
{
    public class Identity(string subdomain)
    {
        private readonly string ispss_tenant_url = $"https://{subdomain}.cyberark.cloud";
        private Task<string?> IdentitySystemInfo;
        private string? podId;

        public async Task<string> GetFinalRedirection()
        {
            string finalUrl = string.Empty;
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync(ispss_tenant_url, HttpCompletionOption.ResponseHeadersRead);

                    if (response.Headers.Location != null)
                    {
                        finalUrl = response.Headers.Location.ToString();
                    }
                    else if ((response.StatusCode == System.Net.HttpStatusCode.Redirect ||
                             response.StatusCode == System.Net.HttpStatusCode.OK) &&
                             response.RequestMessage != null &&
                             response.RequestMessage.RequestUri != null)
                    {
                        finalUrl = response.RequestMessage.RequestUri.Host;     
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return finalUrl;
        }
        public string GetTenantId(string IdentityURL)
        {
            string tenantId = string.Empty;
            if (!string.IsNullOrEmpty(IdentityURL))
            {
                string[] parts = IdentityURL.Split(['.']);
                if (parts.Length > 0)
                {
                    tenantId = parts[0];
                }
            }
            return tenantId;
        }

        private async Task<string?> GetIdnetitySystemInfo(string IdentityURL) {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync($"https://{IdentityURL}/sysinfo/version");

                    // Ensure a successful response before reading the content
                    response.EnsureSuccessStatusCode();

                    // Read the content as a string
                    string content = await response.Content.ReadAsStringAsync();

                    return content;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        private string ParseIdentityPod(string identitySysinfo)
        {
            try
            {
                // Parse the JSON content
                dynamic sysinfoObject = Newtonsoft.Json.JsonConvert.DeserializeObject(identitySysinfo);

                // Extract the desired information
                string name = sysinfoObject.Result.Name;
                string identitypod = name.Split('.')[0];

                return identitypod;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing Identity Pod: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetPodId(string IdentityURL)
        {
            await (IdentitySystemInfo = GetIdnetitySystemInfo(IdentityURL));
            if(IdentitySystemInfo.Result != null)
            {
                podId = ParseIdentityPod(IdentitySystemInfo.Result);
            }
            
            return podId;
        }
    }
}
