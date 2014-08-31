﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Wikibase;

namespace De.AHoerstemeier.Tambon.UI
{
    public partial class EntityBrowserForm : Form
    {
        #region fields

        private const Boolean _showDolaErrors = false;
        private List<Entity> localGovernments = new List<Entity>();
        private Entity baseEntity;
        private Dictionary<EntityType, String> _deWikipediaLink;

        #endregion fields

        #region properties

        public UInt32 StartChangwatGeocode
        {
            get;
            set;
        }

        public PopulationDataSourceType PopulationDataSource
        {
            get;
            set;
        }

        public Int16 PopulationReferenceYear
        {
            get;
            set;
        }

        public Boolean CheckWikiData
        {
            get;
            set;
        }

        #endregion properties

        #region constructor

        public EntityBrowserForm()
        {
            InitializeComponent();
            PopulationDataSource = PopulationDataSourceType.DOPA;
            PopulationReferenceYear = 2013;
            _deWikipediaLink = new Dictionary<EntityType, String>()
            {
                {EntityType.ThesabanNakhon, "[[Thesaban#Großstadt|Thesaban Nakhon]]"},
                {EntityType.ThesabanMueang, "[[Thesaban#Stadt|Thesaban Mueang]]"},
                {EntityType.ThesabanTambon, "[[Thesaban#Kleinstadt|Thesaban Tambon]]"},
                {EntityType.TAO, "[[Verwaltungsgliederung Thailands#Tambon-Verwaltungsorganisationen|Tambon-Verwaltungsorganisationen]]"},
            };
        }

        #endregion constructor

        #region private methods

        private void EntityBrowserForm_Load(object sender, EventArgs e)
        {
            baseEntity = GlobalData.CompleteGeocodeList();
            baseEntity.PropagatePostcodeRecursive();
            var allEntities = baseEntity.FlatList().Where(x => !x.IsObsolete).ToList();
            var allLocalGovernmentParents = allEntities.Where(x => x.type == EntityType.Tambon || x.type == EntityType.Changwat).ToList();
            foreach ( var tambon in allLocalGovernmentParents )
            {
                var localGovernmentEntity = tambon.CreateLocalGovernmentDummyEntity();
                if ( localGovernmentEntity != null )
                {
                    localGovernments.Add(localGovernmentEntity);
                }
            }
            foreach ( var item in allEntities.Where(x => x.type.IsLocalGovernment()) )
            {
                localGovernments.Add(item);
            }

            GlobalData.LoadPopulationData(PopulationDataSource, PopulationReferenceYear);
            PopulationDataToTreeView();
        }

        private TreeNode EntityToTreeNode(Entity data)
        {
            TreeNode retval = null;
            if ( data != null )
            {
                retval = new TreeNode(data.english);
                retval.Tag = data;
                if ( !data.type.IsThirdLevelAdministrativeUnit() )  // No Muban in Treeview
                {
                    foreach ( Entity entity in data.entity )
                    {
                        if ( !entity.IsObsolete && !entity.type.IsLocalGovernment() )
                        {
                            retval.Nodes.Add(EntityToTreeNode(entity));
                        }
                    }
                }
            }
            return retval;
        }

        private void PopulationDataToTreeView()
        {
            treeviewSelection.BeginUpdate();
            treeviewSelection.Nodes.Clear();

            TreeNode baseNode = EntityToTreeNode(baseEntity);
            treeviewSelection.Nodes.Add(baseNode);
            baseNode.Expand();
            foreach ( TreeNode node in baseNode.Nodes )
            {
                if ( ((Entity)(node.Tag)).geocode == StartChangwatGeocode )
                {
                    treeviewSelection.SelectedNode = node;
                    node.Expand();
                }
            }
            treeviewSelection.EndUpdate();
        }

        private void treeviewSelection_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var selectedNode = treeviewSelection.SelectedNode;
            var entity = (Entity)(selectedNode.Tag);
            EntityToCentralAdministrativeListView(entity);
            EntityToLocalAdministrativeListView(entity);
            SetInfo(entity);
            CheckForErrors(entity);
            CalcElectionData(entity);
            CalcMubanData(entity);

            mnuMubanDefinitions.Enabled = AreaDefinitionAnnouncements(entity).Any();
        }

