#if UNITY_EDITOR

namespace Crosstales.TPS.Util
{
   /// <summary>Collected constants of very general utility for the asset.</summary>
   public abstract class Constants : Common.Util.BaseConstants
   {
      #region Constant variables

      /// <summary>Name of the asset.</summary>
      public const string ASSET_NAME = "Turbo Switch PRO";

      /// <summary>Short name of the asset.</summary>
      public const string ASSET_NAME_SHORT = "TPS PRO";

      /// <summary>Version of the asset.</summary>
      public const string ASSET_VERSION = "2020.2.3";

      /// <summary>Build number of the asset.</summary>
      public const int ASSET_BUILD = 20200524;

      /// <summary>Create date of the asset (YYYY, MM, DD).</summary>
      public static readonly System.DateTime ASSET_CREATED = new System.DateTime(2016, 9, 22);

      /// <summary>Change date of the asset (YYYY, MM, DD).</summary>
      public static readonly System.DateTime ASSET_CHANGED = new System.DateTime(2020, 5, 24);

      /// <summary>URL of the PRO asset in UAS.</summary>
      public const string ASSET_PRO_URL = "https://assetstore.unity.com/packages/slug/60040?aid=1011lNGT";

      /// <summary>URL of the 2019 asset in UAS.</summary>
      public const string ASSET_2019_URL = "https://www.assetstore.unity3d.com/#!/content/60040?aid=1011lNGT"; //TODO TBD!

      /// <summary>URL for update-checks of the asset</summary>
      public const string ASSET_UPDATE_CHECK_URL = "https://www.crosstales.com/media/assets/tps_versions.txt";
      //public const string ASSET_UPDATE_CHECK_URL = "https://www.crosstales.com/media/assets/test/tps_versions_test.txt";

      /// <summary>Contact to the owner of the asset.</summary>
      public const string ASSET_CONTACT = "tps@crosstales.com";

      /// <summary>URL of the asset manual.</summary>
      public const string ASSET_MANUAL_URL = "https://www.crosstales.com/media/data/assets/tps/TurboSwitch-doc.pdf";

      /// <summary>URL of the asset API.</summary>
      public const string ASSET_API_URL = "http://www.crosstales.com/en/assets/tps/api/";

      /// <summary>URL of the asset forum.</summary>
      public const string ASSET_FORUM_URL = "https://forum.unity3d.com/threads/turbo-platform-switch.434860/";

      /// <summary>URL of the asset in crosstales.</summary>
      public const string ASSET_WEB_URL = "https://www.crosstales.com/en/portfolio/tps/";

      /// <summary>URL of the promotion video of the asset (Youtube).</summary>
      public const string ASSET_VIDEO_PROMO = "https://youtu.be/rb1cqypznEg?list=PLgtonIOr6Tb41XTMeeZ836tjHlKgOO84S";

      /// <summary>URL of the tutorial video of the asset (Youtube).</summary>
      public const string ASSET_VIDEO_TUTORIAL = "https://youtu.be/J2zh0EjmrjQ?list=PLgtonIOr6Tb41XTMeeZ836tjHlKgOO84S";

      /// <summary>URL of the asset in crosstales.</summary>
      public const string ASSET_3P_ROCKTOMATE = "https://assetstore.unity.com/packages/slug/156311?aid=1011lNGT";

      // Keys for the configuration of the asset
      public const string KEY_VCS = "CT_CFG_VCS";

      private const string KEY_PREFIX = "TPS_CFG_";
      public const string KEY_CUSTOM_PATH_CACHE = KEY_PREFIX + "CUSTOM_PATH_CACHE";

      public const string KEY_PATH_CACHE = KEY_PREFIX + "PATH_CACHE";

      //public const string KEY_VCS = KEY_PREFIX + "VCS";
      public const string KEY_USE_LEGACY = KEY_PREFIX + "USE_LEGACY";
      public const string KEY_BATCHMODE = KEY_PREFIX + "BATCHMODE";
      public const string KEY_QUIT = KEY_PREFIX + "QUIT";
      public const string KEY_NO_GRAPHICS = KEY_PREFIX + "NO_GRAPHICS";
      public const string KEY_EXECUTE_METHOD_PRE_SWITCH = KEY_PREFIX + "EXECUTE_METHOD_PRE_SWITCH";
      public const string KEY_EXECUTE_METHOD = KEY_PREFIX + "EXECUTE_METHOD";
      public const string KEY_COPY_ASSETS = KEY_PREFIX + "COPY_ASSETS";
      public const string KEY_COPY_LIBRARY = KEY_PREFIX + "COPY_LIBRARY";
      public const string KEY_COPY_SETTINGS = KEY_PREFIX + "COPY_SETTINGS";
      public const string KEY_DELETE_LOCKFILE = KEY_PREFIX + "DELETE_LOCKFILE";
      public const string KEY_CONFIRM_SWITCH = KEY_PREFIX + "CONFIRM_SWITCH";
      public const string KEY_DEBUG = KEY_PREFIX + "DEBUG";
      public const string KEY_UPDATE_CHECK = KEY_PREFIX + "UPDATE_CHECK";
      //public const string KEY_UPDATE_OPEN_UAS = KEY_PREFIX + "UPDATE_OPEN_UAS";
      public const string KEY_COMPILE_DEFINES = Util.Constants.KEY_PREFIX + "COMPILE_DEFINES";

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

