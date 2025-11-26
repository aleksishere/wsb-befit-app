using System.Security.Claims;
using BeFit.Data;
using BeFit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BeFit.Controllers;

[Authorize]
public class ExerciseEntriesController : Controller
{
    private readonly ApplicationDbContext _context;
    
    public ExerciseEntriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var applicationDbContext =
            _context.ExerciseEntries
                .Include(e => e.ExerciseType)
                .Include(e => e.TrainingSession)
                .Where(e => e.TrainingSession.UserId == userId);
        return View(await applicationDbContext.ToListAsync());
    }
    
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var exerciseEntry = await _context.ExerciseEntries
            .Include(e => e.ExerciseType)
            .Include(e => e.TrainingSession)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (exerciseEntry == null)
        {
            return NotFound();
        }

        if (exerciseEntry.TrainingSession.UserId != userId) return Forbid();

        return View(exerciseEntry);
    }
    
    public IActionResult Create()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ViewData["ExerciseTypeId"] = new SelectList(_context.ExerciseTypes, "Id", "Name");
        ViewData["TrainingSessionId"] = new SelectList(
            _context.TrainingSessions.Where(e => e.UserId == userId),
            "Id", "StartTime");
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExerciseEntry exerciseEntry)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Console.WriteLine($"Próba zapisu: SessionId={exerciseEntry.TrainingSessionId}, TypeId={exerciseEntry.ExerciseTypeId}");
        ModelState.Remove(nameof(exerciseEntry.TrainingSession));
        ModelState.Remove(nameof(exerciseEntry.ExerciseType));
        var isMySession = await _context.TrainingSessions
            .AnyAsync(e => e.Id == exerciseEntry.TrainingSessionId && e.UserId == userId);
        if (!isMySession)
        {
            Console.WriteLine("BŁĄD: Sesja nie istnieje lub nie należy do użytkownika! (Zwracam Forbid)");
            return Forbid();
        }
        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(exerciseEntry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.Write($"Błąd zapisu: {ex.Message}");
                throw;
            }
        }
        ViewData["ExerciseTypeId"] = new SelectList(_context.ExerciseTypes, "Id", "Name", exerciseEntry.ExerciseTypeId);
        ViewData["TrainingSessionId"] = new SelectList(_context.TrainingSessions.Where(e => e.UserId == userId), "Id", "StartTime", exerciseEntry.TrainingSessionId);
        return View(exerciseEntry);
    }
    
    // GET: ExerciseEntries/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Pobieramy wpis wraz z sesją, aby sprawdzić uprawnienia
        var exerciseEntry = await _context.ExerciseEntries
            .Include(e => e.TrainingSession) 
            .FirstOrDefaultAsync(m => m.Id == id);

        if (exerciseEntry == null) return NotFound();

        // ZABEZPIECZENIE: Czy ten wpis należy do mnie?
        if (exerciseEntry.TrainingSession.UserId != userId) return Forbid();

        // Dropdowny: Sortowane i filtrowane (tylko moje sesje)
        ViewData["ExerciseTypeId"] = new SelectList(
            _context.ExerciseTypes.OrderBy(e => e.Name), 
            "Id", "Name", exerciseEntry.ExerciseTypeId);
            
        ViewData["TrainingSessionId"] = new SelectList(
            _context.TrainingSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.StartTime), 
            "Id", "StartTime", exerciseEntry.TrainingSessionId);
            
        return View(exerciseEntry);
    }

    // POST: ExerciseEntries/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ExerciseEntry exerciseEntry)
    {
        if (id != exerciseEntry.Id) return NotFound();
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 1. ZABEZPIECZENIE: Sprawdź, czy docelowa sesja należy do użytkownika
        var isMySession = await _context.TrainingSessions
            .AnyAsync(s => s.Id == exerciseEntry.TrainingSessionId && s.UserId == userId);

        if (!isMySession) return Forbid();

        // 2. ZABEZPIECZENIE: Sprawdź, czy edytowany wpis (oryginalny) należy do użytkownika
        // (zapobiega edycji cudzego wpisu przez zmianę ID w narzędziach deweloperskich)
        var originalEntryOwnerId = await _context.ExerciseEntries
            .Where(e => e.Id == id)
            .Select(e => e.TrainingSession.UserId)
            .FirstOrDefaultAsync();

        if (originalEntryOwnerId != userId) return Forbid();

        // Czyszczenie walidacji
        ModelState.Remove(nameof(exerciseEntry.TrainingSession));
        ModelState.Remove(nameof(exerciseEntry.ExerciseType));

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(exerciseEntry);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExerciseEntryExists(exerciseEntry.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // Przywracanie dropdownów w razie błędu walidacji
        ViewData["ExerciseTypeId"] = new SelectList(
            _context.ExerciseTypes.OrderBy(e => e.Name), 
            "Id", "Name", exerciseEntry.ExerciseTypeId);
            
        ViewData["TrainingSessionId"] = new SelectList(
            _context.TrainingSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.StartTime), 
            "Id", "StartTime", exerciseEntry.TrainingSessionId);
            
        return View(exerciseEntry);
    }
    
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var exerciseEntry = await _context.ExerciseEntries
            .Include(e => e.ExerciseType)
            .Include(e => e.TrainingSession)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (exerciseEntry == null)
        {
            return NotFound();
        }
        
        if( exerciseEntry.TrainingSession.UserId != userId) return Forbid();

        return View(exerciseEntry);
    }
    
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var exerciseEntry = await _context.ExerciseEntries
            .Include(e => e.TrainingSession)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (exerciseEntry != null)
        {
            if (exerciseEntry.TrainingSession.UserId != userId) return Forbid();
            _context.ExerciseEntries.Remove(exerciseEntry);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ExerciseEntryExists(int id)
    {
        return _context.ExerciseEntries.Any(e => e.Id == id);
    }
}