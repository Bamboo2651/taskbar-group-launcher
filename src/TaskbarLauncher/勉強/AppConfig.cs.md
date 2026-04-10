# AppConfig.cs

## このファイルの役割

**アプリ1つ分のデータを定義するクラス。**

StackBar のグループに登録された各アプリの「名前」「パス」「アイコン」を1つのまとまりとして管理する。  
グループの中身は `AppConfig` のリストとして保存される。

---

## using（名前空間のインポート）

```csharp
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Text.Json.Serialization;
```

| using | 用途 |
|---|---|
| `System.Drawing` | `Icon.ExtractAssociatedIcon()` でexeからアイコンを取得するために必要 |
| `System.IO` | `File.Exists()` でファイルの存在確認をするために必要 |
| `System.Windows` | `Int32Rect.Empty` などWPFの基本型を使うために必要 |
| `System.Windows.Interop` | `Imaging.CreateBitmapSourceFromHIcon()` でWin32のアイコンをWPF用に変換するために必要 |
| `System.Windows.Media.Imaging` | `BitmapSource`・`BitmapSizeOptions` などWPFで画像を扱う型のために必要 |
| `System.Text.Json.Serialization` | `[JsonIgnore]` 属性を使うために必要 |

---

## クラス定義

```csharp
public class AppConfig
```

`public` … プロジェクト内のどこからでもアクセスできる。  
`class AppConfig` … アプリ1件分のデータをまとめた設計図（クラス）。

このクラスのインスタンス（実体）1つが、グループに登録された**アプリ1つ**に対応する。

---

## プロパティ：Name / Path

```csharp
public string Name { get; set; } = "";
public string Path { get; set; } = "";
```

| プロパティ | 内容 |
|---|---|
| `Name` | アプリの表示名（例：`"Visual Studio Code"`） |
| `Path` | exeまたはlnkファイルの絶対パス（例：`"C:\Users\...\Code.exe"`） |

**`= ""` について**  
初期値を空文字にしている。`null` のままにしておくと、後で文字列操作したときに `NullReferenceException` が起きる可能性があるため、最初から空文字を入れておくのが安全。

**`{ get; set; }` について**  
自動プロパティと呼ばれる書き方。値の取得（get）と設定（set）を両方できる。  
JSONの読み書きにも `set` が必要なため、両方つけている。

---

## `[JsonIgnore]` とは

```csharp
[JsonIgnore]
public BitmapSource? Icon { get; }
```

`[JsonIgnore]` は「このプロパティをJSONの保存・読み込みの対象から外す」という指示。

**なぜIconだけ除外するのか？**

- `Icon` は `BitmapSource`（画像データ）であり、JSONに直接保存できない形式
- アイコンは `Path` さえあれば毎回exeから取得できるので、保存する必要がない
- 保存するのは `Name` と `Path` だけで十分

つまり `[JsonIgnore]` を付けることで、**「保存はしないが、実行中は使う」** プロパティとして扱える。

---

## プロパティ：Icon

アプリのアイコン画像を動的に取得する読み取り専用プロパティ。  
値を保持するのではなく、**アクセスされるたびにexeからアイコンを取得して返す**。

### ① ファイルの存在確認

```csharp
if (!File.Exists(Path))
    return null;
```

指定されたパスにファイルが存在しない場合は `null` を返して終了する。  
アプリが削除・移動された場合のクラッシュ防止。

### ② アイコンの取得

```csharp
var icon = System.Drawing.Icon.ExtractAssociatedIcon(Path);
if (icon == null) return null;
```

`ExtractAssociatedIcon()` はWindowsの機能を使い、exeやlnkに関連付けられたアイコンを取得する。  
取得できなかった場合（ファイルの種類によっては失敗することがある）は `null` を返す。

### ③ WPF用の画像（BitmapSource）に変換

```csharp
return Imaging.CreateBitmapSourceFromHIcon(
    icon.Handle,
    Int32Rect.Empty,
    BitmapSizeOptions.FromEmptyOptions());
```

`ExtractAssociatedIcon()` で取得したアイコンはWin32形式（`System.Drawing.Icon`）のため、そのままWPFのUIに表示できない。  
`CreateBitmapSourceFromHIcon()` を使ってWPF用の `BitmapSource` に変換している。

| 引数 | 意味 |
|---|---|
| `icon.Handle` | Win32アイコンのポインタ（ハンドル） |
| `Int32Rect.Empty` | アイコン全体を使う（切り抜きなし） |
| `BitmapSizeOptions.FromEmptyOptions()` | サイズ変換なし（元のサイズそのまま） |

### ④ 例外処理

```csharp
catch
{
    return null;
}
```

アイコン取得・変換中に予期しないエラーが起きても、アプリ全体がクラッシュしないように `null` を返して握りつぶす。  
アイコンが表示されないだけで、アプリ本体は動き続ける。