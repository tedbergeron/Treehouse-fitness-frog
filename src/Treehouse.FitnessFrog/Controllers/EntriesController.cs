using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Treehouse.FitnessFrog.Data;
using Treehouse.FitnessFrog.Models;

namespace Treehouse.FitnessFrog.Controllers
{
    public class EntriesController : Controller
    {
        private EntriesRepository _entriesRepository = null;

        public EntriesController()
        {
            _entriesRepository = new EntriesRepository();
        }

        public ActionResult Index()
        {
            List<Entry> entries = _entriesRepository.GetEntries();

            // Calculate the total activity.
            double totalActivity = entries
                .Where(e => e.Exclude == false)
                .Sum(e => e.Duration);

            // Determine the number of days that have entries.
            int numberOfActiveDays = entries
                .Select(e => e.Date)
                .Distinct()
                .Count();

            ViewBag.TotalActivity = totalActivity;
            ViewBag.AverageDailyActivity = (totalActivity / (double)numberOfActiveDays);

            return View(entries);
        }

        public ActionResult Add()
        {
            var entry = new Entry()
            {
                Date = DateTime.Today,
            };

            SetupActivitiesSelectListItems();

            return View(entry);
        }

        

        [HttpPost]
        public ActionResult Add(Entry entry)
        {
            // SERVER SIDE VALIDATION HERE!!!

            ////ModelState.AddModelError("", "This is a global message.");

            ValidateEntry(entry);

            if (ModelState.IsValid)
            {
                _entriesRepository.AddEntry(entry);

                return RedirectToAction("Index");

            }

            SetupActivitiesSelectListItems();

            return View(entry);
        }

        

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // get the requested entry from the repo
            Entry entry = _entriesRepository.GetEntry((int)id);

            // return a status of 'not found' if not found.
            if (entry == null)
            {
                return HttpNotFound();
            }

            SetupActivitiesSelectListItems();

            // pass the entry into the view
            return View(entry);
        }

        [HttpPost]
        public ActionResult Edit(Entry entry)
        {
            // validate entry
            ValidateEntry(entry);

            // If the entry is valid
            // 1) use the repo to update the entry
            // 2) redirect the user to the Entries list page
            if (ModelState.IsValid)
            {
                _entriesRepository.UpdateEntry(entry);

                return RedirectToAction("Index");
            }

            SetupActivitiesSelectListItems();

            return View(entry);
        }

        // Get request should always be safe to make
        // return a view to allow user to review before deleting
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Retrieve entry for the provided if parameter value
            Entry entry = _entriesRepository.GetEntry((int)id);

            // Return 'not found' if entry not found
            if (entry == null)
            {
                return HttpNotFound();
            }

            // Pass the entry to the view
            return View(entry);
        }


        [HttpPost]
        public ActionResult Delete (int id)
        {
            // Delete the entry
            _entriesRepository.DeleteEntry(id);

            // Redirect to the entries list page
            return RedirectToAction("Index");
        }


        // Server-side validation
        private void ValidateEntry(Entry entry)
        {
            // If there aren't and Duration field validation errors
            // then make sure the duration is greater than 0
            if (ModelState.IsValidField("Duration") && entry.Duration <= 0)
            {
                ModelState.AddModelError("Duration", "The Duration field value must be greater than '0'.");
            }
        }

        private void SetupActivitiesSelectListItems()
        {
            ViewBag.ActivitiesSelectListItems = new SelectList(
                            Data.Data.Activities, "Id", "Name");
        }

    }
}