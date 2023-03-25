using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;
using eAndon_MVC.Hubs;
using eAndon_MVC.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;


namespace eAndon_MVC.Controllers
{

    public class HomeController : Controller
    {

        private readonly MyDbContext _db;
        private readonly IHubContext<StatusHub> _hubContext;

        public HomeController(MyDbContext db, IHubContext<StatusHub> hubContext)
        {
            _db = db;
            _hubContext = hubContext;
        }

        #region Index

        public IActionResult Index()
        {
            var workcenters = _db.WorkcenterList?.OrderBy(w => w.WorkcenterRow).ToList();
            return View(workcenters);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #endregion

        #region Overview

        public IActionResult Overview()
        {
            var currentOverview = _db.WorkcenterList?.OrderBy(w => w.WorkcenterRow).ToList();
            var settings = _db.Settings.ToList();
            var localizations = _db.Localization.ToList();
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
                            StatusValues = statusValues,
                            Settings = settings,
                            Localizations = localizations
                        };
                        currentOverviewModel.Add(workcenterModel);
                    }
                }
            }

            return View(currentOverviewModel);
        }

        [HttpGet("Home/GetAlarmHistoryModal")]  // for Overview and AndonTerminal
        public IActionResult GetAlarmHistoryModal(string workcenterID)
        {
            var logs = _db.AndonLog.AsQueryable();

            if (!string.IsNullOrEmpty(workcenterID))
            {
                logs = logs.Where(l => l.WorkcenterID == workcenterID);
            }

            // Load the filtered logs into memory
            var logsList = logs.ToList();

            // Order the logs by AlarmStartTime in descending order
            var orderedLogs = logsList.OrderByDescending(l => l.AlarmStartTime).ToList();

            // Group the logs by WorkcenterID, AlarmName, and AlarmStartTime
            var groupedLogs = orderedLogs.GroupBy(l => new { l.WorkcenterID, l.AlarmName, l.AlarmStartTime });

            // Select the appropriate log for each group
            var alarmHistory = groupedLogs.Select(group =>
                {
                    var finishedAlarm = group.FirstOrDefault(l => l.AlarmEndTime != DateTime.MinValue);
                    if (finishedAlarm != null)
                    {
                        return finishedAlarm;
                    }
                    else
                    {
                        return group.First();
                    }
                })
                .Select(l => new
                {
                    AlarmID = l.ID,
                    AlarmName = l.AlarmName,
                    AlarmStartTime = l.AlarmStartTime,
                    AlarmEndTime = l.AlarmEndTime,
                    Duration = l.AlarmEndTime == DateTime.MinValue ? DateTime.Now - l.AlarmStartTime : l.AlarmEndTime - l.AlarmStartTime,
                    AlarmStartText1 = l.AlarmStartText1,
                    AlarmStartText2 = l.AlarmStartText2,
                    AlarmStartText3 = l.AlarmStartText3
                })
                .ToList();

            return Json(alarmHistory);
        }


        #endregion

        #region AndonTerminal

        public IActionResult AndonTerminal(string workcenterID, string workcenterName)
        {

            var statusDefinitions = _db.StatusDefinition.ToList();
            var settings = _db.Settings.ToList();
            var localizations = _db.Localization.ToList();
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
                    StatusValues = statusValues,
                    Settings = settings,
                    Localizations = localizations
                };

                return View(model);

        }

        // updates alarm status in a terminal 
        [HttpPost]
        public IActionResult SetGreen(string workcenterID, string workcenterName, int statusIndex, string alarmName)
        {
            return UpdateStatus(workcenterID, workcenterName, statusIndex, alarmName, "green");
        }

        [HttpPost]
        public IActionResult SetRed(string workcenterID, string workcenterName, int statusIndex, string alarmName, string additionalDataJson)
        {
            dynamic additionalData = JsonConvert.DeserializeObject(additionalDataJson)!;

            string dropdown1Value = additionalData.dropdown1 ?? "";
            string dropdown2Value = additionalData.dropdown2 ?? "";
            string textFieldValue = additionalData.textField ?? "";

            return UpdateStatus(workcenterID, workcenterName, statusIndex, alarmName, "red", dropdown1Value, dropdown2Value, textFieldValue);
        }

        private IActionResult UpdateStatus(string workcenterID, string workcenterName, int statusIndex, string alarmName, string newStatusColor, string dropdown1Value = "", string dropdown2Value = "", string textFieldValue = "")
        {
            var workcenter = _db.WorkcenterList.FirstOrDefault(w => w.WorkcenterID == workcenterID);
            var alarmDetailsEnabled = _db.StatusDefinition.Select(s => s.StatusDetailsEnabled).ToList();
            var alarmStartText1Structure = _db.StatusDefinition.Select(s => s.AlarmStartText1Structure).ToList();
            var alarmStartText2Structure = _db.StatusDefinition.Select(s => s.AlarmStartText2Structure).ToList();
            var alarmStartText3Structure = _db.StatusDefinition.Select(s => s.AlarmStartText3Structure).ToList();

            var statusProperty = typeof(Workcenter).GetProperties().ElementAt(statusIndex + 3); // +3 because Status1 to Status5 are properties 3 to 7
            var currentStatus = (string)statusProperty.GetValue(workcenter)!;
            var now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            var newStatus = newStatusColor == "red" ? $"red|{now}|{dropdown1Value}|{dropdown2Value}|{textFieldValue}" : $"green|{now}";

            statusProperty.SetValue(workcenter, newStatus);

            var logEntry = new AndonLog
            {
                WorkcenterID = workcenterID,
                WorkcenterName = workcenterName,
                StatusIndex = statusIndex,
                AlarmName = alarmName,
                OldStatus = currentStatus.Split('|')[0],
                NewStatus = newStatus.Split('|')[0],
                ChangeDateTime = DateTime.Now,
                AlarmStartText1 = alarmDetailsEnabled[statusIndex] == 1 && alarmStartText1Structure[statusIndex]!.Split('|')[0] == "ON" ? dropdown1Value : "",
                AlarmStartText2 = alarmDetailsEnabled[statusIndex] == 1 && alarmStartText2Structure[statusIndex]!.Split('|')[0] == "ON" ? dropdown2Value : "",
                AlarmStartText3 = alarmDetailsEnabled[statusIndex] == 1 && alarmStartText3Structure[statusIndex]!.Split('|')[0] == "ON" ? textFieldValue : ""
            };

            if (logEntry.NewStatus == "red")
            {
                logEntry.AlarmStartTime = DateTime.Now;
            }
            else
            {
                var lastRedLogEntry = _db.AndonLog
                    .Where(l => l.WorkcenterID == workcenterID && l.StatusIndex == statusIndex && l.NewStatus == "red")
                    .OrderByDescending(l => l.ChangeDateTime)
                    .FirstOrDefault();

                if (lastRedLogEntry != null)  // copy some items from start of alarm entry to the end entry
                {
                    logEntry.AlarmStartTime = lastRedLogEntry.AlarmStartTime;
                    logEntry.AlarmEndTime = DateTime.Now;
                    logEntry.AlarmStartText1 = lastRedLogEntry.AlarmStartText1;
                    logEntry.AlarmStartText2 = lastRedLogEntry.AlarmStartText2;
                    logEntry.AlarmStartText3 = lastRedLogEntry.AlarmStartText3;
                }
            }

            _db.AndonLog.Add(logEntry);

            _db.SaveChanges();

            // Call the SignalR method to update the clients
            _hubContext.Clients.All.SendAsync("ReceiveStatusUpdate", workcenterID, statusIndex, newStatus);

            return Content(newStatus);
        }


        #endregion

        #region Log

        public IActionResult Log(DateTime? startDate, DateTime? endDate, string workcenterID, bool showFinishedAlarms)
        {
            var logs = _db.AndonLog.AsQueryable();

            if (startDate != null)
            {
                var startDateValue = startDate.Value.Date;
                logs = logs.Where(l => l.ChangeDateTime.Date >= startDateValue);
            }

            if (endDate != null)
            {
                var endDateValue = endDate.Value.Date;
                logs = logs.Where(l => l.ChangeDateTime.Date <= endDateValue);
            }

            if (!string.IsNullOrEmpty(workcenterID))
            {
                logs = logs.Where(l => l.WorkcenterID == workcenterID);
            }

            if (showFinishedAlarms)
            {
                logs = logs.Where(l => l.AlarmEndTime != DateTime.MinValue);
            }

            // Load the filtered logs into memory
            var logsList = logs.ToList();

            // Group the logs by WorkcenterID, AlarmName, and AlarmStartTime
            var groupedLogs = logsList.GroupBy(l => new { l.WorkcenterID, l.AlarmName, l.AlarmStartTime });

            // Select the appropriate log for each group
            var selectedLogs = groupedLogs.Select(group =>
            {
                var finishedAlarm = group.FirstOrDefault(l => l.AlarmEndTime != DateTime.MinValue);
                if (finishedAlarm != null)
                {
                    return finishedAlarm;
                }
                else
                {
                    return group.First();
                }
            }).ToList();

            var logsWithDurations = selectedLogs.Select(l => new AndonLog
            {
                ID = l.ID,
                WorkcenterID = l.WorkcenterID,
                WorkcenterName = l.WorkcenterName,
                StatusIndex = l.StatusIndex,
                AlarmName = l.AlarmName,
                OldStatus = l.OldStatus,
                NewStatus = l.NewStatus,
                ChangeDateTime = l.ChangeDateTime,
                AlarmStartTime = l.AlarmStartTime,
                AlarmEndTime = l.AlarmEndTime,
                AlarmStartText1 = l.AlarmStartText1,
                AlarmStartText2 = l.AlarmStartText2,
                AlarmStartText3 = l.AlarmStartText3
            }).ToList();

            ViewBag.Workcenters = _db.WorkcenterList
                .Select(w => new { w.WorkcenterID, w.WorkcenterName })
                .Distinct()
                .ToList();
            ViewBag.StatusDefinitions = _db.StatusDefinition.ToList();

            return View(logsWithDurations);
        }



        #endregion

        #region Statistics

        public IActionResult Statistics(DateTime? startDate, DateTime? endDate, string workcenterID)
        {
            var logs = _db.AndonLog.AsQueryable();

            if (startDate != null)
            {
                var startDateValue = startDate.Value.Date;
                logs = logs.Where(l => l.AlarmStartTime.Date >= startDateValue);
            }

            if (endDate != null)
            {
                var endDateValue = endDate.Value.Date;
                logs = logs.Where(l => l.AlarmEndTime.Date <= endDateValue);
            }

            if (!string.IsNullOrEmpty(workcenterID))
            {
                logs = logs.Where(l => l.WorkcenterID == workcenterID);
            }

            // Load the filtered logs into memory
            var logsList = logs.ToList();

            // Filter logs to include only finished alarms
            var finishedLogs = logsList.Where(l => l.AlarmEndTime != DateTime.MinValue).ToList();

            var totalAlarms = finishedLogs.Count;

            var mttr = finishedLogs.Any()
                ? finishedLogs.Average(l => (l.AlarmEndTime - l.AlarmStartTime).TotalSeconds)
                : 0;

            var mtbf = finishedLogs.Any() && finishedLogs.Count > 1
                ? finishedLogs.Zip(finishedLogs.Skip(1), (a, b) => (b.AlarmStartTime - a.AlarmEndTime).TotalSeconds).Average()
                : 0;

            // Calculate alarms per workcenter
            var workcenters = _db.WorkcenterList.ToList();
            var workcenterStatistics = workcenters
                .Select(w => new WorkcenterStatistic
                {
                    WorkcenterID = w.WorkcenterID,
                    WorkcenterName = w.WorkcenterName,
                    NumberOfAlarms = finishedLogs.Count(l => l.WorkcenterID == w.WorkcenterID)
                })
                .ToList();

            var alarmNameStatistics = finishedLogs
                .GroupBy(l => l.AlarmName)
                .Select(g => new AlarmNameStatistic
                {
                    AlarmName = g.Key,
                    NumberOfAlarms = g.Count()
                })
                .ToList();

            ViewBag.AlarmNameStatistics = alarmNameStatistics;
            ViewBag.Workcenters = _db.WorkcenterList
                .Select(w => new { w.WorkcenterID, w.WorkcenterName })
                .Distinct()
                .ToList();
            ViewBag.TotalAlarms = totalAlarms;
            ViewBag.MTTR = mttr;
            ViewBag.MTBF = mtbf;
            ViewBag.WorkcenterStatistics = workcenterStatistics; 

            return View();
        }

