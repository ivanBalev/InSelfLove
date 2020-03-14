namespace BDInSelfLove.Web.Infrastructure.TagHelpers
{

    using Microsoft.AspNetCore.Razor.TagHelpers;

    [HtmlTargetElement("h3", Attributes = "[brand-name], [font-awesome-icon]")]
    public class BrandTagHelper : TagHelper
    {
        public string BrandName { get; set; }

        public string FontAwesomeIcon { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Content.SetHtmlContent($"<i class=\"{this.FontAwesomeIcon}\"></i> ");
            output.Content.Append(this.BrandName);

            base.Process(context, output);
        }
    }
}
