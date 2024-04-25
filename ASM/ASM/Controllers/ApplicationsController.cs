using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASM.Data;
using ASM.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ASM.Controllers
{
    public class ApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApplicationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Applications
        [Authorize(Roles = "Admin,Employer,Seeker")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Applications.Include(a => a.JobListing).Include(a => a.JobSeeker);
            return View(await applicationDbContext.ToListAsync());
        }
        // GET: Applications/Details/5
        [Authorize(Roles = "Admin,Employer,Seeker")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Applications == null)
            {
                return NotFound();
            }

            var application = await _context.Applications
                .Include(a => a.JobListing)
                .Include(a => a.JobSeeker)
                .FirstOrDefaultAsync(m => m.ApplicationId == id);
            if (application == null)
            {
                return NotFound();
            }

            return View(application);
        }

        //GET: Applications/Create
        [Authorize(Roles = "Seeker")]
        public async Task<IActionResult> Create(int? jobListingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var jobSeeker = await _context.JobSeekers.FirstOrDefaultAsync(u => u.UserId == userId);
            var existingApplication = await _context.Applications
            .FirstOrDefaultAsync(a => a.JobListingId == jobListingId && a.JobSeekerId == jobSeeker.JobSeekerId);

            if (existingApplication != null)
            {
                TempData["Message"] = "You have already applied for this job.";
                //return RedirectToAction(nameof(Index));
            }
            ViewBag.Message = TempData["Message"];

            var job = await _context.JobListings.FirstOrDefaultAsync(j => j.JobListingId == jobListingId);
            var jobListing = job.JobListingId;                
            ViewData["JobListingId"] = jobListing;
            ViewData["JobSeekerId"] = new SelectList(_context.JobSeekers.Where(u => u.UserId == userId), "JobSeekerId", "Fullname");
            return View();
        }




        // POST: Applications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Seeker")]
        public async Task<IActionResult> Create(int? jobListingId,[Bind("ApplicationId,CoverLetter,Description,ApplicationDate,JobListingId,JobSeekerId,StatusApp")] Application application)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var job = await _context.JobListings.FirstOrDefaultAsync(j => j.JobListingId == jobListingId);
            var jobSeeker = await _context.JobSeekers.FirstOrDefaultAsync(u => u.UserId == userId);

            var existingApplication = await _context.Applications
            .FirstOrDefaultAsync(a => a.JobListingId == jobListingId && a.JobSeekerId == jobSeeker.JobSeekerId);

            if (existingApplication != null)
            {
                
                return RedirectToAction(nameof(Index));

            }
            application.StatusApp = "Not Active";
            application.JobListingId = job.JobListingId;
            if (jobSeeker != null)
            {
                application.JobSeekerId = jobSeeker.JobSeekerId;

                _context.Add(application);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
            
            if (ModelState.IsValid)
            {
                _context.Add(application);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["jobListingId"] = jobListingId;
            ViewData["JobListingId"] = new SelectList(_context.JobListings, "JobListingId", "Name", application.JobListingId);
            ViewData["JobSeekerId"] = new SelectList(_context.JobSeekers, "JobSeekerId", "Fullname", application.JobSeekerId);
            return View(application);
        }



        // GET: Applications/Edit/5
        [Authorize(Roles = "Seeker")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Applications == null)
            {
                return NotFound();
            }

            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ViewData["JobListingId"] = new SelectList(_context.JobListings, "JobListingId", "Name", application.JobListingId);
            ViewData["JobSeekerId"] = new SelectList(_context.JobSeekers.Where(u => u.UserId == userId), "JobSeekerId", "Fullname", application.JobSeekerId);
            return View(application);
        }



        // POST: Applications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Seeker")]
        public async Task<IActionResult> Edit(int id, [Bind("ApplicationId,CoverLetter,Description,ApplicationDate,JobListingId,JobSeekerId")] Application application)
        {
            if (id != application.ApplicationId)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(application);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplicationExists(application.ApplicationId))
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
            ViewData["JobListingId"] = new SelectList(_context.JobListings, "JobListingId", "Name", application.JobListingId);
            ViewData["JobSeekerId"] = new SelectList(_context.JobSeekers, "JobSeekerId", "Fullname", application.JobSeekerId);
            return View(application);
        }

        // GET: Applications/Delete/5
        [Authorize(Roles = "Admin,Employer,Seeker")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Applications == null)
            {
                return NotFound();
            }

            var application = await _context.Applications
                .Include(a => a.JobListing)
                .Include(a => a.JobSeeker)
                .FirstOrDefaultAsync(m => m.ApplicationId == id);
            if (application == null)
            {
                return NotFound();
            }


            return View(application);
        }

        // POST: Applications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employer,Seeker")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Applications == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Application'  is null.");
            }
            var application = await _context.Applications.FindAsync(id);
            if (application != null)
            {
                _context.Applications.Remove(application);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ApplicationExists(int id)
        {
            return (_context.Applications?.Any(e => e.ApplicationId == id)).GetValueOrDefault();
        }

        // GET: Applications/Search
        // GET: Applications/Search
        [Authorize(Roles = "Admin,Employer,Seeker")]
        public async Task<IActionResult> Search(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var applications = _context.Applications
                .Include(a => a.JobListing)
                .Include(a => a.JobSeeker)
                .AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                applications = applications.Where(a => a.Description.Contains(searchString));
            }

            return View(await applications.ToListAsync());
        }


        // GET: Applications/Manage
        [Authorize(Roles = "Admin,Seeker")]
        public async Task<IActionResult> Manage()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var applications = await _context.Applications
                .Include(a => a.JobListing)
                .Include(a => a.JobSeeker)
                .Where(j => j.JobSeeker.UserId == currentUserId)
                .ToListAsync();

            return View(applications);

        }

        public async Task<IActionResult> ApproveApp(int id)
        {
            var app = await _context.Applications.FindAsync(id);
            if (app == null)
            {
                return NotFound();
            }

            app.StatusApp = "Active";
            _context.Applications.Update(app);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageActive));
        }

        public IActionResult ManageActive()
        {
            var manageApp = _context.Applications.Where(j => j.StatusApp == "Not Active").Include(j => j.JobSeeker).Include(a => a.JobListing);
            return View(manageApp);
        }

    }
}
