using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DTPortal.Web.ViewModel.Session
{
    public class JqueryDatatableParam
    {
        public string sEcho { get; set; }
        public string sSearch { get; set; }
        [JsonRequired]
        public int iDisplayLength { get; set; }
        [JsonRequired]
        public int iDisplayStart { get; set; }
        [JsonRequired]
        public int iColumns { get; set; }
        [JsonRequired]
        public int iSortCol_0 { get; set; }
        public string sSortDir_0 { get; set; }
        [JsonRequired]
        public int iSortingCols { get; set; }
        public string sColumns { get; set; }
        [JsonRequired]
        public int start { get; set; }
        [JsonRequired]
        public int length { get; set; }

    }
}
