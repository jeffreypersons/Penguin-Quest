#if UNITY_EDITOR
using UnityEngine;

namespace Crosstales.TPB.Util
{
   /// <summary>Collected constants of very general utility for the asset.</summary>
   public abstract class Constants : Common.Util.BaseConstants
   {
      #region Constant variables

      /// <summary>Name of the asset.</summary>
      public const string ASSET_NAME = "Turbo Builder PRO";

      /// <summary>Short name of the asset.</summary>
      public const string ASSET_NAME_SHORT = "TPB PRO";

      /// <summary>Version of the asset.</summary>
      public const string ASSET_VERSION = "2020.2.1";

      /// <summary>Build number of the asset.</summary>
      public const int ASSET_BUILD = 20200415;

      /// <summary>Create date of the asset (YYYY, MM, DD).</summary>
      public static readonly System.DateTime ASSET_CREATED = new System.DateTime(2018, 3, 4);

      /// <summary>Change date of the asset (YYYY, MM, DD).</summary>
      public static readonly System.DateTime ASSET_CHANGED = new System.DateTime(2020, 4, 15);

      /// <summary>URL of the PRO asset in UAS.</summary>
      public const string ASSET_PRO_URL = "https://www.assetstore.unity3d.com/#!/content/98714?aid=1011lNGT";

      /// <summary>URL for update-checks of the asset</summary>
      public const string ASSET_UPDATE_CHECK_URL = "https://www.crosstales.com/media/assets/tpb_versions.txt";
      //public const string ASSET_UPDATE_CHECK_URL = "https://www.crosstales.com/media/assets/test/tpb_versions_test.txt";

      /// <summary>Contact to the owner of the asset.</summary>
      public const string ASSET_CONTACT = "tpb@crosstales.com";

      /// <summary>URL of the asset manual.</summary>
      public const string ASSET_MANUAL_URL = "https://www.crosstales.com/media/data/assets/TurboBuilder/TurboBuilder-doc.pdf";

      /// <summary>URL of the asset API.</summary>
      public const string ASSET_API_URL = "https://www.crosstales.com/media/data/assets/TurboBuilder/api/";

      /// <summary>URL of the asset forum.</summary>
      public const string ASSET_FORUM_URL = "https://forum.unity.com/threads/turbo-builder-fast-build-solution.644425/";

      /// <summary>URL of the asset in crosstales.</summary>
      public const string ASSET_3P_ROCKTOMATE = "https://assetstore.unity.com/packages/slug/156311?aid=1011lNGT";

/*
        /// <summary>URL of the promotion video of the asset (Youtube).</summary>
        public const string ASSET_VIDEO_PROMO = "https://youtu.be/rb1cqypznEg?list=PLgtonIOr6Tb41XTMeeZ836tjHlKgOO84S"; //TODO change

        /// <summary>URL of the tutorial video of the asset (Youtube).</summary>
        public const string ASSET_VIDEO_TUTORIAL = "https://youtu.be/J2zh0EjmrjQ?list=PLgtonIOr6Tb41XTMeeZ836tjHlKgOO84S"; //TODO change
*/

      /// <summary>URL of the asset in crosstales.</summary>
      public const string ASSET_WEB_URL = "https://www.crosstales.com/en/portfolio/TurboBuilder/";

      // Keys for the configuration of the asset
      public const string KEY_VCS = "CT_CFG_VCS";

      private const string KEY_PREFIX = "TPB_CFG_";
      public const string KEY_CUSTOM_PATH_BUILD = KEY_PREFIX + "CUSTOM_PATH_BUILD";

      public const string KEY_PATH_BUILD = KEY_PREFIX + "PATH_BUILD";

      //public const string KEY_VCS = KEY_PREFIX + "VCS";
      public const string KEY_ADD_NAME_TO_PATH = KEY_PREFIX + "ADD_NAME_TO_PATH";

      public const string KEY_ADD_DATE_TO_PATH = KEY_PREFIX + "ADD_DATE_TO_PATH";

