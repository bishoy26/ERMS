using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERMS.Data;
using ERMS.Models;

namespace ERMS.Controllers
{
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProjectController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Project
        // For Admin and Manager: show all; for Employee: show only projects in which they have tasks.
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Employee"))
            {
                string currentUserId = _userManager.GetUserId(User);
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.IdentityUserId == currentUserId);
                if (employee == null)
                    return RedirectToAction("AccessDenied", "Home", new { message = "Employee record not found." });

                var projects = await _context.Projects
                    .Where(p => p.Tasks.Any(t => t.AssignedEmployee.EmployeeID == employee.EmployeeID))
                    .ToListAsync();
                return View(projects);
            }
            else
            {
                return View(await _context.Projects.ToListAsync());
            }
        }

        // GET: Project/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectID == id);
            if (project == null)
                return NotFound();
            return View(project);
        }

        // GET: Project/Create – Only Admin and Manager
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Project/Create – Only Admin and Manager
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([Bind("ProjectID,Name,Description,StartDate,EndDate")] Project project)
        {
            if (ModelState.IsValid)
            {
                // If a Manager is creating, assign their user id to ManagerId.
                if (User.IsInRole("Manager"))
                    project.ManagerId = _userManager.GetUserId(User);
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Project/Edit/5 – Only Admin and Manager (Managers can edit only their own projects)
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound();

            if (User.IsInRole("Manager"))
            {
                string currentUserId = _userManager.GetUserId(User);
                if (project.ManagerId != currentUserId)
                    return RedirectToAction("AccessDenied", "Home", new { message = "You can only edit projects assigned to you." });
            }
            return View(project);
        }

        // POST: Project/Edit/5 – Only Admin and Manager.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("ProjectID,Name,Description,StartDate,EndDate,ManagerId")] Project project)
        {
            if (id != project.ProjectID)
                return NotFound();

            if (User.IsInRole("Manager"))
            {
                string currentUserId = _userManager.GetUserId(User);
                if (project.ManagerId != currentUserId)
                    return RedirectToAction("AccessDenied", "Home", new { message = "You can only edit projects assigned to you." });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.ProjectID))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Project/Delete/5 – Only Admin and Manager.
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectID == id);
            if (project == null)
                return NotFound();
            if (User.IsInRole("Manager"))
            {
                string currentUserId = _userManager.GetUserId(User);
                if (project.ManagerId != currentUserId)
                    return RedirectToAction("AccessDenied", "Home", new { message = "You can only delete projects assigned to you." });
            }
            return View(project);
        }

        // POST: Project/Delete/5 – Only Admin and Manager.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                if (User.IsInRole("Manager"))
                {
                    string currentUserId = _userManager.GetUserId(User);
                    if (project.ManagerId != currentUserId)
                        return RedirectToAction("AccessDenied", "Home", new { message = "You can only delete projects assigned to you." });
                }
                _context.Projects.Remove(project);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.ProjectID == id);
        }
    }
}
