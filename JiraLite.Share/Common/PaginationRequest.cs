using System;

namespace JiraLite.Share.Common;

public class PaginationRequest(int pageIndex = 0, int pageSize = 10)

{
    public int PageIndex { get; set; } = pageIndex;
    public int PageSize { get; set; } = pageSize;

}