        private void CalcElectionData(Entity entity)
        {
            var localGovernmentsInEntity = LocalGovernmentEntitiesOf(entity);
            var dummyEntity = new Entity();
            dummyEntity.entity.AddRange(localGovernmentsInEntity);

            var itemsWithCouncilElectionsPending = new List<EntityTermEnd>();
            var itemsWithOfficialElectionsPending = new List<EntityTermEnd>();
            var itemsWithOfficialElectionResultUnknown = new List<EntityTermEnd>();

            var itemsWithCouncilElectionPendingInParent = dummyEntity.EntitiesWithCouncilElectionPending();
            itemsWithCouncilElectionsPending.AddRange(itemsWithCouncilElectionPendingInParent);
            itemsWithCouncilElectionsPending.Sort((x, y) => x.CouncilTerm.begin.CompareTo(y.CouncilTerm.begin));

            var itemsWithOfficialElectionPendingInParent = dummyEntity.EntitiesWithOfficialElectionPending();
            itemsWithOfficialElectionsPending.AddRange(itemsWithOfficialElectionPendingInParent);
            itemsWithOfficialElectionsPending.Sort((x, y) => x.OfficialTerm.begin.CompareTo(y.OfficialTerm.begin));

            var itemsWithOfficialElectionResultUnknownInParent = dummyEntity.EntitiesWithLatestOfficialElectionResultUnknown();
            itemsWithOfficialElectionResultUnknown.AddRange(itemsWithOfficialElectionResultUnknownInParent);
            itemsWithOfficialElectionResultUnknown.Sort((x, y) => x.OfficialTerm.begin.CompareTo(y.OfficialTerm.begin));

            var result = String.Empty;
            var councilBuilder = new StringBuilder();
            Int32 councilCount = 0;
            foreach ( var item in itemsWithCouncilElectionsPending )
            {
                DateTime end;
                if ( item.CouncilTerm.endSpecified )
                {
                    end = item.CouncilTerm.end;
                }
                else
                {
                    end = item.CouncilTerm.begin.AddYears(4).AddDays(-1);
                }
                councilBuilder.AppendFormat(CultureInfo.CurrentUICulture, "{0} ({1}): {2:d}", item.Entity.english, item.Entity.geocode, end);
                councilBuilder.AppendLine();
                councilCount++;
            }
            if ( councilCount > 0 )
            {
                result +=
                    String.Format("{0} LAO council elections pending", councilCount) + Environment.NewLine +
                    councilBuilder.ToString() + Environment.NewLine;
            }

            var officialBuilder = new StringBuilder();
            Int32 officialCount = 0;
            foreach ( var item in itemsWithOfficialElectionsPending )
            {
                String officialTermEnd = "unknown";
                if ( (item.OfficialTerm.begin != null) && (item.OfficialTerm.begin.Year > 1900) )
                {
                    DateTime end;
                    if ( item.OfficialTerm.endSpecified )
                    {
                        end = item.OfficialTerm.end;
                    }
                    else
                    {
                        end = item.OfficialTerm.begin.AddYears(4).AddDays(-1);
                    }
                    officialTermEnd = String.Format(CultureInfo.CurrentUICulture, "{0:d}", end);
                }
                officialBuilder.AppendFormat(CultureInfo.CurrentUICulture, "{0} ({1}): {2}", item.Entity.english, item.Entity.geocode, officialTermEnd);
                officialBuilder.AppendLine();
                officialCount++;
            }
            if ( officialCount > 0 )
            {
                result +=
                    String.Format("{0} LAO official elections pending", officialCount) + Environment.NewLine +
                    officialBuilder.ToString() + Environment.NewLine;
            }

            var officialUnknownBuilder = new StringBuilder();
            Int32 officialUnknownCount = 0;
            foreach ( var item in itemsWithOfficialElectionResultUnknown )
            {
                if ( (item.OfficialTerm.begin != null) && (item.OfficialTerm.begin.Year > 1900) )  // must be always true
                {
                    officialUnknownBuilder.AppendFormat(CultureInfo.CurrentUICulture, "{0} ({1}): {2:d}", item.Entity.english, item.Entity.geocode, item.OfficialTerm.begin);
                    officialUnknownBuilder.AppendLine();
                    officialUnknownCount++;
                }
            }
            if ( officialUnknownCount > 0 )
            {
                result +=
                    String.Format("{0} LAO official elections result missing", officialUnknownCount) + Environment.NewLine +
                    officialUnknownBuilder.ToString() + Environment.NewLine;
            }
            txtElections.Text = result;
        }

