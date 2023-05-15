using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using NugetVetPet.Models;
using System.Security.Claims;

namespace MvcVetPet.Helpers
{
    public class HelperClaims
    {
        private IHttpContextAccessor contextAccessor;

        public HelperClaims(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        public async Task GetClaims(Usuario user)
        {
            ClaimsIdentity identity =
                    new ClaimsIdentity
                    (CookieAuthenticationDefaults.AuthenticationScheme,
                    ClaimTypes.Name, ClaimTypes.Role);

            Claim ClaimName = new Claim(ClaimTypes.Name, user.Apodo.ToString());
            identity.AddClaim(ClaimName);

            Claim ClaimId = new Claim(ClaimTypes.NameIdentifier
                , user.IdUsuario.ToString());
            identity.AddClaim(ClaimId);

            Claim claimImagen =
                new Claim("Imagen", user.Imagen.ToString());
            identity.AddClaim(claimImagen);

            Claim claimEmail =
                new Claim("Email", user.Email.ToString());
            identity.AddClaim(claimEmail);

            Claim claimTelefono =
               new Claim("Telefono", user.Telefono.ToString());
            identity.AddClaim(claimTelefono);

            Claim claimNombre =
               new Claim("Nombre", user.Nombre.ToString());
            identity.AddClaim(claimNombre);

            Claim claimRole =
                new Claim(ClaimTypes.Role, "Usuario");
            identity.AddClaim(claimRole);

            ClaimsPrincipal userPrincipal =
                new ClaimsPrincipal(identity);

            await this.contextAccessor.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                        userPrincipal, new AuthenticationProperties
                        {
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                        });
        }
    }
}
