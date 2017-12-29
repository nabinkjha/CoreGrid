using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;

namespace AspNet.Core.Grid.TagHelpers
{
    [HtmlTargetElement("pageing", TagStructure = TagStructure.WithoutEndTag)]
    public class PagingTagHelper : TagHelper
    {
        public PagerOptions PagerOptions => new PagerOptions { RouteValues = Model.RouteValues };

        [HtmlAttributeName("model")]
        public BaseFilterModel Model { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "table-page";
            output.TagMode = TagMode.StartTagAndEndTag;
            var sb = new StringBuilder();
            sb.Append(ToHtmlString());
            sb.AppendFormat("<div>");
            output.PreContent.SetHtmlContent(sb.ToString());
            output.PostContent.SetHtmlContent("</div>");
        }

        public virtual PaginationModel BuildPaginationModel(Func<int, int, string> generateUrl)
        {
            var pageCount = (int)Math.Ceiling(Model.TotalRows / (double)Model.PageSize);
            var model = new PaginationModel { PageSize = Model.PageSize, CurrentPage = Model.PageNumber, TotalItemCount = Model.TotalRows, PageCount = pageCount, SearchString = Model.FilterText };

            // Previous
            model.PaginationLinks.Add(Model.PageNumber > 1 ? new PaginationLink { Active = true, DisplayText = "Previous", PageIndex = Model.PageNumber - 1, Url = generateUrl(Model.PageNumber - 1, Model.PageSize) } : new PaginationLink { Active = false, DisplayText = "Previous" });

            var start = 1;
            var end = pageCount;
            var nrOfPagesToDisplay = PagerOptions.MaxNrOfPages;

            if (pageCount > nrOfPagesToDisplay)
            {
                var middle = (int)Math.Ceiling(nrOfPagesToDisplay / 2d) - 1;
                var below = (Model.PageNumber - middle);
                var above = (Model.PageNumber + middle);

                if (below < 2)
                {
                    above = nrOfPagesToDisplay;
                    below = 1;
                }
                else if (above > (pageCount - 2))
                {
                    above = pageCount;
                    below = (pageCount - nrOfPagesToDisplay + 1);
                }

                start = below;
                end = above;
            }

            if (start > 1)
            {
                model.PaginationLinks.Add(new PaginationLink { Active = true, PageIndex = 1, DisplayText = "1", Url = generateUrl(1, Model.PageSize) });
                if (start > 3)
                {
                    model.PaginationLinks.Add(new PaginationLink { Active = true, PageIndex = 2, DisplayText = "2", Url = generateUrl(2, Model.PageSize) });
                }
                if (start > 2)
                {
                    model.PaginationLinks.Add(new PaginationLink { Active = false, DisplayText = "...", IsSpacer = true });
                }
            }

            for (var i = start; i <= end; i++)
            {
                if (i == Model.PageNumber || (Model.PageNumber <= 0 && i == 1))
                {
                    model.PaginationLinks.Add(new PaginationLink { Active = true, PageIndex = i, IsCurrent = true, DisplayText = i.ToString() });
                }
                else
                {
                    model.PaginationLinks.Add(new PaginationLink { Active = true, PageIndex = i, DisplayText = i.ToString(), Url = generateUrl(i, Model.PageSize) });
                }
            }
            if (end < pageCount)
            {
                if (end < pageCount - 1)
                {
                    model.PaginationLinks.Add(new PaginationLink { Active = false, DisplayText = "...", IsSpacer = true });
                }
                if (pageCount - 2 > end)
                {
                    model.PaginationLinks.Add(new PaginationLink { Active = true, PageIndex = pageCount - 1, DisplayText = (pageCount - 1).ToString(), Url = generateUrl(pageCount - 1, Model.PageSize) });
                }

                model.PaginationLinks.Add(new PaginationLink { Active = true, PageIndex = pageCount, DisplayText = pageCount.ToString(), Url = generateUrl(pageCount, Model.PageSize) });
            }

            // Next
            model.PaginationLinks.Add(Model.PageNumber < pageCount ? new PaginationLink { Active = true, PageIndex = Model.PageNumber + 1, DisplayText = "Next", Url = generateUrl(Model.PageNumber + 1, Model.PageSize) } : new PaginationLink { Active = false, DisplayText = "Next" });
            // Total No of Records
            model.PaginationLinks.Add(new PaginationLink { Active = false, DisplayText = string.Format("{0} Record{1}", Model.TotalRows, Model.TotalRows == 1 ? "" : "s"), IsSpacer = true });
            // Records per Page
            //model.PaginationLinks.Add(new PaginationLink { Active = false, DisplayText = string.Format("{0} Record{1}", totalItemCount, totalItemCount == 1 ? "" : "s"), IsSpacer = true });
            // AjaxOptions

            model.Options = PagerOptions;
            return model;
        }
        public string ToHtmlString()
        {
            var model = BuildPaginationModel(GeneratePageUrl);
            var pageSize = new List<int> { 10, 20, 100 };
            if (model.PageSize > 0 && !pageSize.Contains(model.PageSize))
                pageSize.Add(model.PageSize);
            var orderedPageSize = pageSize.OrderBy(p => p).ToList();
            // orderedPageSize.Add(0);


            var sb = new StringBuilder();
            if (model.PaginationLinks.Count >= 3)
            {
                sb.Append("<div class='tc'>");
                sb.Append("<ul class='pagination'>");
                foreach (var paginationLink in model.PaginationLinks)
                {
                    if (paginationLink.Active)
                    {
                        if (paginationLink.IsCurrent)
                        {
                            sb.Append("<li class=\"paginate_button active\">");
                            sb.Append($"<a href=\"#\" >{paginationLink.DisplayText}</a>");
                        }
                        else if (!paginationLink.PageIndex.HasValue)
                        {
                            sb.Append("<li class=\"paginate_button\">");
                            sb.AppendFormat(paginationLink.DisplayText);
                        }
                        else
                        {
                            sb.Append("<li class=\"paginate_button\">");
                            var linkBuilder = new StringBuilder("<a");
                            linkBuilder.AppendFormat(" href=\"{0}\">{1}</a>", paginationLink.Url, paginationLink.DisplayText);
                            sb.Append(linkBuilder);
                        }
                    }
                    else
                    {
                        if (!paginationLink.IsSpacer)
                        {
                            sb.Append("<li class=\"paginate_button disabled\">");
                            sb.Append($"<a href=\"#\" >{paginationLink.DisplayText}</a>");
                        }
                        else
                        {
                            sb.Append("<li class=\"paginate_button\">");
                            sb.Append($"<span class=\"spacer\">{paginationLink.DisplayText}</span>");
                        }
                    }
                    sb.Append("</li>");
                }
                sb.Append("<li>");
                sb.Append("<span class=\"spacer\" style=\"padding:4.4px 6px;\"><div class=\"spacer select-editable\">");
                sb.AppendFormat("<select id=\"PageSize\" onchange=\"filterByPageSize($(this).val(), '{0}', '{1}', '{2}',{3}) \" >", HttpUtility.UrlEncode(Model.FilterText), GeneratePageUrl(1, Model.PageSize), Model.TargetGridId, Model.CallbackFunctionName);
                foreach (var p in orderedPageSize)
                {
                    sb.AppendFormat(
                        p == model.PageSize
                            ? "<option selected=\"true\" "
                            : "<option ");
                    sb.AppendFormat("value=\"{0}\">{1}</option>", p, p == 0 ? "All" : p.ToString());
                }
                sb.Append("</select>");
                sb.AppendFormat("<input type=\"text\" name=\"pagesize\" value=\"{0}\" onchange=\"resetPageSize($(this).val(), '{1}', '{2}', '{3}', {4}) \" />", Model.PageSize, HttpUtility.UrlEncode(Model.FilterText), GeneratePageUrl(1, model.PageSize), Model.TargetGridId, Model.CallbackFunctionName);
                sb.Append("</div></span>");
                sb.Append("</li>");
                sb.Append("</ul>");
                sb.Append("</div>");
            }
            return sb.ToString();
        }
        protected virtual string GeneratePageUrl(int pageNumber, int pageSize)
        {
            var queryString = "";
            var routeDataValues = ViewContext.RouteData.Values;
            var pageLinkValueDictionary = new RouteValueDictionary(PagerOptions.RouteValues) { { PagerOptions.PageRouteValueKey, pageNumber } };
            if (pageSize != 0)
            {
                pageLinkValueDictionary.Add("PageSize", pageSize);
            }

            // To be sure we get the right route, ensure the controller and action are specified.
            if (!pageLinkValueDictionary.ContainsKey("controller") && routeDataValues.ContainsKey("controller"))
            {
               // pageLinkValueDictionary.Add("controller", routeDataValues["controller"]);
                queryString += $"/{routeDataValues["controller"]}";
            }
            if (!pageLinkValueDictionary.ContainsKey("action") && routeDataValues.ContainsKey("action"))
            {
               // pageLinkValueDictionary.Add("action", routeDataValues["action"]);
                queryString += $"/{routeDataValues["action"]}";
            }
            int ctr = 0;
            foreach (var item in pageLinkValueDictionary.Where(x=>x.Value !=null))
            {
                if (ctr ==0)
                {
                    queryString += $"?{item.Key}={item.Value}";
                }
                else
                {
                    queryString += $"&{item.Key}={item.Value}";
                }
               
                ctr++;
            }
            return queryString;
        }
    }
}