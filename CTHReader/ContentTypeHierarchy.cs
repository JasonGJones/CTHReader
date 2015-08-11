using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTHReader
{
    public class ContentTypeHierarchy
    {
        private CTStructure rootCT = new CTStructure() { CTName = "System", CTParentName = "System" };
        private List<CTStructure> _orphanedCT = new List<CTStructure>();
        public void AddToHierarchy(string CTName, string CTParentName, string CTId)
        {
            if (!rootCT.AddToHierarchy(CTName, CTParentName))
            {
                _orphanedCT.Add(new CTStructure() { CTName = CTName, CTParentName = CTParentName, CTID = CTId });
            }
        }

        public void AddToOrphanCT(string CTName, string CTParentName, string CTId)
        {
            CTStructure cts = new CTStructure() { CTName = CTName, CTParentName = CTParentName, CTID = CTId };
            _orphanedCT.Insert(0, cts);
        }


        public int GetOrphanCount()
        {
            
            List<CTStructure> distinctOrphans = new List<CTStructure>();
            foreach(var orphan in _orphanedCT.GroupBy(x => x.CTName).Distinct())
            {
                distinctOrphans.Add(orphan.First());
            }
            _orphanedCT = distinctOrphans;
            return _orphanedCT.Count;
        }

        public bool AssociateOrphansToHierarchy()
        {
            bool orphansRemoved = false;
            int orphanCount = _orphanedCT.Count;
            int orphanCountOriginal = orphanCount;

            List<CTStructure> orphansToRemove = new List<CTStructure>();

            foreach(CTStructure orphan in _orphanedCT)
            {
                if(rootCT.AddToHierarchy(orphan.CTName, orphan.CTParentName))
                {
                    orphanCount--;
                    orphansToRemove.Add(orphan);
                }
            }
            _orphanedCT.RemoveAll(x => orphansToRemove.Contains(x));

            if (orphanCount < orphanCountOriginal)
            {
                AssociateOrphansToHierarchy();
                orphansRemoved = true;
            }
            return orphansRemoved;            
        }

        public string GetTabbedHierarchy()
        {
            return rootCT.GetHierarchy(0);
        }

        private class CTStructure
        {
            internal string CTName;            
            internal string CTParentName;
            internal string CTID;
            internal List<CTStructure> CTChildren = new List<CTStructure>();

            public string GetHierarchy(int NumberOfTabs)
            {
                string tabs = new String('\t', NumberOfTabs);
                StringBuilder hierarchy = new StringBuilder();
                hierarchy.AppendLine(String.Format("{0}{1}", tabs, CTName));
                foreach(CTStructure ct in CTChildren)
                {
                    int newTabCount = NumberOfTabs + 1;
                    hierarchy.Append(ct.GetHierarchy(newTabCount));
                }
                return hierarchy.ToString();
            }

            public bool AddToHierarchy(string AppendingCTName, string AppendingCTParentName)
            {
                bool successfullyAdded = false;
                if (AppendingCTParentName == CTParentName)
                {
                    int itemCount = CTChildren.Count(x => x.CTName == AppendingCTName);
                    if (AppendingCTName != CTName && itemCount == 0)
                    {
                        CTChildren.Add(new CTStructure() { CTName = AppendingCTName, CTParentName = AppendingCTParentName });
                    }
                    successfullyAdded = true;
                }
                else
                {
                    foreach (CTStructure cts in CTChildren)
                    {
                        if (cts.CTName == AppendingCTParentName)
                        {
                            int itemCount = cts.CTChildren.Count(x => x.CTName == AppendingCTName);
                            if (itemCount == 0)
                            {
                                cts.CTChildren.Add(new CTStructure() { CTName = AppendingCTName, CTParentName = AppendingCTParentName });
                            }
                            successfullyAdded = true;
                            break;
                        }
                        else
                        {
                            if (cts.AddToHierarchy(AppendingCTName, AppendingCTParentName))
                            {
                                successfullyAdded = true;
                                break;
                            }
                        }
                    }
                }
                return successfullyAdded;
            }
        }



        internal string GetOrphanCTList()
        {
            var distinctOrphans = _orphanedCT.Select(x => x.CTName + "\t parent: " + x.CTParentName).Distinct();
            return string.Join(Environment.NewLine, distinctOrphans);
        }

        internal List<string> GetOrphansIds()
        {
            return _orphanedCT.Select(x => x.CTID).ToList();
        }
    }
}