        private void CheckForErrors(Entity entity)
        {
            var text = String.Empty;
            var wrongGeocodes = entity.WrongGeocodes();
            if ( wrongGeocodes.Any() )
            {
                text += "Wrong geocodes:" + Environment.NewLine;
                foreach ( var code in wrongGeocodes )
                {
                    text += String.Format(" {0}", code) + Environment.NewLine;
                }
                text += Environment.NewLine;
            }
            var localGovernmentsInEntity = LocalGovernmentEntitiesOf(entity).ToList();
            var localEntitiesWithOffice = localGovernmentsInEntity.Where(x => x.Dola != null).ToList();  // Dola != null when there is a local government office
            if ( _showDolaErrors )
            {
                var entitiesWithDolaCode = localEntitiesWithOffice.Where(x => x.Dola.codeSpecified).ToList();
                var allDolaCodes = entitiesWithDolaCode.Select(x => x.Dola.code).ToList();
                var duplicateDolaCodes = allDolaCodes.GroupBy(s => s).SelectMany(grp => grp.Skip(1)).ToList();
                if ( duplicateDolaCodes.Any() )
                {
                    text += "Duplicate DOLA codes:" + Environment.NewLine;
                    foreach ( var code in duplicateDolaCodes )
                    {
                        text += String.Format(" {0}", code) + Environment.NewLine;
                    }
                    text += Environment.NewLine;
                }
                var invalidDolaCodeEntities = entitiesWithDolaCode.Where(x => !x.DolaCodeValid()).ToList();
                if ( invalidDolaCodeEntities.Any() )
                {
                    text += "Invalid DOLA codes:" + Environment.NewLine;
                    foreach ( var dolaEntity in invalidDolaCodeEntities )
                    {
                        text += String.Format(" {0} {1} ({2})", dolaEntity.Dola.code, dolaEntity.english, dolaEntity.type) + Environment.NewLine;
                    }
                    text += Environment.NewLine;
                }
            }

            var localEntitiesWithoutParent = localEntitiesWithOffice.Where(x => !x.parent.Any());
            if ( localEntitiesWithoutParent.Any() )
            {
                text += "Local governments without parent:" + Environment.NewLine;
                foreach ( var subEntity in localEntitiesWithoutParent )
                {
                    text += String.Format(" {0} {1}", subEntity.geocode, subEntity.english) + Environment.NewLine;
                }
                text += Environment.NewLine;
            }

            var allTambon = entity.FlatList().Where(x => x.type == EntityType.Tambon && !x.IsObsolete);
            var localGovernmentCoverages = new List<LocalGovernmentCoverageEntity>();
            foreach ( var item in localEntitiesWithOffice )
            {
                localGovernmentCoverages.AddRange(item.LocalGovernmentAreaCoverage);
            }
            var localGovernmentCoveragesByTambon = localGovernmentCoverages.GroupBy(s => s.geocode);
            var tambonWithMoreThanOneCoverage = localGovernmentCoveragesByTambon.Where(x => x.Count() > 1);
            var duplicateCompletelyCoveredTambon = tambonWithMoreThanOneCoverage.Where(x => x.Any(y => y.coverage == CoverageType.completely)).Select(x => x.Key);
            var invalidlocalGovernmentCoverages = localGovernmentCoveragesByTambon.Where(x => !allTambon.Any(y => y.geocode == x.Key));
            // var tambonWithMoreThanOneCoverage = localGovernmentCoveragesByTambon.SelectMany(grp => grp.Skip(1)).ToList();
            // var duplicateCompletelyCoveredTambon = tambonWithMoreThanOneCoverage.Where(x => x.coverage == CoverageType.completely);
            if ( invalidlocalGovernmentCoverages.Any() )
            {
                text += "Invalid Tambon references by areacoverage:" + Environment.NewLine;
                foreach ( var code in invalidlocalGovernmentCoverages )
                {
                    text += String.Format(" {0}", code.Key) + Environment.NewLine;
                }
                text += Environment.NewLine;
            }
            if ( duplicateCompletelyCoveredTambon.Any() )
            {
                text += "Tambon covered completely more than once:" + Environment.NewLine;
                foreach ( var code in duplicateCompletelyCoveredTambon )
                {
                    text += String.Format(" {0}", code) + Environment.NewLine;
                }
                text += Environment.NewLine;
            }
            var partialLocalGovernmentCoverages = localGovernmentCoverages.Where(x => x.coverage == CoverageType.partially);
            var partiallyCoveredTambon = partialLocalGovernmentCoverages.GroupBy(s => s.geocode);
            var onlyOnePartialCoverage = partiallyCoveredTambon.Select(group => new
            {
                code = group.Key,
                count = group.Count()
            }).Where(x => x.count == 1).Select(y => y.code);
            if ( onlyOnePartialCoverage.Any() )
            {
                text += "Tambon covered partially only once:" + Environment.NewLine;
                foreach ( var code in onlyOnePartialCoverage )
                {
                    text += String.Format(" {0}", code) + Environment.NewLine;
                }
                text += Environment.NewLine;
            }
            var tambonWithoutCoverage = allTambon.Where(x => !localGovernmentCoveragesByTambon.Any(y => y.Key == x.geocode));
            if ( tambonWithoutCoverage.Any() )
            {
                text += String.Format("Tambon without coverage ({0}):", tambonWithoutCoverage.Count()) + Environment.NewLine;
                foreach ( var tambon in tambonWithoutCoverage )
                {
                    text += String.Format(" {0}", tambon.geocode) + Environment.NewLine;
                }
                text += Environment.NewLine;
            }
            var localGovernmentWithoutCoverage = localEntitiesWithOffice.Where(x => x.type != EntityType.PAO && !x.LocalGovernmentAreaCoverage.Any());
            if ( localGovernmentWithoutCoverage.Any() )
            {
                text += String.Format("LAO without coverage ({0}):", localGovernmentWithoutCoverage.Count()) + Environment.NewLine;
                foreach ( var tambon in localGovernmentWithoutCoverage )
                {
                    text += String.Format(" {0}", tambon.geocode) + Environment.NewLine;
                }
                text += Environment.NewLine;
            }

            var tambonWithoutPostalCode = allTambon.Where(x => !x.codes.post.value.Any());
            if ( tambonWithoutPostalCode.Any() )
            {
                text += String.Format("Tambon without postal code ({0}):", tambonWithoutPostalCode.Count()) + Environment.NewLine;
                foreach ( var tambon in tambonWithoutPostalCode )
                {
                    text += String.Format(" {0}", tambon.geocode) + Environment.NewLine;
                }
                text += Environment.NewLine;
            }

            text += CheckCode(entity, new List<EntityType>() { EntityType.Changwat }, "FIPS10", (Entity x) => x.codes.fips10.value, "TH\\d\\d");
            text += CheckCode(entity, new List<EntityType>() { EntityType.Changwat }, "ISO3166", (Entity x) => x.codes.iso3166.value, "TH-(\\d\\d|S)");
            text += CheckCode(entity, new List<EntityType>() { EntityType.Changwat, EntityType.Amphoe }, "HASC", (Entity x) => x.codes.hasc.value, "TH(\\.[A-Z]{2}){1,2}");
            text += CheckCode(entity, new List<EntityType>() { EntityType.Changwat, EntityType.Amphoe }, "SALB", (Entity x) => x.codes.salb.value, "THA[\\d{3}]{1,2}");

            // check areacoverages
            txtErrors.Text = text;
        }