      //public const string KEY_BATCHMODE = KEY_PREFIX + "BATCHMODE";
      //public const string KEY_QUIT = KEY_PREFIX + "QUIT";
      //public const string KEY_NO_GRAPHICS = KEY_PREFIX + "NO_GRAPHICS";
      public const string KEY_EXECUTE_METHOD_PRE_BUILDING = KEY_PREFIX + "EXECUTE_METHOD_PRE_BUILDING";
      public const string KEY_EXECUTE_METHOD_POST_BUILDING = KEY_PREFIX + "EXECUTE_METHOD_POST_BUILDING";
      public const string KEY_EXECUTE_METHOD_PRE_BUILD = KEY_PREFIX + "EXECUTE_METHOD_PRE_BUILD";
      public const string KEY_EXECUTE_METHOD_POST_BUILD = KEY_PREFIX + "EXECUTE_METHOD_POST_BUILD";
      public const string KEY_DELETE_LOCKFILE = KEY_PREFIX + "DELETE_LOCKFILE";
      public const string KEY_CONFIRM_BUILD = KEY_PREFIX + "CONFIRM_BUILD";
      public const string KEY_DEBUG = KEY_PREFIX + "DEBUG";
      public const string KEY_UPDATE_CHECK = KEY_PREFIX + "UPDATE_CHECK";

#if CT_TPS
        public const string KEY_TARGETS = KEY_PREFIX + "TARGETS";
#endif
      public const string KEY_BATCHMODE = KEY_PREFIX + "BATCHMODE";

      public const string KEY_PLATFORM_WINDOWS = KEY_PREFIX + "PLATFORM_WINDOWS";
      public const string KEY_PLATFORM_MAC = KEY_PREFIX + "PLATFORM_MAC";
      public const string KEY_PLATFORM_LINUX = KEY_PREFIX + "PLATFORM_LINUX";
      public const string KEY_PLATFORM_ANDROID = KEY_PREFIX + "PLATFORM_ANDROID";
      public const string KEY_PLATFORM_IOS = KEY_PREFIX + "PLATFORM_IOS";
      public const string KEY_PLATFORM_WSA = KEY_PREFIX + "PLATFORM_WSA";
      public const string KEY_PLATFORM_WEBGL = KEY_PREFIX + "PLATFORM_WEBGL";
      public const string KEY_PLATFORM_TVOS = KEY_PREFIX + "PLATFORM_TVOS";
      public const string KEY_PLATFORM_PS4 = KEY_PREFIX + "PLATFORM_PS4";
      public const string KEY_PLATFORM_XBOXONE = KEY_PREFIX + "PLATFORM_XBOXONE";
      public const string KEY_PLATFORM_SWITCH = KEY_PREFIX + "PLATFORM_SWITCH";
#if !UNITY_2018_2_OR_NEWER
      public const string KEY_PLATFORM_WIIU = KEY_PREFIX + "PLATFORM_WIIU";
      public const string KEY_PLATFORM_3DS = KEY_PREFIX + "PLATFORM_3DS";
      public const string KEY_PLATFORM_PSP2 = KEY_PREFIX + "PLATFORM_PSP2";
#endif
      public const string KEY_ARCH_WINDOWS = KEY_PREFIX + "ARCH_WINDOWS";

      //public const string KEY_ARCH_MAC = KEY_PREFIX + "ARCH_MAC";
      public const string KEY_ARCH_LINUX = KEY_PREFIX + "ARCH_LINUX";

      public const string KEY_TEX_ANDROID = KEY_PREFIX + "TEX_ANDROID";

      public const string KEY_BO_SHOW_BUILT_PLAYER = KEY_PREFIX + "BO_SHOW_BUILT_PLAYER";
      public const string KEY_BO_DEVELOPMENT = KEY_PREFIX + "BO_DEVELOPMENT";
      public const string KEY_BO_PROFILER = KEY_PREFIX + "BO_PROFILER";
      public const string KEY_BO_SCRIPTDEBUG = KEY_PREFIX + "BO_SCRIPTDEBUG";

