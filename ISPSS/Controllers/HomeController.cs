using ISPSS.Models;
using ISPSS.Services;
using Microsoft.AspNetCore.Mvc;
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
            


            return View();
        }

        public async Task<IActionResult> Networks()        
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Networks(Subdomain obj)
        {
            if (obj.domain.Contains('.')) {
                ModelState.AddModelError("", "Not a valid subdomain. Subdomain should not contain '.'");
            }

            
            IPAddress[] addresses = [];
            if (obj.domain != null)
            {
                string subdomain = obj.domain;
                var ispss_vault_address = $"vault-{subdomain}.privilegecloud.cyberark.cloud";
                try
                {
                    addresses = await Dns.GetHostAddressesAsync(ispss_vault_address);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Not a valid ISPSS subdomain.");
                    return View(); 
                }

                string awsRegion = string.Empty;



                if (addresses.Length > 0)
                {
                    string ip = addresses[0].ToString();
                    // Do something with the IP address
                    ViewData["IP"] = ip;
                    awsregionobj = new AwsRegionResolverService("Misc/ip.json");
                    awsRegion = awsregionobj.GetAwsRegionByIpAddress(ip);
                    obj.AwsRegion = awsRegion;
                    ViewData["region"] = awsRegion;
                }
                else
                {
                    ViewData["IP"] = ("No IP addresses found for the specified host.");
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

            return View(obj);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
