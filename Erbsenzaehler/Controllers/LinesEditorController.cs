﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Erbsenzaehler.ViewModels.LinesEditor;

namespace Erbsenzaehler.Controllers
{
    [Authorize]
    [RoutePrefix("LinesEditor")]
    public class LinesEditorController : ControllerBase
    {
        [ChildActionOnly]
        [Route("")]
        public ActionResult Index(string month)
        {
            return PartialView("_LinesEditor");
        }

        [HttpGet]
        [Route("Json")]
        public async Task<ActionResult> LoadLines(string month)
        {
            int? selectedYear = null;
            int? selectedMonth = null;
            if (!string.IsNullOrEmpty(month) && month.Contains("-"))
            {
                selectedYear = int.Parse(month.Split('-')[0]);
                selectedMonth = int.Parse(month.Split('-')[1]);
            }

            var viewModel = await new JsonViewModel().Fill(await GetCurrentClient(), Db, selectedYear, selectedMonth);
            return new JsonResult
            {
                Data = viewModel,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        [HttpPut]
        [Route("Json")]
        public async Task<ActionResult> UpdateLine(JsonViewModel.Line line, string month)
        {
            var currentClient = await GetCurrentClient();

            var lineInDatebase = Db.Lines.FirstOrDefault(x => x.Id == line.Id);
            if (lineInDatebase == null || lineInDatebase.Account.ClientId != currentClient.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            // sanitize the category
            if (line.Category != null)
            {
                line.Category = line.Category.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace("<br>", "");
                line.Category = line.Category.Trim();
                if (line.Category == "")
                {
                    line.Category = null;
                }
            }

            if (lineInDatebase.Ignore != line.Ignore)
            {
                lineInDatebase.Ignore = line.Ignore;
                lineInDatebase.IgnoreUpdatedManually = true;
            }

            var newDate = DateTime.Parse(line.Date);
            if (lineInDatebase.Date != newDate)
            {
                lineInDatebase.Date = newDate;
                lineInDatebase.DateUpdatedManually = true;
            }

            if (lineInDatebase.Category != line.Category)
            {
                lineInDatebase.Category = line.Category;
                lineInDatebase.CategoryUpdatedManually = true;
            }

            decimal amountAsDecimal;
            if (decimal.TryParse(line.Amount, out amountAsDecimal))
            {
                if (lineInDatebase.Amount != amountAsDecimal)
                {
                    lineInDatebase.Amount = amountAsDecimal;
                    lineInDatebase.AmountUpdatedManually = true;
                }
            }

            Db.SaveChanges();

            return await LoadLines(month);
        }


        [HttpDelete]
        [Route("Json/{id}/")]
        public async Task<ActionResult> DeleteLine(int id, string month)
        {
            var currentClient = await GetCurrentClient();

            var lineInDatebase = await Db.Lines.FirstOrDefaultAsync(x => x.Id == id);
            if (lineInDatebase == null || lineInDatebase.Account.ClientId != currentClient.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            Db.Lines.Remove(lineInDatebase);
            await Db.SaveChangesAsync();
            
            return await LoadLines(month);
        }
    }
}