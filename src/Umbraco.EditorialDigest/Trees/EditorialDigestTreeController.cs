using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.EditorialDigest.Constants;
using Umbraco.Extensions;

namespace Umbraco.EditorialDigest.Trees;

[PluginController(EditorialDigestConstants.AreaName)]
[Tree(Umbraco.Cms.Core.Constants.Applications.Settings, EditorialDigestConstants.SettingsTreeAlias, TreeTitle = EditorialDigestConstants.PackageName, SortOrder = 20)]
public sealed class EditorialDigestTreeController : TreeController
{
    private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;

    public EditorialDigestTreeController(
        ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IMenuItemCollectionFactory menuItemCollectionFactory,
        IEventAggregator eventAggregator)
        : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
    {
        _menuItemCollectionFactory = menuItemCollectionFactory;
    }

    protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
    {
        var nodes = new TreeNodeCollection();
        if (id != Umbraco.Cms.Core.Constants.System.Root.ToInvariantString())
        {
            return nodes;
        }

        nodes.Add(CreateNode("digests", id, queryStrings, "Digests", "icon-mail", "settings/editorialDigest/digests"));
        nodes.Add(CreateNode("global-settings", id, queryStrings, "Global Settings", "icon-settings", "settings/editorialDigest/global-settings"));
        return nodes;
    }

    protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        => _menuItemCollectionFactory.Create();

    private TreeNode CreateNode(string id, string parentId, FormCollection queryStrings, string title, string icon, string routePath)
    {
        var node = CreateTreeNode(id, parentId, queryStrings, title, icon, false);
        node.RoutePath = routePath;
        node.MenuUrl = null;
        return node;
    }
}
