using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using System.Globalization;
using System.Windows.Forms;

namespace De.AHoerstemeier.Tambon
{
    public class TambonHelper
    {

        #region constants
        private const string PHOSO = "พ.ศ.";
        #endregion
        static public RoyalGazetteList GlobalGazetteList = new RoyalGazetteList();
        static public List<PopulationDataEntry> Geocodes = new List<PopulationDataEntry>();
        static public Encoding ThaiEncoding = Encoding.GetEncoding(874);
        static public CultureInfo CultureInfoUS = new CultureInfo("en-us");
        public static Dictionary<EntityLeaderType, String> EntityLeaderName = new Dictionary<EntityLeaderType, String>()
        {
            {EntityLeaderType.Governor, "ผู้ว่าราชการจังหวัด"},
            {EntityLeaderType.ViceGovernor, "รองผู้ว่าราชการจังหวัด"},
            {EntityLeaderType.DistrictOfficer,"นายอำเภอ"},
            {EntityLeaderType.MinorDistrictOfficer,"หัวหน้ากิ่งอำเภอ"},
            {EntityLeaderType.Kamnan,"กำนัน"},
            {EntityLeaderType.PhuYaiBan,"ผู้ใหญ่บ้าน"},
            {EntityLeaderType.Mayor,"นายกเทศมนตรี"},
            {EntityLeaderType.TAOChairman,"นายกองค์การบริหารส่วนตำบล"},
            {EntityLeaderType.PAOChairman,"นายกองค์การบริหารส่วนจังหวัด"},
            {EntityLeaderType.ChumchonChairman,"ประธานชุมชน"}
        };
        public static Dictionary<GazetteSignPosition, String> GazetteSignPositionThai = new Dictionary<GazetteSignPosition, string>()
        {
            {GazetteSignPosition.PrimeMinister, "นายกรัฐมนตรี" },
            {GazetteSignPosition.MinisterOfInterior, "รัฐมนตรีว่าการกระทรวงมหาดไทย" },
            {GazetteSignPosition.DeputyMinisterOfInterior, "รัฐมนตรีช่วยว่าการกระทรวงมหาดไทย" },
            {GazetteSignPosition.MinistryOfInteriorPermanentSecretary, "ปลัดกระทรวงมหาดไทย" },
            {GazetteSignPosition.ProvinceGovernor, "ผู้ว่าราชการจังหวัด" },
            // {GazetteSignPosition.BangkokGovernor, ""},
            {GazetteSignPosition.BangkokPermanentSecretary, "ปลัดกรุงเทพ" },
            {GazetteSignPosition.DeputyBangkokPermanentSecretary, "รองปลัดกรุงเทพ" },
            {GazetteSignPosition.MinisterOfInformationAndCommunicationTechnology, "รัฐมนตรีว่าการกระทรวงเทคโนโลยีสารสนเทศและการสื่อสาร" },
            {GazetteSignPosition.ElectionCommissionPresident, "ประธานกรรมการการเลือกตั้ง" },
            {GazetteSignPosition.RoyalInstitutePresident, "นายกราชบัณฑิตยสถาน" } ,
            {GazetteSignPosition.RoyalInstituteActingPresident, "รักษาการตำแหน่งนายกราชบัณฑิตยสถาน" }
        };

