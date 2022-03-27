﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Polycode.NostalgicPlayer.Agent.Player.OctaMed.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Original player by Teijo Kinnunen.
        ///Converted to C# by Thomas Neumann.
        ///
        ///OctaMED was chiefly used by musicians to create stand-alone works, rather than by game or demo musicians to make tunes that play in the context of a computer game or demo.
        ///
        ///Firstly, this is because the MED and OctaMED music replay routine is simply too slow to be used in a game or demo. Most trackers are optimised for speed of replay code, taking less than 3% of CPU time. MED took roughly 20% of CPU time. Secondly, and this is also  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string IDS_MED_DESCRIPTION {
            get {
                return ResourceManager.GetString("IDS_MED_DESCRIPTION", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Original player by Teijo Kinnunen.
        ///Converted to C# by Thomas Neumann.
        ///
        ///This player plays 4 channels modules created with MED v2.1 to MED v3.22. This is the same as 4 channels MMD0 files..
        /// </summary>
        internal static string IDS_MED_DESCRIPTION_AGENT1 {
            get {
                return ResourceManager.GetString("IDS_MED_DESCRIPTION_AGENT1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Original player by Teijo Kinnunen.
        ///Converted to C# by Thomas Neumann.
        ///
        ///This player plays 8 channels modules created with OctaMED v1.01 to OctaMED v2.00b. This is the same as 8 channels MMD0 files..
        /// </summary>
        internal static string IDS_MED_DESCRIPTION_AGENT2 {
            get {
                return ResourceManager.GetString("IDS_MED_DESCRIPTION_AGENT2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Original player by Teijo Kinnunen.
        ///Converted to C# by Thomas Neumann.
        ///
        ///This player plays 4-16 channels modules created with OctaMED Professional v3.00 to v4.xx. This is the same as MMD1 files..
        /// </summary>
        internal static string IDS_MED_DESCRIPTION_AGENT3 {
            get {
                return ResourceManager.GetString("IDS_MED_DESCRIPTION_AGENT3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Original player by Teijo Kinnunen.
        ///Converted to C# by Thomas Neumann.
        ///
        ///This player plays 1-64 channels modules created with OctaMED Professional v5.00 to v6.xx. This is the same as MMD2 files..
        /// </summary>
        internal static string IDS_MED_DESCRIPTION_AGENT4 {
            get {
                return ResourceManager.GetString("IDS_MED_DESCRIPTION_AGENT4", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Original player by Teijo Kinnunen.
        ///Converted to C# by Thomas Neumann.
        ///
        ///This player plays 1-64 channels modules created with OctaMED Soundstudio. This is the same as MMD3 files.
        ///
        ///Note that this player does not support mixer effects, e.g. echo yet!.
        /// </summary>
        internal static string IDS_MED_DESCRIPTION_AGENT5 {
            get {
                return ResourceManager.GetString("IDS_MED_DESCRIPTION_AGENT5", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t read the module header.
        /// </summary>
        internal static string IDS_MED_ERR_LOADING_HEADER {
            get {
                return ResourceManager.GetString("IDS_MED_ERR_LOADING_HEADER", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t read the instrument information.
        /// </summary>
        internal static string IDS_MED_ERR_LOADING_INSTRUMENTS {
            get {
                return ResourceManager.GetString("IDS_MED_ERR_LOADING_INSTRUMENTS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t read the module samples.
        /// </summary>
        internal static string IDS_MED_ERR_LOADING_SAMPLES {
            get {
                return ResourceManager.GetString("IDS_MED_ERR_LOADING_SAMPLES", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Packed samples are not supported yet.
        /// </summary>
        internal static string IDS_MED_ERR_PACKED_SAMPLES {
            get {
                return ResourceManager.GetString("IDS_MED_ERR_PACKED_SAMPLES", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An instrument has an unknown type defined. Can&apos;t load the module.
        /// </summary>
        internal static string IDS_MED_ERR_UNKNOWN_INSTRUMENT {
            get {
                return ResourceManager.GetString("IDS_MED_ERR_UNKNOWN_INSTRUMENT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A sample has an unknown type defined. Can&apos;t load the module.
        /// </summary>
        internal static string IDS_MED_ERR_UNKNOWN_SAMPLE {
            get {
                return ResourceManager.GetString("IDS_MED_ERR_UNKNOWN_SAMPLE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Song length:.
        /// </summary>
        internal static string IDS_MED_INFODESCLINE0 {
            get {
                return ResourceManager.GetString("IDS_MED_INFODESCLINE0", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Used blocks:.
        /// </summary>
        internal static string IDS_MED_INFODESCLINE1 {
            get {
                return ResourceManager.GetString("IDS_MED_INFODESCLINE1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Used samples:.
        /// </summary>
        internal static string IDS_MED_INFODESCLINE2 {
            get {
                return ResourceManager.GetString("IDS_MED_INFODESCLINE2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Current speed:.
        /// </summary>
        internal static string IDS_MED_INFODESCLINE3 {
            get {
                return ResourceManager.GetString("IDS_MED_INFODESCLINE3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Current tempo (Hz):.
        /// </summary>
        internal static string IDS_MED_INFODESCLINE4 {
            get {
                return ResourceManager.GetString("IDS_MED_INFODESCLINE4", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to OctaMED.
        /// </summary>
        internal static string IDS_MED_NAME {
            get {
                return ResourceManager.GetString("IDS_MED_NAME", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MED.
        /// </summary>
        internal static string IDS_MED_NAME_AGENT1 {
            get {
                return ResourceManager.GetString("IDS_MED_NAME_AGENT1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to OctaMED.
        /// </summary>
        internal static string IDS_MED_NAME_AGENT2 {
            get {
                return ResourceManager.GetString("IDS_MED_NAME_AGENT2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to OctaMED Professional 3.00 - 4.xx.
        /// </summary>
        internal static string IDS_MED_NAME_AGENT3 {
            get {
                return ResourceManager.GetString("IDS_MED_NAME_AGENT3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to OctaMED Professional 5.00 - 6.xx.
        /// </summary>
        internal static string IDS_MED_NAME_AGENT4 {
            get {
                return ResourceManager.GetString("IDS_MED_NAME_AGENT4", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to OctaMED Soundstudio.
        /// </summary>
        internal static string IDS_MED_NAME_AGENT5 {
            get {
                return ResourceManager.GetString("IDS_MED_NAME_AGENT5", resourceCulture);
            }
        }
    }
}
