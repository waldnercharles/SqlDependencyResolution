using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqlDependencyResolution
{
    [TestClass]
    public class TopologicalSorterTests : TestBase
    {
        [TestMethod]
        public void PerformTopologicalSort()
        {
            
            foreach (var id in sorted)
            {
                Assert.IsTrue(groups.Count > 0);

                var currentGroup = groups[0];

                Assert.IsTrue(currentGroup.Contains(id));
                currentGroup.Remove(id);

                if (currentGroup.Count == 0)
                {
                    groups.RemoveAt(0);
                }
            }
        }
    }
}
