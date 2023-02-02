﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Can&apos;t load external sample with name {0}. Make sure you have an Instruments, Synthsounds and Hybrids folders in the same folder as the module, which have all needed samples and synthsounds.
        /// </summary>
        internal static string IDS_ERR_LOADING_EXTERNAL_SAMPLE {
            get {
                return ResourceManager.GetString("IDS_ERR_LOADING_EXTERNAL_SAMPLE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t read the module header.
        /// </summary>
        internal static string IDS_ERR_LOADING_HEADER {
            get {
                return ResourceManager.GetString("IDS_ERR_LOADING_HEADER", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t read the instrument information.
        /// </summary>
        internal static string IDS_ERR_LOADING_INSTRUMENTS {
            get {
                return ResourceManager.GetString("IDS_ERR_LOADING_INSTRUMENTS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t read the pattern information.
        /// </summary>
        internal static string IDS_ERR_LOADING_PATTERNS {
            get {
                return ResourceManager.GetString("IDS_ERR_LOADING_PATTERNS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t read the sample information.
        /// </summary>
        internal static string IDS_ERR_LOADING_SAMPLEINFO {
            get {
                return ResourceManager.GetString("IDS_ERR_LOADING_SAMPLEINFO", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t read the sample data.
        /// </summary>
        internal static string IDS_ERR_LOADING_SAMPLES {
            get {
                return ResourceManager.GetString("IDS_ERR_LOADING_SAMPLES", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t read the track information.
        /// </summary>
        internal static string IDS_ERR_LOADING_TRACKS {
            get {
                return ResourceManager.GetString("IDS_ERR_LOADING_TRACKS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Written by Thomas Neumann.
        ///
        ///Converts different module formats (mostly Amiga formats) to another format NostalgicPlayer understands.
        ///
        ///Current version can convert these formats:
        ///
        ///Fred Editor (Final) -&gt; Fred Editor
        ///Future Composer 1.0 - 1.3 -&gt; Future Composer 1.4
        ///SoundFX 1-x &gt; SoundFX 2.0
        ///MED 2.10 MED4 -&gt; MED 2.10 MMD0.
        /// </summary>
        internal static string IDS_MODCONV_DESCRIPTION {
            get {
                return ResourceManager.GetString("IDS_MODCONV_DESCRIPTION", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Original player by SuperSero.
        ///Converted to C# by Thomas Neumann.
        ///
        ///This is an old format from the Amiga. It was developed by Jochen Hippel and he used it to compose some of his game music, such as &quot;Rings of Medusa&quot; and &quot;Shaolin&quot;. A lot of cracker intros used this format in the early days, so you can find a lot of modules in this format on the internet..
        /// </summary>
        internal static string IDS_MODCONV_DESCRIPTION_AGENT1 {
            get {
                return ResourceManager.GetString("IDS_MODCONV_DESCRIPTION_AGENT1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Original player by Linel Software.
        ///Converted to C# by Thomas Neumann.
        ///
        ///The SoundFX file format is like SoundTracker. It only have a few effects, but it also have some special pattern commands, which SoundTracker doesn&apos;t have..
        /// </summary>
        internal static string IDS_MODCONV_DESCRIPTION_AGENT2 {
            get {
                return ResourceManager.GetString("IDS_MODCONV_DESCRIPTION_AGENT2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Original player by Frederic Hahn &amp; Julien Clermonte.
        ///Converted to C# by Thomas Neumann.
        ///
        ///Previously this music format has been known as &apos;Fredmon&apos; or &apos;Fred Monitor&apos; which is wrong.
        ///
        ///The modules contain the player in 68000 assembler in the beginning of the files, but this player will extract the music data and only use that..
        /// </summary>
        internal static string IDS_MODCONV_DESCRIPTION_AGENT3 {
            get {
                return ResourceManager.GetString("IDS_MODCONV_DESCRIPTION_AGENT3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Original player by Teijo Kinnunen.
        ///Converted to C# by Thomas Neumann.
        ///
        ///This player plays modules created with MED v2.10 to MED v3.22. This format have both a real module format, where song data and samples are combined into a single file and song+sample format. The player can load both types of files. For song+sample format, you need to have the samples beside the song files. The player will load the samples from a folder named &quot;Instruments&quot;, synth sounds from a folder named &quot;Synthsounds&quot; and hybrid samp [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string IDS_MODCONV_DESCRIPTION_AGENT4 {
            get {
                return ResourceManager.GetString("IDS_MODCONV_DESCRIPTION_AGENT4", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Module Converter.
        /// </summary>
        internal static string IDS_MODCONV_NAME {
            get {
                return ResourceManager.GetString("IDS_MODCONV_NAME", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Future Composer 1.0 - 1.3.
        /// </summary>
        internal static string IDS_MODCONV_NAME_AGENT1 {
            get {
                return ResourceManager.GetString("IDS_MODCONV_NAME_AGENT1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SoundFX 1.x.
        /// </summary>
        internal static string IDS_MODCONV_NAME_AGENT2 {
            get {
                return ResourceManager.GetString("IDS_MODCONV_NAME_AGENT2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Fred Editor (Final).
        /// </summary>
        internal static string IDS_MODCONV_NAME_AGENT3 {
            get {
                return ResourceManager.GetString("IDS_MODCONV_NAME_AGENT3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MED 2.10 MED4.
        /// </summary>
        internal static string IDS_MODCONV_NAME_AGENT4 {
            get {
                return ResourceManager.GetString("IDS_MODCONV_NAME_AGENT4", resourceCulture);
            }
        }
    }
}
