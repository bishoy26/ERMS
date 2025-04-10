using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // For user info
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERMS.Data;
using ERMS.Models;

namespace ERMS.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EmployeeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Employee 
        // Admin and Manager: view all; Employee: view only own record.
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Employee"))
            {
                string currentUserId = _userManager.GetUserId(User);
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.IdentityUserId == currentUserId);
                if (employee == null)
                {
                    return RedirectToAction("AccessDenied", "Home", new { message = "Your employee record was not found." });
                }
                return View(new Employee[] { employee });
            }
            else
            {
                return View(await _context.Employees.ToListAsync());
            }
        }

        // GET: Employee/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();
            var employee = await _context.Employees.FirstOrDefaultAsync(m => m.EmployeeID == id);
            if (employee == null)
                return NotFound();

            if (User.IsInRole("Employee"))
            {
                string currentUserId = _userManager.GetUserId(User);
                if (employee.IdentityUserId != currentUserId)
                    return RedirectToAction("AccessDenied", "Home", new { message = "You are not authorized to view other employees' details." });
            }
            return View(employee);
        }

        // GET: Employee/Create – Only Admin can create employees.
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employee/Create – Only Admin.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("EmployeeID,FirstName,LastName,Email,PhoneNumber,Role")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // GET: Employee/Edit/5 
        // Admin can edit any record; Employee can edit only their own record.
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound();

            if (User.IsInRole("Employee"))
            {
                string currentUserId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(employee.IdentityUserId) && employee.IdentityUserId != currentUserId)
                    return RedirectToAction("AccessDenied", "Home", new { message = "You can only edit your own information." });
            }
            return View(employee);
        }

        // POST: Employee/Edit/5 
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int id, [Bind("EmployeeID,FirstName,LastName,Email,PhoneNumber,Role,IdentityUserId")] Employee employee)
        {
            if (id != employee.EmployeeID)
                return NotFound();

            if (User.IsInRole("Employee"))
            {
                string currentUserId = _userManager.GetUserId(User);
                // If the record doesn't have an IdentityUserId yet, assign it.
                if (string.IsNullOrEmpty(employee.IdentityUserId))
                {
                    employee.IdentityUserId = currentUserId;
                }
                else if (employee.IdentityUserId != currentUserId)
                {
                    return RedirectToAction("AccessDenied", "Home", new { message = "You are not allowed to edit other employees' information." });
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.EmployeeID))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // GET: Employee/Delete/5 – Only Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();
            var employee = await _context.Employees.FirstOrDefaultAsync(m => m.EmployeeID == id);
            if (employee == null)
                return NotFound();
            return View(employee);
        }

        // POST: Employee/Delete/5 – Only Admin
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
                _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeID == id);
        }
    }
}
