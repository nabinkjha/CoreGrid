using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;

namespace Core.Utilities.TagHelpers
{
    [HtmlTargetElement("pageing", TagStructure = TagStructure.WithoutEndTag)]
    public class PagingTagHelper : TagHelper
    {
        public PagerOptions PagerOptions => new PagerOptions {  };

        [HtmlAttributeName("model")]
        public BaseFilterModel Model { get; set; }

        [HtmlAttributeName("url")]
        public string Url { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            Model = Model ?? (BaseFilterModel)ViewContext.ViewData.Model;
            output.TagName = "table-page";
            output.TagMode = TagMode.StartTagAndEndTag;
            var sb = new StringBuilder();
            sb.Append(ToHtmlString());
            sb.AppendFormat("<div>");
            output.PreContent.SetHtmlContent(sb.ToString());
            output.PostContent.SetHtmlContent("</div>");
        }

        public virtual PaginationModel BuildPaginationModel(Func<int, int, string, string> generateUrl)
        {
            var pageCount = (int)Math.Ceiling(Model.TotalRows / (double)Model.PageSize);
            var model = new PaginationModel { PageSize = Model.PageSize, CurrentPage = Model.PageNumber, TotalItemCount = Model.TotalRows, PageCount = pageCount, SearchString = Model.FilterText };

            // Previous
            model.PaginationLinks.Add(Model.PageNumber > 1 ? new PaginationLink { Active = true, DisplayText = "Previous", PageIndex = Model.PageNumber - 1, Url = generateUrl(Model.PageNumber - 1, Model.PageSize, Model.FilterText) } : new PaginationLink { Active = false, DisplayText = "Previous" });

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
                model.PaginationLinks.Add(new PaginationLink { Active = true, PageIndex = 1, DisplayText = "1", Url = generateUrl(1, Model.PageSize, Model.FilterText) });
                if (start > 3)
                {
                    model.PaginationLinks.Add(new PaginationLink { Active = true, PageIndex = 2, DisplayText = "2", Url = generateUrl(2, Model.PageSize, Model.FilterText) });
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
                    model.PaginationLinks.Add(new PaginationLink { Active = true, PageIndex = i, DisplayText = i.ToString(), Url = generateUrl(i, Model.PageSize, Model.FilterText) });
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
                    model.PaginationLinks.Add(new PaginationLink { Active = true, PageIndex = pageCount - 1, DisplayText = (pageCount - 1).ToString(), Url = generateUrl(pageCount - 1, Model.PageSize, Model.FilterText) });
                }

                model.PaginationLinks.Add(new PaginationLink { Active = true, PageIndex = pageCount, DisplayText = pageCount.ToString(), Url = generateUrl(pageCount, Model.PageSize, Model.FilterText) });
            }

