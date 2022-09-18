﻿using Core.Enums;
using Domain.Common;

namespace Core.Entities
{
    public sealed class OrderDetail : BaseEntity
    {
        public string OrderId { get; set; }
        public int FoodId { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public OrderDetailStatus Status { get; set; }

        public Order Order { get; set; }
        public Food Food { get; set; }
    }
}