      public const string KEY_SHOW_COLUMN_PLATFORM = KEY_PREFIX + "SHOW_COLUMN_PLATFORM";
      public const string KEY_SHOW_COLUMN_ARCHITECTURE = KEY_PREFIX + "SHOW_COLUMN_ARCHITECTURE";
      public const string KEY_SHOW_COLUMN_TEXTURE = KEY_PREFIX + "SHOW_COLUMN_TEXTURE";
      public const string KEY_SHOW_COLUMN_CACHE = KEY_PREFIX + "SHOW_COLUMN_CACHE";

      public const string KEY_SWITCH_DATE = KEY_PREFIX + "SWITCH_DATE";
      public const string KEY_SETUP_DATE = KEY_PREFIX + "SETUP_DATE";

      public const string KEY_UPDATE_DATE = KEY_PREFIX + "UPDATE_DATE";

      public const string KEY_LAUNCH = KEY_PREFIX + "LAUNCH";

      public const string CACHE_DIRNAME = "TPS_cache";

      // Default values
      public const string DEFAULT_ASSET_PATH = "/Plugins/crosstales/TurboSwitch/";
      public static readonly string DEFAULT_PATH_CACHE = Helper.ValidatePath(APPLICATION_PATH + CACHE_DIRNAME);
      public const bool DEFAULT_CUSTOM_PATH_CACHE = false;
      public const int DEFAULT_VCS = 1; //git
      public const bool DEFAULT_USE_LEGACY = false;
      public const bool DEFAULT_BATCHMODE = false;
      public const bool DEFAULT_QUIT = true;
      public const bool DEFAULT_NO_GRAPHICS = false;
#if UNITY_2018_1_OR_NEWER && !UNITY_2019_1_OR_NEWER && UNITY_EDITOR_OSX
        public const bool DEFAULT_DELETE_LOCKFILE = true;
#else
      public const bool DEFAULT_DELETE_LOCKFILE = false;
#endif
      public const bool DEFAULT_COPY_ASSETS = false;
      public const bool DEFAULT_COPY_LIBRARY = true;
      public const bool DEFAULT_COPY_SETTINGS = false;
      public const bool DEFAULT_CONFIRM_SWITCH = true;
      public const bool DEFAULT_UPDATE_CHECK = false;
      public const bool DEFAULT_COMPILE_DEFINES = true;
      /*
      public const bool DEFAULT_PLATFORM_WINDOWS = false;
      public const bool DEFAULT_PLATFORM_MAC = false;
      public const bool DEFAULT_PLATFORM_LINUX = false;
      public const bool DEFAULT_PLATFORM_ANDROID = false;
      public const bool DEFAULT_PLATFORM_IOS = false;
      public const bool DEFAULT_PLATFORM_WSA = false;
      public const bool DEFAULT_PLATFORM_WEBGL = false;
      public const bool DEFAULT_PLATFORM_TVOS = false;
      public const bool DEFAULT_PLATFORM_PS4 = false;
      public const bool DEFAULT_PLATFORM_XBOXONE = false;
      public const bool DEFAULT_PLATFORM_SWITCH = false;
#if !UNITY_2018_2_OR_NEWER
      public const bool DEFAULT_PLATFORM_WIIU = false;
      public const bool DEFAULT_PLATFORM_3DS = false;
      public const bool DEFAULT_PLATFORM_PSP2 = false;
#endif
      */

      public const int DEFAULT_ARCH_WINDOWS = 1;

      //public const int DEFAULT_ARCH_MAC = 1;
      public const int DEFAULT_ARCH_LINUX = 1;

      public const int DEFAULT_TEX_ANDROID = 0;

      public const bool DEFAULT_SHOW_COLUMN_PLATFORM = true;
      public const bool DEFAULT_SHOW_COLUMN_PLATFORM_LOGO = false;
      public const bool DEFAULT_SHOW_COLUMN_ARCHITECTURE = true;
      public const bool DEFAULT_SHOW_COLUMN_TEXTURE = false;
      public const bool DEFAULT_SHOW_COLUMN_CACHE = true;

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
         get { return "60040"; }
      }

      /// <summary>Returns the UID of the asset.</summary>
      /// <returns>The UID of the asset.</returns>
      public static System.Guid ASSET_UID
      {
         get { return new System.Guid("2d03d693-219a-4fa4-a9b0-83e5a59ebe01"); }
      }

      #endregion
   }
}
#endif
// © 2016-2020 crosstales LLC (https://www.crosstales.com)