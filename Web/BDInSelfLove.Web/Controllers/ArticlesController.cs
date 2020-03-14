using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDInSelfLove.Web.Controllers
{
    public class ArticlesController : BaseController
    {
        public IActionResult Index()
        {
            return this.View();
        }

        public IActionResult Single(int id)
        {
            return this.View();
        }
    }
}