      public const string KEY_SHOW_COLUMN_PLATFORM = KEY_PREFIX + "SHOW_COLUMN_PLATFORM";

      public const string KEY_SHOW_COLUMN_ARCHITECTURE = KEY_PREFIX + "SHOW_COLUMN_ARCHITECTURE";
      //public const string KEY_SHOW_COLUMN_TEXTURE = KEY_PREFIX + "SHOW_COLUMN_TEXTURE";

      public const string KEY_UPDATE_DATE = KEY_PREFIX + "UPDATE_DATE";

      public const string KEY_LAUNCH = KEY_PREFIX + "LAUNCH";

      //public const string BUILD_DIRNAME = "TPB";
      public const string BUILD_DIRNAME = "Builds";

      /// <summary>Application path.</summary>
      public static readonly string PATH = Helper.ValidatePath(Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/') + 1));

      // Default values
      public const string DEFAULT_ASSET_PATH = "/Plugins/crosstales/TurboBuilder/";
      public static readonly string DEFAULT_PATH_CACHE = Helper.ValidatePath(PATH + BUILD_DIRNAME);
      public const bool DEFAULT_CUSTOM_PATH_BUILD = false;
      public const int DEFAULT_VCS = 1; //git
      public const bool DEFAULT_ADD_NAME_TO_PATH = false;
      public const bool DEFAULT_ADD_DATE_TO_PATH = false;
      //public const bool DEFAULT_BATCHMODE = false;
      //public const bool DEFAULT_QUIT = true;
      //public const bool DEFAULT_NO_GRAPHICS = false;
#if UNITY_2018_1_OR_NEWER && !UNITY_2019_1_OR_NEWER && UNITY_EDITOR_OSX
        public const bool DEFAULT_DELETE_LOCKFILE = true;
#else
      public const bool DEFAULT_DELETE_LOCKFILE = false;
#endif
      public const bool DEFAULT_CONFIRM_BUILD = true;
      public const bool DEFAULT_UPDATE_CHECK = false;

      public const int DEFAULT_ARCH_WINDOWS = 1;

      //public const int DEFAULT_ARCH_MAC = 1;
      public const int DEFAULT_ARCH_LINUX = 1;

      public const int DEFAULT_TEX_ANDROID = 0;

      public const bool DEFAULT_BO_SHOW_BUILT_PLAYER = false;
      public const bool DEFAULT_BO_DEVELOPMENT = false;
      public const bool DEFAULT_BO_PROFILER = false;
      public const bool DEFAULT_BO_SCRIPTDEBUG = false;

      public const bool DEFAULT_SHOW_COLUMN_PLATFORM = true;
      public const bool DEFAULT_SHOW_COLUMN_PLATFORM_LOGO = false;

      public const bool DEFAULT_SHOW_COLUMN_ARCHITECTURE = true;
      //public const bool DEFAULT_SHOW_COLUMN_TEXTURE = false;

      public const string TEXT_NO_BUILDS = "no builds";

      #endregion


      #region Properties

      /// <summary>Returns the URL of the asset in UAS.</summary>
      /// <returns>The URL of the asset in UAS.</returns>
      public static string ASSET_URL
      {
         get { return ASSET_PRO_URL; }
      }


      /// <summary>Returns the ID of the asset in UAS.</summary>
      /// <returns>The ID of the asset in UAS.</returns>
      public static string ASSET_ID
      {
         get { return "98714"; }
      }

      /// <summary>Returns the UID of the asset.</summary>
      /// <returns>The UID of the asset.</returns>
      public static System.Guid ASSET_UID
      {
         get { return new System.Guid("afef0ff3-ba0b-4e0e-9aa7-3d5fabf279b9"); }
      }

      #endregion
   }
}
#endif
// © 2018-2020 crosstales LLC (https://www.crosstales.com)