        private String CheckCode(Entity entity, IEnumerable<EntityType> entityTypes, String codeName, Func<Entity, String> selector, String format)
        {
            String text = String.Empty;
            var allEntites = entity.FlatList().Where(x => !x.IsObsolete);
            var allEntityOfFittingType = allEntites.Where(x => x.type.IsCompatibleEntityType(entityTypes));
            var entitiesWithoutCode = allEntityOfFittingType.Where(x => String.IsNullOrEmpty(selector(x)));
            if ( entitiesWithoutCode.Any() )
            {
                text += String.Format("Entity without {0} code ({1}):", codeName, entitiesWithoutCode.Count()) + Environment.NewLine;
                foreach ( var subEntity in entitiesWithoutCode )
                {
                    text += String.Format(" {0}", subEntity.geocode) + Environment.NewLine;
                }
                text += Environment.NewLine;
            }
            var allCodes = allEntites.Where(x => !String.IsNullOrEmpty(selector(x))).Select(y => selector(y)).ToList();
            var duplicateCodes = allCodes.GroupBy(s => s).SelectMany(grp => grp.Skip(1)).ToList();
            if ( duplicateCodes.Any() )
            {
                text += String.Format("Duplicate {0} codes:", codeName) + Environment.NewLine;
                foreach ( var code in duplicateCodes )
                {
                    text += String.Format(" {0}", code) + Environment.NewLine;
                }
                text += Environment.NewLine;
            }
            var regex = new Regex(format);
            var invalidCodes = allCodes.Where(x => !regex.IsMatch(x));
            if ( invalidCodes.Any() )
            {
                text += String.Format("Invalid {0} codes:", codeName) + Environment.NewLine;
                foreach ( var code in invalidCodes )
                {
                    text += String.Format(" {0}", code) + Environment.NewLine;
                }
                text += Environment.NewLine;
            }

            return text;
        }

        private void SetInfo(Entity entity)
        {
            var value = String.Empty;
            var populationData = entity.population.FirstOrDefault(x => x.Year == PopulationReferenceYear && x.source == PopulationDataSource);
            if ( populationData != null )
            {
                value = String.Format("Population: {0} ({1} male,  {2} female)",
                    populationData.TotalPopulation.total,
                    populationData.TotalPopulation.male,
                    populationData.TotalPopulation.female) + Environment.NewLine + Environment.NewLine;
            }
            value += EntitySubDivisionCount(entity) + Environment.NewLine;

            txtSubDivisions.Text = value;
        }

        private void CalcMubanData(Entity entity)
        {
            String result = String.Empty;
            var allTambon = entity.FlatList().Where(x => !x.IsObsolete && x.type == EntityType.Tambon);
            var allMuban = entity.FlatList().Where(x => !x.IsObsolete && x.type == EntityType.Muban);
            var mubanNumbers = allTambon.GroupBy(x => x.entity.Count(y => !y.IsObsolete && y.type == EntityType.Muban))
                .Select(g => g.Key).ToList();
            mubanNumbers.Sort();
            if ( allMuban.Count() == 0 )
            {
                result = "No Muban" + Environment.NewLine;
            }
            else
            {
                result = String.Format("{0} Muban; Tambon have between {1} and {2} Muban" + Environment.NewLine,
                    allMuban.Count(),
                    mubanNumbers.First(),
                    mubanNumbers.Last());
            }
            // could add: Muban creations in last years
            var tambonWithInvalidMubanNumber = TambonWithInvalidMubanNumber(allTambon);
            if ( tambonWithInvalidMubanNumber.Any() )
            {
                result += Environment.NewLine + "Muban inconsistent for:" + Environment.NewLine;
                foreach ( var tambon in tambonWithInvalidMubanNumber )
                {
                    result += String.Format("{0}: {1}", tambon.geocode, tambon.english) + Environment.NewLine;
                }
            }
            txtMuban.Text = result;
        }

        private IEnumerable<Entity> TambonWithInvalidMubanNumber(IEnumerable<Entity> allTambon)
        {
            var result = new List<Entity>();
            foreach ( var tambon in allTambon.Where(x => x.type == EntityType.Tambon) )
            {
                if ( !tambon.MubanNumberConsistent() )
                {
                    result.Add(tambon);
                }
            }
            return result;
        }

