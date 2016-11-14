﻿using EDUGraphAPI.Infrastructure;
using EDUGraphAPI.Models;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Education;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;

namespace EDUGraphAPI.Utils
{
    public enum Permissions
    {
        Delegated,
        Application
    }

    public class AuthenticationHelper
    {
        public static async Task<ActiveDirectoryClient> GetActiveDirectoryClientAsync(Permissions permissions = Permissions.Delegated)
        {
            var accessToken = await GetAccessTokenAsync(Constants.Resources.AADGraph, permissions);
            var serviceRoot = new Uri(new Uri(Constants.Resources.AADGraph), ClaimsPrincipal.Current.GetTenantId());
            return new ActiveDirectoryClient(serviceRoot, () => Task.FromResult(accessToken));
        }

        public static async Task<GraphServiceClient> GetGraphServiceClientAsync(Permissions permissions = Permissions.Delegated)
        {
            var accessToken = await GetAccessTokenAsync(Constants.Resources.MSGraph, permissions);
            var serviceRoot = Constants.Resources.MSGraph + "/v1.0/" + ClaimsPrincipal.Current.GetTenantId();
            return new GraphServiceClient(serviceRoot, new BearerAuthenticationProvider(accessToken));
        }

        public static async Task<EducationServiceClient> GetEducationServiceClientAsync(Permissions permissions = Permissions.Delegated)
        {
            var accessToken = await GetAccessTokenAsync(Constants.Resources.AADGraph, permissions);
            var serviceRoot = new Uri(new Uri(Constants.Resources.AADGraph), ClaimsPrincipal.Current.GetTenantId());
            return new EducationServiceClient(serviceRoot, () => Task.FromResult(accessToken));
        }

        public static ActiveDirectoryClient GetActiveDirectoryClient(AuthenticationResult result)
        {
            var serviceRoot = new Uri(new Uri(Constants.Resources.AADGraph), result.TenantId);
            return new ActiveDirectoryClient(serviceRoot, () => Task.FromResult(result.AccessToken));
        }

        public static GraphServiceClient GetGraphServiceClient(AuthenticationResult result)
        {
            var serviceRoot = Constants.Resources.MSGraph + "/v1.0/" + ClaimsPrincipal.Current.GetTenantId();
            return new GraphServiceClient(serviceRoot, new BearerAuthenticationProvider(result.AccessToken));
        }

        public static async Task<string> GetAccessTokenAsync(string resource, Permissions permissions = Permissions.Delegated)
        {
            var result = await GetAuthenticationResult(resource, permissions);
            return result.AccessToken;
        }

        public static async Task<AuthenticationResult> GetAuthenticationResult(string resource, Permissions permissions)
        {
            var context = GetAuthenticationContext(ClaimsPrincipal.Current.Identity as ClaimsIdentity, permissions);
            var clientCredential = new ClientCredential(Constants.AADClientId, Constants.AADClientSecret);

            if (permissions == Permissions.Delegated)
            {
                var userObjectId = ClaimsPrincipal.Current.GetObjectIdentifier();
                var userIdentifier = new UserIdentifier(userObjectId, UserIdentifierType.UniqueId);
                return await context.AcquireTokenSilentAsync(resource, clientCredential, userIdentifier);
            }
            else if (permissions == Permissions.Application)
                return await context.AcquireTokenAsync(resource, clientCredential);

            else
                throw new NotImplementedException();
        }

        public static AuthenticationContext GetAuthenticationContext(ClaimsIdentity claimsIdentity, Permissions permissions)
        {
            var tenantID = claimsIdentity.GetTenantId();
            var userId = claimsIdentity.GetObjectIdentifier();
            var signedInUserID = permissions == Permissions.Delegated ? userId : tenantID;

            var authority = string.Format("{0}{1}", Constants.AADInstance, tenantID);
            var tokenCache = new ADALTokenCache(signedInUserID);
            return new AuthenticationContext(authority, tokenCache);
        }

        public static async Task<AuthenticationResult> GetAuthenticationResultAsync(string authorizationCode)
        {
            var credential = new ClientCredential(Constants.AADClientId, Constants.AADClientSecret);
            var authContext = new AuthenticationContext(Constants.Authority);
            var redirectUri = new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path));
            return await authContext.AcquireTokenByAuthorizationCodeAsync(authorizationCode, redirectUri, credential);
        }

        public static async Task<string> GetAppOnlyAccessTokenForDaemonAppAsync(string resource, string certPath, string certPassword, string tenantId)
        {
            var authority = string.Format("{0}{1}", Constants.AADInstance, tenantId);
            var authenticationContext = new AuthenticationContext(authority, false);
            var cert = GetClientAssertionCertificate(certPath, certPassword);
            var authenticationResult = await authenticationContext.AcquireTokenAsync(resource, cert);
            return authenticationResult.AccessToken;
        }

        private static ClientAssertionCertificate GetClientAssertionCertificate(string certPath, string certPassword)
        {
            var certData = System.IO.File.ReadAllBytes(certPath);
            var flags = X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet;
            var cert = new X509Certificate2(certData, certPassword, flags);
            return new ClientAssertionCertificate(Constants.AADClientId, cert);
        }
    }
}