using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.DTOs
{
    
    public class CategoriesDTO
    {
        public int Id { get; set; }

        public string CategoryName { get; set; }

        public string LabelName { get; set; }

        public DateTime? CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }
    }
}

