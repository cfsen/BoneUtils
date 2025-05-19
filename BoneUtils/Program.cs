using BoneUtils.RayLib;
using Raylib_cs;

RaylibDemoRunner rldr = new();

Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint);
Raylib.SetConfigFlags(ConfigFlags.VSyncHint);
Raylib.InitWindow(800, 600, "BoneUtils demo");
Raylib.SetTargetFPS(60);
rldr.InitDemos();
rldr.RayRender.Init(); // Late init of rendering to avoid constructor eagerness

while (!Raylib.WindowShouldClose()) {
	rldr.HandleInput();
	rldr.Update(Raylib.GetFrameTime());
	rldr.RayRender.UpdateLights();

    Raylib.BeginDrawing();
		Raylib.ClearBackground(Color.Black);

		Raylib.BeginMode3D(rldr.Camera);
			rldr.DrawDemo3D();
		Raylib.EndMode3D();

		rldr.DrawRunner2D();
		rldr.DrawDemo2D();
    Raylib.EndDrawing();
}

Raylib.CloseWindow();
