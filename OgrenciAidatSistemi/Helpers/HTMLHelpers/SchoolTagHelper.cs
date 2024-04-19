using Microsoft.AspNetCore.Razor.TagHelpers;
using OgrenciAidatSistemi.Models;

namespace OgrenciAidatSistemi.TagHelpers
{
    public class SchoolTagHelper : TagHelper
    {
        public SchoolView? SchoolView { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (SchoolView == null)
            {
                NotFound(output);
                return;
            }
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "card");

            output.Content.AppendHtml($"<div>ID: {SchoolView.Id}</div>");
            output.Content.AppendHtml($"<div>Name: {SchoolView.Name}</div>");
            output.Content.AppendHtml($"<div>Created At: {SchoolView.CreatedAt}</div>");
            output.Content.AppendHtml($"<div>Updated At: {SchoolView.UpdatedAt}</div>");

            if (SchoolView.SchoolAdmins != null)
            {
                output.Content.AppendHtml("<div>School Admins:</div>");
                output.Content.AppendHtml("<ul>");
                foreach (var schoolAdmin in SchoolView.SchoolAdmins)
                {
                    output.Content.AppendHtml($"<li>{schoolAdmin.EmailAddress}</li>");
                }
                output.Content.AppendHtml("</ul>");
            }
        }

        public void NotFound(TagHelperOutput output)
        {
            output.TagName = "p";
            output.Attributes.SetAttribute("class", "alert alert-danger");
            output.Content.SetHtmlContent("School not found!");
        }
    }
}