        private Dictionary<EntityType, Int32> CountSubdivisions(IEnumerable<Entity> entities)
        {
            var counted = entities.GroupBy(x => x.type).Select(group => new
            {
                type = group.Key,
                count = group.Count()
            });
            var result = new Dictionary<EntityType, Int32>();
            foreach ( var keyvaluepair in counted )
            {
                result[keyvaluepair.type] = keyvaluepair.count;
            }
            return result;
        }

        private Dictionary<EntityType, Int32> CountSubdivisions(Entity entity)
        {
            var toCount = localGovernments.Where(x => x.parent.Contains(entity.geocode) || GeocodeHelper.IsBaseGeocode(entity.geocode, x.geocode)).ToList();
            toCount.AddRange(entity.FlatList().Where(x => !x.type.IsLocalGovernment()));
            toCount.RemoveAll(x => x.type == EntityType.Unknown || x.IsObsolete);
            return CountSubdivisions(toCount);
        }

        private Dictionary<EntityType, Int32> CountSubdivisionsWithoutLocation(Entity entity)
        {
            var toCount = localGovernments.Where(x => x.parent.Contains(entity.geocode) || GeocodeHelper.IsBaseGeocode(entity.geocode, x.geocode)).ToList();
            toCount.AddRange(entity.FlatList().Where(x => !x.type.IsLocalGovernment()));
            toCount.RemoveAll(x => x.type == EntityType.Unknown || x.IsObsolete);
            toCount.RemoveAll(x => x.office.Any(y => y.Point != null));
            return CountSubdivisions(toCount);
        }

        private String EntitySubDivisionCount(Entity entity)
        {
            var counted = CountSubdivisions(entity);
            var noLocation = CountSubdivisionsWithoutLocation(entity);

            var result = String.Empty;
            foreach ( var keyvaluepair in counted )
            {
                Int32 noLocationCount = 0;
                if ( noLocation.TryGetValue(keyvaluepair.Key, out noLocationCount) )
                {
                    result += String.Format("{0}: {1} ({2} without location)", keyvaluepair.Key, keyvaluepair.Value, noLocationCount) + Environment.NewLine;
                }
                else
                {
                    result += String.Format("{0}: {1}", keyvaluepair.Key, keyvaluepair.Value) + Environment.NewLine;
                }
            }
            return result;
        }

        private void EntityToCentralAdministrativeListView(Entity entity)
        {
            listviewCentralAdministration.BeginUpdate();
            listviewCentralAdministration.Items.Clear();
            foreach ( Entity subEntity in entity.entity.Where(x => !x.IsObsolete && !x.type.IsLocalGovernment()) )
            {
                ListViewItem item = listviewCentralAdministration.Items.Add(subEntity.english);
                item.SubItems.Add(subEntity.name);
                item.SubItems.Add(subEntity.geocode.ToString());
                var populationData = subEntity.population.FirstOrDefault(x => x.Year == PopulationReferenceYear && x.source == PopulationDataSource);
                if ( populationData != null )
                {
                    item.SubItems.Add(populationData.TotalPopulation.total.ToString());
                }
            }
            listviewCentralAdministration.EndUpdate();
        }

        private IEnumerable<Entity> LocalGovernmentEntitiesOf(Entity entity)
        {
            return localGovernments.Where(x => x.parent.Contains(entity.geocode) || GeocodeHelper.IsBaseGeocode(entity.geocode, x.geocode) && !x.IsObsolete);
        }

        private void EntityToLocalAdministrativeListView(Entity entity)
        {
            listviewLocalAdministration.BeginUpdate();
            listviewLocalAdministration.Items.Clear();
            var localGovernmentsInEntity = LocalGovernmentEntitiesOf(entity).ToList();
            foreach ( Entity subEntity in localGovernmentsInEntity )
            {
                ListViewItem item = listviewLocalAdministration.Items.Add(subEntity.english);
                item.SubItems.Add(subEntity.name);
                item.SubItems.Add(subEntity.type.ToString());
                if ( subEntity.geocode > 9999 )
                {
                    // generated geocode
                    item.SubItems.Add(String.Empty);
                }
                else
                {
                    item.SubItems.Add(subEntity.geocode.ToString());
                }
                String dolaCode = String.Empty;
                var office = subEntity.office.FirstOrDefault(x => x.type == OfficeType.TAOOffice || x.type == OfficeType.PAOOffice || x.type == OfficeType.MunicipalityOffice);
                if ( office != null )
                {
                    if ( (office.dola != null) && (office.dola.codeSpecified) )
                    {
                        dolaCode = office.dola.code.ToString();
                    }
                }
                item.SubItems.Add(dolaCode);
                var populationData = subEntity.population.FirstOrDefault(x => x.Year == PopulationReferenceYear && x.source == PopulationDataSource);
                if ( populationData != null )
                {
                    item.SubItems.Add(populationData.TotalPopulation.total.ToString());
                }
            }
            listviewLocalAdministration.EndUpdate();
        }

        private void treeviewSelection_MouseUp(Object sender, MouseEventArgs e)
        {
            if ( e.Button == MouseButtons.Right )
            {
                // Select the clicked node
                treeviewSelection.SelectedNode = treeviewSelection.GetNodeAt(e.X, e.Y);

                if ( treeviewSelection.SelectedNode != null )
                {
                    contextMenuStrip1.Show(treeviewSelection, e.Location);
                }
            }
        }

