using AutoMapper;
using Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Profiles;
using Services;
using System;
using System.Reflection;

namespace CitiesInfo.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddNewtonsoftJson(); //Service to fix API error:
            //The JSON value could not be converted to Microsoft.AspNetCore.JsonPatch.JsonPatchDocument

            services.AddControllers()
                .AddMvcOptions(o =>
                {
                    //o.OutputFormatters.Add(new XmlSerializerOutputFormatter());  

                    //The XmlSerializerOutputFormatter is an asp.net core outputformatter that uses the XmlSerializer internally, 
                    //whereas the DataContractSerializerOutputFormatter uses the DataContractSerializer internally.

                    //The DataContractSerializer is more flexible in configuration.For example it supports reference detection to prevent the serializer from recursively serializing items, 
                    //which would normally cause an endless loop.
                    //o.OutputFormatters.RemoveType(typeof(SystemTextJsonOutputFormatter)); //this changes the default to xml format

                    o.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());

                    //o.OutputFormatters.Add(new SystemTextJsonOutputFormatter(jsonSerializerOptions:null)); //since default is xml, now we again add json for specific requests
                });
            // 3 importants to add custom service, these are based on the lifetime 
            //services.AddScoped<>
            //  Created Once for each request
            //services.AddTransient<>
            //  Created everytime they are requested; for light wighted stateless service
            //services.AddSingleton<>
            //  Created the first time they are requested
#if DEBUG //Compiler Directives
            services.AddTransient<IMailService, LocalMailService>();
#else // means release
            services.AddTransient<IMailService, CloudMailService>();
#endif

            services.AddDbContext<CityInfoContext>(options =>  //addDbContext is scoped by default
            {
                options.UseSqlServer(Configuration["DbConnectionString:CityInfoDb"]);
            });

            //service for database get,update,delete etc
            services.AddScoped<ICityInfoRepository, CityInfoRepository>();
            
            //we need to load this assembly of profiles in order to get all the profiles into automapper,
            //since these are different projects, the Profiles Class Library isnt loaded
            //so we manually load it

            string path = @"C:\Users\Koderlabs\Desktop\Projects\CityInfoApi\CitiesInfo\Profiles\bin\Debug\netcoreapp3.1\Profiles.dll";
            Assembly ProfilesAssembly = Assembly.LoadFile(path);

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());  //this will search for profiles in all assemblies

            //we can also do this to add an indivisual profile
            //var type = typeof(CityProfile);
            //services.AddAutoMapper(type);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            //app.UseStatusCodePages(); // uses status codes pages that returns texts

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
