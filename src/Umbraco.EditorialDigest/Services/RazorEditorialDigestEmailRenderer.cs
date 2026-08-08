using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace Umbraco.EditorialDigest.Services;

public sealed class RazorEditorialDigestEmailRenderer : IEditorialDigestEmailRenderer
{
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;

    public RazorEditorialDigestEmailRenderer(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IServiceProvider serviceProvider)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
    }

    public async Task<string> RenderAsync(EditorialDigestEmailModel model, CancellationToken cancellationToken = default)
    {
        var actionContext = new ActionContext(new DefaultHttpContext { RequestServices = _serviceProvider }, new RouteData(), new ActionDescriptor());
        var view = FindView(actionContext, EditorialDigestTemplatePathResolver.Resolve(model.Configuration))
            ?? FindView(actionContext, EditorialDigestTemplatePathResolver.GetDefaultPath())
            ?? throw new InvalidOperationException("The Editorial Digest email template could not be found.");

        await using var writer = new StringWriter();
        var viewData = new ViewDataDictionary<EditorialDigestEmailModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model };
        var viewContext = new ViewContext(actionContext, view, viewData, new TempDataDictionary(actionContext.HttpContext, _tempDataProvider), writer, new HtmlHelperOptions());
        await view.RenderAsync(viewContext);
        cancellationToken.ThrowIfCancellationRequested();
        return writer.ToString();
    }

    private IView? FindView(ActionContext actionContext, string path)
    {
        var result = _viewEngine.GetView(null, path, true);
        return result.Success ? result.View : null;
    }
}
