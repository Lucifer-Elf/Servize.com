﻿using Servize.Domain.Model.VendorModel;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Servize.Domain.Model.OrderDetail
{
    public class OrderItem : BaseEntity
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public OrderSummary Order { get; set; }

        public int VendorId { get; set; }
        [ForeignKey(nameof(VendorId))]
        public Vendor Vendor { get; set; }

        public int Quantity { get; set; }
        public double PricePerItem { get; set; }
        public double ItemDiscount { get; set; }
        public DateTime OrderDateTimne { get; set; }
        public DateTime OrderPlacedDate { get; set; }

        [DefaultValue(5)]
        public int Vat { get; set; }
    }
}
