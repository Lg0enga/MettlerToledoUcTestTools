using System;
using System.Collections.Generic;
using System.Security.Principal;
using UcScaleTool.Native;

namespace UcScaleTool.Hardware
{
    public class DetectionResult
    {
        public BackendKind Recommended = BackendKind.Auto;
        public List<string> JidaBoardNames = new List<string>();
        public UspBoardInfo UspInfo;
        public List<string> Lines = new List<string>();

        public bool Found { get { return Recommended != BackendKind.Auto; } }

        public string Summary
        {
            get
            {
                if (Recommended == BackendKind.Jida)
                    return "JIDA - " + (JidaBoardNames.Count > 0 ? JidaBoardNames[0] : "?");
                if (Recommended == BackendKind.UspEpc)
                    return "UspEpc - " + (UspInfo != null ? UspInfo.Platform : "?");
                return "Geen board gevonden";
            }
        }
    }

    /// <summary>
    /// Finds out which board support library works on this scale (JIDA or UspEpc) and
    /// what the JIDA board name is. Only opens/queries the board, never touches I2C devices.
    /// </summary>
    public static class BoardDetector
    {
        public static DetectionResult Detect()
        {
            DetectionResult result = new DetectionResult();
            List<string> lines = result.Lines;

            lines.Add("Windows: " + Environment.OSVersion.VersionString);
            lines.Add(".NET runtime: " + Environment.Version + ", proces " + (IntPtr.Size * 8) + "-bit");
            lines.Add("Administrator: " + (IsAdministrator() ? "ja" : "NEE - start de tool als administrator"));
            lines.Add("");

            DetectJida(result);
            lines.Add("");
            DetectUspEpc(result);
            lines.Add("");

            if (result.JidaBoardNames.Count > 0)
                result.Recommended = BackendKind.Jida;
            else if (result.UspInfo != null)
                result.Recommended = BackendKind.UspEpc;

            lines.Add("Conclusie: " + (result.Found
                ? "gebruik " + result.Summary
                : "geen werkende JIDA- of UspEpc-driver gevonden"));
            return result;
        }

        private static void DetectJida(DetectionResult result)
        {
            List<string> lines = result.Lines;
            lines.Add("[JIDA - jida.dll]");
            bool initialized = false;
            try
            {
                int version = JidaNative.JidaDllGetVersion();
                lines.Add(string.Format("  jida.dll versie {0}.{1}", (version >> 16) & 0xFFFF, version & 0xFFFF));

                initialized = JidaNative.JidaDllInitialize();
                if (!initialized || !JidaNative.JidaDllIsAvailable())
                {
                    lines.Add("  Driver niet beschikbaar (geen Kontron board, of geen administrator)");
                    return;
                }

                result.JidaBoardNames = JidaBackend.ListBoardNames();
                lines.Add("  Boards gevonden: " + result.JidaBoardNames.Count);
                foreach (string name in result.JidaBoardNames)
                    lines.Add("    Board name: " + name);
            }
            catch (DllNotFoundException)
            {
                lines.Add("  jida.dll niet gevonden");
            }
            catch (BadImageFormatException)
            {
                lines.Add("  jida.dll kan niet geladen worden (32/64-bit verschil)");
            }
            catch (Exception ex)
            {
                lines.Add("  Fout: " + ex.Message);
            }
            finally
            {
                if (initialized)
                    SafeUninitializeJida();
            }
        }

        private static void SafeUninitializeJida()
        {
            try
            {
                JidaNative.JidaDllUninitialize();
            }
            catch (Exception)
            {
            }
        }

        private static void DetectUspEpc(DetectionResult result)
        {
            List<string> lines = result.Lines;
            lines.Add("[UspEpc - UspEpc.dll]");
            bool open = false;
            try
            {
                open = UspEpcNative.UspOpen();
                if (!open)
                {
                    lines.Add("  UspOpen mislukt: " + UspEpcBackend.LastError());
                    return;
                }

                uint version = 0;
                if (UspEpcNative.UspGetDllVersion(ref version))
                    lines.Add(string.Format("  UspEpc.dll versie {0:X}", version));

                result.UspInfo = UspEpcBackend.ReadBoardInfo();
                if (result.UspInfo == null)
                {
                    lines.Add("  Driver werkt, maar board info niet beschikbaar");
                    result.UspInfo = new UspBoardInfo { Platform = "<unknown>" };
                    return;
                }

                lines.Add("  Platform: " + result.UspInfo.Platform);
                lines.Add("  BIOS: " + result.UspInfo.BiosVersion);
                lines.Add("  HW revisie: " + result.UspInfo.HardwareRevision);
                lines.Add("  Serienummer: " + result.UspInfo.SerialNumber);
            }
            catch (DllNotFoundException)
            {
                lines.Add("  UspEpc.dll niet gevonden");
            }
            catch (BadImageFormatException)
            {
                lines.Add("  UspEpc.dll kan niet geladen worden (32/64-bit verschil)");
            }
            catch (Exception ex)
            {
                lines.Add("  Fout: " + ex.Message);
            }
            finally
            {
                if (open)
                {
                    try { UspEpcNative.UspClose(); }
                    catch (Exception) { }
                }
            }
        }

        public static bool IsAdministrator()
        {
            try
            {
                WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Opens the backend selected in the settings, or the detected one for "Auto".
        /// Throws <see cref="BoardException"/> when nothing could be opened.
        /// </summary>
        public static IBoardBackend OpenBackend(AppSettings settings, List<string> log)
        {
            BackendKind kind = settings.Backend;
            if (kind == BackendKind.Auto)
            {
                DetectionResult detection = Detect();
                if (!detection.Found)
                    throw new BoardException("Geen JIDA- of UspEpc-board gevonden (zie tab Hardware > Detecteer)");
                kind = detection.Recommended;
                log.Add("Automatisch gekozen: " + detection.Summary);
            }

            if (kind == BackendKind.Jida)
                return JidaBackend.Open(settings.JidaBoardName, log);
            return UspEpcBackend.Open();
        }
    }
}
