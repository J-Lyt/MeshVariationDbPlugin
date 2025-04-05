using Frosty.Controls;
using Frosty.Core;
using Frosty.Core.Controls;
using Frosty.Core.Controls.Editors;
using Frosty.Core.Viewport;
using Frosty.Core.Windows;
using FrostyEditor.Windows;
using FrostySdk;
using FrostySdk.Ebx;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers.Entries;
using MeshVariationDbPlugin.Windows;
using SharpDX;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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
            toolbarItems.Add(new ToolbarItem("Replace Shaders", null, "MeshVariationDbPlugin;component/Images/Alpha.png", new RelayCommand((object state) => { AlphaButton_Click(this, new RoutedEventArgs()); })));
            toolbarItems.Add(new ToolbarItem("Find & Replace", null, "FrostyEditor;component/Images/Properties.png", new RelayCommand((object state) => { FindReplaceButton_Click(this, new RoutedEventArgs()); })));
            toolbarItems.Add(new ToolbarItem("Find Namehash", null, "MeshVariationDbPlugin;component/Images/Hash.png", new RelayCommand((object state) => { FindHashButton_Click(this, new RoutedEventArgs()); })));
            
            return toolbarItems;
        }

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
                    App.EditorWindow.DataExplorer.RefreshItems();

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

            bool isAll;
            bool isMesh;
            bool isMaterial;

            Guid guid = Guid.Empty;

            ReplaceShadersWindow win = new ReplaceShadersWindow();
            if (win.ShowDialog() == true)
            {
                isAll = win.isAll;
                isMesh = win.isMesh;
                isMaterial = win.isMaterial;

                guid = win.Guid;
            }
            else
            {
                return;
            }

            if (varEntryPre.Count > 0)
            {
                for (int i = 0; i < varEntryPre.Count; i++)
                {
                    var entriesPre = varEntryPre[i];

                    if (isAll)
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
                    else if (isMesh)
                    {
                        Guid pointerRefGuidPre = ((PointerRef)entriesPre.Mesh).External.ClassGuid;

                        if (pointerRefGuidPre == guid)
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
                    else if (isMaterial)
                    {
                        foreach (var meshVariationMaterialPre in entriesPre.Materials)
                        {
                            Guid pointerRefGuidPre = ((PointerRef)meshVariationMaterialPre.Material).External.ClassGuid;

                            if (pointerRefGuidPre == guid && meshVariationMaterialPre.SurfaceShaderId == 3024908220) //Master
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
                MessageBoxResult result;

                if (isAll)
                {
                    result = FrostyMessageBox.Show("Do you wish to replace all shaders?", "Replace Shaders", MessageBoxButton.YesNo);
                }
                else if (isMesh || isMaterial)
                {
                    result = FrostyMessageBox.Show("Do you wish to replace shaders assigned to:" + "\n\n" + "[" + guid.ToString() + "]?", "Replace Shaders", MessageBoxButton.YesNo);
                }
                else
                {
                    return;
                }
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

                            if (isAll)
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
                            else if (isMesh)
                            {
                                Guid pointerRefGuid = ((PointerRef)entries.Mesh).External.ClassGuid;

                                if (pointerRefGuid == guid)
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
                            else if (isMaterial)
                            {
                                foreach (var meshVariationMaterial in entries.Materials)
                                {
                                    Guid pointerRefGuid = ((PointerRef)meshVariationMaterial.Material).External.ClassGuid;

                                    bool isEqual = pointerRefGuid == guid;

                                    if (isEqual && meshVariationMaterial.SurfaceShaderId == 3024908220) //Master
                                    {
                                        meshVariationMaterial.SurfaceShaderId = 1748236261;
                                        meshVariationMaterial.SurfaceShaderGuid = Guid.Parse("5c665d88-fe38-4515-b8b1-0713fd6a558f");

                                        shaderNum++;
                                        shaderMasterNum++;
                                    }
                                    else if (isEqual && meshVariationMaterial.SurfaceShaderId == 3366952955) //Decay
                                    {
                                        meshVariationMaterial.SurfaceShaderId = 2989973410;
                                        meshVariationMaterial.SurfaceShaderGuid = Guid.Parse("91b79ab7-1f6a-4e76-a00f-372d9b4f64f0");

                                        shaderNum++;
                                        shaderDecayNum++;
                                    }
                                    else if (isEqual && meshVariationMaterial.SurfaceShaderId == 390866619) //DissolveBB
                                    {
                                        meshVariationMaterial.SurfaceShaderId = 2336327458;
                                        meshVariationMaterial.SurfaceShaderGuid = Guid.Parse("c247f7f5-2bc3-4f16-8459-c5022b851360");

                                        shaderNum++;
                                        shaderDissolveBBNum++;
                                    }
                                    else if (isEqual && meshVariationMaterial.SurfaceShaderId == 1571638619) //Frozen
                                    {
                                        meshVariationMaterial.SurfaceShaderId = 1828819106;
                                        meshVariationMaterial.SurfaceShaderGuid = Guid.Parse("762d48c1-097b-4a5d-bc28-2bb78d2a17a5");

                                        shaderNum++;
                                        shaderFrozenNum++;
                                    }
                                    else if (isEqual && meshVariationMaterial.SurfaceShaderId == 2107662139) //DoTMaster
                                    {
                                        meshVariationMaterial.SurfaceShaderId = 4224110914;
                                        meshVariationMaterial.SurfaceShaderGuid = Guid.Parse("a9e6496a-5bf7-4fb9-af37-36dfc47f7cce");

                                        shaderNum++;
                                        shaderDoTMasterNum++;
                                    }
                                    else if (isEqual && meshVariationMaterial.SurfaceShaderId == 1184284987) //Petrify
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

                    App.EditorWindow.DataExplorer.RefreshItems();
                    typeof(MainWindow).InvokeMember("RefreshTabs", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, Application.Current.MainWindow, []);

                    string loggerText = " shader(s) have been replaced successfully.";

                    if (isMesh || isMaterial)
                    {
                        loggerText = " shader(s) assigned to [" + guid.ToString() + "] have been replaced successfully.";
                    }

                    App.Logger.Log(
                        shaderNum + loggerText + "\n" +
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
                if (isAll)
                {
                    FrostyMessageBox.Show("There are no non-alpha shaders to replace.", "Replace Shaders");
                }
                else if (isMesh || isMaterial)
                {
                    FrostyMessageBox.Show("There are no non-alpha shaders assigned to:" + "\n\n" + "[" + guid.ToString() + "]", "Replace Shaders");
                }
                
                return;
            }
        }

        private void FindReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            EbxAssetEntry entry = (EbxAssetEntry)AssetEntry;

            dynamic varEntriesPre = App.AssetManager.GetEbx(entry).RootObject;
            dynamic varEntryPre = varEntriesPre.Entries;

            bool isAvailable = false;
            bool isShader = true;

            int shaderNum = 0;

            uint surfaceShaderIdNum = 0;
            uint surfaceShaderIdNum2 = 0;

            Guid surfaceShaderGuid2 = Guid.Empty;

            FindAndReplaceWindow win = new FindAndReplaceWindow();
            if (win.ShowDialog() == true)
            {
                surfaceShaderIdNum = win.SurfaceShaderId;
                surfaceShaderIdNum2 = win.SurfaceShaderId2;
                surfaceShaderGuid2 = win.SurfaceShaderGuid2;
                isShader = win.isShader;
            }
            else
            {
                return;
            }

            if (varEntryPre.Count > 0)
            {
                for (int i = 0; i < varEntryPre.Count; i++)
                {
                    var entriesPre = varEntryPre[i];

                    if (isShader)
                    {
                        foreach (var meshVariationMaterialPre in entriesPre.Materials)
                        {
                            if (meshVariationMaterialPre.SurfaceShaderId == surfaceShaderIdNum)
                            {
                                isAvailable = true;
                                break;
                            }
                        }
                    }
                    else if (!isShader)
                    {
                        if (entriesPre.VariationAssetNameHash == surfaceShaderIdNum)
                        {
                            isAvailable = true;
                            break;
                        }
                        else if (entriesPre.VariationAssetNameHash != surfaceShaderIdNum)
                        {
                            continue;
                        }
                    }
                    
                }
            }

            string typeText = "shaders";

            if (isAvailable == true)
            {
                if (!isShader)
                {
                    typeText = "namehashes";
                }

                MessageBoxResult result = FrostyMessageBox.Show("Do you wish to replace " + typeText + " ?", "Find and Replace", MessageBoxButton.YesNo);
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

                            if (isShader)
                            {
                                foreach (var meshVariationMaterial in entries.Materials)
                                {
                                    if (meshVariationMaterial.SurfaceShaderId == surfaceShaderIdNum)
                                    {
                                        meshVariationMaterial.SurfaceShaderId = surfaceShaderIdNum2;
                                        meshVariationMaterial.SurfaceShaderGuid = surfaceShaderGuid2;

                                        shaderNum++;
                                    }
                                }
                            }
                            else if (!isShader)
                            {
                                if (entries.VariationAssetNameHash == surfaceShaderIdNum)
                                {
                                    entries.VariationAssetNameHash = surfaceShaderIdNum2;
                                    
                                    shaderNum++;
                                }
                                else if (entries.VariationAssetNameHash != surfaceShaderIdNum)
                                {
                                    continue;
                                }
                            }
                        }
                    }

                    InvokeOnAssetModified();

                    App.EditorWindow.DataExplorer.RefreshItems();
                    typeof(MainWindow).InvokeMember("RefreshTabs", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, Application.Current.MainWindow, []);

                    App.Logger.Log(shaderNum + " " + typeText + " have been replaced successfully.");
                }
            }

            if (!isAvailable)
            {
                FrostyMessageBox.Show("There are no " + typeText + " to replace.", "Find and Replace");
                return;
            }
        }

        private void FindHashButton_Click(object sender, RoutedEventArgs e)
        {
            EbxAssetEntry entry = (EbxAssetEntry)AssetEntry;

            dynamic varEntries = App.AssetManager.GetEbx(entry).RootObject;
            dynamic varEntry = varEntries.Entries;

            bool isAvailable = false;

            uint nameHashNum = 0;

            FindHashWindow win = new FindHashWindow();
            if (win.ShowDialog() == true)
            {
                nameHashNum = win.NameHash;
            }
            else
            {
                return;
            }

            if (varEntry.Count > 0)
            {
                for (int i = 0; i < varEntry.Count; i++)
                {
                    var entries = varEntry[i];
                
                    try
                    {
                        if (entries.VariationAssetNameHash == nameHashNum)
                        {
                            isAvailable = true;
                            App.Logger.Log(nameHashNum + " is in entry: [" + i + "]");
                        }
                        else if (entries.VariationAssetNameHash != nameHashNum)
                        {
                            continue;
                        }
                    }
                    catch
                    {
                        App.Logger.Log("Namehash is invalid.");
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