        private delegate String CountAsString(Int32 count);

        private void mnuWikipediaGerman_Click(Object sender, EventArgs e)
        {
            var numberStrings = new Dictionary<Int32, String>() {
                { 1, "eine" },
                { 2, "zwei" },
                { 3, "drei" },
                { 4, "vier" },
                { 5, "fünf" },
                { 6, "sechs" },
                { 7, "sieben" },
                { 8, "acht" },
                { 9, "neun" },
                { 10, "zehn" },
                { 11, "elf" },
                { 12, "zwölf" },
            };
            var selectedNode = treeviewSelection.SelectedNode;
            var entity = (Entity)(selectedNode.Tag);
            if ( entity.type.IsCompatibleEntityType(EntityType.Amphoe) )
            {
                var germanCulture = new CultureInfo("de-DE");

                String headerBangkok = "== Verwaltung ==" + Environment.NewLine;
                String textBangkok = "Der Bezirk {0} ist in {1} ''[[Khwaeng]]'' („Unterbezirke““) eingeteilt." + Environment.NewLine + Environment.NewLine;
                String headerAmphoe = "== Verwaltung ==" + Environment.NewLine + "=== Provinzverwaltung ===" + Environment.NewLine;
                String textAmphoe = "Der Landkreis {0} ist in {1} ''[[Tambon]]'' („Unterbezirke“ oder „Gemeinden“) eingeteilt, die sich weiter in {2} ''[[Muban]]'' („Dörfer“) unterteilen." + Environment.NewLine + Environment.NewLine;
                String tableHeaderAmphoe =
                    "{{| class=\"wikitable\"" + Environment.NewLine +
                    "! Nr." + Environment.NewLine +
                    "! Name" + Environment.NewLine +
                    "! Thai" + Environment.NewLine +
                    "! Muban" + Environment.NewLine +
                    "! Einw.{0}" + Environment.NewLine;
                String tableHeaderBangkok =
                    "{{| class=\"wikitable\"" + Environment.NewLine +
                    "! Nr." + Environment.NewLine +
                    "! Name" + Environment.NewLine +
                    "! Thai" + Environment.NewLine +
                    "! Einw.{0}" + Environment.NewLine;
                String tableEntryAmphoe = "|-" + Environment.NewLine +
                    "||{0}.||{1}||{{{{lang|th|{2}}}}}||{3}||{4}" + Environment.NewLine;
                String tableEntryBangkok = "|-" + Environment.NewLine +
                    "||{0}.||{1}||{{{{lang|th|{2}}}}}||{3}" + Environment.NewLine;
                String tableFooter = "|}" + Environment.NewLine;

                String headerLocal = "=== Lokalverwaltung ===" + Environment.NewLine;
                String textLocalSingular = "Es gibt eine Kommune mit „{0}“-Status ''({1})'' im Landkreis:" + Environment.NewLine;
                String textLocalPlural = "Es gibt {0} Kommunen mit „{1}“-Status ''({2})'' im Landkreis:" + Environment.NewLine;
                String taoWithThesaban = "Außerdem gibt es {0} „[[Verwaltungsgliederung Thailands#Tambon-Verwaltungsorganisationen|Tambon-Verwaltungsorganisationen]]“ ({{lang|th|องค์การบริหารส่วนตำบล}} – Tambon Administrative Organizations, TAO)" + Environment.NewLine;
                String taoWithoutThesaban = "Im Landkreis gibt es {0} „[[Verwaltungsgliederung Thailands#Tambon-Verwaltungsorganisationen|Tambon-Verwaltungsorganisationen]]“ ({{lang|th|องค์การบริหารส่วนตำบล}} – Tambon Administrative Organizations, TAO)" + Environment.NewLine;
                String entryLocal = "* {0} (Thai: {{{{lang|th|{1}}}}})";
                String entryLocalCoverage = " bestehend aus {0}.";
                String entryLocalCoverageTwo = " bestehend aus {0} und {1}.";
                String tambonCompleteSingular = "dem kompletten Tambon {0}";
                String tambonPartiallySingular = "Teilen des Tambon {0}";
                String tambonCompletePlural = "den kompletten Tambon {0}";
                String tambonPartiallyPlural = "den Teilen der Tambon {0}";

                CountAsString countAsString = delegate(Int32 count)
                {
                    String countAsStringResult;
                    if ( !numberStrings.TryGetValue(count, out countAsStringResult) )
                    {
                        countAsStringResult = count.ToString(germanCulture);
                    }
                    return countAsStringResult;
                };

                var allEntities = entity.entity.ToList();
                var local = LocalGovernmentEntitiesOf(entity).Where(x => !x.IsObsolete);
                allEntities.AddRange(local);
                Dictionary<Entity, String> wikipediaLinks = new Dictionary<Entity, String>();
                if ( CheckWikiData )
                {
                    wikipediaLinks = RetrieveWikpediaLinks(allEntities, Language.German);
                }
                var counted = CountSubdivisions(entity);
                if ( !counted.ContainsKey(EntityType.Muban) )
                {
                    counted[EntityType.Muban] = 0;
                }
                String result = String.Empty;

                if ( entity.type == EntityType.Khet )
                {
                    result = headerBangkok +
                        String.Format(germanCulture, textBangkok, entity.english, countAsString(counted[EntityType.Khwaeng])) +
                        String.Format(germanCulture, tableHeaderBangkok, PopulationDataDownloader.WikipediaReference(GeocodeHelper.ProvinceCode(entity.geocode), PopulationReferenceYear, Language.German));
                }
                else
                {
                    result = headerAmphoe +
                        String.Format(germanCulture, textAmphoe, entity.english, countAsString(counted[EntityType.Tambon]), countAsString(counted[EntityType.Muban])) +
                        String.Format(germanCulture, tableHeaderAmphoe, PopulationDataDownloader.WikipediaReference(GeocodeHelper.ProvinceCode(entity.geocode), PopulationReferenceYear, Language.German));
                }
                var maxPopulation = 0;
                foreach ( var tambon in entity.entity.Where(x => x.type.IsCompatibleEntityType(EntityType.Tambon) && !x.IsObsolete) )
                {
                    var populationData = tambon.population.FirstOrDefault(x => x.Year == PopulationReferenceYear && x.source == PopulationDataSource);
                    if ( populationData != null )
                    {
                        maxPopulation = Math.Max(maxPopulation, populationData.TotalPopulation.total);
                    }
                }
                var allTambon = entity.entity.Where(x => x.type.IsCompatibleEntityType(EntityType.Tambon) && !x.IsObsolete).ToList();
                foreach ( var tambon in allTambon )
                {
                    var subCounted = CountSubdivisions(tambon);
                    var muban = 0;
                    if ( !subCounted.TryGetValue(EntityType.Muban, out muban) )
                    {
                        muban = 0;
                    }
                    var citizen = 0;
                    var populationData = tambon.population.FirstOrDefault(x => x.Year == PopulationReferenceYear && x.source == PopulationDataSource);
                    if ( populationData != null )
                    {
                        citizen = populationData.TotalPopulation.total;
                    }
                    var geocodeString = (tambon.geocode % 100).ToString(germanCulture);
                    if ( entity.geocode % 100 < 10 )
                    {
                        geocodeString = "{{0}}" + geocodeString;
                    }
                    String mubanString;
                    if ( muban == 0 )
                    {
                        mubanString = "-";
                    }
                    else if ( muban < 10 )
                    {
                        mubanString = "{{0}}" + muban.ToString(germanCulture);
                    }
                    else
                    {
                        mubanString = muban.ToString();
                    }
                    var citizenString = citizen.ToString("###,##0");
                    for ( int i = citizenString.Length ; i < maxPopulation.ToString("###,##0", germanCulture).Length ; i++ )
                    {
                        citizenString = "{{0}}" + citizenString;
                    }
                    var english = tambon.english;
                    var link = String.Empty;
                    if ( wikipediaLinks.TryGetValue(tambon, out link) )
                    {
                        english = WikiLink(link, english);
                    }

                    if ( entity.type == EntityType.Khet )
                    {
                        result += String.Format(germanCulture, tableEntryBangkok, geocodeString, english, tambon.name, citizenString);
                    }
                    else
                    {
                        result += String.Format(germanCulture, tableEntryAmphoe, geocodeString, english, tambon.name, mubanString, citizenString);
                    }
                }
                result += tableFooter + Environment.NewLine;

                var localTypes = CountSubdivisions(local);
                if ( localTypes.Any() )
                {
                    result += headerLocal;
                    var check = new List<EntityType>()
                {
                    EntityType.ThesabanNakhon,
                    EntityType.ThesabanMueang,
                    EntityType.ThesabanTambon,
                    EntityType.TAO,
                };
                    foreach ( var entityType in check )
                    {
                        Int32 count = 0;
                        if ( localTypes.TryGetValue(entityType, out count) )
                        {
                            if ( entityType == EntityType.TAO )
                            {
                                if ( localTypes.Keys.Count == 1 )
                                {
                                    result += String.Format(germanCulture, taoWithoutThesaban, countAsString(count));
                                }
                                else
                                {
                                    result += String.Format(germanCulture, taoWithThesaban, countAsString(count));
                                }
                            }
                            else
                            {
                                if ( count == 1 )
                                {
                                    result += String.Format(germanCulture, textLocalSingular, entityType.Translate(Language.German), _deWikipediaLink[entityType]);
                                }
                                else
                                {
                                    result += String.Format(germanCulture, textLocalPlural, countAsString(count), entityType.Translate(Language.German), _deWikipediaLink[entityType]);
                                }
                            }
                            foreach ( var localEntity in local.Where(x => x.type == entityType) )
                            {
                                // TODO - How to sort?
                                var english = localEntity.english;
                                var link = String.Empty;
                                if ( wikipediaLinks.TryGetValue(localEntity, out link) )
                                {
                                    english = WikiLink(link, english);
                                }
                                result += String.Format(germanCulture, entryLocal, english, localEntity.FullName);
                                if ( localEntity.LocalGovernmentAreaCoverage.Any() )
                                {
                                    var coverage = localEntity.LocalGovernmentAreaCoverage.GroupBy(x => x.coverage).Select(group => new
                                    {
                                        Coverage = group.Key,
                                        TambonCount = group.Count()
                                    });
                                    var textComplete = String.Empty;
                                    var textPartially = String.Empty;

                                    if ( coverage.Any(x => x.Coverage == CoverageType.completely) )
                                    {
                                        var completeTambon = localEntity.LocalGovernmentAreaCoverage.
                                            Where(x => x.coverage == CoverageType.completely).
                                            Select(x => entity.FlatList().FirstOrDefault(y => y.geocode == x.geocode));
                                        var tambonString = String.Join(", ", completeTambon.Select(x => x.english));
                                        if ( coverage.First(x => x.Coverage == CoverageType.completely).TambonCount == 1 )
                                        {
                                            textComplete = String.Format(germanCulture, tambonCompleteSingular, tambonString);
                                        }
                                        else
                                        {
                                            textComplete = String.Format(germanCulture, tambonCompletePlural, tambonString);
                                        }
                                    }
                                    if ( coverage.Any(x => x.Coverage == CoverageType.partially) )
                                    {
                                        var completeTambon = localEntity.LocalGovernmentAreaCoverage.
                                            Where(x => x.coverage == CoverageType.partially).
                                            Select(x => entity.FlatList().FirstOrDefault(y => y.geocode == x.geocode));
                                        var tambonString = String.Join(", ", completeTambon.Select(x => x.english));
                                        if ( coverage.First(x => x.Coverage == CoverageType.partially).TambonCount == 1 )
                                        {
                                            textPartially = String.Format(germanCulture, tambonPartiallySingular, tambonString);
                                        }
                                        else
                                        {
                                            textPartially = String.Format(germanCulture, tambonPartiallyPlural, tambonString);
                                        }
                                    }
                                    if ( !String.IsNullOrEmpty(textPartially) && !String.IsNullOrEmpty(textComplete) )
                                    {
                                        result += String.Format(germanCulture, entryLocalCoverageTwo, textComplete, textPartially);
                                    }
                                    else
                                    {
                                        result += String.Format(germanCulture, entryLocalCoverage, textComplete + textPartially);
                                    }
                                }
                                result += Environment.NewLine;
                            }
                            result += Environment.NewLine;
                        }
                    }
                }

                Clipboard.Clear();
                Clipboard.SetText(result);
            }
        }

