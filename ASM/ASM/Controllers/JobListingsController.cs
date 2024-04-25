using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASM.Data;
using ASM.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ASM.Controllers
{
    public class JobListingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobListingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: JobListings
        [Authorize(Roles = "Admin,Seeker,Employer")]
        public async Task<IActionResult> Index()
        {

            var applicationDbContext = _context.JobListings.Where(j => j.Status == "Active").Include(j => j.Employer);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: JobListings/Details/5
        [Authorize(Roles = "Admin,Seeker,Employer")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.JobListings == null)
            {
                return NotFound();
            }

            var jobListing = await _context.JobListings
                .Include(j => j.Employer)
                .FirstOrDefaultAsync(m => m.JobListingId == id);
            if (jobListing == null)
            {
                return NotFound();
            }

            return View(jobListing);
        }

        // GET: JobListings/Create
        [Authorize(Roles = "Admin,Employer")]
        public IActionResult Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["EmployerId"] = new SelectList(_context.Employers.Where(u => u.UserId == userId), "EmployerId", "CompanyName");
            return View();
        }

        // POST: JobListings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("JobListingId,Name,Description,Requirement,Deadline,EmployerId,Status")] JobListing jobListing)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            jobListing.Status = "Not Active";
            if (ModelState.IsValid)
            {
                _context.Add(jobListing);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployerId"] = new SelectList(_context.Employers, "EmployerId", "CompanyName", jobListing.EmployerId);
            return View(jobListing);
        }

        // GET: JobListings/Edit/5
        [Authorize(Roles = "Admin,Employer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.JobListings == null)
            {
                return NotFound();
            }

            var jobListing = await _context.JobListings.FindAsync(id);
            if (jobListing == null)
            {
                return NotFound();
            }
            ViewData["EmployerId"] = new SelectList(_context.Employers, "EmployerId", "CompanyName", jobListing.EmployerId);
            return View(jobListing);
        }

        // POST: JobListings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employer")]
        public async Task<IActionResult> Edit(int id, [Bind("JobListingId,Name,Description,Requirement,Deadline,EmployerId")] JobListing jobListing)
        {
            if (id != jobListing.JobListingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(jobListing);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobListingExists(jobListing.JobListingId))
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
            ViewData["EmployerId"] = new SelectList(_context.Employers, "EmployerId", "CompanyName", jobListing.EmployerId);
            return View(jobListing);
        }

        // GET: JobListings/Delete/5
        [Authorize(Roles = "Admin,Employer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.JobListings == null)
            {
                return NotFound();
            }

            var jobListing = await _context.JobListings
                .Include(j => j.Employer)
                .FirstOrDefaultAsync(m => m.JobListingId == id);
            if (jobListing == null)
            {
                return NotFound();
            }

            return View(jobListing);
        }

        // POST: JobListings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employer")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.JobListings == null)
            {
                return Problem("Entity set 'ApplicationDbContext.JobListings'  is null.");
            }
            var jobListing = await _context.JobListings.FindAsync(id);
            if (jobListing != null)
            {
                _context.JobListings.Remove(jobListing);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JobListingExists(int id)
        {
          return (_context.JobListings?.Any(e => e.JobListingId == id)).GetValueOrDefault();
        }

        // Thêm phương thức Search
        public async Task<IActionResult> Search(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var jobListings = _context.JobListings.Include(a => a.Employer).AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                jobListings = jobListings.Where(j => j.Name.Contains(searchString));
            }

            return View(await jobListings.ToListAsync());
        }

        public async Task<IActionResult> ApproveJob(int id)
        {
            var job = await _context.JobListings.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            job.Status = "Active";
            _context.JobListings.Update(job);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageActive));
        }

        public IActionResult ManageActive()
        {      
            var manageJob = _context.JobListings.Where(j => j.Status == "Not Active").Include(j => j.Employer);
            return View(manageJob); 
        }

        [Authorize(Roles = "Admin,Employer")]
        public async Task<IActionResult> Manage()
        {
            // Lấy ID của nhà tuyển dụng hiện tại
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy danh sách công việc của nhà tuyển dụng hiện tại
            var jobs = await _context.JobListings
                .Include(j => j.Employer)
                .Where(j => j.Employer.UserId == currentUserId)
                .ToListAsync();

            return View(jobs);
        }

    }
}
