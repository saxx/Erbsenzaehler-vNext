﻿using System.Data.Entity;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Erbsenzaehler.ViewModels.ManageUser;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace Erbsenzaehler.Controllers
{
    [Authorize]
    public class ManageUserController : ControllerBase
    {
        #region Constructor
        private ApplicationSignInManager _signInManager;


        public ManageUserController()
        {
        }


        public ManageUserController(ApplicationSignInManager signInManager)
        {
            SignInManager = signInManager;
        }


        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }
        #endregion

        #region Index
        public async Task<ActionResult> Index()
        {
            var viewModel = new IndexViewModel().Fill(await GetCurrentUser());
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(IndexViewModel model)
        {
            var currentUser = await GetCurrentUser();
            if (ModelState.IsValid)
            {
                var currentUserClosure = currentUser;
                var dbUser = await Db.Users.FirstOrDefaultAsync(x => x.Id == currentUserClosure.Id);

                dbUser.Email = model.Email;
                dbUser.SummaryMailInterval = model.SummaryMailInterval;
                await Db.SaveChangesAsync();
            }

            // reload user
            currentUser = await GetCurrentUser();
            return View(model.Fill(currentUser));
        }
        #endregion

        #region ChangePassword
        public ActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }

                model.WasSuccessful = true;
            }
            else
            {
                AddErrors(result);
            }
            return View(model);
        }


        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
        #endregion
    }
}