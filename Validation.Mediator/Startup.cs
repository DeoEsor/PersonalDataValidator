using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQSender.Interfaces;
using RabbitMQSender.Sender;
using Validation.Mediator.Services;

namespace Validation.Mediator
{
    public class Startup
    {
        public static IConfiguration Configuration { get; set; }
        public Startup(IConfiguration configuration)
        {
            if (configuration == null) 
                throw new ArgumentNullException(nameof(configuration));
            
            var builder = new ConfigurationBuilder()
                .AddJsonFile("validatorsConfig.json", optional: false, reloadOnChange: true);
            configuration = builder.Build();
            Configuration = configuration;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddSingleton<IRPCMQSender<NSPValidationRequest, NSPValidationReply>,
                MqRpcSender<NSPValidationRequest, NSPValidationReply>>
                (s =>
                    new MqRpcSender<NSPValidationRequest, NSPValidationReply>(Configuration.GetSection("NSPValidator"), s.GetService<ILogger>()));
            services.AddSingleton<IRPCMQSender<AddressValidationRequests, AddressValidationReplies>,
                MqRpcSender<AddressValidationRequests, AddressValidationReplies>>
                (s =>
                    new MqRpcSender<AddressValidationRequests, AddressValidationReplies>(Configuration.GetSection("AddressValidator"), s.GetService<ILogger>()));
            services.AddSingleton<IRPCMQSender<EmailValidationRequests, EmailValidationReplies>,
                MqRpcSender<EmailValidationRequests, EmailValidationReplies>>
                (s =>
                    new MqRpcSender<EmailValidationRequests, EmailValidationReplies>(Configuration.GetSection("EmailValidator"), s.GetService<ILogger>()));
            services.AddSingleton<IRPCMQSender<PhoneNumberValidationRequests, PhoneNumberValidationReplies>,
                MqRpcSender<PhoneNumberValidationRequests, PhoneNumberValidationReplies>>
                (s =>
                    new MqRpcSender<PhoneNumberValidationRequests, PhoneNumberValidationReplies>(Configuration.GetSection("PhoneNumberValidator"), s.GetService<ILogger>()));
            services.AddSingleton<IRPCMQSender<BirthDayValidationRequest, BirthDayValidationReply>,
                MqRpcSender<BirthDayValidationRequest, BirthDayValidationReply>>
                (s =>
                    new MqRpcSender<BirthDayValidationRequest, BirthDayValidationReply>(Configuration.GetSection("BirthDayValidator"), s.GetService<ILogger>()));
            
            services.AddSingleton<MediatorService>(); 
            
        }

        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<MediatorService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
