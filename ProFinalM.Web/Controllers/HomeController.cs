using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProFinalM.Web.Models;
using ProFinalM.Web.Services;

namespace ProFinalM.Web.Controllers;

public class HomeController : Controller
{
    private readonly NumericMethodService _numericMethodService = new();
    private readonly ChallengeScoreService _challengeScoreService;

    public HomeController(ChallengeScoreService challengeScoreService)
    {
        _challengeScoreService = challengeScoreService;
    }

    public IActionResult Index()
    {
        var model = new NumericMethodsViewModel();
        model.TaylorResult = _numericMethodService.CalculateTaylor(model.Taylor);
        model.LeastSquaresResult = _numericMethodService.CalculateLeastSquares(model.LeastSquares);
        return View(model);
    }

    public IActionResult Historia()
    {
        return View();
    }

    public IActionResult Retos()
    {
        return View(new ChallengeViewModel
        {
            Scores = _challengeScoreService.GetTopScores().ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveScore([FromBody] SaveScoreRequest request)
    {
        try
        {
            _challengeScoreService.Save(request);
            return Json(new
            {
                ok = true,
                scores = _challengeScoreService.GetTopScores()
            });
        }
        catch (Exception ex)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return Json(new { ok = false, message = ex.Message });
        }
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
