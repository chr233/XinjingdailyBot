using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace XinjingdailyBot.WebAPI.Authorization;

//public class AppendAuthorizeFilter : IOperationFilter
//{
//    public void Apply(OpenApiOperation operation, OperationFilterContext context)
//    {
//        var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
//        var isAuthorized = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is AuthorizationAttribute);

//        if (isAuthorized)
//        {
//            if (operation.Parameters == null)
//                operation.Parameters = new List<OpenApiParameter>();

//            operation.Parameters.Add(new OpenApiParameter {
//                Name = "SYSTOKEN",
//                In = ParameterLocation.Header,
//                Description = "身份验证",
//                Required = false
//            });
//        }
//    }
//}
//using Microsoft.AspNetCore.Http;
//using System;
//using System.IdentityModel.Tokens.Jwt;
//using System.Linq;
//using System.Threading.Tasks;

//public class JwtAuthenticationMiddleware
//{
//    private readonly RequestDelegate _next;

//    public JwtAuthenticationMiddleware(RequestDelegate next)
//    {
//        _next = next;
//    }

//    public async Task Invoke(HttpContext context)
//    {
//        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

//        if (token != null)
//        {
//            AttachUserToContext(context, token);
//        }

//        await _next(context);
//    }

//    private void AttachUserToContext(HttpContext context, string token)
//    {
//        try
//        {
//            var tokenHandler = new JwtSecurityTokenHandler();
//            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

//            // 在这里可以根据需要获取用户信息和权限信息
//            var userId = jwtToken.Claims.First(x => x.Type == "userId").Value;
//            var roles = jwtToken.Claims.Where(x => x.Type == "role").Select(x => x.Value).ToList();

//            // 将用户信息和权限信息附加到HttpContext中，以便后续使用
//            context.Items["UserId"] = userId;
//            context.Items["Roles"] = roles;
//        }
//        catch (Exception)
//        {
//            // 处理无效的Token或其他错误
//            // 可以根据需要进行日志记录或其他操作
//        }
//    }
//}

