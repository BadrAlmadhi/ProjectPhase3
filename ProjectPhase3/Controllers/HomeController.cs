using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProjectPhase3.Models;

namespace ProjectPhase3.Controllers;

public class HomeController: Controller
{

    public IActionResult Index()
    {
        if (User.IsInRole("Student"))
        {
            return Redirect("/Student/Index");
        }
        if (User.IsInRole("Professor"))
        {
            return Redirect("/Professor/Index");
        }
        if (User.IsInRole("Administrator"))
        {
            return Redirect("/Administrator/Index");
        }

        return View();
    }


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