[HttpGet("Home/GetWorkcenterAlarmTypeStats")]
public IActionResult GetWorkcenterAlarmTypeStats(DateTime? startDate, DateTime? endDate, string workcenterID)
{
    var logs = _db.AndonLog.AsQueryable();

    if (startDate != null)
    {
        var startDateValue = startDate.Value.Date;
        logs = logs.Where(l => l.AlarmStartTime.Date >= startDateValue);
    }

    if (endDate != null)
    {
        var endDateValue = endDate.Value.Date;
        logs = logs.Where(l => l.AlarmEndTime.Date <= endDateValue);
    }

    if (!string.IsNullOrEmpty(workcenterID))
    {
        logs = logs.Where(l => l.WorkcenterID == workcenterID);
    }

    // Load the filtered logs into memory
    var logsList = logs.ToList();

    // Filter logs to include only finished alarms
    var finishedLogs = logsList.Where(l => l.AlarmEndTime != DateTime.MinValue).ToList();

    // Calculate the total number of alarms
    var totalAlarms = finishedLogs.Count;

    var alarmNameStatistics = finishedLogs
        .GroupBy(l => l.AlarmName)
        .Select(g => new AlarmNameStatistic
        {
            AlarmName = g.Key,
            NumberOfAlarms = g.Count(),
            PercentageOfTotal = (double)g.Count() / totalAlarms * 100
        })
        .OrderByDescending(a => a.NumberOfAlarms) 
        .ToList();

    var alarmLocationStatistics = finishedLogs
        .Where(l => !string.IsNullOrEmpty(l.AlarmStartText1))
        .GroupBy(l => l.AlarmStartText1)
        .Select(g => new AlarmLocationStatistic
        {
            AlarmLocation = g.Key,
            NumberOfAlarms = g.Count(),
            PercentageOfTotal = (double)g.Count() / totalAlarms * 100
        })
        .OrderByDescending(a => a.NumberOfAlarms)
        .ToList();

    var alarmTypeStatistics = finishedLogs
        .Where(l => !string.IsNullOrEmpty(l.AlarmStartText2))
        .GroupBy(l => l.AlarmStartText2)
        .Select(g => new AlarmTypeStatistic
        {
            AlarmType = g.Key,
            NumberOfAlarms = g.Count(),
            PercentageOfTotal = (double)g.Count() / totalAlarms * 100
        })
        .OrderByDescending(a => a.NumberOfAlarms)
        .ToList();

    var result = new
    {
        AlarmNameStatistics = alarmNameStatistics,
        AlarmLocationStatistics = alarmLocationStatistics,
        AlarmTypeStatistics = alarmTypeStatistics
    };

    return Json(result);
}


        #endregion

    }
}