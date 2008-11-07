﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace De.AHoerstemeier.Tambon
{
    abstract class CreationStatistics : AnnouncementStatistics
    {
        #region properties
        private FrequencyCounter mCreationsPerAnnouncement = new FrequencyCounter();
        protected FrequencyCounter CreationsPerAnnouncement { get { return mCreationsPerAnnouncement; } }
        private Int32 mNumberOfCreations;
        public Int32 NumberOfCreations { get { return mNumberOfCreations; } }
        protected Int32[] mNumberOfCreationsPerChangwat = new Int32[100];
        #endregion
        #region methods
        protected override void Clear()
        {
            base.Clear();
            mCreationsPerAnnouncement = new FrequencyCounter();
            mNumberOfCreationsPerChangwat = new Int32[100];
            mNumberOfCreations = 0;
        }
        protected abstract Boolean EntityFitting(EntityType iEntityType);
        protected Boolean ContentFitting(RoyalGazetteContent iContent)
        {
            Boolean retval = false;
            if (iContent is RoyalGazetteContentCreate)
            {
                RoyalGazetteContentCreate lCreate = (RoyalGazetteContentCreate)iContent;
                retval = EntityFitting(lCreate.Status);
            }
            return retval;
        }
        protected virtual void ProcessContent(RoyalGazetteContent iContent)
        {
            RoyalGazetteContentCreate lCreate = (RoyalGazetteContentCreate)iContent;
            mNumberOfCreations++;

            Int32 lChangwatGeocode = lCreate.Geocode;
            while (lChangwatGeocode > 100)
            {
                lChangwatGeocode = lChangwatGeocode / 100;
            }
            mNumberOfCreationsPerChangwat[lChangwatGeocode]++;
        }
        protected override void ProcessAnnouncement(RoyalGazette iEntry)
        {
            Int32 lCount = 0;
            Int32 lProvinceGeocode = 0;
            foreach (RoyalGazetteContent lContent in iEntry.Content)
            {
                if (ContentFitting(lContent))
                {
                    lCount++;
                    ProcessContent(lContent);
                    lProvinceGeocode = lContent.Geocode;
                    while (lProvinceGeocode / 100 != 0)
                    {
                        lProvinceGeocode = lProvinceGeocode / 100;
                    }

                }
            }
            if (lCount > 0)
            {
                mNumberOfAnnouncements++;
                mCreationsPerAnnouncement.IncrementForCount(lCount, lProvinceGeocode);
            }
        }
        #endregion
    }
}
