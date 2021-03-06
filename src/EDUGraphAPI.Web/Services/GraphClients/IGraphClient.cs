﻿using EDUGraphAPI.Web.Models;
using System.Threading.Tasks;

namespace EDUGraphAPI.Web.Services.GraphClients
{
    public interface IGraphClient
    {
        Task<UserInfo> GetCurrentUserAsync();

        Task<TenantInfo> GetTenantAsync(string tenantId);
    }
}