            // Next
            model.PaginationLinks.Add(Model.PageNumber < pageCount ? new PaginationLink { Active = true, PageIndex = Model.PageNumber + 1, DisplayText = "Next", Url = generateUrl(Model.PageNumber + 1, Model.PageSize, Model.FilterText) } : new PaginationLink { Active = false, DisplayText = "Next" });
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
                sb.Append("<div class='d-flex flex-row justify-content-center'>");
                sb.Append("<ul class='pagination'>");
                foreach (var paginationLink in model.PaginationLinks)
                {
                    if (paginationLink.Active)
                    {
                        if (paginationLink.IsCurrent)
                        {
                            sb.Append("<li class='paginate_button page-item active'>");
                            sb.Append($"<a class='page-link' href='#' >{paginationLink.DisplayText}</a>");
                        }
                        else if (!paginationLink.PageIndex.HasValue)
                        {
                            sb.Append("<li class='paginate_button page-item '>");
                            sb.AppendFormat(paginationLink.DisplayText);
                        }
                        else
                        {
                            sb.Append("<li class='paginate_button page-item '>");
                            var linkBuilder = new StringBuilder("<a");
                            linkBuilder.AppendFormat(" class='page-link' href='{0}'>{1}</a>", paginationLink.Url, paginationLink.DisplayText);
                            sb.Append(linkBuilder);
                        }
                    }
                    else
                    {
                        if (!paginationLink.IsSpacer)
                        {
                            sb.Append("<li class='paginate_button page-item  disabled'>");
                            sb.Append($"<a class='page-link' href='#' >{paginationLink.DisplayText}</a>");
                        }
                        else
                        {
                            sb.Append("<li class='paginate_button page-item '>");
                            sb.Append($"<span class='spacer'>{paginationLink.DisplayText}</span>");
                        }
                    }
                    sb.Append("</li>");
                }
                sb.Append("<li class='paginate_button page-item m-l'>");
                sb.Append("<a disabled='disabled' class='page-link' href='#' >select</a>");
                sb.Append("<div class='pagination'>");
                sb.AppendFormat("<select style='height:28px' id='PageSize' onchange=\"filterByPageSize($(this).val(), '{0}', '{1}', '{2}',{3}) \" >", HttpUtility.UrlEncode(Model.FilterText), GeneratePageUrl(1, Model.PageSize, Model.FilterText), Model.TargetGridId, Model.CallbackFunctionName);
                foreach (var p in orderedPageSize)
                {
                    sb.AppendFormat(
                        p == model.PageSize
                            ? "<option selected='true' "
                            : "<option ");
                    sb.AppendFormat("value='{0}'>{1}</option>", p, p == 0 ? "All" : p.ToString());
                }
                sb.Append("</select>");
                sb.Append("</li>");
                sb.Append("<li class='paginate_button page-item m-l'>");
                sb.Append("<a disabled='disabled' class='page-link' href='#' >custom</a>");
                sb.AppendFormat("<input style='width:50px;height:28px' type='text' name='pagesize' value=\"{0}\" onchange=\"resetPageSize($(this).val(), '{1}', '{2}', '{3}', {4}) \" />", Model.PageSize, HttpUtility.UrlEncode(Model.FilterText), GeneratePageUrl(1, model.PageSize, Model.FilterText), Model.TargetGridId, Model.CallbackFunctionName);
                sb.Append("</div>");
                sb.Append("</li>");
                sb.Append("</ul>");
                sb.Append("</div>");
            }
            return sb.ToString();
        }
        protected virtual string GeneratePageUrl(int pageNumber, int pageSize, string filterText)
        {
            var queryString = Url;
            var routeDataValues = ViewContext.RouteData.Values;
            var pageLinkValueDictionary = new RouteValueDictionary() { { PagerOptions.PageRouteValueKey, pageNumber } };
            if (pageSize != 0)
            {
                pageLinkValueDictionary.Add("PageSize", pageSize);
            }

            if (!string.IsNullOrWhiteSpace(filterText))
            {
                pageLinkValueDictionary.Add("FilterText", filterText);
            }
            if (string.IsNullOrWhiteSpace(queryString))
            {
                // To be sure we get the right route, ensure the controller and action are specified.
                if (!string.IsNullOrWhiteSpace(Model.ControllerName))
                {
                    queryString += $"/{ Model.ControllerName}";
                }
                if (!string.IsNullOrWhiteSpace(Model.ActionName))
                {
                    queryString += $"/{ Model.ActionName}";
                }
                if (!pageLinkValueDictionary.ContainsKey("controller") && routeDataValues.ContainsKey("controller") && string.IsNullOrWhiteSpace(Model.ControllerName))
                {
                    // pageLinkValueDictionary.Add("controller", routeDataValues["controller"]);
                    queryString += $"/{routeDataValues["controller"]}";
                }
                if (!pageLinkValueDictionary.ContainsKey("action") && routeDataValues.ContainsKey("action") && string.IsNullOrWhiteSpace(Model.ActionName))
                {
                    // pageLinkValueDictionary.Add("action", routeDataValues["action"]);
                    queryString += $"/{routeDataValues["action"]}";
                }

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
