using System.ComponentModel.DataAnnotations;

namespace eAndon_MVC.Models
{
    public class AndonTerminalModel
    {
        public string WorkcenterID { get; set; } = null!;
        public string WorkcenterName { get; set; } = null!;
        public List<string>? StatusValues { get; set; }
        public List<StatusDefinition> StatusDefinitions { get; set; } = null!;
        public List<Settings>? Settings { get; set; }
        public List<Localization>? Localizations { get; set; }
    }

    public class Workcenter
    {
        [Key]
        public int WorkcenterRow { get; init; }
        public string WorkcenterID { get; set; } = null!;
        public string WorkcenterName { get; set; } = null!;
        public string Status1 { get; set; } = null!;
        public string Status2 { get; set; } = null!;
        public string Status3 { get; set; } = null!;
        public string Status4 { get; set; } = null!;
        public string Status5 { get; set; } = null!;
    }

    public class StatusDefinition
    {
        [Key]
        public int StatusRow { get; set; }
        public string StatusName { get; set; } = null!;
        public bool StatusEnabled { get; set; }
        public int StatusDetailsEnabled { get; set; }
        public string? IconName { get; set; } = null!;
        public string? AlarmStartText1Structure { get; set; }
        public string? AlarmStartText2Structure { get; set; }
        public string? AlarmStartText3Structure { get; set; }
        public string? AlarmEndText1Structure { get; set; }
        public string? AlarmEndText2Structure { get; set; }
        public string? AlarmEndText3Structure { get; set; }
        public string? AlarmEndText4Structure { get; set; }
    }

    public class AndonLog
    {
        [Key]
        public int ID { get; set; }
        public string WorkcenterID { get; set; } = null!;
        public string? WorkcenterName { get; set; } = null!;
        public int StatusIndex { get; set; }
        public string? AlarmName { get; set; }
        public string OldStatus { get; set; } = null!;
        public string NewStatus { get; set; } = null!;
        public DateTime ChangeDateTime { get; set; }
        public DateTime AlarmStartTime { get; set; }
        public DateTime AlarmEndTime { get; set; }
        public string? AlarmStartText1 { get; set; }
        public string? AlarmStartText2 { get; set; }
        public string? AlarmStartText3 { get; set; }
        public string? AlarmEndText1 { get; set; }
        public string? AlarmEndText2 { get; set; }
        public string? AlarmEndText3 { get; set; }
        public string? AlarmEndText4 { get; set; }
    }

    public class Localization
    {
        [Key]
        public string Id { get; set; } = null!;
        public string English { get; set; } = null!;
        public string Translation { get; set; } = null!;
    }

    public class Settings
    {
        [Key]
        public int SettingID { get; set; } 
        public string SettingName { get; set; } = null!;
        public string CurrentSetting { get; set; } = null!;
        public string PossibleSettings { get; set; } = null!;
        public string DefaultSetting { get; set; } = null!;
    }

    public class WorkcenterStatistic
    {
        public string? WorkcenterID { get; set; }
        public string? WorkcenterName { get; set; }
        public int NumberOfAlarms { get; set; }
    }

    public class AlarmNameStatistic
    {
        public string? AlarmName { get; set; }
        public int NumberOfAlarms { get; set; }
        public double PercentageOfTotal { get; set; }
    }

    public class AlarmLocationStatistic
    {
        public string? AlarmLocation { get; set; }
        public int NumberOfAlarms { get; set; }
        public double PercentageOfTotal { get; set; }
    }

    public class AlarmTypeStatistic
    {
        public string? AlarmType { get; set; }
        public int NumberOfAlarms { get; set; }
        public double PercentageOfTotal { get; set; }
    }
}