        private String WikiLink(String link, String title)
        {
            if ( link == title )
            {
                return "[[" + title + "]]";
            }
            else
            {
                return "[[" + link + "|" + title + "]]";
            }
        }

        private Dictionary<Entity, String> RetrieveWikpediaLinks(IEnumerable<Entity> entities, Language language)
        {
            var result = new Dictionary<Entity, String>();
            var api = new WikibaseApi("https://www.wikidata.org", "TambonBot");
            var helper = new WikiDataHelper(api);
            foreach ( var entity in entities.Where(x => x.wiki != null && !String.IsNullOrEmpty(x.wiki.wikidata)) )
            {
                var item = helper.GetWikiDataItemForEntity(entity);
                if ( item != null )
                {
                    var links = item.getSitelinks();
                    String languageLink;
                    String wikiIdentifier = String.Empty;
                    switch ( language )
                    {
                        case Language.German:
                            wikiIdentifier = "dewiki";
                            break;
                        case Language.English:
                            wikiIdentifier = "enwiki";
                            break;
                        case Language.Thai:
                            wikiIdentifier = "thwiki";
                            break;
                    }
                    if ( item.getSitelinks().TryGetValue(wikiIdentifier, out languageLink) )
                    {
                        result[entity] = languageLink;
                    }
                }
            }
            return result;
        }

