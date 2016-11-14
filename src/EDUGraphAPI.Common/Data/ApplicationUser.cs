﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDUGraphAPI.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int? OrganizationId { get; set; }

        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; }

        public string Token { get; set; }

        public string O365UserId { get; set; }

        public string O365Email { get; set; }

        public string JobTitle { get; set; }

        public string Department { get; set; }

        public string Mobile { get; set; }

        public string FavoriteColor { get; set; }

        public string FullName => FirstName + " " + LastName;

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            if (Organization != null)
                userIdentity.AddTenantIdClaim(Organization.TenantId);

            if (O365UserId != null)
                userIdentity.AddObjectIdentifierClaim(O365UserId);

            var roles = await manager.GetRolesAsync(Id);
            foreach (var role in roles) userIdentity.AddClaim(ClaimTypes.Role, role);

            return userIdentity;
        }
    }
}