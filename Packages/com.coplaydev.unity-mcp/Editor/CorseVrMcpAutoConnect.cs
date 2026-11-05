using System;
using MCPForUnity.Editor.Constants;
using MCPForUnity.Editor.Services;
using UnityEditor;
using UnityEngine;

namespace CorseVR.EditorSetup
{
    // Starts the local MCP bridge when this project opens.
    [InitializeOnLoad]
    internal static class CorseVrMcpAutoConnect
    {
        private static bool starting;
        private static double retryAfter;

        static CorseVrMcpAutoConnect()
        {
            EditorPrefs.SetBool(EditorPrefKeys.AutoRegisterEnabled, false);
            EditorApplication.update += WaitUntilReady;
        }

        private static void WaitUntilReady()
        {
            if (starting || EditorApplication.isCompiling || EditorApplication.isUpdating
                || EditorApplication.timeSinceStartup < retryAfter)
                return;
            StartBridge();
        }

        private static async void StartBridge()
        {
            starting = true;
            try
            {
                EditorPrefs.SetString(EditorPrefKeys.UvxPathOverride,
                    @"C:\Users\jeanj\Documents\Codex\2026-10-22\je-x20\work\unity-mcp-setup\tools\uvx.exe");
                EditorPrefs.SetBool(EditorPrefKeys.SetupCompleted, true);
                EditorConfigurationCache.Instance.SetUseHttpTransport(false);
                var bridge = MCPServiceLocator.Bridge;
                if (!bridge.IsRunning && !await bridge.StartAsync())
                    throw new InvalidOperationException("MCP bridge did not start.");
                EditorApplication.update -= WaitUntilReady;
                Debug.Log("[CorseVR] MCP bridge ready for Codex (local stdio).");
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[CorseVR] MCP connection will retry: " + exception.Message);
                retryAfter = EditorApplication.timeSinceStartup + 15;
            }
            finally { starting = false; }
        }
    }
}