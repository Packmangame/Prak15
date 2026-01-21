using System;
using System.Collections.Generic;

namespace Prakt15.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public double Rating { get; set; }

    public DateOnly CreatedAt { get; set; }

    public int CategoryId { get; set; }

    public int BrandId { get; set; }

    public virtual Brand Brand { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    public string CategoryName => Category?.Name ?? string.Empty;
    public string TagsString => Tags != null ? string.Join(", ", Tags.Select(t => $"#{t.Name}")) : string.Empty;
    public bool IsLowStock => Stock < 10;

}
