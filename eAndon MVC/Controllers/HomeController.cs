using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;
using eAndon_MVC.Models;
using Microsoft.EntityFrameworkCore;


namespace eAndon_MVC.Controllers
{

    public class HomeController : Controller
    {

        private readonly MyDbContext _db;

        public HomeController(MyDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var workcenters = _db.WorkcenterList?.OrderBy(w => w.WorkcenterRow).ToList();
            return View(workcenters);
        }

        public IActionResult Overview()
        {
            var currentOverview = _db.WorkcenterList?.OrderBy(w => w.WorkcenterRow).ToList();
            var currentOverviewModel = new List<AndonTerminalModel>();

            if (currentOverview == null) return View(currentOverviewModel);
            {
                foreach (var workcenter in currentOverview)
                {
                    var statusDefinitions = _db.StatusDefinition.ToList();

                    var statusValues = _db.WorkcenterList?.Where(w => w.WorkcenterID == workcenter.WorkcenterID)
                        .Select(w => new List<string>
                        {
                            statusDefinitions[0].StatusEnabled == true ? w.Status1 : "",
                            statusDefinitions[1].StatusEnabled == true ? w.Status2 : "",
                            statusDefinitions[2].StatusEnabled == true ? w.Status3 : "",
                            statusDefinitions[3].StatusEnabled == true ? w.Status4 : "",
                            statusDefinitions[4].StatusEnabled == true ? w.Status5 : ""
                        })
                        .FirstOrDefault();

                    // Create an instance of the custom model and populate it with the workcenter information and the status values
                    if (statusValues != null)
                    {
                        var workcenterModel = new AndonTerminalModel
                        {
                            WorkcenterID = workcenter.WorkcenterID,
                            WorkcenterName = workcenter.WorkcenterName,
                            StatusDefinitions = statusDefinitions,
                            StatusValues = statusValues
                        };
                        currentOverviewModel.Add(workcenterModel);
                    }
                }
            }

            return View(currentOverviewModel);
        }


        public IActionResult AndonTerminal(string workcenterID, string workcenterName)
        {

            var statusDefinitions = _db.StatusDefinition.ToList();
            // Retrieve the status values for the workcenter from the database
            var statusValues = _db.WorkcenterList
                    .Where(w => w.WorkcenterID == workcenterID)
                    .Select(w => new List<string> {                         statusDefinitions[0].StatusEnabled == true ? w.Status1 : "",
                        statusDefinitions[1].StatusEnabled == true ? w.Status2 : "",
                        statusDefinitions[2].StatusEnabled == true ? w.Status3 : "",
                        statusDefinitions[3].StatusEnabled == true ? w.Status4 : "",
                        statusDefinitions[4].StatusEnabled == true ? w.Status5 : ""})
                    .FirstOrDefault();

                // Create an instance of the custom model and populate it with the workcenter information and the status values
                var model = new AndonTerminalModel
                {
                    WorkcenterID = workcenterID,
                    WorkcenterName = workcenterName,
                    StatusDefinitions = statusDefinitions,
                    StatusValues = statusValues
                };

                return View(model);

        }

        public IActionResult UpdateStatus(string workcenterID, int statusIndex)
        {
            var workcenter = _db.WorkcenterList.FirstOrDefault(w => w.WorkcenterID == workcenterID);

            var statusProperty = typeof(Workcenter).GetProperties().ElementAt(statusIndex + 3); // +3 because Status1 to Status5 are properties 3 to 7
            var currentStatus = (string)statusProperty.GetValue(workcenter)!;
            var now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            var newStatus = currentStatus.Split('|')[0] == "green" ? "red|" + now : "green|" + now;
            statusProperty.SetValue(workcenter, newStatus);

            var logEntry = new WorkcenterStatusLog
            {
                WorkcenterID = workcenterID,
                StatusIndex = statusIndex,
                OldStatus = currentStatus.Split('|')[0],
                NewStatus = newStatus.Split('|')[0],
                ChangeDateTime = DateTime.Now
            };

            _db.WorkcenterStatusLog.Add(logEntry);

            _db.SaveChanges();

            return Content(newStatus);
        }

        public IActionResult Log(DateTime? startDate, DateTime? endDate, string workcenterID)
        {
            var logs = _db.WorkcenterStatusLog.AsQueryable();

            if (startDate != null)
            {
                logs = logs.Where(l => l.ChangeDateTime >= startDate);
            }

            if (endDate != null)
            {
                logs = logs.Where(l => l.ChangeDateTime <= endDate);
            }

            if (!string.IsNullOrEmpty(workcenterID))
            {
                logs = logs.Where(l => l.WorkcenterID == workcenterID);
            }

            ViewBag.Workcenters = _db.WorkcenterList.Select(w => w.WorkcenterID).Distinct().ToList();
            ViewBag.StatusDefinitions = _db.StatusDefinition.ToList();

            return View(logs.ToList());
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}