        public static Dictionary<String, String> ChangwatMisspellings = new Dictionary<String, String>()
        {
            {"สุราษฏร์ธานี","สุราษฎร์ธานี"}
        };
        public static Dictionary<String, PersonTitle> PersonTitleStrings = new Dictionary<String, PersonTitle>()
        {
            {"นาย",PersonTitle.Mister},
            {"นาง",PersonTitle.Mistress},
            {"น.ส.",PersonTitle.Miss},
            {"พล.อ.",PersonTitle.General},
            {"พล.ท.",PersonTitle.LieutenantGeneral},
            {"พล.ต.",PersonTitle.MajorGeneral},
            {"พลตรี",PersonTitle.MajorGeneral},
            {"พ.อ.",PersonTitle.Colonel},
            {"พ.ท.",PersonTitle.LieutenantColonel},
            {"พ.ต.",PersonTitle.Major},
            {"ร.อ.",PersonTitle.Captain},
            {"ร้อยเอก",PersonTitle.Captain},
            {"ร.ท.",PersonTitle.FirstLieutenant},
            {"ร.ต.",PersonTitle.SecondLieutenant},
            {"ร้อยตรี",PersonTitle.SecondLieutenant},
            {"ว่าที่ ร.ต.",PersonTitle.ActingSecondLieutenant},
            {"ว่าที่ร.ต.",PersonTitle.ActingSecondLieutenant},
            {"ว่าที่ร้อยตรี",PersonTitle.ActingSecondLieutenant},
            {"เรือตรี",PersonTitle.SubLieutenant}

            // พล.ต.อ. Police General
            // พล.ต.ท. Police Lieutenant General
            // พล.ต.ต. = พลตำรวจตรี Police Major General
            // พ.ต.อ. Police Colonel
            // พ.ต.ท. Police Lieutenant Colonel
            // พ.ต.ต. Police Major
            // ร.ต.อ. Police Captain
            // ร.ต.ท. Police Lieutenant
            // ร.ต.ต. Police Sub-Lieutenant
            // นายกองเอก ?
            // อำมาตย์โท
            // อำมาตย์เอก
            // เรืออากาศตรี
        };
        internal static Dictionary<Char, Byte> ThaiNumerals = new Dictionary<char, byte> 
        {
            {'๐',0}, 
            {'๑',1}, 
            {'๒',2}, 
            {'๓',3}, 
            {'๔',4},
            {'๕',5}, 
            {'๖',6},
            {'๗',7},
            {'๘',8},
            {'๙',9}
        };
        public static Dictionary<String, Byte> ThaiMonthNames = new Dictionary<string, byte>
        {
            {"มกราคม",1},
            {"กุมภาพันธ์",2},
            {"มีนาคม",3},
            {"เมษายน",4},
            {"พฤษภาคม",5},
            {"มิถุนายน",6},
            {"กรกฎาคม",7},
            {"สิงหาคม",8},
            {"กันยายน",9},
            {"ตุลาคม",10},
            {"พฤศจิกายน",11},
            {"ธันวาคม",12}
        };
        public static Dictionary<String, Byte> ThaiMonthAbbreviations = new Dictionary<string, byte>
        {
            {"ม.ค.",1},
            {"ก.พ.",2},
            {"มี.ค.",3},
            {"เม.ย.",4},
            {"พ.ค.",5},
            {"มิ.ย.",6},
            {"ก.ค.",7},
            {"สิ.ค.",8},
            {"ส.ค.",8},
            {"ก.ย.",9},
            {"ต.ค.",10},
            {"พ.ย.",11},
            {"ธ.ค.",12}
        };

        // XML utilities
        public static XmlDocument XmlDocumentFromNode(XmlNode iNode)
        {
            XmlDocument retval = null;

            if (iNode is XmlDocument)
            {
                retval = (XmlDocument)iNode;
            }
            else
            {
                retval = iNode.OwnerDocument;
            }

            return retval;
        }
        public static Boolean HasAttribute(XmlNode iNode, String iAttributeName)
        {
            Boolean retval = false;
            if (iNode != null && iNode.Attributes != null)
            {
                foreach (XmlAttribute i in iNode.Attributes)
                {
                    retval = retval | (i.Name == iAttributeName);
                }
            }
            return retval;
        }
        public static String GetAttribute(XmlNode iNode, String iAttributeName)
        {
            String RetVal = String.Empty;
            if (iNode != null && iNode.Attributes != null && (iNode.Attributes.Count > 0) && !String.IsNullOrEmpty(iAttributeName))
            {
                RetVal = iNode.Attributes.GetNamedItem(iAttributeName).Value;
            }
            return RetVal;
        }
        public static String GetAttributeOptionalString(XmlNode iNode, String iAttributeName)
        {
            String RetVal = String.Empty;
            if (HasAttribute(iNode, iAttributeName))
            {
                RetVal = iNode.Attributes.GetNamedItem(iAttributeName).Value;
            }
            return RetVal;
        }
        public static Int32 GetAttributeOptionalInt(XmlNode iNode, String iAttributeName, Int32 iReplace)
        {
            Int32 RetVal = iReplace;
            if (HasAttribute(iNode, iAttributeName))
            {
                string s = iNode.Attributes.GetNamedItem(iAttributeName).Value;
                if (!String.IsNullOrEmpty(s))
                {
                    try
                    {
                        RetVal = Convert.ToInt32(s);
                    }
                    catch
                    {
                    }
                }
            }
            return RetVal;
        }
        public static Boolean GetAttributeOptionalBool(XmlNode iNode, String iAttributeName, Boolean iReplace)
        {
            String lValue = GetAttributeOptionalString(iNode, iAttributeName);
            switch (lValue)
            {
                case "": return iReplace;
                case "0":
                case "false": return false;
                case "1":
                case "true": return true;
                default: throw new ArgumentOutOfRangeException("Invalid boolean value "+lValue);
            }
        }

