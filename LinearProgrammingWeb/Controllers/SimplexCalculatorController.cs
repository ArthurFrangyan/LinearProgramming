using LinearProgrammingWeb.Models;
using Microsoft.AspNetCore.Mvc;
using SimplexMethod;

namespace LinearProgrammingWeb.Controllers;

public class SimplexCalculatorController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    [HttpPost("")]
    public IActionResult Calculate(string tableString)
    {
        try
        {
            var table = new Table(tableString);
            var calculationResult = Simplex.Calculate(table);
            return View("Index", new SimplexMethodModel(calculationResult));
        }
        catch (Exception)
        {
            return RedirectToAction("Index");
        }
    }
}