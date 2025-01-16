using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Security;
using System.Security.Claims;

namespace WTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        public record UserInfo(string User, string Password);

        [HttpPost]
        public bool Post([FromForm] UserInfo data)
        {
            if (data.Password == "QWE")
            {
                this.HttpContext.Session.SetString("user", data.User);
                return true;
            }
            return false;
        }
    }
}
