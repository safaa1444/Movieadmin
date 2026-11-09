
using Microsoft.AspNetCore.Mvc;
using Auth0.AspNetCore.Authentication;

namespace Movieadmin.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            var properties = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri("/")
                .Build();

            return Challenge(properties, Auth0Constants.AuthenticationScheme);
        }

        public IActionResult Logout()
        {
            var properties = new LogoutAuthenticationPropertiesBuilder()
                .WithRedirectUri("/")
                .Build();

            return SignOut(
                properties,
                Auth0Constants.AuthenticationScheme,
                "Cookies");
        }
    }
}
