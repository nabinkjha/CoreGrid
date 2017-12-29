using System.Collections;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;

namespace AspNet.Core.Grid.TagHelpers
{
    [HtmlTargetElement("sort", TagStructure = TagStructure.WithoutEndTag)]
    public class SortingTagHelper : TagHelper
    {
        public PagerOptions PagerOptions => new PagerOptions { RouteValues = Model.RouteValues };

        [HtmlAttributeName("model")]
        public BaseFilterModel Model { get; set; }
        [HtmlAttributeName("url")]
        public string Url { get; set; }

        [HtmlAttributeName("column-name")]
        public string ColumnName { get; set; }
        [HtmlAttributeName("display-name")]
        public string DisplayName { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "th";
            output.TagMode = TagMode.StartTagAndEndTag;
            var sb = new StringBuilder();
            sb.Append(SortColumn());
            output.PreContent.SetHtmlContent(sb.ToString());
        }
        public string SortColumn() 
        {
            var excludedParameter = new[] { "SortColumn", "SortDirection", "PageNumber", "PageSize", "FilterText", "SelectedIds" };
            string queryString = $"?SortColumn={ColumnName}";
            int pageNumber = Model.PageNumber == 0 ? 1 : Model.PageNumber;
            int pageSize = Model.PageSize == 0 ? 8 : Model.PageSize;
            queryString = $"{queryString}&SortDirection={SetSortDirection(ColumnName, Model.SortColumn, Model.SortDirection)}";
            queryString = $"{queryString}&PageNumber={pageNumber}";
            queryString = $"{queryString}&PageSize={pageSize}";
            var hashTable = Model.FilterParameters;
            if (hashTable != null)
            {
                queryString = hashTable.Cast<DictionaryEntry>().Where(dictionaryEntry => !excludedParameter.Contains(dictionaryEntry.Key)).Aggregate(queryString, (current, dictionaryEntry) => string.Format("{0}&{1}={2}", current, dictionaryEntry.Key, dictionaryEntry.Value));
            }
            var icon = SetSortIcon(ColumnName, Model.SortColumn, Model.SortDirection);
            var propertyName = DisplayName??ColumnName;
            var routeDataValues = ViewContext.RouteData.Values;
            var url = "";
            var pageLinkValueDictionary = new RouteValueDictionary(PagerOptions.RouteValues);
            if (!pageLinkValueDictionary.ContainsKey("controller") && routeDataValues.ContainsKey("controller"))
            {
                // pageLinkValueDictionary.Add("controller", routeDataValues["controller"]);
                url += $"/{routeDataValues["controller"]}";
            }
            if (!pageLinkValueDictionary.ContainsKey("action") && routeDataValues.ContainsKey("action"))
            {
                // pageLinkValueDictionary.Add("action", routeDataValues["action"]);
                url += $"/{routeDataValues["action"]}";
            }
            return $@"<a class='col-sort' style='padding: 0' href='{Url??url}{queryString}'>{propertyName}{icon}</a>";
        }

        private static string SetSortDirection(string columnName, string sortColumn, string sortDirection)
        {
            var result = "Ascending";
            if (!string.IsNullOrWhiteSpace(sortColumn) && sortColumn.ToLower() == columnName.ToLower())
            {
                if (sortDirection.ToLower() == "ascending")
                {
                    result = "Descending";
                }
            }
            return result;
        }

        private static string SetSortIcon(string propertyName, string sortColumn, string sortDirection)
        {
            var result = string.Empty;
            if (propertyName != sortColumn) return result;
            result = sortDirection.ToLower() == "ascending" ? "<span class='glyphicon glyphicon glyphicon-chevron-up pull-right'></span>" : "<span class='glyphicon glyphicon glyphicon glyphicon-chevron-down pull-right'></span>";
            return result;
        }
    }

   
}