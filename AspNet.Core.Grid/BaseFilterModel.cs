using System.Collections;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace AspNet.Core.Grid
{
    public class BaseFilterModel
    {
        public BaseFilterModel()
        {
            
        }

        public string TargetGridId { get; set; }
        public string CallbackFunctionName { get; set; }
        public string FilterText { get; set; }
        public string SelectedIds { get; set; }
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRows { get; set; }
        public RouteValueDictionary RouteValues { get; set; }
        public Hashtable FilterParameters { get; set; }

        public async Task<bool> InitializeRouteValues()
        {
            RouteValues = new RouteValueDictionary
            {
                {"SortColumn", SortColumn},
                {"SortDirection", SortDirection},
                {"FilterText", FilterText},
                {"SelectedIds", SelectedIds}
            };
            return true;
        }
    }
}