        #endregion private methods

        private IEnumerable<GazetteEntry> AreaDefinitionAnnouncements(Entity entity)
        {
            var result = new List<GazetteEntry>();
            var allAboutGeocode = GlobalData.AllGazetteAnnouncements.AllGazetteEntries.Where(x => x.IsAboutGeocode(entity.geocode, true));
            var allAreaDefinitionAnnouncements = allAboutGeocode.Where(x => x.Items.Any(y => y is GazetteAreaDefinition));
            foreach ( var announcement in allAreaDefinitionAnnouncements )
            {
                var areaDefinitions = announcement.Items.Where(x => x is GazetteAreaDefinition);
                if ( areaDefinitions.Any(x => (x as GazetteAreaDefinition).IsAboutGeocode(entity.geocode, true)) )
                {
                    result.Add(announcement);
                }
            }
            return result;
        }

        private void mnuMubanDefinitions_Click(Object sender, EventArgs e)
        {
            var selectedNode = treeviewSelection.SelectedNode;
            var entity = (Entity)(selectedNode.Tag);
            foreach ( var entry in AreaDefinitionAnnouncements(entity) )
            {
                ShowPgf(entry);
            }
        }

        private void ShowPgf(GazetteEntry entry)
        {
            try
            {
                entry.MirrorToCache();
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                // TODO
                String pgfFilename = entry.LocalPdfFileName;

                if ( File.Exists(pgfFilename) )
                {
                    p.StartInfo.FileName = pgfFilename;
                    p.Start();
                }
            }
            catch
            {
                // throw;
            }
        }
    }
}