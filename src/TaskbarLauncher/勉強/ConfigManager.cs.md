# ConfigManager.cs

## このファイルの役割

**グループデータの読み書き・アイコン生成・ショートカット作成をまとめた管理クラス。**

StackBar のデータ永続化に関わる処理はほぼここに集約されている。  
他のクラスはこの `ConfigManager` を通じてデータの保存・読み込みを行う。

---

## using（名前空間のインポート）

```csharp
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.Json;
using System.Windows;
using TaskbarLauncher.Models;
using MessageBox = System.Windows.MessageBox;
```

| using | 用途 |
|---|---|
| `System` | `Environment.GetFolderPath()` などの基本機能 |
| `System.Collections.Generic` | `List<GroupConfig>` を使うため |
| `System.Drawing` | `Bitmap`・`Graphics`・`Icon` などの画像処理 |
| `System.Drawing.Imaging` | `ImageFormat.Png` でPNG形式を指定するため |
| `System.IO` | ファイル・ディレクトリの読み書き |
| `System.Text.Json` | JSONのシリアライズ・デシリアライズ |
| `System.Windows` | WPFの `MessageBox` などUI関連 |
| `TaskbarLauncher.Models` | `GroupConfig`・`AppConfig` を使うため |
| `MessageBox = System.Windows.MessageBox` | WPFとWinFormsで `MessageBox` が重複するため、WPF側を明示的に指定 |

---

## 定数：ConfigPath / IconCacheDir

```csharp
private static readonly string ConfigPath =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "StackBar", "groups.json");

private static readonly string IconCacheDir =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "StackBar", "icons");
```

**`Environment.SpecialFolder.ApplicationData` とは？**  
`C:\Users\ユーザー名\AppData\Roaming` のパスをOSから取得する。  
ハードコードせずOSに聞くことで、どのPCでも正しいパスになる。

**なぜ `AppData` に保存するのか？**  
アプリのデータを保存する場所としてWindowsが推奨しているフォルダ。  
インストール先（`Program Files`）はアクセス権の関係で書き込めないことがある。

**`static readonly` とは？**  
`static` … インスタンスを作らなくてもクラス単体で持つ値。  
`readonly` … 最初に設定したら変更できない定数扱い。  
パスは起動中に変わらないため、この組み合わせが適切。

---

## LoadGroups()

```csharp
public List<GroupConfig> LoadGroups()
{
    if (!File.Exists(ConfigPath))
        return new List<GroupConfig>();

    string json = File.ReadAllText(ConfigPath);
    return JsonSerializer.Deserialize<List<GroupConfig>>(json) ?? new List<GroupConfig>();
}
```

JSONファイルからグループ一覧を読み込んで返すメソッド。

**流れ**
1. ファイルが存在しない（初回起動など）場合は空のリストを返す
2. ファイルが存在する場合はテキストとして読み込む
3. JSONを `List<GroupConfig>` に変換（デシリアライズ）して返す

**`?? new List<GroupConfig>()` とは？**  
`??` はnull合体演算子。`Deserialize` が `null` を返した場合（JSONが空など）に空リストで代替する。クラッシュ防止のための安全策。

---

## SaveGroups()

```csharp
public void SaveGroups(List<GroupConfig> groups)
{
    string dir = Path.GetDirectoryName(ConfigPath)!;
    if (!Directory.Exists(dir))
        Directory.CreateDirectory(dir);

    string json = JsonSerializer.Serialize(groups, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(ConfigPath, json);
}
```

グループ一覧をJSONファイルに保存するメソッド。

**流れ**
1. 保存先フォルダが存在しない場合は作成する（初回起動時対策）
2. `List<GroupConfig>` をJSON文字列に変換（シリアライズ）
3. ファイルに書き込む（既存ファイルは上書き）

**`WriteIndented = true` とは？**  
JSONを整形（インデント付き）で出力するオプション。  
`false` だと1行にまとまって読みにくくなるため、デバッグや手動確認がしやすいようにtrueにしている。

---

## GetAppIcon()

```csharp
private Bitmap? GetAppIcon(string exePath)
{
    try
    {
        var icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
        return icon?.ToBitmap();
    }
    catch
    {
        return null;
    }
}
```

exeまたはlnkのパスからアイコン画像（Bitmap）を取得するプライベートメソッド。  
`AppConfig.Icon` と似ているが、こちらは**合成処理で使うためBitmapとして返す**。

`private` なので `ConfigManager` 内からのみ呼び出せる。  
失敗した場合は `null` を返してクラッシュを防ぐ。

---

## CreateGroupIcon()

