using System.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OgrenciAidatSistemi.TagHelpers
{
    [HtmlTargetElement("list-page-link-item")]
    public class ListPageLinkItemTagHelper(IHtmlGenerator generator) : AnchorTagHelper(generator)
    {
        // Your properties and constructor here

        // [HtmlAttributeName("view-data")]
        // public ViewDataDictionary? ViewData { get; set; }

        [HtmlAttributeName("page-index")]
        public int? PageIndex { get; set; }

        [HtmlAttributeName("page-size")]
        public int? PageSize { get; set; }

        [HtmlAttributeName("search-string")]
        public string? SearchString { get; set; }

        [HtmlAttributeName("search-field")]
        public string? SearchField { get; set; }

        [HtmlAttributeName("sort-order")]
        public string? SortOrder { get; set; }

        [HtmlAttributeName("text")]
        public string? Text { get; set; }

        [HtmlAttributeName("class")]
        public string? Class { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "a";
            // if class not set, set it to page-link
            output.Attributes.SetAttribute("class", Class ?? "page-link");

            // Get the current query string parameters
            var query = ViewContext.HttpContext.Request.Query;

            // Set individual route values
            RouteValues["pageIndex"] = PageIndex?.ToString() ?? query["PageIndex"];
            RouteValues["pageSize"] = PageSize == 0 ? query["PageSize"] : PageSize.ToString();
            RouteValues["searchString"] = SearchString ?? query["SearchString"];
            RouteValues["searchField"] = SearchField ?? query["SearchField"];
            RouteValues["sortOrder"] = SortOrder ?? query["SortOrder"];

            // if value is null, remove it from the route values
            foreach (var routeValue in RouteValues.ToList())
            {
                if (string.IsNullOrEmpty(routeValue.Value))
                {
                    RouteValues.Remove(routeValue.Key);
                }
            }

            // Call the base Process method to generate the href attribute
            base.Process(context, output);

            if (Text != null)
            {
                output.Content.SetContent(Text);
            }
        }
    }
}
