using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Controlzmo.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        [BindProperty]
        [Display(Name = "First Field")]
        //[DataType(DataType.Date)]
        public String Field1 { get; set; }
        [BindProperty]
        [Display(Name = "Second Field")]
        public String Field2 { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
            Field1 = "value 1";
            Field2 = "value 2";
        }

        public void OnGet()
        {

        }
    }
}
