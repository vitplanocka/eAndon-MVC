using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eAndon_MVC.Models
{
    public class AndonTerminalModel
    {
        public string WorkcenterID { get; set; } = null!;
        public string WorkcenterName { get; set; } = null!;
        public List<string>? StatusValues { get; set; }
        public List<StatusDefinition> StatusDefinitions { get; set; } = null!;
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
    }

    public class WorkcenterStatusLog
    {
        [Key]
        public int ID { get; set; }
        public string WorkcenterID { get; set; } = null!;
        public int StatusIndex { get; set; }
        public string OldStatus { get; set; } = null!;
        public string NewStatus { get; set; } = null!;
        public DateTime ChangeDateTime { get; set; }
    }
}
