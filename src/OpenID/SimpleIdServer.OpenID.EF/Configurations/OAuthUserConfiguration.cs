﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using SimpleIdServer.OAuth.Domains;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
namespace SimpleIdServer.OpenID.EF.Configurations
{
    public class OAuthUserConfiguration : IEntityTypeConfiguration<OAuthUser>
    {
        public void Configure(EntityTypeBuilder<OAuthUser> builder)
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Claims).HasConversion(
                c => JsonConvert.SerializeObject(c.Select(d => new KeyValuePair<string, string>(d.Type, d.Value))),
                c => JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(c).Select(kvp => new Claim(kvp.Key, kvp.Value)).ToList());
            builder.HasMany(u => u.Consents).WithOne().OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(u => u.Credentials).WithOne().OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(u => u.Sessions).WithOne().OnDelete(DeleteBehavior.Cascade);
        }
    }
}