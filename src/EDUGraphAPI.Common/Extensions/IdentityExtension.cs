﻿using System.Security.Claims;
using System.Security.Principal;

namespace EDUGraphAPI
{
    public static class IdentityExtension
    {
        static readonly string TenantId = "http://schemas.microsoft.com/identity/claims/tenantid";

        static readonly string ObjectIdentifier = "http://schemas.microsoft.com/identity/claims/objectidentifier";


        public static string GetTenantId(this ClaimsIdentity identity)
        {
            return identity.FindFirst(TenantId)?.Value;
        }

        public static string GetObjectIdentifier(this ClaimsIdentity identity)
        {
            return identity.FindFirst(ObjectIdentifier)?.Value;
        }

        public static string GetTenantId(this IPrincipal user)
        {
            var claimsIdentity = user.Identity as ClaimsIdentity;
            if (claimsIdentity == null) return null;
            return GetTenantId(claimsIdentity);
        }

        public static string GetObjectIdentifier(this IPrincipal user)
        {
            var claimsIdentity = user.Identity as ClaimsIdentity;
            if (claimsIdentity == null) return null;
            return GetObjectIdentifier(claimsIdentity);
        }

        public static void AddClaim(this ClaimsIdentity identity, string key, string value)
        {
            identity.AddClaim(new Claim(key, value));
        }

        public static void AddTenantIdClaim(this ClaimsIdentity identity, string tenantId)
        {
            identity.AddClaim(TenantId, tenantId);
        }

        public static void AddObjectIdentifierClaim(this ClaimsIdentity identity, string value)
        {
            AddClaim(identity, ObjectIdentifier, value);
        }
    }
}