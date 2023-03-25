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
            var statusDefinitionsList = await _db.StatusDefinition.OrderBy(w => w.StatusRow).ToListAsync();
            var workcenterList = await _db.WorkcenterList.ToListAsync();
            var settingsList = await _db.Settings.ToListAsync();
            var localizationList = await _db.Localization.ToListAsync();

            // Add lists to the ViewBag
            ViewBag.StatusList = statusDefinitionsList;
            ViewBag.Workcenters = workcenterList;
            ViewBag.SettingsList = settingsList;
            ViewBag.Localization = localizationList;
            
            return View("~/Views/Home/Settings.cshtml", statusDefinitionsList);
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
            var workcenterToDelete = await _db.WorkcenterList.FindAsync(workcenterRow - 1);
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
                var higherWc = await _db.WorkcenterList.FindAsync(wc.WorkcenterRow + 1);
                if (higherWc != null)
                {
                    wc.WorkcenterID = higherWc.WorkcenterID; // Update WorkcenterID
                    wc.WorkcenterName = higherWc.WorkcenterName; // Update WorkcenterName
                    wc.Status1 = higherWc.Status1;
                    wc.Status2 = higherWc.Status2;
                    wc.Status3 = higherWc.Status3;
                    wc.Status4 = higherWc.Status4;
                    wc.Status5 = higherWc.Status5;
                }

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
            if (status != null) status.WorkcenterName = workcenterName;

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
            if (status != null) status.WorkcenterID = workcenterID;

            // Save changes to the database
            _db.SaveChanges();

            // Redirect back to the Settings page
            return RedirectToAction("Settings", "Settings");
        }


        #endregion

        #region AlarmTypes
        
        [HttpPost]
        public async Task<IActionResult> MoveAlarmTypeUp(int statusRow)
        {
            // Find the alarm type with the given row number
            var status = await _db.StatusDefinition.FindAsync(statusRow);
            if (status == null)
            {
                return NotFound();
            }
    
            // Find the alarm type with the row number one less than the given row number
            var prevStatus = await _db.StatusDefinition.FirstOrDefaultAsync(w => w.StatusRow == statusRow - 1);
            if (prevStatus == null)
            {
                return BadRequest("Cannot move status up any further");
            }
    
            // Swap the other fields of the two alarm types

            var tempStatusName = status.StatusName;
            var tempStatusEnabled = status.StatusEnabled;
            var tempStatusDetailsEnabled = status.StatusDetailsEnabled;
            var tempIconName = status.IconName;

            status.StatusName = prevStatus.StatusName;
            status.StatusEnabled = prevStatus.StatusEnabled;
            status.StatusDetailsEnabled = prevStatus.StatusDetailsEnabled;
            status.IconName = prevStatus.IconName;
   
            prevStatus.StatusName = tempStatusName;
            prevStatus.StatusEnabled = tempStatusEnabled;
            prevStatus.StatusDetailsEnabled = tempStatusDetailsEnabled;
            prevStatus.IconName = tempIconName;

            // Save changes to the database
            await _db.SaveChangesAsync();

            return RedirectToAction("Settings");
        }

        
        [HttpPost]
        public async Task<IActionResult> MoveAlarmTypeDown(int statusRow)
        {
            // Find the alarm type with the given row number
            var status = await _db.StatusDefinition.FindAsync(statusRow);
            if (status == null)
            {
                return NotFound();
            }
    
            // Find the alarm type with the row number one more than the given row number
            var nextStatus = await _db.StatusDefinition.FirstOrDefaultAsync(w => w.StatusRow == statusRow + 1);
            if (nextStatus == null)
            {
                return BadRequest("Cannot move status down any further");
            }
    
            // Swap the other fields of the two alarm types

            var tempStatusName = status.StatusName;
            var tempStatusEnabled = status.StatusEnabled;
            var tempStatusDetailsEnabled = status.StatusDetailsEnabled;
            var tempIconName = status.IconName;

            status.StatusName = nextStatus.StatusName;
            status.StatusEnabled = nextStatus.StatusEnabled;
            status.StatusDetailsEnabled = nextStatus.StatusDetailsEnabled;
            status.IconName = nextStatus.IconName;
   
            nextStatus.StatusName = tempStatusName;
            nextStatus.StatusEnabled = tempStatusEnabled;
            nextStatus.StatusDetailsEnabled = tempStatusDetailsEnabled;
            nextStatus.IconName = tempIconName;

            // Save changes to the database
            await _db.SaveChangesAsync();

            return RedirectToAction("Settings");
        }


        [HttpPost]
        public IActionResult UpdateStatusEnabled(int statusRow, bool statusEnabled)
        {
            // Retrieve the StatusDefinition object with the given StatusRow
            var status = _db.StatusDefinition.FirstOrDefault(s => s.StatusRow == statusRow);

            // Update the StatusEnabled property of the StatusDefinition object
            if (status != null) status.StatusEnabled = statusEnabled;

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
            if (status != null) status.StatusDetailsEnabled = statusDetailsEnabled;

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
            if (status != null) status.StatusName = statusName;

            // Save changes to the database
            _db.SaveChanges();

            // Redirect back to the Settings page
            return RedirectToAction("Settings", "Settings");
        }

        [HttpPost]
        public IActionResult UpdateAlarmTypeIcon(int statusRow, string iconName)
        {
            // Update the alarm type with the specified statusRow to use the specified iconName
            // Retrieve the StatusDefinition object with the given StatusRow
            var status = _db.StatusDefinition.FirstOrDefault(s => s.StatusRow == statusRow);

            // Update the StatusName property of the StatusDefinition object
            if (status != null) status.IconName = iconName;

            // Save changes to the database
            _db.SaveChanges();

            // Redirect back to the settings page
            return RedirectToAction("Settings", "Settings");
        }
        
        [HttpPost]
        public IActionResult UpdateAlarmStartDetails(int statusIndex, string failureLocationOptions, string failureTypeOptions, string detailsTextOptions)
        {
            // Retrieve the StatusDefinition object with the given statusIndex
            var status = _db.StatusDefinition.FirstOrDefault(s => s.StatusRow == statusIndex + 1);

            // Update the AlarmStartText1Structure, AlarmStartText2Structure, and AlarmStartText3Structure properties of the StatusDefinition object
            if (status != null)
            {
                status.AlarmStartText1Structure = failureLocationOptions;
                status.AlarmStartText2Structure = failureTypeOptions;
                status.AlarmStartText3Structure = detailsTextOptions;
            }

            // Save changes to the database
            _db.SaveChanges();

            // Return a JSON response indicating success or failure
            return Json(new { success = status != null });
        }
        
        #endregion

        #region InterfaceSettings

        public IActionResult ResetInterfaceSettings()
        {
            // Retrieve the StatusDefinition object with the given StatusRow
            var settings = _db.Settings;

            // Return to default settings
            foreach (var setting in settings)
            {
                setting.CurrentSetting = setting.DefaultSetting;
            }

            // Save changes to the database
            _db.SaveChanges();

            // Redirect back to the Settings page
            return RedirectToAction("Settings", "Settings");
        }

        [HttpPost]
        public IActionResult UpdateSetting(int settingID, string settingValue)
        {
            // Retrieve the setting object with the given SettingName
            var setting = _db.Settings.FirstOrDefault(s => s.SettingID == settingID);

            if (setting != null)
            {
                // Update the setting's CurrentSetting value
                setting.CurrentSetting = settingValue;

                // Save changes to the database
                _db.SaveChanges();
            }

            // Redirect back to the Settings page
            return RedirectToAction("Settings", "Settings");
        }

        public class UpdateSettingRequest
        {
            public int SettingID { get; set; }
            public string SettingValue { get; set; } = null!;
        }

        [HttpPost]
        public IActionResult UpdateSettingFromOverview([FromBody] UpdateSettingRequest request)
        {
            // Retrieve the setting object with the given SettingName
            var setting = _db.Settings.FirstOrDefault(s => s.SettingID == request.SettingID);

            if (setting != null)
            {
                // Update the setting's CurrentSetting value
                setting.CurrentSetting = request.SettingValue;

                // Save changes to the database
                _db.SaveChanges();
            }

            // Return a success response
            return Ok();
        }


        #endregion

        #region Localization

        [HttpPost]
        public IActionResult UpdateTranslation(string localizationId, string localizedText)
        {
            var localization = _db.Localization.FirstOrDefault(l => l.Id == localizationId);
            if (localization != null)
            {
                localization.Translation = localizedText;
                _db.SaveChanges();
            }
            return RedirectToAction("Settings");
        }

        #endregion


    }
}
