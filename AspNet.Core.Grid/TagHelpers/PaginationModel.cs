using System.Collections.Generic;

namespace AspNet.Core.Grid.TagHelpers
{
    public class PaginationModel
    {
        public int PageSize { get; internal set; }
        public int CurrentPage { get; internal set; }
        public int PageCount { get; internal set; }
        public int TotalItemCount { get; internal set; }
        public string SearchString { get; internal set; }
        public IList<PaginationLink> PaginationLinks { get; private set; }
        public PagerOptions Options { get; internal set; }

        public PaginationModel()
        {
            PaginationLinks = new List<PaginationLink>();
            Options = null;
        }
    }

    public class PaginationLink
    {
        public bool Active { get; set; }

        public bool IsCurrent { get; set; }

        public int? PageIndex { get; set; }

        public string DisplayText { get; set; }

        public string Url { get; set; }

        public bool IsSpacer { get; set; }
    }
}