﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Xml;

namespace De.AHoerstemeier.Geo
{
    public enum PositionInRectangle
    {
        TopLeft,
        TopMiddle,
        TopRight,
        BottomLeft,
        BottomMiddle,
        BottomRight,
        MiddleLeft,
        MiddleRight,
        MiddleMiddle,
    }

    internal class Helper
    {
        static public CultureInfo CultureInfoUS = new CultureInfo("en-us");
        internal static XmlDocument XmlDocumentFromNode(XmlNode iNode)
        {
            XmlDocument retval = null;

            if ( iNode is XmlDocument )
            {
                retval = (XmlDocument)iNode;
            }
            else
            {
                retval = iNode.OwnerDocument;
            }

            return retval;
        }
    }
}
