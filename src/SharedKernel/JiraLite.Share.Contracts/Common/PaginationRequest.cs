using System;
using System.Text.Json.Serialization;

namespace JiraLite.Share.Common;

public class PaginationRequest(int pageIndex = 1, int pageSize = 10)

{
    public int PageIndex { get; set; } = pageIndex;
    public int PageSize { get; set; } = pageSize;

}

