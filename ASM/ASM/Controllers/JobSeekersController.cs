using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASM.Data;
using ASM.Models;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace ASM.Controllers
{
    public class JobSeekersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment webHostEnvironment;

        public JobSeekersController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment, UserManager<IdentityUser> userManager)
        {
            _context = context;
            webHostEnvironment = hostEnvironment;
            _userManager = userManager;
        }

        // GET: JobSeekers
        // GET: JobSeekers/Index
        [Authorize(Roles = "Admin,Seeker")]
        public async Task<IActionResult> Index()
        {
            // Lấy UserId của người dùng hiện tại
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound(); // Xử lý trường hợp không tìm thấy người dùng hiện tại
            }

            // Lọc danh sách JobSeeker dựa trên UserId của người dùng hiện tại
            var jobSeeker = await _context.JobSeekers.Where(j => j.UserId == currentUser.Id).ToListAsync();

            return View(jobSeeker); // Trả về view chỉ hiển thị JobSeeker thuộc về người dùng hiện tại
        }


        // GET: JobSeekers/Details/5
        [Authorize(Roles = "Admin,Employer,Seeker")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.JobSeekers == null)
            {
                return NotFound();
            }

            var jobSeeker = await _context.JobSeekers
                .Include(j => j.User)
                .FirstOrDefaultAsync(m => m.JobSeekerId == id);
            if (jobSeeker == null)
            {
                return NotFound();
            }

            return View(jobSeeker);
        }

        // GET: JobSeekers/Create
        [Authorize(Roles = "Seeker")]
        public IActionResult Create()
        {
            //Lấy id đúng với id user đang đăng nhập.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["UserId"] = new SelectList(_context.Users.Where(u => u.Id == userId), "Id", "Email");
            return View();
        }

        // POST: JobSeekers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Seeker")]
        public async Task<IActionResult> Create([Bind("JobSeekerId,Fullname,Phone,Address,Experience,Skill,PictureImage,UserId")] JobSeeker jobSeeker)
        {

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (jobSeeker.PictureImage != null)
                {
                    string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + jobSeeker.PictureImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await jobSeeker.PictureImage.CopyToAsync(fileStream);
                    }
                    jobSeeker.UrlImage = "/images/" + uniqueFileName;
                }
                _context.Add(jobSeeker);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", jobSeeker.UserId);
            return View(jobSeeker);
        }

        // GET: JobSeekers/Edit/5
        [Authorize(Roles = "Seeker")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.JobSeekers == null)
            {
                return NotFound();
            }

            var jobSeeker = await _context.JobSeekers.FindAsync(id);
            if (jobSeeker == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["UserId"] = new SelectList(_context.Users.Where(u => u.Id == userId), "Id", "Id", jobSeeker.UserId);
            return View(jobSeeker);
        }

        // POST: JobSeekers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Seeker")]
        public async Task<IActionResult> Edit(int id, [Bind("JobSeekerId,Fullname,Phone,Address,Experience,Skill,PictureImage,UserId")] JobSeeker jobSeeker)
        {
            if (id != jobSeeker.JobSeekerId)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {

                try
                {
                    var jobSeekerToUpdate = await _context.JobSeekers.FindAsync(id);
                    if (jobSeekerToUpdate == null)
                    {
                        return NotFound();
                    }

                    // Copy các thông tin mới từ jobSeeker vào jobSeekerToUpdate
                    jobSeekerToUpdate.Fullname = jobSeeker.Fullname;
                    //jobSeekerToUpdate.Email = jobSeeker.Email;
                    jobSeekerToUpdate.Phone = jobSeeker.Phone;
                    jobSeekerToUpdate.Address = jobSeeker.Address;
                    jobSeekerToUpdate.Experience = jobSeeker.Experience;
                    jobSeekerToUpdate.Skill = jobSeeker.Skill;

                    // Xử lý tải lên ảnh mới và cập nhật đường dẫn ảnh mới
                    if (jobSeeker.PictureImage != null)
                    {
                        string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + jobSeeker.PictureImage.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await jobSeeker.PictureImage.CopyToAsync(fileStream);
                        }
                        jobSeekerToUpdate.UrlImage = "/images/" + uniqueFileName;
                    }

                    _context.Update(jobSeekerToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobSeekerExists(jobSeeker.JobSeekerId))
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["UserId"] = new SelectList(_context.Users.Where(u => u.Id == userId), "Id", "Email", jobSeeker.UserId);
            return View(jobSeeker);
        }


        // GET: JobSeekers/Delete/5
        [Authorize(Roles = "Seeker")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.JobSeekers == null)
            {
                return NotFound();
            }

            var jobSeeker = await _context.JobSeekers
                .Include(j => j.User)
                .FirstOrDefaultAsync(m => m.JobSeekerId == id);
            if (jobSeeker == null)
            {
                return NotFound();
            }

            return View(jobSeeker);
        }

        // POST: JobSeekers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Seeker")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.JobSeekers == null)
            {
                return Problem("Entity set 'ApplicationDbContext.JobSeeker'  is null.");
            }
            var jobSeeker = await _context.JobSeekers.FindAsync(id);
            if (jobSeeker != null)
            {
                _context.JobSeekers.Remove(jobSeeker);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JobSeekerExists(int id)
        {
            return (_context.JobSeekers?.Any(e => e.JobSeekerId == id)).GetValueOrDefault();
        }
    }
}
