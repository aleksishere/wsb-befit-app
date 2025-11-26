using System.Security.Claims;
using BeFit.Data;
using BeFit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeFit.Controllers
{
    [Authorize]
    public class StatsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var fourWeeksAgo = DateTime.Now.AddDays(-28);

            var stats = await _context.ExerciseEntries
                .Where(ee => ee.TrainingSession.StartTime >= fourWeeksAgo &&
                             ee.TrainingSession.UserId == userId)
                .GroupBy(ee => ee.ExerciseType.Name)
                .Select(g => new ExerciseStatsViewModel 
                {
                    ExerciseTypeName = g.Key,
                    SessionsCount = g.Count(),
                    TotalRepetitions = g.Sum(ee => ee.Sets * ee.RepetitionsPerSet),
                    AverageLoad = g.Average(ee => (float?)ee.Load) ?? 0,
                    MaxLoad = g.Max(ee => (float?)ee.Load) ?? 0
                })
                .ToListAsync();

            return View(stats);
        }
    }
}