グループ内のアプリアイコンを合成して `.ico` ファイルを生成するメソッド。  
ショートカットのアイコンとしてタスクバーに表示される。

### ① 古いアイコンの削除

```csharp
string oldIcoPath = Path.Combine(IconCacheDir, $"{group.Id}.ico");
if (File.Exists(oldIcoPath))
    File.Delete(oldIcoPath);
```

グループのアプリ構成が変わったとき、古いアイコンが残らないように先に削除する。  
ファイル名はグループIDを使うため、グループごとに別ファイルになる。

### ② アイコンの収集（最大4個）

```csharp
foreach (var app in apps)
{
    if (bitmaps.Count >= 4) break;
    var bmp = GetAppIcon(app.Path);
    if (bmp != null)
        bitmaps.Add(bmp);
}
```

グループ内のアプリから順番にアイコンを取得し、最大4個まで集める。  
取得できなかったアプリはスキップする。

### ③ アプリ数によるレイアウト分岐

```csharp
if (count == 1)
{
    g.DrawImage(bitmaps[0], 16, 16, 224, 224);
}
else if (count == 2)
{
    g.DrawImage(bitmaps[0], 4, 52, 120, 120);
    g.DrawImage(bitmaps[1], 132, 52, 120, 120);
}
else
{
    // 3〜4個：2×2グリッド
}
```

| アプリ数 | レイアウト |
|---|---|
| 1個 | 中央に大きく表示 |
| 2個 | 左右に並べる |
| 3〜4個 | 2×2グリッドに配置 |

スマホのフォルダアイコンと同じ考え方。

### ④ Bitmapの解放

```csharp
foreach (var bmp in bitmaps)
    bmp.Dispose();
canvas.Dispose();
```

`Bitmap` はメモリを直接確保するため、使い終わったら `Dispose()` で明示的に解放する。  
解放しないとメモリリークの原因になる。

---

## SaveAsIco()

```csharp
private void SaveAsIco(Bitmap bitmap, string path)
```

Bitmap を `.ico` ファイルとして保存するプライベートメソッド。

**なぜ手動でバイナリを書くのか？**  
.NET には `.ico` を直接書き出す標準APIがないため、ICO フォーマットの仕様に従ってバイナリを自分で構築している。

**ICO ファイルの構造**

```
[ICOヘッダー 6バイト]
  - 予約（0）
  - タイプ（1 = アイコン）
  - 画像の枚数

[画像ディレクトリエントリ 16バイト]
  - 幅・高さ（0=256px）
  - カラー数・予約
  - プレーン数・ビット深度
  - データサイズ
  - データの開始位置

[PNGデータ本体]
```

内部にPNGデータをそのまま埋め込む形式で保存している。  
Windows Vista以降のICO仕様で許可された方法。

---

## CreateShortcut()

グループのショートカット（`.lnk`）をデスクトップに作成するメソッド。

### PowerShell を使う理由

```csharp
FileName = "powershell.exe",
Arguments = $"-ExecutionPolicy Bypass -Command \"{script}\""
```

C# には `.lnk` ファイルを直接作成する標準APIがない。  
PowerShell の `WScript.Shell` COM オブジェクトを使うことで、引数・アイコン・説明文を含む完全なショートカットを作成できる。

### グループIDを引数に渡す

```csharp
$shortcut.Arguments = '--group {group.Id}'
```

ショートカットをクリックするとStackBarが `--group [ID]` という引数付きで起動する。  
`App.xaml` でこの引数を受け取り、対応するグループのポップアップを表示する仕組み。

### アイコンキャッシュのリフレッシュ

```csharp
System.Threading.Thread.Sleep(500);
SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
```

Windowsはアイコンをキャッシュしているため、新しいショートカットを作っても古いアイコンが表示されることがある。  
`SHChangeNotify` でOSにアイコン更新を通知することで、すぐに反映される。  
`Thread.Sleep(500)` は `.lnk` ファイルの書き込みが完了するのを待つための待機。

---

## SHChangeNotify()

```csharp
[System.Runtime.InteropServices.DllImport("shell32.dll")]
private static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);
```

**`DllImport` とは？**  
C# から Windows のネイティブDLL（`shell32.dll`）の関数を呼び出すための仕組み。  
P/Invoke（Platform Invocation Services）と呼ばれる。

**`extern` とは？**  
「この関数の実装はC#ではなく外部（DLL）にある」という宣言。  
メソッド本体は書かずに、DLLに処理を委ねる。

`0x08000000` はアイコンキャッシュを更新するイベントIDで、Windowsのシェルに変更を通知する。