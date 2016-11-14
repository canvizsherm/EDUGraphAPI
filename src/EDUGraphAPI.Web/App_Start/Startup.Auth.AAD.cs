﻿using EDUGraphAPI.Data;
using EDUGraphAPI.Utils;
using EDUGraphAPI.Web.Exceptions;
using EDUGraphAPI.Web.Services.GraphClients;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace EDUGraphAPI.Web
{
    public partial class Startup
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public void ConfigureAADAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions { });

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    Caption = "Microsoft Work or school account",
                    ClientId = Constants.AADClientId,
                    Authority = Constants.Authority,
                    TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
                    {
                        // instead of using the default validation (validating against a single issuer value, as we do in line of business apps), 
                        // we inject our own multitenant validation logic
                        ValidateIssuer = false,
                    },
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        RedirectToIdentityProvider = (context) =>
                        {
                            // This ensures that the address used for sign in and sign out is picked up dynamically from the request
                            // this allows you to deploy your app (to Azure Web Sites, for example)without having to change settings
                            // Remember that the base URL of the address used here must be provisioned in Azure AD beforehand.
                            string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;
                            context.ProtocolMessage.RedirectUri = appBaseUrl + "/";
                            context.ProtocolMessage.PostLogoutRedirectUri = appBaseUrl;
                            return Task.FromResult(0);
                        },
                        // we use this notification for injecting our custom logic
                        //SecurityTokenValidated = async (context) =>
                        //{
                        //    var tenantID = context.AuthenticationTicket.Identity.GetTenantId();
                        //    if (!await db.Organizations.AnyAsync(a => a.TenantId == tenantID && a.AdminConsented))
                        //        throw new TenantNotByAdminConsentException();
                        //},
                        //SecurityTokenValidated = (context) =>
                        //{
                        //    var identity = context.AuthenticationTicket.Identity;
                        //    return Task.FromResult(0);
                        //},
                        AuthorizationCodeReceived = async (context) =>
                        {
                            var identity = context.AuthenticationTicket.Identity;

                            // Get token with authorization code
                            var redirectUri = new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path));
                            var credential = new ClientCredential(Constants.AADClientId, Constants.AADClientSecret);
                            var authContext = AuthenticationHelper.GetAuthenticationContext(identity, Permissions.Delegated);
                            var authResult = await authContext.AcquireTokenByAuthorizationCodeAsync(context.Code, redirectUri, credential, Constants.Resources.AADGraph);

                            // Get user's roles and add them to claims
                            var activeDirectoryClient = authResult.CreateActiveDirectoryClient();
                            var graphClient = new AADGraphClient(activeDirectoryClient);
                            var user = await graphClient.GetCurrentUserAsync();
                            foreach (var role in user.Roles)
                                identity.AddClaim(ClaimTypes.Role, role);
                        },
                        AuthenticationFailed = (context) =>
                        {
                            var redirectUrl = context.Exception is TenantNotByAdminConsentException ? "/Admin/" : "/Home/Error";
                            context.OwinContext.Response.Redirect(redirectUrl);
                            context.HandleResponse(); // Suppress the exception
                            return Task.FromResult(0);
                        }
                    }
                });
        }
    }
}