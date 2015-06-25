using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CTHReader
{
    class CTHReader
    {
        private string _siteUrl;
        private string _outputFileCTH;
        private string _outputFileSiteCols;

        private const string comma = ",";

        public enum CTHQueryMode
        {
            CTHOnly,
            SiteColumnsOnly,
            CTHAndSiteColumns
        }



        public string ProcessCTH(string siteUrl, string outputDirectory, CTHQueryMode operatingMode)
        {
            _siteUrl = siteUrl;
            calculateOutputFileNames(outputDirectory);

            string outputResult = string.Empty;
            try
            {
                if (operatingMode == CTHQueryMode.CTHOnly || operatingMode == CTHQueryMode.CTHAndSiteColumns)
                {
                    XDocument ctDoc = RemoveUnwantedAttributes(ReadCTHXML());
                    //Save the output to disk
                    SaveCTHXMLToCsv(ctDoc);
                    outputResult += "Document Saved to " + _outputFileCTH + Environment.NewLine;
                }

                if (operatingMode == CTHQueryMode.SiteColumnsOnly || operatingMode == CTHQueryMode.CTHAndSiteColumns)
                {
                    StringBuilder scDoc = ReadSiteColumns();
                    WriteFile(scDoc.ToString(), _outputFileSiteCols);
                    outputResult += "Document Saved to " + _outputFileSiteCols + Environment.NewLine;
                }

            }
            catch (Exception err)
            {
                outputResult = String.Format("Error occurred: {0}", err.Message);

            }
           
            return outputResult;
        }

        private StringBuilder ReadSiteColumns()
        {
            StringBuilder csv = AddSiteColumnHeaderToCSV();
            using (ClientContext clientContext = new ClientContext(_siteUrl))
            {
                var siteCols = clientContext.Web.Fields;
                clientContext.Load(siteCols);

                clientContext.ExecuteQuery();
                XDocument allFields = new XDocument(new XElement("SiteColumns"));

                foreach (Field sc in siteCols)
                {
                    StringBuilder csvLine = new StringBuilder();
                        csvLine.Append(TidyField(sc.DefaultValue) + comma);
                        csvLine.Append(TidyField(sc.Description) + comma);
                        csvLine.Append(sc.Direction + comma);
                        csvLine.Append(sc.EnforceUniqueValues + comma);
                        csvLine.Append(sc.EntityPropertyName + comma);
                        csvLine.Append(sc.Filterable + comma);
                        csvLine.Append(sc.FromBaseType + comma);
                        csvLine.Append(sc.Group + comma);
                        csvLine.Append(sc.Hidden + comma);
                        csvLine.Append(sc.Id + comma);
                        csvLine.Append(sc.Indexed + comma);
                        csvLine.Append(sc.InternalName + comma);
                        csvLine.Append(sc.JSLink + comma);
                        csvLine.Append(sc.ReadOnlyField + comma);
                        csvLine.Append(sc.Required + comma);
                        csvLine.Append(sc.Scope + comma);
                        csvLine.Append(sc.Sealed + comma);
                        csvLine.Append(sc.Sortable + comma);
                        csvLine.Append(sc.StaticName + comma);
                        csvLine.Append(sc.Title + comma);
                        csvLine.Append(sc.FieldTypeKind + comma);
                        csvLine.Append(sc.TypeAsString + comma);
                        csvLine.Append(sc.TypeDisplayName + comma);
                        csvLine.Append(TidyField(sc.TypeShortDescription) + comma);
                        csvLine.Append(TidyField(sc.ValidationFormula) + comma);
                        csvLine.Append(sc.ValidationMessage);

                        csv.AppendLine(csvLine.ToString());
                }
            }

            return csv;
        }

        private string TidyField(string fieldValue)
        {
            if (fieldValue != null)
            {
                return fieldValue.Replace(comma, "");
            }

            return fieldValue;

        }

        private StringBuilder AddSiteColumnHeaderToCSV()
        {
            StringBuilder scCSV = new StringBuilder();
            scCSV.Append("DefaultValue" + comma);
            scCSV.Append("Description" + comma);
            scCSV.Append("Direction" + comma);
            scCSV.Append("EnforceUniqueValues" + comma);
            scCSV.Append("EntityPropertyName" + comma);
            scCSV.Append("Filterable" + comma);
            scCSV.Append("FromBaseType" + comma);
            scCSV.Append("Group" + comma);
            scCSV.Append("Hidden" + comma);
            scCSV.Append("Id" + comma);
            scCSV.Append("Indexed" + comma);
            scCSV.Append("InternalName" + comma);
            scCSV.Append("JSLink" + comma);
            scCSV.Append("ReadOnlyField" + comma);
            scCSV.Append("Required" + comma);
            scCSV.Append("Scope" + comma);
            scCSV.Append("Sealed" + comma);
            scCSV.Append("Sortable" + comma);
            scCSV.Append("StaticName" + comma);
            scCSV.Append("Title" + comma);
            scCSV.Append("FieldTypeKind" + comma);
            scCSV.Append("TypeAsString" + comma);
            scCSV.Append("TypeDisplayName" + comma);
            scCSV.Append("TypeShortDescription" + comma);
            scCSV.Append("ValidationFormula" + comma);
            scCSV.Append("ValidationMessage" + Environment.NewLine); 

            return scCSV;
        }

        private void calculateOutputFileNames(string outputDirectory)
        {
            string siteName = _siteUrl.Split('/').Last();
            //assume dir exists
            _outputFileCTH = Path.Combine(outputDirectory, string.Format("CTH_{0}_{1}.csv", siteName, DateTime.Now.ToString("yyyy-MM-dd")));
            _outputFileSiteCols = Path.Combine(outputDirectory, string.Format("SiteCols_{0}_{1}.csv", siteName, DateTime.Now.ToString("yyyy-MM-dd")));
        }

        private void SaveCTHXMLToCsv(XDocument ctDoc)
        {
            
            StringBuilder csvLine = new StringBuilder();
            StringBuilder csv = new StringBuilder();
            var headers = ctDoc.Root.Elements().First().Attributes().Select(n => n.Name).ToList();
            foreach(var header in headers)
            {
                csvLine.Append(header + comma);
            }

            RemoveLastComma(ref csvLine);
            csv.AppendLine(csvLine.ToString());
            string attr = String.Empty;
            foreach(XElement e in ctDoc.Root.Descendants("Field"))
            {
                csvLine = new StringBuilder();
                foreach(var header in headers)
                {
                    try
                    {
                        if (e.Attribute(header.ToString()) != null)
                        {
                            attr = e.Attributes(header.ToString()).First().Value;
                        }
                        else
                        {
                            attr = String.Empty;
                        }
                    }
                    catch
                    {
                        attr = String.Empty;
                    }


                    csvLine.Append(attr + comma);
                }
                RemoveLastComma(ref csvLine);
                csv.AppendLine(csvLine.ToString());
                
            }
            WriteFile(csv.ToString(), _outputFileCTH);

        }
        private void WriteFile(string outputString, string fileSaveLocation)
        {
            using (StreamWriter sw = new StreamWriter(fileSaveLocation, false, Encoding.UTF8))
            {

                sw.Write(outputString);
            }

        }

        private void RemoveLastComma(ref StringBuilder csvLine)
        {
            csvLine.Remove(csvLine.Length - 1, 1);
        }


        private XDocument ReadCTHXML()
        {
            using (ClientContext clientContext = new ClientContext(_siteUrl))
            {
                var cTypes = clientContext.Web.ContentTypes;
                clientContext.Load(cTypes);

                clientContext.ExecuteQuery();
                XDocument allCTs = new XDocument(new XElement("ContentTypes"));

                foreach (ContentType ct in cTypes)
                {
                    XDocument ctDoc = XDocument.Parse(ct.SchemaXml, LoadOptions.None);
                    allCTs.Root.Add(ctDoc.Root);
                }
                return allCTs;
            }
        }
        private XDocument RemoveUnwantedAttributes(XDocument xd)
        {

            #region unwantedAttributes


            string[] attributesToRemove = new string[] { "PIAttribute", 
                                                            "RenderXMLUsingPattern",
                                                            "SourceID",
                                                            "UnlimitedLengthInDocumentLibrary",
                                                            "AppendOnly",
                                                            "Indexed",
                                                            "IsolateStyles",
                                                            "EnforceUniqueValues",
                                                            "NumLines",
                                                            "RestrictedMode",
                                                            "RichText",
                                                            "RichTextMode",
                                                            "Sortable",
                                                            "Sealed",
                                                            "PITarget",
                                                            "Customization",
                                                            "Percentage",
                                                            "PrimaryPITarget",
                                                            "PrimaryPIAttribute",
                                                            "Aggregation",
                                                            "Node",
                                                            "AllowDeletion",
                                                            "FromBaseType",
                                                            "ShowInNewForm",
                                                            "ShowInEditForm",
                                                            "List",
                                                            "ShowField",
                                                            "Mult",
                                                            "MaxLength",
                                                            "DisplaceOnUpgrade",
                                                            "UserSelectionMode",
                                                            "UserSelectionScope",
                                                            "ReadOnlyEnforced",
                                                            "Format",
                                                            "DisplayNameSrcField",
                                                            "ClassInfo",
                                                            "AuthoringInfo",
                                                            "FillInChoice",
                                                            "Min",
                                                            "WebId",
                                                            "ShowInViewForms",
                                                            "CanToggleHidden",
                                                            "FieldRef",
                                                            "ResultType",
                                                            "ShowInDisplayForm",
                                                            "Filterable",
                                                            "HeaderImage",
                                                            "ShowInFileDlg",
                                                            "ShowInVersionHistory",
                                                            "JoinColName",
                                                            "JoinRowOrdinal",
                                                            "JoinType",
                                                            "ColName",
                                                            "RowOrdinal",
                                                            "StorageTZ",
                                                            "ShowInListSettings",
                                                            "FriendlyDisplayFormat",
                                                            "Decimals",
                                                            "Max",
                                                            "JSLink",
                                                            "DefaultListField",
                                                            "ForcePromoteDemote",
                                                            "XName",
                                                            "DisplaySize",
                                                            "WikiLinking",
                                                            "DisplayImage",
                                                            "ExceptionImage",
                                                            "NoEditFormBreak",
                                                            "CalType",
                                                            "PrependId",
                                                            "Dir",
                                                            "IMEMode",
                                                            "Width",
                                                            "Height",
                                                            "ListItemMenuAllowed",
                                                            "LinkToItemAllowed",
                                                            "Title"
                                                        };

            #endregion

            XDocument outputDoc = new XDocument();
            outputDoc.Add(new XElement("Fields"));

            foreach (XElement xe in xd.Descendants("Field"))
            {
                var unwantedAtts = from attributes in xe.Attributes()
                              join removes in attributesToRemove on attributes.Name equals removes
                              select attributes;
                // unwantedAtts.Remove();

                try
                {
                    XAttribute attrChoices = GenerateChoicesAttribute(xe);
                    XAttribute attrFieldRefs = GenerateFieldRefsAttribute(xe);
                    XAttribute attrDisplayPattern = GenerateFlattenedAttribute(xe, "DisplayPattern");
                    XAttribute attrXmlDocuments = GenerateFlattenedAttribute(xe, "XmlDocuments");
                    XAttribute attrDefault = GenerateFlattenedAttribute(xe, "Default");
                    XAttribute attrMAPPINGS = GenerateFlattenedAttribute(xe, "MAPPINGS");

                    

                    XAttribute attrCTID = new XAttribute("CTID", xe.Parent.Parent.Attribute("ID").Value);
                    XAttribute attrCTName = new XAttribute("CTName", xe.Parent.Parent.Attribute("Name").Value);
                    XAttribute attrCTGroup = new XAttribute("CTGroup", xe.Parent.Parent.Attribute("Group").Value);

                    xe.Add(new XAttribute(attrChoices));
                    xe.Add(new XAttribute(attrFieldRefs));
                    xe.Add(new XAttribute(attrDisplayPattern));
                    xe.Add(new XAttribute(attrXmlDocuments));
                    xe.Add(new XAttribute(attrDefault));
                    xe.Add(new XAttribute(attrMAPPINGS));
                    xe.Add(new XAttribute(attrCTID));
                    xe.Add(new XAttribute(attrCTName));
                    xe.Add(new XAttribute(attrCTGroup));

                    outputDoc.Root.Add(xe);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("error in RemoveUnwantedAttributes\t{0}\t{1}", ex.Message, ex.InnerException));
                }
            }
            return outputDoc;
        }

        private XAttribute GenerateFlattenedAttribute(XElement xe, string nodeName)
        {
            string displayPattern = String.Empty;
            foreach (XElement attrItem in xe.Descendants(nodeName))
            {
                displayPattern += attrItem.ToString().Replace("<" + nodeName + ">", "").Replace("</" + nodeName + ">", "").Replace(Environment.NewLine, "").Replace(",","");
            }
            //RemoveLastComma(ref choicesList);
            XAttribute attrOut = new XAttribute(nodeName, displayPattern);

            xe.Descendants(nodeName).Remove();

            return attrOut;
        }

        private XAttribute GenerateFieldRefsAttribute(XElement xe)
        {
            string fieldRefsList = String.Empty;
            foreach (XElement attrItem in xe.Descendants("FieldRef"))
            {

                fieldRefsList += getSafeAttributeValue(attrItem, "Name") + " " + getSafeAttributeValue(attrItem, "ID") + ";";
            }
            XAttribute attrOut = new XAttribute("FieldRefs", fieldRefsList);

            xe.Descendants("FieldRefs").Remove();

            return attrOut;
        }

        private string getSafeAttributeValue(XElement attrItem, string attributeName)
        {
            if (attrItem.Attribute(attributeName) != null)
            {
                return attrItem.Attribute(attributeName).Value;
            }
            return String.Empty;
        }

        private XAttribute GenerateChoicesAttribute(XElement xe)
        {
            string choicesList = String.Empty;
            foreach (XElement attrItem in xe.Descendants("CHOICE"))
            {
                choicesList += attrItem.Value + ";";
            }
            
            XAttribute attrOut = new XAttribute("Choices", choicesList);
            
            xe.Descendants("CHOICES").Remove();

            return attrOut;
        }
    }
}
