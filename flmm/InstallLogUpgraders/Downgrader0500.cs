﻿using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using Fomm.PackageManager.ModInstallLog;

namespace Fomm.InstallLogUpgraders
{
  /// <summary>
  ///   Upgrades the Install Log to the current version from version 0.1.1.0.
  /// </summary>
  internal class Downgrader0500 : Upgrader
  {
    /// <summary>
    ///   Downgrades the Install Log to the current version from version 0.5.0.0.
    /// </summary>
    /// <remarks>
    ///   NMM pointlessly changed the XML format in a way incompatible with FOMM
    ///   without a lot of effort.  Since it will painlessly (and silently) update
    ///   this file to 0.5.0.0 if needed, we will (not silently) bring it back
    ///   down to 0.2.0.0 if needed.
    /// </remarks>
    protected override void DoUpgrade()
    {
      /*
      Converts the modList entries from
        <mod key="x2hrojjw" path="Dummy Mod: ORIGINAL_VALUES">
          <version machineVersion="0">0</version>
          <name>ORIGINAL_VALUES</name>
          <installDate>03/30/2014 00:00:00</installDate>
        </mod>
      TO
        <mod name="the mod configuration menu" key="qdq0aqvc">
          <version machineVersion="1.5">1.5</version>
        </mod>
      *** This copies the name element value into the name property of the mod tag and deletes the 'path' property,
      *** then deletes the name and installdate elements.
       

      Converts the dataFiles entries from 
        <file path="data\test.esp">
          <installingMods>
            <mod key="qlyv0rki" />
          </installingMods>
        </file>
      TO
        <file path="test.esp">
          <installingMods>
            <mod key="qlyv0rki" />
          </installingMods>
        </file>
       *** This is simply removing 'data\' from the path in each file element.
       */

      // Load the document
      var doc = XDocument.Load(InstallLog.Current.InstallLogPath);
      var root = doc.Element("installLog");
      var modlist = root.Element("modList");
      var datafiles = root.Element("dataFiles");

      // Set current version
      root.SetAttributeValue("fileVersion", InstallLog.CURRENT_VERSION);

      // Reset datafile entries
      foreach (var el in datafiles.Descendants("file"))
      {
        // Check to see that data is set as the first path element.  If not, throw an exception
        // indicating that the user should disable that mod with NMM and try again -- FOMM cannot
        // presently uninstall/deactivate mods that operate in the game folder above the data
        // folder.

        var strPath = el.Attribute("path").Value.ToLowerInvariant();
        var strData = "data" + Path.DirectorySeparatorChar;
        strPath = strPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        if (strPath.IndexOf(strData) == 0)
        {
          strPath = strPath.Substring(strData.Length);
          el.SetAttributeValue("path", strPath);
        }
        else
        {
          throw new Exception(
            "NMM or another mod manager installed the file " + strPath + " which FOMM cannot uninstall.\n" +
            "The upgrade cannot proceed.\nPlease deactivate the mod which installed that file in NMM and try again."
            );
        }
      }

      // Reset mod entries
      foreach (var el in modlist.Descendants("mod"))
      {
        var szPath = el.Attribute("path")?.Value;
        if (szPath == null)
        {
          el.Remove();
          continue;
        }

        if (szPath.StartsWith("Dummy Mod: "))
        {
          szPath = szPath.Substring(11, szPath.Length - 11);
        }
        else if (szPath.EndsWith(".fomod", true, CultureInfo.CurrentCulture))
        {
          szPath = szPath.Substring(0, szPath.Length - 6).ToLower();
        }

        if (szPath.Equals("ORIGINAL_VALUE"))
        {
          szPath += "S";
        }

        if (szPath.Equals("MOD_MANAGER_VALUE"))
        {
          szPath = InstallLog.FOMM;
        }

        // Set name attribute equal to name element value
        el.SetAttributeValue("name", szPath);

        // Remove path attribute
        el.SetAttributeValue("path", null);

        // Remove name element
        el.Element("name")?.Remove();

        // Remove installdate element
        el.Element("installDate")?.Remove();
      }

      doc.Save(InstallLog.Current.InstallLogPath);
    }
  }
}