using System.Runtime.InteropServices;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

namespace NodeEditor.Demo;

internal static class Program
{
    // Candidate paths for Arial on Windows.  We fall back through the list until
    // one resolves so the demo works for both per-user and system font installs.
    private static readonly string[] ArialCandidates =
    [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf"),
        @"C:\Windows\Fonts\arial.ttf",
    ];

    [STAThread]
    private static void Main()
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.VSyncHint);
        Raylib.InitWindow(1600, 1000, "NodeEditor Demo");
        Raylib.SetExitKey(KeyboardKey.Null);
        Raylib.SetTargetFPS(60);

        rlImGui.Setup(darkTheme: true, enableDocking: true);

        // ── Font pipeline ────────────────────────────────────────────────────
        // Fonts must be added to the atlas BEFORE it is baked and uploaded to
        // the GPU.  rlImGui.Setup already baked a default atlas, so we rebuild
        // it here with the additional TrueType faces.
        //
        // The first font added becomes the implicit default for all ImGui widgets
        // (menus, panels, etc.).  The scaled canvas fonts are only applied via
        // explicit PushFont/PopFont blocks inside NodeRenderer and PinRenderer.
        var io = ImGui.GetIO();

        // Always keep the built-in pixel font as the global default so that all
        // structural UI (menus, toolbars, panels) continues to use it unchanged.
        io.Fonts.AddFontDefault();

        // Resolve Arial and bake it at three discrete sizes for crisp text
        // across the typical canvas zoom range (0.5× – 3×).
        string? arialPath = ArialCandidates.FirstOrDefault(File.Exists);

        var fonts = new Dictionary<float, nint>();

        if (arialPath is not null)
        {
            // Include the default Latin + extended Latin + Cyrillic ranges so
            // node titles and pin labels render correctly for a wide UTF-8 corpus.
            nint glyphRanges = io.Fonts.GetGlyphRangesDefault();

            unsafe
            {
                fonts[16f] = (nint)io.Fonts.AddFontFromFileTTF(arialPath, 16f, null, glyphRanges).NativePtr;
                fonts[24f] = (nint)io.Fonts.AddFontFromFileTTF(arialPath, 24f, null, glyphRanges).NativePtr;
                fonts[32f] = (nint)io.Fonts.AddFontFromFileTTF(arialPath, 32f, null, glyphRanges).NativePtr;
            }
        }
        // If Arial is absent the dictionary stays empty and the theme falls
        // back to the default ImGui font automatically.

        // Re-upload the augmented atlas to the GPU.
        rlImGui.ReloadFonts();
        // ────────────────────────────────────────────────────────────────────

        var demo = new DemoShell(fonts);
        double lastTime = Raylib.GetTime();

        while (!Raylib.WindowShouldClose())
        {
            double now     = Raylib.GetTime();
            double elapsed = now - lastTime;
            lastTime = now;

            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(26, 26, 26, 255));

            rlImGui.Begin();
            demo.Frame(elapsed);
            rlImGui.End();

            Raylib.EndDrawing();
        }

        rlImGui.Shutdown();
        Raylib.CloseWindow();
    }
}
