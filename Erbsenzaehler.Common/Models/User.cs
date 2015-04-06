﻿using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Erbsenzaehler.Models
{
    public class User : IdentityUser
    {
        public int ClientId { get; set; }
        public virtual Client Client { get; set; }
        public SummaryMailIntervalOptions SummaryMailInterval { get; set; }
        public ICollection<SummaryMailLog> SummaryMailLogs { get; set; }


        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}