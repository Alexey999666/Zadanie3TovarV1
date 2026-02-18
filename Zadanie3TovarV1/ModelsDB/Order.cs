using System;
using System.Collections.Generic;

namespace Zadanie3TovarV1.ModelsDB;

public partial class Order
{
    public int OrderId { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime OrderDeliveryDate { get; set; }

    public string OrderPickupPoint { get; set; } = null!;

    public string? OrderFioclient { get; set; }

    public string OrderCodeReceive { get; set; } = null!;

    public string OrderStatus { get; set; } = null!;

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
}
