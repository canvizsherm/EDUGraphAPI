﻿using System;

namespace EDUGraphAPI.Utils
{
    public static class AuthorizationHelper
    {
        public static class Prompt
        {
            public static readonly string Consent = "consent";
            public static readonly string Login = "login";
            public static readonly string AdminConsent = "admin_consent";
        }

        public static string GetUrl(string redirectUrl, string state, string resource, string prompt = null)
        {
            var url = string.Format("{0}oauth2/authorize?response_type=code&client_id={1}&resource={2}&redirect_uri={3}&state={4}",
                Constants.Authority,
                Uri.EscapeDataString(Constants.AADClientId),
                Uri.EscapeDataString(resource),
                Uri.EscapeDataString(redirectUrl),
                Uri.EscapeDataString(state)
            );
            if (prompt.IsNotNullAndEmpty()) url += "&prompt=" + Uri.EscapeDataString(prompt);
            return url;
        }
    }
}