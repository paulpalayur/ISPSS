using ISPSS.Models;
using ISPSS.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Diagnostics;
using System.Net;

namespace ISPSS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private Identity identityObj;
        private Task<string> identityUrl;
        private string IdentityTenantId;
        private Task<string?> IdentityPODId;
        private AwsRegionResolverService awsregionobj;
        
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            this.ViewData["Index"] = "TestHome";

            Log.Information($"The source IP is {Request.Headers["X-Forwarded-For"].ToString()}");

            return View();
        }

        public async Task<IActionResult> Networks()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Networks(Subdomain obj)
        {
            string remoteIP = Request.Headers["X-Forwarded-For"].ToString();
            Log.Information($"The source IP is {remoteIP}");
            if (obj.domain.Contains('.'))
            {
                Log.Information($"Subdomain: {obj.domain}, Remote IP {remoteIP}");
                ModelState.AddModelError("", "Subdomain should not contain '.'");
                return View();
            }

            IPAddress[] addresses = [];
            if (obj.domain != null)
            {
                string subdomain = obj.domain;
                var ispss_vault_address = $"vault-{subdomain}.privilegecloud.cyberark.cloud";
                Log.Information($"Subdomain: {obj.domain}, Remote IP {remoteIP}");
                try
                {
                    addresses = await Dns.GetHostAddressesAsync(ispss_vault_address);
                }
                catch (Exception ex)
                {                    
                    Log.Error(ex, $"Subdomain: {obj.domain}, Remote IP {remoteIP}");
                    ModelState.AddModelError("", "Not a valid ISPSS subdomain.");
                    return View();
                }

                string awsRegion = string.Empty;

                if (addresses.Length > 0)
                {
                    string ip = addresses[0].ToString();
                    ViewData["IP"] = ip;
                    awsregionobj = new AwsRegionResolverService("Misc/ip-ranges.json");
                    awsRegion = awsregionobj.GetAwsRegionByIpAddress(ip);
                    obj.AwsRegion = awsRegion;
                    ViewData["region"] = awsRegion;
                }
                else
                {
                    ViewData["IP"] = ("No IP addresses found for the specified host.");
                    Log.Error($"Subdomain: {obj.domain}, Remote IP {remoteIP} .Could not find IP address for the specified subdomain");
                }
                identityObj = new Identity(subdomain);
                await (identityUrl = identityObj.GetFinalRedirection());
                if (identityUrl.Result != null)
                {
                    IdentityTenantId = identityObj.GetTenantId(identityUrl.Result);
                    obj.IdentityTenantId = IdentityTenantId;
                    await (IdentityPODId = identityObj.GetPodId(identityUrl.Result));
                    obj.IdentityPodId = IdentityPODId.Result;

                    ViewData["IdentityURL"] = identityUrl.Result;
                    ViewData["IdentityTenantID"] = IdentityTenantId;
                }
            }
            Log.Information($"object: {obj}");
            return View(obj);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [HttpPost]
        public IActionResult DownloadFile(string content) {
            
            byte[] fileContents = System.Text.Encoding.UTF8.GetBytes(content);
            string fileName = "ISP_Networks.txt";            
            string contentType = "text/plain";
            
            return File(fileContents, contentType, fileName);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
