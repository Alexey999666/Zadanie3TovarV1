using System;
using System.Collections.Generic;

namespace Zadanie3TovarV1.ModelsDB;

public partial class Product
{
    public string ProductArticleNumber { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string ProductUnitMeasurement { get; set; } = null!;

    public decimal ProductCost { get; set; }

    public byte ProductDiscountAmountMax { get; set; }

    public string ProductManufacturer { get; set; } = null!;

    public string ProductSupplier { get; set; } = null!;

    public string ProductCategory { get; set; } = null!;

    public byte? ProductDiscountAmount { get; set; }

    public int ProductQuantityInStock { get; set; }

    public string ProductDescription { get; set; } = null!;

    public string? ProductPhoto { get; set; }

    public string ProductStatus { get; set; } = null!;

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
}
