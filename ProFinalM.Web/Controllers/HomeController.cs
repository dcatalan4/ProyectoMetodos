using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProFinalM.Web.Models;
using ProFinalM.Web.Services;

namespace ProFinalM.Web.Controllers;

public class HomeController : Controller
{
    private readonly NumericMethodService _numericMethodService = new();

    public IActionResult Index()
    {
        var model = new NumericMethodsViewModel();
        model.TaylorResult = _numericMethodService.CalculateTaylor(model.Taylor);
        model.LeastSquaresResult = _numericMethodService.CalculateLeastSquares(model.LeastSquares);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(NumericMethodsViewModel model, string method)
    {
        try
        {
            if (method == "taylor")
            {
                model.TaylorResult = _numericMethodService.CalculateTaylor(model.Taylor);
            }
            else if (method == "leastSquares")
            {
                model.LeastSquaresResult = _numericMethodService.CalculateLeastSquares(model.LeastSquares);
            }
        }
        catch (Exception ex)
        {
            model.ErrorMessage = ex.Message;
        }

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
