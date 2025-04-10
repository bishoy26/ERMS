using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ERMS.Data;
using ERMS.Models;

namespace ERMS.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class TaskItemController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TaskItemController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: TaskItem – Admin and Manager can view all tasks.
        public async Task<IActionResult> Index()
        {
            var taskItems = _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedEmployee);
            return View(await taskItems.ToListAsync());
        }

        // GET: TaskItem/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedEmployee)
                .FirstOrDefaultAsync(m => m.TaskID == id);
            if (taskItem == null) return NotFound();
            return View(taskItem);
        }

        // GET: TaskItem/Create
        public IActionResult Create()
        {
            var projects = _context.Projects.ToList();
            ViewData["ProjectID"] = new SelectList(projects, "ProjectID", "Name");
            var employeeList = _context.Employees
                .Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName })
                .ToList();
            ViewData["AssignedEmployeeID"] = new SelectList(employeeList, "EmployeeID", "FullName");
            // Dropdown for Status
            ViewBag.StatusOptions = new SelectList(new[] { "Not Started", "In Progress", "Completed" });
            return View();
        }

        // POST: TaskItem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TaskID,ProjectID,AssignedEmployeeID,Description,Priority,Status,DueDate")] TaskItem taskItem)
        {
            if (!ModelState.IsValid)
            {
                var projects = _context.Projects.ToList();
                ViewData["ProjectID"] = new SelectList(projects, "ProjectID", "Name", taskItem.ProjectID);
                var employeeList = _context.Employees
                    .Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName })
                    .ToList();
                ViewData["AssignedEmployeeID"] = new SelectList(employeeList, "EmployeeID", "FullName", taskItem.AssignedEmployeeID);
                ViewBag.StatusOptions = new SelectList(new[] { "Not Started", "In Progress", "Completed" }, taskItem.Status);
                return View(taskItem);
            }
            _context.Add(taskItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: TaskItem/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null) return NotFound();
            var projects = _context.Projects.ToList();
            ViewData["ProjectID"] = new SelectList(projects, "ProjectID", "Name", taskItem.ProjectID);
            var employeeList = _context.Employees
                .Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName })
                .ToList();
            ViewData["AssignedEmployeeID"] = new SelectList(employeeList, "EmployeeID", "FullName", taskItem.AssignedEmployeeID);
            ViewBag.StatusOptions = new SelectList(new[] { "Not Started", "In Progress", "Completed" }, taskItem.Status);
            return View(taskItem);
        }

        // POST: TaskItem/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TaskID,ProjectID,AssignedEmployeeID,Description,Priority,Status,DueDate")] TaskItem taskItem)
        {
            if (id != taskItem.TaskID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskItemExists(taskItem.TaskID))
                        return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            var projects = _context.Projects.ToList();
            ViewData["ProjectID"] = new SelectList(projects, "ProjectID", "Name", taskItem.ProjectID);
            var employeeList = _context.Employees
                .Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName })
                .ToList();
            ViewData["AssignedEmployeeID"] = new SelectList(employeeList, "EmployeeID", "FullName", taskItem.AssignedEmployeeID);
            ViewBag.StatusOptions = new SelectList(new[] { "Not Started", "In Progress", "Completed" }, taskItem.Status);
            return View(taskItem);
        }

        // GET: TaskItem/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedEmployee)
                .FirstOrDefaultAsync(m => m.TaskID == id);
            if (taskItem == null) return NotFound();
            return View(taskItem);
        }

        // POST: TaskItem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem != null)
            {
                _context.TaskItems.Remove(taskItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TaskItemExists(int id)
        {
            return _context.TaskItems.Any(e => e.TaskID == id);
        }

        // New Action: UpdateStatus – Allows Employees to update the status on their own tasks.
        [Authorize(Roles = "Admin,Manager,Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null)
                return NotFound();

            if (User.IsInRole("Employee"))
            {
                string currentUserId = _userManager.GetUserId(User);
                var employee = _context.Employees.FirstOrDefault(e => e.IdentityUserId == currentUserId);
                if (employee == null || taskItem.AssignedEmployeeID != employee.EmployeeID)
                    return RedirectToAction("AccessDenied", "Home", new { message = "You can only update the status of your own tasks." });
            }

            var validStatus = new[] { "Not Started", "In Progress", "Completed" };
            if (!validStatus.Contains(newStatus))
                return RedirectToAction("AccessDenied", "Home", new { message = "Invalid status value." });

            taskItem.Status = newStatus;
            _context.Update(taskItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
