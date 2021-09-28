# Persistence

By default, all the assets like “Representation”,”Schemas” are stored in memory. The following data storage can be used. 

## SQLServer

**Pre-requisite** : SCIM2.0 API must be installed in the Visual Studio Solution. 

SQL Server data storage can be configured like this : 

1. Install the Nuget package `SimpleIdServer.Scim.Persistence.EF`. 
2. Install the Nuget package `Microsoft.EntityFrameworkCore.SqlServer`.
3. Create a class `SCIMMigration` and replace the content with the following code. The namespace and connectionstring must be updated. 

```
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SimpleIdServer.Scim.Persistence.EF;
using System.Reflection;
namespace <<NAMESPACE>>
{
    public class SCIMMigration : IDesignTimeDbContextFactory<SCIMDbContext> 
    { 
        public SCIMDbContext CreateDbContext(string[] args) 
        { 
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name; 
            var builder = new DbContextOptionsBuilder<SCIMDbContext>(); 
            builder.UseSqlServer("<<CONNECTIONSTRING>>", optionsBuilder => optionsBuilder.MigrationsAssembly(migrationsAssembly));
            return new SCIMDbContext(builder.Options); 
        }
    }
}
```

4. Open a command prompt, navigate to the directory of your project and execute the command line : `dotnet ef add Init`. The migration scripts will be created. 
5. Execute the command line `dotnet ef database update`. The tables will be created in the database.
6. Open the "Startup.cs" file, inside the "ConfigureServices" procedure add the following line : 

```
var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
services.AddScimStoreEF(options => 
{ 
	options.UseSqlServer(Configuration.GetConnectionString("db"), o => o.MigrationsAssembly(migrationsAssembly)); 
}); 
```

7. Open the “Startup.cs” file, create a new private `InitializeDatabase` method. It will be called by the `Configure` procedure in order to create the database. Each SCIM Schema must be added into the SCIMSchemaLst collection. 

```
private void InitializeDatabase(IApplicationBuilder app) 
{
	using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope()) 
	{ 
		using (var context = scope.ServiceProvider.GetService<SCIMDbContext>()) 
		{ 
			context.Database.Migrate(); 
			var userSchema = SCIMSchemaExtractor.Extract(Path.Combine(basePath, "UserSchema.json"), SCIMConstants.SCIMEndpoints.User, true);
			var basePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Schemas"); 
			if (!context.SCIMSchemaLst.Any())
			{
				context.SCIMSchemaLst.Add(userSchema.ToModel()); 
			}

			context.SaveChanges(); 
		}
	}
}
```

8. Run the application and verify JSON is returned when you browse the following url : `https://localhost:<<sslPort>/Schemas`. 


## MongoDB

**Pre-requisite** : SCIM2.0 API must be installed in the Visual Studio Solution. 

MongoDB data storage can be configured like this : 

1. Install the Nuget package `SimpleIdServer.Scim.Persistence.MongoDB`.
2. Open the “Startup.cs” file, inside the `ConfigureServices` procedure add the following line adn replace the parameters.

```
var basePath = Path.Combine(Env.ContentRootPath, "Schemas"); 
var userSchema = SCIMSchemaExtractor.Extract(Path.Combine(basePath, "UserSchema.json"), SCIMConstants.SCIMEndpoints.User, true); 
var schemas = new List<SCIMSchema> { userSchema }; 
services.AddScimStoreMongoDB(opt => 
{ 
	opt.ConnectionString = "<<CONNECTION_STRING>>"; 
	opt.Database = "<<DATABASE_NAME>>";  
	opt.CollectionSchemas = “mappings”; 
	opt.CollectionSchemas = “schemas”; 
    opt.CollectionRepresentations = ”representations”; 
	opt.SupportTransaction = <<SUPPORT_TRANSACTION>>; 
}, schemas); 
```

3. Run the application and verify JSON is returned when you browse the following url : `https://localhost:<<sslPort>/Schemas`. 

# Swagger

There is a problem when swagger is installed in a SCIM2.0 project. All the properties of SCIM representations are documented in the SCIM Schema and by default Swagger is not able to fetch them  : 

1. Install the Nuget package `SimpleIdServer.Scim.Swashbuckle`.
2. Open the “Startup.cs” file, copy and past the code below into `ConfigureService` :

```
services.AddSwaggerGen(c => 
{ 
	var currentAssembly = Assembly.GetExecutingAssembly(); 
	var xmlDocs = currentAssembly.GetReferencedAssemblies() 
		.Union(new AssemblyName[] { currentAssembly.GetName() }) 
		.Select(a => Path.Combine(Path.GetDirectoryName(currentAssembly.Location), $"{a.Name}.xml")) 
		.Where(f => File.Exists(f)).ToArray(); 
	Array.ForEach(xmlDocs, (d) => 
	{ 
		c.IncludeXmlComments(d); 
	}); 
});
services.AddSCIMSwagger();   
```

3. Browse the URL `https://localhost:<sslPort>/swagger`, the documentation is now fetched from the SCIM Schemas. 