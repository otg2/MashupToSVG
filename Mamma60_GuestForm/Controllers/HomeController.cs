using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mamma60_GuestForm.Models;
using Microsoft.AspNetCore.Identity;
using Mamma60_GuestForm.Data;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Mamma60_GuestForm.Custom;



/*
 * 
 test1@mail.com - test1mail
 test2@mail.com - test2mail
 test3@mail.com - test3mail
 *
 */

namespace Mamma60_GuestForm.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserProfileContext _context;
        private readonly UserManager<IdentityUser> _userManager;


        public HomeController(ILogger<HomeController> logger,
            UserManager<IdentityUser> userManager,
            UserProfileContext context)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View();
            }

            var model = _context.ProfileViewModel.Where(a => a.Id == user.Id).FirstOrDefault();
            if (model == null)
            {
                var emptyModel = new ProfileViewModel()
                {
                    Name = "Nafnið þitt hér",
                    Memory = "Minning eða afmæliskveðja hér",
                    ProfilePicture = null
                };
                return View(emptyModel);
            }

            return View(model);
        }



        [HttpPost]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'. Session expired.");
            }

            bool firstTimeCreation = false;
            var userProfile = _context.ProfileViewModel.Where(x => x.Id == user.Id).FirstOrDefault();

            if(userProfile == null)
            {
                firstTimeCreation = true;
                userProfile = new ProfileViewModel();
            }

            userProfile.Id = user.Id;
            userProfile.DateCreated = DateTime.Now;

            userProfile.Name = model.Name;
            userProfile.Memory = model.Memory;

            var imageFlag = false;

            if (HttpContext.Request.Form.Files.Count > 0)
            {
                var file = HttpContext.Request.Form.Files[0];
                byte[] p1 = null;
                using (MemoryStream ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    p1 =  ms.ToArray();
                }

                userProfile.ProfilePicture = p1;

                // If a new picture was added, turn flag true here
                imageFlag = true;
            }

            userProfile.HexColor = model.HexColor;

            if(firstTimeCreation)
            {
                _context.Add(userProfile);
            }
            _context.SaveChanges();

            await _context.SaveChangesAsync();

            // Send 
            if (imageFlag)
            {
                var exportImage = new ImageExport(userProfile);
                exportImage.SaveImage();
            }

            return RedirectToAction();
        }

        // Or look at
        // https://stackoverflow.com/questions/49082312/activating-conda-environment-from-c-sharp-code-or-what-is-the-differences-betwe
        public void ExecuteCommand(string Command)
        {
            ProcessStartInfo ProcessInfo;
            Process Process;

            ProcessInfo = new ProcessStartInfo("cmd.exe", "/K " + Command);
            ProcessInfo.CreateNoWindow = true;
            ProcessInfo.UseShellExecute = true;

            Process = Process.Start(ProcessInfo);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
