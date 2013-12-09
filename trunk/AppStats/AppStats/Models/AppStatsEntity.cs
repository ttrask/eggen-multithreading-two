using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppStats.Models
{
    public abstract class AppStatsEntity
    {
    }

    public partial class DropFile : AppStatsEntity { }
    public partial class Environment : AppStatsEntity { }
    public partial class Language : AppStatsEntity { }
}