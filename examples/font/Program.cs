using static RaylibSharp.Raylib;

const int screenWidth = 800;
const int screenHeight = 450;

InitWindow(screenWidth, screenHeight, "raylib [text] example - font loading"u8);

var msg = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHI\nJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklm\nnopqrstuvwxyz{|}~¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒ\nÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö\n÷øùúûüýþÿ"u8;

Font fontBm = LoadFont("resources/pixantiqua.fnt"u8);
Font fontTtf = LoadFontEx("resources/pixantiqua.ttf"u8, 32, 250);

SetTextLineSpacing(16);

bool useTtf = false;

SetTargetFPS(60);

while (!WindowShouldClose())
{
  if (IsKeyDown(KEY_SPACE)) useTtf = true;
  else useTtf = false;

  BeginDrawing();

  ClearBackground(RAYWHITE);

  DrawText("Hold SPACE to use TTF generated font"u8, 20, 20, 20, LIGHTGRAY);

  if (!useTtf)
  {
    DrawTextEx(fontBm, msg, new Vector2() { x = 20.0f, y = 100.0f }, (float)fontBm.baseSize, 2, MAROON);
    DrawText("Using BMFont (Angelcode) imported"u8, 20, GetScreenHeight() - 30, 20, GRAY);
  }
  else
  {
    DrawTextEx(fontTtf, msg, new Vector2() { x = 20.0f, y = 100.0f }, (float)fontTtf.baseSize, 2, LIME);
    DrawText("Using TTF font generated"u8, 20, GetScreenHeight() - 30, 20, GRAY);
  }

  EndDrawing();
}

UnloadFont(fontBm);
UnloadFont(fontTtf);

CloseWindow();