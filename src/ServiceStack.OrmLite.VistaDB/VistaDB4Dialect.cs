using ServiceStack.OrmLite.VistaDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceStack.OrmLite
{
    public static class VistaDBD4ialect
    {
        public static IOrmLiteDialectProvider Provider { get { return VistaDB4DialectProvider.Instance; } }
    }
}