        public static DateTime GetAttributeOptionalDateTime(XmlNode iNode, String iAttributeName)
        {
            DateTime RetVal = new DateTime();
            if (HasAttribute(iNode, iAttributeName))
            {
                RetVal = Convert.ToDateTime(iNode.Attributes.GetNamedItem(iAttributeName).Value);
            }
            return RetVal;
        }
        internal static DateTime GetAttributeDateTime(XmlNode iNode, String iAttributeName)
        {
            DateTime RetVal = Convert.ToDateTime(iNode.Attributes.GetNamedItem(iAttributeName).Value);
            return RetVal;
        }

        public static void StreamCopy(Stream iInput, Stream ioOutput)
        {
            byte[] lBuffer = new byte[2048];
            int lRead = 0;

            do
            {
                lRead = iInput.Read(lBuffer, 0, lBuffer.Length);
                ioOutput.Write(lBuffer, 0, lRead);
            } while (lRead > 0);
        }
        public static bool IsNumeric(String iValue)
        {
            for (int i = 0; i < iValue.Length; i++)
            {
                if (!(Convert.ToInt32(iValue[i]) >= 48 && Convert.ToInt32(iValue[i]) <= 57))
                {
                    return false;
                }
            }
            return !String.IsNullOrEmpty(iValue);
        }

        public static string ReplaceThaiNumerals(string iValue)
        {
            string RetVal = String.Empty;

            if (!String.IsNullOrEmpty(iValue))
            {
                foreach (char c in iValue)
                {
                    if (ThaiNumerals.ContainsKey(c))
                    {
                        RetVal = RetVal + ThaiNumerals[c].ToString();
                    }
                    else
                    {
                        RetVal = RetVal + c;
                    }
                }
            }
            return RetVal;
        }
        internal static string UseThaiNumerals(string iValue)
        {
            string RetVal = String.Empty;

            if (!String.IsNullOrEmpty(iValue))
            {
                foreach (Char c in iValue)
                {
                    if ((c >= '0') | (c <= '9'))
                    {
                        Int32 lValue = Convert.ToInt32(c) - Convert.ToInt32('0');
                        foreach (KeyValuePair<Char, Byte> lKeyValuePair in ThaiNumerals)
                        {
                            if (lKeyValuePair.Value == lValue)
                            {
                                RetVal = RetVal + lKeyValuePair.Key;
                            }
                        }
                    }
                    else
                    {
                        RetVal = RetVal + c;
                    }
                }
            }
            return RetVal;
        }

        internal static DateTime ParseThaiDate(string iValue)
        {
            String lMonthString = String.Empty;
            Int32 lMonth = 0;
            String lYearString = String.Empty;
            Int32 lYear = 0;
            Int32 lDay = 0;
            Int32 lPosition = 0;

            String lDate = ReplaceThaiNumerals(iValue);

            lPosition = lDate.IndexOf(' ');
            lDay = Convert.ToInt32(lDate.Substring(0, lPosition));
            lDate = lDate.Substring(lPosition + 1, lDate.Length - lPosition - 1);
            lPosition = lDate.IndexOf(' ');
            lMonthString = lDate.Substring(0, lPosition).Trim();
            lMonth = ThaiMonthNames[lMonthString];
            // TODO: Kamen da nicht auch welche mit KhoSo vor?
            lPosition = lDate.IndexOf(PHOSO) + PHOSO.Length;
            lYearString = lDate.Substring(lPosition, lDate.Length - lPosition);
            lYear = Convert.ToInt32(lYearString);
            if (lYear < 2100)
            {
                lYear = lYear + 543;  // there are entries in KhoSo but with "พ.ศ." in the returned info
            }

            if ((lYear < 2484) & (lMonth < 4))
            {
                lYear = lYear - 542;
            }
            else
            {
                lYear = lYear - 543;
            }
            return new DateTime(lYear, lMonth, lDay); ;
        }

