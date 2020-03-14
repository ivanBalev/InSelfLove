using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDInSelfLove.Web.Controllers
{
    public class VideosController : BaseController
    {
        public IActionResult Index()
        {
            return this.View();
        }
    }
}
