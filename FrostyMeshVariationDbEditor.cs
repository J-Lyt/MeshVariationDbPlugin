using Frosty.Controls;
using Frosty.Core;
using Frosty.Core.Controls;
using Frosty.Core.Viewport;
using Frosty.Core.Windows;
using FrostyEditor.Windows;
using FrostySdk.Ebx;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers.Entries;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace MeshVariationDbPlugin
{
    public class MeshVariationDatabaseDefinition : AssetDefinition
    {
        public MeshVariationDatabaseDefinition()
        {
        }

        public override FrostyAssetEditor GetEditor(ILogger logger)
        {
            return new FrostyMeshVariationDbEditor(logger);
        }
    }

    public class FrostyMeshVariationDbEditor : FrostyAssetEditor
    {
        public FrostyMeshVariationDbEditor(ILogger inLogger)
        : base(inLogger)
        {
        }

        public override List<ToolbarItem> RegisterToolbarItems()
        {
            List<ToolbarItem> toolbarItems = base.RegisterToolbarItems();
            toolbarItems.Add(new ToolbarItem("Export", "Export Database", "FrostyEditor;component/Images/Export.png", new RelayCommand((object state) => { ExportButton_Click(this, new RoutedEventArgs()); })));
            toolbarItems.Add(new ToolbarItem("Import", "Import Database", "FrostyEditor;component/Images/Import.png", new RelayCommand((object state) => { ImportButton_Click(this, new RoutedEventArgs()); })));
            toolbarItems.Add(new ToolbarItem("Replace Shaders (All)", null, "MeshVariationDbPlugin;component/Images/Alpha.png", new RelayCommand((object state) => { AlphaButton_Click(this, new RoutedEventArgs()); })));
            toolbarItems.Add(new ToolbarItem("Replace Shaders (Mesh)", null, "MeshVariationDbPlugin;component/Images/AlphaMesh.png", new RelayCommand((object state) => { AlphaMeshButton_Click(this, new RoutedEventArgs()); })));
            toolbarItems.Add(new ToolbarItem("Find Namehash", null, "MeshVariationDbPlugin;component/Images/Hash.png", new RelayCommand((object state) => { FindHashButton_Click(this, new RoutedEventArgs()); })));
            
            return toolbarItems;
        }

        FrostyDataExplorer dataExplorer = (FrostyDataExplorer)typeof(MainWindow).GetField("dataExplorer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Application.Current.MainWindow);

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            EbxAssetEntry entry = (EbxAssetEntry)AssetEntry;

            AssetDefinition assetDefinition = App.PluginManager.GetAssetDefinition(entry.Type) ?? new AssetDefinition();

            List<AssetImportType> filters = new List<AssetImportType>();
            assetDefinition.GetSupportedImportTypes(filters);

            string filterString = "";
            foreach (AssetImportType filter in filters)
                filterString += "|" + filter.FilterString;
            filterString = filterString.Trim('|');

            FrostyOpenFileDialog ofd = new FrostyOpenFileDialog("Import Database", filterString, assetDefinition.GetType().Name);
            if (ofd.ShowDialog())
            {
                if (assetDefinition.Import(entry, ofd.FileName, filters[ofd.FilterIndex - 1]))
                {
                    dataExplorer.RefreshItems();

                    App.Logger.Log("Imported {0} into {1}", ofd.FileName, entry.Name);
                }
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            EbxAssetEntry entry = (EbxAssetEntry)AssetEntry;
            if (entry != null)
            {
                AssetDefinition assetDefinition = App.PluginManager.GetAssetDefinition(entry.Type) ?? new AssetDefinition();

                List<AssetExportType> filters = new List<AssetExportType>();
                assetDefinition.GetSupportedExportTypes(filters);

                string filterString = "";
                foreach (AssetExportType filter in filters)
                    filterString += "|" + filter.FilterString;
                filterString = filterString.Trim('|');

                string entryName = entry.Name.ToString().Split('/')[0];
                string entryGuid = entry.Guid.ToString().Substring(0, 8);

                FrostySaveFileDialog sfd = new FrostySaveFileDialog("Export Database", filterString, assetDefinition.GetType().Name, entry.Filename + "_" + entryName.ToUpper() + "_" + entryGuid);
                if (sfd.ShowDialog())
                {
                    if (assetDefinition.Export(entry, sfd.FileName, filters[sfd.FilterIndex - 1].Extension))
                    {
                        App.Logger.Log("Exported {0} to {1}", entry.Name, sfd.FileName);
                    }
                }
            }
        }

        private void AlphaButton_Click(object sender, RoutedEventArgs e)
        {
            EbxAssetEntry entry = (EbxAssetEntry)AssetEntry;
            
            dynamic varEntriesPre = App.AssetManager.GetEbx(entry).RootObject;
            dynamic varEntryPre = varEntriesPre.Entries;

            bool isAvailable = false;
            
            int shaderNum = 0;
            int shaderMasterNum = 0;
            int shaderDecayNum = 0;
            int shaderDissolveBBNum = 0;
            int shaderFrozenNum = 0;
            int shaderDoTMasterNum = 0;
            int shaderPetrifyNum = 0;

            string shaderMasterText = "Character_Wardrobe_Master_2Sided_Alpha";

            if (varEntryPre.Count > 0)
            {
                for (int i = 0; i < varEntryPre.Count; i++)
                {
                    var entriesPre = varEntryPre[i];

                    foreach (var meshVariationMaterialPre in entriesPre.Materials)
                    {
                        if (meshVariationMaterialPre.SurfaceShaderId == 3024908220) //Master
                        {
                            isAvailable = true;
                            break;
                        }
                    }
                }
            }

            if (isAvailable == true)
            {
                MessageBoxResult result = FrostyMessageBox.Show("Do you wish to replace shaders?", "Replace Shaders", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    App.AssetManager.ModifyEbx(entry.Name, asset);

                    dynamic varEntries = App.AssetManager.GetEbx(entry).RootObject;
                    dynamic varEntry = varEntries.Entries;

                    if (varEntry.Count > 0)
                    {
                        for (int i = 0; i < varEntry.Count; i++)
                        {
                            var entries = varEntry[i];

                            foreach (var meshVariationMaterial in entries.Materials)
                            {
                                if (meshVariationMaterial.SurfaceShaderId == 3024908220) //Master
                                {
                                    meshVariationMaterial.SurfaceShaderId = 1748236261;
                                    meshVariationMaterial.SurfaceShaderGuid = Guid.Parse("5c665d88-fe38-4515-b8b1-0713fd6a558f");

                                    shaderNum++;
                                    shaderMasterNum++;
                                }
                                else if (meshVariationMaterial.SurfaceShaderId == 3366952955) //Decay
                                {
                                    meshVariationMaterial.SurfaceShaderId = 2989973410;
                                    meshVariationMaterial.SurfaceShaderGuid = Guid.Parse("91b79ab7-1f6a-4e76-a00f-372d9b4f64f0");

                                    shaderNum++;
                                    shaderDecayNum++;
                                }
                                else if (meshVariationMaterial.SurfaceShaderId == 390866619) //DissolveBB
                                {
                                    meshVariationMaterial.SurfaceShaderId = 2336327458;
                                    meshVariationMaterial.SurfaceShaderGuid = Guid.Parse("c247f7f5-2bc3-4f16-8459-c5022b851360");

                                    shaderNum++;
                                    shaderDissolveBBNum++;
                                }
                                else if (meshVariationMaterial.SurfaceShaderId == 1571638619) //Frozen
                                {
                                    meshVariationMaterial.SurfaceShaderId = 1828819106;
                                    meshVariationMaterial.SurfaceShaderGuid = Guid.Parse("762d48c1-097b-4a5d-bc28-2bb78d2a17a5");

                                    shaderNum++;
                                    shaderFrozenNum++;
                                }
                                else if (meshVariationMaterial.SurfaceShaderId == 2107662139) //DoTMaster
                                {
                                    meshVariationMaterial.SurfaceShaderId = 4224110914;
                                    meshVariationMaterial.SurfaceShaderGuid = Guid.Parse("a9e6496a-5bf7-4fb9-af37-36dfc47f7cce");

                                    shaderNum++;
                                    shaderDoTMasterNum++;
                                }
                                else if (meshVariationMaterial.SurfaceShaderId == 1184284987) //Petrify
                                {
                                    meshVariationMaterial.SurfaceShaderId = 2534832770;
                                    meshVariationMaterial.SurfaceShaderGuid = Guid.Parse("974e1887-a9da-4f98-a698-178b0d2e4d23");

                                    shaderNum++;
                                    shaderPetrifyNum++;
                                }
                            }
                        }
                    }

                    InvokeOnAssetModified();

                    dataExplorer.RefreshItems();
                    typeof(MainWindow).InvokeMember("RefreshTabs", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, Application.Current.MainWindow, []);

                    App.Logger.Log(
                        shaderNum + " shader(s) have been replaced successfully." + "\n" +
                        shaderMasterText + ": " + shaderMasterNum + "\n" +
                        shaderMasterText + "_Decay_VAR: " + shaderDecayNum + "\n" +
                        shaderMasterText + "_DissolveBB_VAR: " + shaderDissolveBBNum + "\n" +
                        shaderMasterText + "_Frozen_VAR: " + shaderFrozenNum + "\n" +
                        shaderMasterText + "_DoTMaster_VAR: " + shaderDoTMasterNum + "\n" +
                        shaderMasterText + "_Petrify_VAR: " + shaderPetrifyNum);
                }
            }

            if (!isAvailable)
            {
                FrostyMessageBox.Show("There are no non-alpha shaders to replace.", "Replace Shaders");
                return;
            }
        }

        private void AlphaMeshButton_Click(object sender, RoutedEventArgs e)
        {
            EbxAssetEntry entry = (EbxAssetEntry)AssetEntry;
            
            EbxAssetEntry selectedAssetEntry = App.SelectedAsset;
            EbxAsset selectedAsset = App.AssetManager.GetEbx(selectedAssetEntry);

            dynamic varEntriesPre = App.AssetManager.GetEbx(entry).RootObject;
            dynamic varEntryPre = varEntriesPre.Entries;

            bool isAvailable = false;

            int shaderNum = 0;
            int shaderMasterNum = 0;
            int shaderDecayNum = 0;
            int shaderDissolveBBNum = 0;
            int shaderFrozenNum = 0;
            int shaderDoTMasterNum = 0;
            int shaderPetrifyNum = 0;

            string shaderMasterText = "Character_Wardrobe_Master_2Sided_Alpha";

            if (varEntryPre.Count > 0)
            {
                for (int i = 0; i < varEntryPre.Count; i++)
                {
                    var entriesPre = varEntryPre[i];

                    FrostyPropertyGridItemData pointerRefPreGrid = (FrostyPropertyGridItemData)DataContext;

                    Guid pointerRefGuidPre;
                    Guid selectedAssetGuidPre;

                    pointerRefGuidPre = ((PointerRef)entriesPre.Mesh).External.FileGuid;

                    selectedAssetGuidPre = selectedAsset.FileGuid;

                    if (pointerRefGuidPre == selectedAssetGuidPre)
                    {
                        foreach (var meshVariationMaterialPre in entriesPre.Materials)
                        {
                            if (meshVariationMaterialPre.SurfaceShaderId == 3024908220) //Master
                            {
                                isAvailable = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (isAvailable == true)
            {
                MessageBoxResult result = FrostyMessageBox.Show("Do you wish to replace shaders assigned to '" + selectedAssetEntry.Filename + "'?", "Replace Shaders", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    App.AssetManager.ModifyEbx(entry.Name, asset);

                    dynamic varEntries = App.AssetManager.GetEbx(entry).RootObject;
                    dynamic varEntry = varEntries.Entries;

                    if (varEntry.Count > 0)
                    {
                        for (int i = 0; i < varEntry.Count; i++)
                        {
                            var entries = varEntry[i];

                            FrostyPropertyGridItemData pointerRefGrid = (FrostyPropertyGridItemData)DataContext;

                            Guid pointerRefGuid;
                            Guid selectedAssetGuid;

                            pointerRefGuid = ((PointerRef)entries.Mesh).External.FileGuid;

                            selectedAssetGuid = selectedAsset.FileGuid;

                            if (pointerRefGuid == selectedAssetGuid)
                            {
                                foreach (var meshVariationMaterial in entries.Materials)
                                {
                                    if (meshVariationMaterial.SurfaceShaderId == 3024908220) //Master
                                    {
                                        meshVariationMaterial.SurfaceShaderId = 1748236261;
                                        meshVariationMaterial.SurfaceShaderGuid = Guid.Parse("5c665d88-fe38-4515-b8b1-0713fd6a558f");

                                        shaderNum++;
                                        shaderMasterNum++;
                                    }
                                    else if (meshVariationMaterial.SurfaceShaderId == 3366952955) //Decay
                                    {
                                        meshVariationMaterial.SurfaceShaderId = 2989973410;
                                        meshVariationMaterial.SurfaceShaderGuid = Guid.Parse("91b79ab7-1f6a-4e76-a00f-372d9b4f64f0");

                                        shaderNum++;
                                        shaderDecayNum++;
                                    }
                                    else if (meshVariationMaterial.SurfaceShaderId == 390866619) //DissolveBB
                                    {
                                        meshVariationMaterial.SurfaceShaderId = 2336327458;
                                        meshVariationMaterial.SurfaceShaderGuid = Guid.Parse("c247f7f5-2bc3-4f16-8459-c5022b851360");

                                        shaderNum++;
                                        shaderDissolveBBNum++;
                                    }
                                    else if (meshVariationMaterial.SurfaceShaderId == 1571638619) //Frozen
                                    {
                                        meshVariationMaterial.SurfaceShaderId = 1828819106;
                                        meshVariationMaterial.SurfaceShaderGuid = Guid.Parse("762d48c1-097b-4a5d-bc28-2bb78d2a17a5");

                                        shaderNum++;
                                        shaderFrozenNum++;
                                    }
                                    else if (meshVariationMaterial.SurfaceShaderId == 2107662139) //DoTMaster
                                    {
                                        meshVariationMaterial.SurfaceShaderId = 4224110914;
                                        meshVariationMaterial.SurfaceShaderGuid = Guid.Parse("a9e6496a-5bf7-4fb9-af37-36dfc47f7cce");

                                        shaderNum++;
                                        shaderDoTMasterNum++;
                                    }
                                    else if (meshVariationMaterial.SurfaceShaderId == 1184284987) //Petrify
                                    {
                                        meshVariationMaterial.SurfaceShaderId = 2534832770;
                                        meshVariationMaterial.SurfaceShaderGuid = Guid.Parse("974e1887-a9da-4f98-a698-178b0d2e4d23");

                                        shaderNum++;
                                        shaderPetrifyNum++;
                                    }
                                }
                            }
                        }
                    }

                    InvokeOnAssetModified();

                    dataExplorer.RefreshItems();
                    typeof(MainWindow).InvokeMember("RefreshTabs", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, Application.Current.MainWindow, []);

                    App.Logger.Log(
                        shaderNum + " shader(s) assigned to '" + selectedAssetEntry.Filename + "' have been replaced successfully." + "\n" +
                        shaderMasterText + ": " + shaderMasterNum + "\n" +
                        shaderMasterText + "_Decay_VAR: " + shaderDecayNum + "\n" +
                        shaderMasterText + "_DissolveBB_VAR: " + shaderDissolveBBNum + "\n" +
                        shaderMasterText + "_Frozen_VAR: " + shaderFrozenNum + "\n" +
                        shaderMasterText + "_DoTMaster_VAR: " + shaderDoTMasterNum + "\n" +
                        shaderMasterText + "_Petrify_VAR: " + shaderPetrifyNum);
                }
            }

            if (!isAvailable)
            {
                FrostyMessageBox.Show("There are no non-alpha shaders assigned to '" + selectedAssetEntry.Filename + "'.", "Replace Shaders");
                return;
            }
        }

        private void FindHashButton_Click(object sender, RoutedEventArgs e)
        {
            EbxAssetEntry entry = (EbxAssetEntry)AssetEntry;

            dynamic varEntries = App.AssetManager.GetEbx(entry).RootObject;
            dynamic varEntry = varEntries.Entries;

            bool isAvailable = false;

            if (varEntry.Count > 0)
            {
                for (int i = 0; i < varEntry.Count; i++)
                {
                    var entries = varEntry[i];

                    string clipText = Clipboard.GetText();
                    uint clipNum = 0;
                    
                    try
                    {
                        if (clipText.All(char.IsDigit))
                        {
                            clipNum = UInt32.Parse(clipText);
                        }
                        else
                        {
                            App.Logger.Log("Clipboard contents must be a number.");
                            return;
                        }

                        if (entries.VariationAssetNameHash == clipNum)
                        {
                            isAvailable = true;
                            App.Logger.Log(clipNum + " is in entry: [" + i + "]");
                        }
                        else if (entries.VariationAssetNameHash != clipNum)
                        {
                            continue;
                        }
                    }
                    catch
                    {
                        App.Logger.Log("Clipboard contents are invalid.");
                        return;
                    }
                }
            }

            if (!isAvailable)
            {
                App.Logger.Log("Namehash could not be found.");
                return;
            }
        }
    }
}