        internal static Boolean IsSameGeocode(Int32 iGeocodeToFind, Int32 iGeocodeToCheck, Boolean iIncludeSubEntities)
        {
            Boolean retval = false;
            if (iIncludeSubEntities)
            {
                Int32 lGeocode = iGeocodeToCheck;
                while (lGeocode != 0)
                {
                    retval = (retval | (iGeocodeToFind == lGeocode));
                    lGeocode = lGeocode / 100;
                }
            }
            else
            {
                retval = (iGeocodeToFind == iGeocodeToCheck);
            }
            return retval;
        }
        internal static Boolean IsBaseGeocode(Int32 iBaseGeocode, Int32 iGeocodeToCheck)
        {
            Boolean retval = false;
            if (iBaseGeocode == 0)
            {
                retval = true;
            }
            else if (iGeocodeToCheck == 0)
            {
                retval = false;
            }
            else
            {
                Int32 lLevel = 1;
                while (iBaseGeocode < 1000000)
                {
                    iBaseGeocode = iBaseGeocode * 100;
                    lLevel = lLevel * 100;
                }
                while (iGeocodeToCheck < 1000000)
                {
                    iGeocodeToCheck = iGeocodeToCheck * 100;
                }
                Int32 lTest = iGeocodeToCheck - iBaseGeocode;
                
                retval = (!(lTest<0)) & (lTest < lLevel);
            }
            return retval;
        }

        internal static Int64 GetDateJavaScript(DateTime iValue)
        {
            // milliseconds since January 1, 1970
            TimeSpan lDifference = iValue.ToUniversalTime() - new DateTime(1970, 1, 1);
            Int64 retval = Convert.ToInt64(lDifference.TotalMilliseconds);
            return retval;
        }

        internal static String OnlyNumbers(String iValue)
        {
            String retval = String.Empty;
            foreach (Char c in iValue)
            {
                if ((c >= '0') && (c <= '9'))
                {
                    retval = retval + c;
                }
            }
            return retval;
        }
        internal static Int32 GetGeocode(String iChangwat)
        {
            XElement lGeocodeXML = XElement.Load(BasicGeocodeFileName());
            Int32 lProvinceID = 0;
            var lQuery = from c in lGeocodeXML.Descendants("entity")
                         where (String)c.Attribute("name") == iChangwat
                         select (Int32)c.Attribute("geocode");

            foreach (Int32 lEntry in lQuery)
            {
                lProvinceID = lEntry;
            }
            return lProvinceID;
        }
        internal static Int32 GetGeocode(String iChangwat, String iName, EntityType iType)
        {
            Int32 lProvinceID = GetGeocode(iChangwat);
            Int32 lGeocode = 0;
            if (lProvinceID != 0)
            {
                String lSearchName = iName;
                if (lSearchName.Contains(" "))
                {
                    lSearchName = lSearchName.Substring(0, lSearchName.IndexOf(" "));
                }
                XElement lChangwatXML = XElement.Load(GeocodeSourceFile(lProvinceID));
                var lAmphoeQuery = from c in lChangwatXML.Descendants("entity")
                                   where (
                                     ((String)c.Attribute("name") == lSearchName) &&
                                     EntityTypeHelper.IsCompatibleEntityType(
                                       (EntityType)Enum.Parse(typeof(EntityType), (String)c.Attribute("type"))
                                       , iType))
                                   select (Int32)c.Attribute("geocode");

                foreach (Int32 lEntry in lAmphoeQuery)
                {
                    lGeocode = lEntry;
                }
            }
            return lGeocode;
        }
        internal static String GeocodeXmlSourceDir()
        {
            String retval = Path.GetDirectoryName(Application.ExecutablePath) + "\\geocode\\";
            return retval;
        }
        static public String GeocodeSourceFile(Int32 iGeocode)
        {
            String lFilename = GeocodeXmlSourceDir() + "geocode" + iGeocode.ToString("D2") + ".XML";
            return lFilename;
        }
        static public String BasicGeocodeFileName()
        {
            String lFilename = Path.GetDirectoryName(Application.ExecutablePath) + "\\geocode.xml";
            return lFilename;
        }
        static public void LoadGeocodeList()
        {
            XElement lGeocodeXML = XElement.Load(BasicGeocodeFileName());

            var lQuery = from c in lGeocodeXML.Descendants("entity")
                         orderby (string)c.Attribute("english")
                         select new PopulationDataEntry
                         {
                             Name = (string)c.Attribute("name"),
                             English = (string)c.Attribute("english"),
                             Type = (EntityType)Enum.Parse(typeof(EntityType), (string)c.Attribute("type")),
                             Geocode = (Int32)c.Attribute("geocode")
                         };

            Geocodes.Clear();
            Geocodes.AddRange(lQuery);
        }

        public static Boolean IsSameMubanName(String iName1, String iName2)
        {
            Boolean RetVal = (StripBan(iName1)==StripBan(iName2));
            return RetVal;
        }
        public static String StripBan(String iName)
        {
            const String ThaiStringBan = "บ้าน";
            String retval = iName;
            if (iName.StartsWith(ThaiStringBan))
            {
                retval = iName.Remove(0, ThaiStringBan.Length).Trim();
            }
            return retval;
        }
    }
}
