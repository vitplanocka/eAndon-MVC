using eAndon_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace eAndon_MVC.Controllers
{
    public class SettingsController : Controller
    {
        private readonly MyDbContext _db;

        public SettingsController(MyDbContext db)
        {
            _db = db;
        }

        // GET: StatusDefinition
        public async Task<IActionResult> Settings()
        {
            var statusDefinitions = await _db.StatusDefinition.OrderBy(w => w.StatusRow).ToListAsync();
            
            // Retrieve the list of WorkcenterList objects
            var workcenterList = _db.WorkcenterList.ToList();

            // Add both lists to the ViewBag
            ViewBag.StatusList = statusDefinitions;
            ViewBag.Workcenters = workcenterList;
            
            return View("~/Views/Home/Settings.cshtml", statusDefinitions);
        }

        #region WorkcenterList

        
        [HttpPost]
        public async Task<IActionResult> MoveWorkcenterUp(int workcenterRow)
        {
            // Find the workcenter with the given row number
            var workcenter = await _db.WorkcenterList.FindAsync(workcenterRow);
            if (workcenter == null)
            {
                return NotFound();
            }
    
            // Find the workcenter with the row number one less than the given row number
            var prevWorkcenter = await _db.WorkcenterList.FirstOrDefaultAsync(w => w.WorkcenterRow == workcenterRow - 1);
            if (prevWorkcenter == null)
            {
                return BadRequest("Cannot move workcenter up any further");
            }
    
            // Swap the other fields of the two workcenters
            var tempWorkcenterID = workcenter.WorkcenterID;
            var tempWorkcenterName = workcenter.WorkcenterName;
            var tempStatus1 = workcenter.Status1;
            var tempStatus2 = workcenter.Status2;
            var tempStatus3 = workcenter.Status3;
            var tempStatus4 = workcenter.Status4;
            var tempStatus5 = workcenter.Status5;

            workcenter.WorkcenterID = prevWorkcenter.WorkcenterID;
            workcenter.WorkcenterName = prevWorkcenter.WorkcenterName;
            workcenter.Status1 = prevWorkcenter.Status1;
            workcenter.Status2 = prevWorkcenter.Status2;
            workcenter.Status3 = prevWorkcenter.Status3;
            workcenter.Status4 = prevWorkcenter.Status4;
            workcenter.Status5 = prevWorkcenter.Status5;

            prevWorkcenter.WorkcenterID = tempWorkcenterID;
            prevWorkcenter.WorkcenterName = tempWorkcenterName;
            prevWorkcenter.Status1 = tempStatus1;
            prevWorkcenter.Status2 = tempStatus2;
            prevWorkcenter.Status3 = tempStatus3;
            prevWorkcenter.Status4 = tempStatus4;
            prevWorkcenter.Status5 = tempStatus5;

            // Save changes to the database
            await _db.SaveChangesAsync();

            return RedirectToAction("Settings");
        }

        [HttpPost]
        public async Task<IActionResult> MoveWorkcenterDown(int workcenterRow)
        {
            // Find the workcenter with the given row number
            var workcenter = await _db.WorkcenterList.FindAsync(workcenterRow);
            if (workcenter == null)
            {
                return NotFound();
            }
    
            // Find the workcenter with the row number one more than the given row number
            var nextWorkcenter = await _db.WorkcenterList.FirstOrDefaultAsync(w => w.WorkcenterRow == workcenterRow + 1);
            if (nextWorkcenter == null)
            {
                return BadRequest("Cannot move workcenter down any further");
            }
    
            // Swap the other fields of the two workcenters
            var tempWorkcenterID = workcenter.WorkcenterID;
            var tempWorkcenterName = workcenter.WorkcenterName;
            var tempStatus1 = workcenter.Status1;
            var tempStatus2 = workcenter.Status2;
            var tempStatus3 = workcenter.Status3;
            var tempStatus4 = workcenter.Status4;
            var tempStatus5 = workcenter.Status5;

            workcenter.WorkcenterID = nextWorkcenter.WorkcenterID;
            workcenter.WorkcenterName = nextWorkcenter.WorkcenterName;
            workcenter.Status1 = nextWorkcenter.Status1;
            workcenter.Status2 = nextWorkcenter.Status2;
            workcenter.Status3 = nextWorkcenter.Status3;
            workcenter.Status4 = nextWorkcenter.Status4;
            workcenter.Status5 = nextWorkcenter.Status5;

            nextWorkcenter.WorkcenterID = tempWorkcenterID;
            nextWorkcenter.WorkcenterName = tempWorkcenterName;
            nextWorkcenter.Status1 = tempStatus1;
            nextWorkcenter.Status2 = tempStatus2;
            nextWorkcenter.Status3 = tempStatus3;
            nextWorkcenter.Status4 = tempStatus4;
            nextWorkcenter.Status5 = tempStatus5;

            // Save changes to the database
            await _db.SaveChangesAsync();

            return RedirectToAction("Settings");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteWorkcenter(int workcenterRow)
        {
            // Find the workcenter with the given row number
            var workcenterToDelete = await _db.WorkcenterList.FindAsync(workcenterRow);
            if (workcenterToDelete == null)
            {
                return NotFound();
            }

            // Find all workcenters with WorkcenterRow greater than the deleted workcenter's WorkcenterRow
            var workcentersToUpdate = await _db.WorkcenterList
                .Where(w => w.WorkcenterRow > workcenterToDelete.WorkcenterRow)
                .ToListAsync();

            // Shift the columns for the remaining workcenters in the table
            foreach (var wc in workcentersToUpdate)
            {
                var lowerWc = await _db.WorkcenterList.FindAsync(wc.WorkcenterRow - 1);
                wc.WorkcenterID = lowerWc.WorkcenterID; // Update WorkcenterID
                wc.WorkcenterName = lowerWc.WorkcenterName; // Update WorkcenterName
                wc.Status1 = lowerWc.Status1;
                wc.Status2 = lowerWc.Status2;
                wc.Status3 = lowerWc.Status3;
                wc.Status4 = lowerWc.Status4;
                wc.Status5 = lowerWc.Status5;
                _db.Entry(wc).State = EntityState.Modified;
            }

            // Delete the last workcenter row
            var lastWorkcenter = await _db.WorkcenterList
                .OrderByDescending(w => w.WorkcenterRow)
                .FirstOrDefaultAsync();
            if (lastWorkcenter != null)
            {
                _db.WorkcenterList.Remove(lastWorkcenter);
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("Settings");
        }


        [HttpPost]
        public async Task<IActionResult> AddWorkcenter(string newWorkcenterID, string newWorkcenterName, int workcenterRow)
        {
            // Check if the new workcenter ID is unique
            var isUnique = await _db.WorkcenterList.AllAsync(wc => wc.WorkcenterID != newWorkcenterID);
            if (!isUnique)
            {
                ModelState.AddModelError("newWorkcenterID", "The workcenter ID must be unique.");
            }

            if (ModelState.IsValid)
            {
                // Add the new workcenter to the database
                var newWorkcenter = new Workcenter
                {
                    WorkcenterRow = workcenterRow,
                    WorkcenterID = newWorkcenterID,
                    WorkcenterName = newWorkcenterName,
                    Status1 = "green",
                    Status2 = "green",
                    Status3 = "green",
                    Status4 = "green",
                    Status5 = "green"
                };
                _db.WorkcenterList.Add(newWorkcenter);
                await _db.SaveChangesAsync();

                return RedirectToAction("Settings");
            }
            else
            {
                // Return to the settings page with validation errors
                ViewBag.Workcenters = await _db.WorkcenterList.OrderBy(wc => wc.WorkcenterRow).ToListAsync();
                return RedirectToAction("Settings");
            }
        }


        
        [HttpPost]
        public IActionResult UpdateWorkcenterName(int workcenterRow, string workcenterName)
        {
            // Retrieve the StatusDefinition object with the given StatusRow
            var status = _db.WorkcenterList.FirstOrDefault(s => s.WorkcenterRow == workcenterRow);

            // Update the StatusName property of the StatusDefinition object
            status.WorkcenterName = workcenterName;

            // Save changes to the database
            _db.SaveChanges();

            // Redirect back to the Settings page
            return RedirectToAction("Settings", "Settings");
        }

        [HttpPost]
        public IActionResult UpdateWorkcenterID(int workcenterRow, string workcenterID)
        {
            // Retrieve the StatusDefinition object with the given StatusRow
            var status = _db.WorkcenterList.FirstOrDefault(s => s.WorkcenterRow == workcenterRow);

            // Update the StatusName property of the StatusDefinition object
            status.WorkcenterID = workcenterID;

            // Save changes to the database
            _db.SaveChanges();

            // Redirect back to the Settings page
            return RedirectToAction("Settings", "Settings");
        }


        #endregion

        #region AlarmTypes



        public IActionResult DeactivateAlarmType(int statusRow)
        {
            // Retrieve the StatusDefinition object with the given StatusRow
            var status = _db.StatusDefinition.FirstOrDefault(s => s.StatusRow == statusRow);

            // Update the properties of the StatusDefinition object
            status.StatusName = "--unused--";
            status.StatusEnabled = false;
            status.StatusDetailsEnabled = 0;

            // Save changes to the database
            _db.SaveChanges();

            // Redirect back to the Settings page
            return RedirectToAction("Settings", "Settings");
        }

        [HttpPost]
        public IActionResult UpdateStatusEnabled(int statusRow, bool statusEnabled)
        {
            // Retrieve the StatusDefinition object with the given StatusRow
            StatusDefinition status = _db.StatusDefinition.FirstOrDefault(s => s.StatusRow == statusRow);

            // Update the StatusEnabled property of the StatusDefinition object
            status.StatusEnabled = statusEnabled;

            // Save changes to the database
            _db.SaveChanges();

            // Redirect back to the Settings page
            return RedirectToAction("Settings", "Settings");
        }

        [HttpPost]
        public IActionResult UpdateAlarmDetailsEnabled(int statusRow, int statusDetailsEnabled)
        {
            // Retrieve the StatusDefinition object with the given StatusRow
            var status = _db.StatusDefinition.FirstOrDefault(s => s.StatusRow == statusRow);

            // Update the StatusEnabled property of the StatusDefinition object
            status.StatusDetailsEnabled = statusDetailsEnabled;

            // Save changes to the database
            _db.SaveChanges();

            // Redirect back to the Settings page
            return RedirectToAction("Settings", "Settings");
        }

        [HttpPost]
        public IActionResult UpdateAlarmTypeName(int statusRow, string statusName)
        {
            // Retrieve the StatusDefinition object with the given StatusRow
            var status = _db.StatusDefinition.FirstOrDefault(s => s.StatusRow == statusRow);

            // Update the StatusName property of the StatusDefinition object
            status.StatusName = statusName;

            // Save changes to the database
            _db.SaveChanges();

            // Redirect back to the Settings page
            return RedirectToAction("Settings", "Settings");
        }


        #endregion



    }
}
