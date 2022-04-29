using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQSender.Interfaces;
using RabbitMQSender.Sender;
using Validation.Mediator.Services;

namespace Validation.Mediator
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddTransient<IRPCMQSender<NSPValidationRequest, NSPValidationReply>,
                MqRpcSender<NSPValidationRequest, NSPValidationReply>>();
            services.AddTransient<IRPCMQSender<AddressValidationRequests, AddressValidationReplies>,
                MqRpcSender<AddressValidationRequests, AddressValidationReplies>>();
            services.AddTransient<IRPCMQSender<EmailValidationRequests, EmailValidationReplies>,
                MqRpcSender<EmailValidationRequests, EmailValidationReplies>>();
            services.AddTransient<IRPCMQSender<PhoneNumberValidationRequests, PhoneNumberValidationReplies>,
                MqRpcSender<PhoneNumberValidationRequests, PhoneNumberValidationReplies>>();
            services.AddTransient<IRPCMQSender<BirthDayValidationRequest, BirthDayValidationReply>,
                MqRpcSender<BirthDayValidationRequest, BirthDayValidationReply>>();
            
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
