﻿namespace PerfumeShop.Web.ViewModels;

public class PagedInfoViewModel
{
    public int PageId { get; set; }
    public int TotalPages { get; set; }
    public int ItemsPerPage { get; set; }
    public int TotalItems { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}
