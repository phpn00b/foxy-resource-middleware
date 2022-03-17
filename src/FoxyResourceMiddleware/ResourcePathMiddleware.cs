using FoxyResourceMiddleware.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;

namespace FoxyResourceMiddleware;

public class ResourcePathMiddleware
{
	private const int StreamCopyBufferSize = 64 * 1024;

	//private readonly ILogger _logger;
	private readonly bool _isDisabled;
	private readonly RequestDelegate _next;
	private readonly IConfiguration _configuration;
	private readonly IEmbeddedResourceCache _embeddedResourceCache;

	public ResourcePathMiddleware(RequestDelegate next, IConfiguration configuration, IEmbeddedResourceCache embeddedResourceCache)
	{
		_next = next;
		_configuration = configuration;
		_embeddedResourceCache = embeddedResourceCache;
		_isDisabled = _configuration["FoxyResourceMiddleware:Disabled"] == "true";
	}

	private async Task<string> GetContentType(string resource)
	{
		var ext = resource.Split('.', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
		switch (ext)
		{
			case "css":
				return "text/css";
			case "js":
				return "application/javascript";
			case "png":
				return "image/png";
			case "jpg":
				return "image/jpeg";
			case "gif":
				return "image/gif";
			case "svg":
				return "image/svg+xml";
			case "woff":
				return "application/font-woff";
			case "woff2":
				return "application/font-woff2";
			case "ttf":
				return "application/font-ttf";
			case "eot":
				return "application/vnd.ms-fontobject";
			case "otf":
				return "application/font-otf";
			case "mp4":
				return "video/mp4";
			case "webm":
				return "video/webm";
			case "ogg":
				return "video/ogg";
			case "mp3":
				return "audio/mpeg";
			case "wav":
				return "audio/wav";
			case "zip":
				return "application/zip";
			case "pdf":
				return "application/pdf";
			case "doc":
				return "application/msword";
			case "docx":
				return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
			case "xls":
				return "application/vnd.ms-excel";
			case "xlsx":
				return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
			case "ppt":
				return "application/vnd.ms-powerpoint";
			case "pptx":
				return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
			case "json":
				return "application/json";
			default:
				return "text/plain";
		}
	}
	public async Task Invoke(HttpContext httpContext)
	{
		if (httpContext.Request.Path.StartsWithSegments("/embedded-resources"))
		{
			if (!_isDisabled)
			{
				var cachedResource = _embeddedResourceCache.GetResourceStream(httpContext.Request.Path.Value.Replace("/embedded-resources/", ""));
				if (cachedResource != null)
				{
					string contentType = await GetContentType(httpContext.Request.Path.Value);
					httpContext.Response.ContentType = contentType;
					//await httpContext.Response.Send.Body.WriteAsync(cachedResource, 0, cachedResource.Length);
					using (var stream = new MemoryStream(cachedResource))
					{
						await StreamCopyOperation.CopyToAsync(stream, httpContext.Response.Body, cachedResource.Length, StreamCopyBufferSize, httpContext.RequestAborted);
					}

					return;
				}
			}
		}

		await _next(httpContext); // calling next middleware
	}
}