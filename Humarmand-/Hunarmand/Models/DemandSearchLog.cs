using System;

namespace Hunarmand.Models
{
    public class DemandSearchLog
    {
        public int SearchID { get; set; }
        public int? CategoryID { get; set; }
        public int? CustomerID { get; set; }
        public DateTime SearchedAt { get; set; }
    }
}
