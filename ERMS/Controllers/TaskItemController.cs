using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ERMS.Data;
using ERMS.Models;

namespace ERMS.Controllers
{
    public class TaskItemController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskItemController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TaskItem
        public async Task<IActionResult> Index()
        {
            // Include related Project and AssignedEmployee for display
            var taskItems = _context.TaskItems
                                    .Include(t => t.Project)
                                    .Include(t => t.AssignedEmployee);
            return View(await taskItems.ToListAsync());
        }

        // GET: TaskItem/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedEmployee)
                .FirstOrDefaultAsync(m => m.TaskID == id);
            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

        // GET: TaskItem/Create
        public IActionResult Create()
        {
            // Populate dropdown for Projects
            var projects = _context.Projects.ToList();
            ViewData["ProjectID"] = new SelectList(projects, "ProjectID", "Name");

            // Populate dropdown for Employees
            var employeeList = _context.Employees
                .Select(e => new
                {
                    e.EmployeeID,
                    FullName = e.FirstName + " " + e.LastName
                })
                .ToList();
            ViewData["AssignedEmployeeID"] = new SelectList(employeeList, "EmployeeID", "FullName");

            return View();
        }

        // POST: TaskItem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("TaskID,ProjectID,AssignedEmployeeID,Description,Priority,Status,DueDate")]
            TaskItem taskItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(taskItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If invalid, re-populate dropdowns and return view
            var projects = _context.Projects.ToList();
            ViewData["ProjectID"] = new SelectList(projects, "ProjectID", "Name", taskItem.ProjectID);

            var employeeList = _context.Employees
                .Select(e => new
                {
                    e.EmployeeID,
                    FullName = e.FirstName + " " + e.LastName
                })
                .ToList();
            ViewData["AssignedEmployeeID"] = new SelectList(employeeList, "EmployeeID", "FullName", taskItem.AssignedEmployeeID);

            return View(taskItem);
        }

        // GET: TaskItem/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }

            // For the dropdowns
            var projects = _context.Projects.ToList();
            ViewData["ProjectID"] = new SelectList(projects, "ProjectID", "Name", taskItem.ProjectID);

            var employeeList = _context.Employees
                .Select(e => new
                {
                    e.EmployeeID,
                    FullName = e.FirstName + " " + e.LastName
                })
                .ToList();
            ViewData["AssignedEmployeeID"] = new SelectList(employeeList, "EmployeeID", "FullName", taskItem.AssignedEmployeeID);

            return View(taskItem);
        }

        // POST: TaskItem/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("TaskID,ProjectID,AssignedEmployeeID,Description,Priority,Status,DueDate")]
            TaskItem taskItem)
        {
            if (id != taskItem.TaskID)
            {
                return NotFound();
            }

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
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Re-populate if invalid
            var projects = _context.Projects.ToList();
            ViewData["ProjectID"] = new SelectList(projects, "ProjectID", "Name", taskItem.ProjectID);

            var employeeList = _context.Employees
                .Select(e => new
                {
                    e.EmployeeID,
                    FullName = e.FirstName + " " + e.LastName
                })
                .ToList();
            ViewData["AssignedEmployeeID"] = new SelectList(employeeList, "EmployeeID", "FullName", taskItem.AssignedEmployeeID);

            return View(taskItem);
        }

        // GET: TaskItem/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedEmployee)
                .FirstOrDefaultAsync(m => m.TaskID == id);
            if (taskItem == null)
            {
                return NotFound();
            }

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
    }
}
