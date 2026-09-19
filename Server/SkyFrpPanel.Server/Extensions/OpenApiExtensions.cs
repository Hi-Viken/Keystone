using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SkyFrpPanel.Infrastructure.Helper;

namespace SkyFrpPanel.Extensions
{
    internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
            if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
            {
                var requirements = new Dictionary<string, IOpenApiSecurityScheme>
                {
                    ["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer", // "bearer" refers to the header name here
                        In = ParameterLocation.Header,
                        BearerFormat = "Json Web Token"
                    }
                };
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes = requirements;
            }
        }
    }

    /// <summary>
    /// XML 注释文档转换器 - 为 Tag（控制器）添加中文名称和描述
    /// </summary>
    internal sealed class XmlCommentDocumentTransformer(XmlCommentHelper xmlCommentHelper) : IOpenApiDocumentTransformer
    {
        // 已知包含控制器的程序集
        private static readonly Assembly[] ControllerAssemblies = new[]
        {
            typeof(SkyFrpPanel.Controllers.CommonController).Assembly,
            typeof(SkyFrpPanel.Mall.Controllers.BrandController).Assembly,
        };

        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            // 第一步：构建 英文Tag名 → 中文名称 的映射
            var tagChineseNames = new Dictionary<string, string>();

            if (document.Tags != null)
            {
                foreach (var tag in document.Tags)
                {
                    var controllerName = tag.Name + "Controller";
                    foreach (var assembly in ControllerAssemblies)
                    {
                        Type controllerType = null;
                        try
                        {
                            controllerType = assembly.GetTypes()
                                .FirstOrDefault(t => t.Name == controllerName
                                    && typeof(Microsoft.AspNetCore.Mvc.ControllerBase).IsAssignableFrom(t));
                        }
                        catch { }

                        if (controllerType != null)
                        {
                            var comment = xmlCommentHelper.GetTypeComment(controllerType, "summary");
                            if (!string.IsNullOrEmpty(comment))
                            {
                                tagChineseNames[tag.Name] = comment;
                            }
                            break;
                        }
                    }
                }
            }

            if (tagChineseNames.Count == 0)
                return Task.CompletedTask;

            // 第二步：重建 document.Tags（中文名 + 英文描述）
            var newDocTags = new HashSet<OpenApiTag>();
            if (document.Tags != null)
            {
                foreach (var tag in document.Tags)
                {
                    if (tagChineseNames.TryGetValue(tag.Name, out var chineseName))
                    {
                        newDocTags.Add(new OpenApiTag
                        {
                            Name = chineseName,
                            Description = tag.Description
                        });
                    }
                    else
                    {
                        newDocTags.Add(tag);
                    }
                }
            }
            document.Tags = newDocTags;

            // 第三步：遍历所有操作，替换 tag 引用为中文名
            if (document.Paths != null)
            {
                foreach (var pathItem in document.Paths.Values)
                {
                    if (pathItem.Operations == null) continue;
                    foreach (var op in pathItem.Operations.Values)
                    {
                        if (op.Tags != null)
                        {
                            var newTags = new HashSet<OpenApiTagReference>();
                            foreach (var t in op.Tags)
                            {
                                if (tagChineseNames.TryGetValue(t.Name, out var cn))
                                {
                                    newTags.Add(new OpenApiTagReference(cn));
                                }
                                else
                                {
                                    newTags.Add(t);
                                }
                            }
                            op.Tags = newTags;
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// XML 注释操作转换器 - 为 API 操作（方法）添加参数和返回值描述
    /// </summary>
    internal sealed class XmlCommentOperationTransformer(XmlCommentHelper xmlCommentHelper) : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            var endpointMetadata = context.Description.ActionDescriptor.EndpointMetadata;

            if (endpointMetadata.FirstOrDefault(m => m is MethodInfo) is MethodInfo methodInfo)
            {
                var summary = xmlCommentHelper.GetMethodComment(methodInfo, "summary");
                if (!string.IsNullOrEmpty(summary))
                {
                    operation.Summary = summary;
                }

                if (operation.Parameters != null)
                {
                    var parameters = methodInfo.GetParameters();
                    foreach (var param in parameters)
                    {
                        var paramComment = xmlCommentHelper.GetParameterComment(param);
                        if (!string.IsNullOrEmpty(paramComment))
                        {
                            var openApiParam = operation.Parameters.FirstOrDefault(p => p.Name == param.Name);
                            if (openApiParam != null)
                            {
                                openApiParam.Description = paramComment;
                            }
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
    }

    internal sealed class AddVersionToHeaderTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            var actionMetadata = context.Description.ActionDescriptor.EndpointMetadata;
            if (actionMetadata != null)
            {
                operation.Parameters ??= [];
                var apiVersionMetadata = actionMetadata.Any(metadataItem => metadataItem is AsParametersAttribute);
                if (apiVersionMetadata)
                {
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = "API-Version",
                        In = ParameterLocation.Header,
                        Description = "API Version header value",
                        Schema = new OpenApiSchema
                        {
                            Type = JsonSchemaType.String,
                            Description = "API Version"
                        }
                    });
                }
            }

            return Task.CompletedTask;
        }
    }
}
