using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eAndon_MVC.Models
{
    public class AndonTerminalModel
    {
        public string WorkcenterID { get; set; }
        public string WorkcenterName { get; set; }
        public List<string> StatusValues { get; set; }
        public List<StatusDefinition> StatusDefinitions { get; set; }
    }

    public class Workcenter
    {
        [Key]
        public int WorkcenterRow { get; set; }
        public string WorkcenterID { get; set; }
        public string WorkcenterName { get; set; }
        public string Status1 { get; set; }
        public string Status2 { get; set; }
        public string Status3 { get; set; }
        public string Status4 { get; set; }
        public string Status5 { get; set; }
    }

    public class StatusDefinition
    {
        [Key]
        public int StatusRow { get; set; }
        public string StatusName { get; set; }
        public bool StatusEnabled { get; set; }
        public int StatusDetailsEnabled { get; set; }
    }

    public class WorkcenterStatusLog
    {
        [Key]
        public int ID { get; set; }
        public string WorkcenterID { get; set; }
        public int StatusIndex { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public DateTime ChangeDateTime { get; set; }
    }
}
