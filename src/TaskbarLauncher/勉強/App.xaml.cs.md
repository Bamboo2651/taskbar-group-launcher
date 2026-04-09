# App.xaml.cs 解説

## このファイルの役割

**アプリ起動時の処理をすべて担うファイル。**  
`App.xaml` とセットで動き、アプリが立ち上がった瞬間に何をするかを定義している。

具体的には以下を担当：
- 起動引数の解析（ポップアップ起動か、設定画面起動かの判断）
- タスクトレイアイコンの初期化
- NamedPipeサーバーの起動
- 表示するウィンドウの決定

---

## コード全文

```csharp
using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using SystemApplication = System.Windows.Application;

namespace TaskbarLauncher
{
    public partial class App : SystemApplication
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray();

            System.Diagnostics.Debug.WriteLine($"[App.OnStartup] 引数の数: {args.Length}");
            for (int i = 0; i < args.Length; i++)
            {
                System.Diagnostics.Debug.WriteLine($"[App.OnStartup] Args[{i}]: {args[i]}");
            }

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            bool isGroupLaunch = args.Length >= 2 && args[0] == "--group";
            string? groupId = null;

            // ★ 起動時に1回だけ設定を読み込んでキャッシュする
            var configManager = new ConfigManager();
            var groups = configManager.LoadGroups();
            NamedPipeServer.SetCachedGroups(groups);

            // タスクトレイアイコンの初期化
            var notifyIcon = new System.Windows.Forms.NotifyIcon();
            try
            {
                notifyIcon.Icon = System.IO.File.Exists("taskbar_icon.ico")
                    ? new System.Drawing.Icon("taskbar_icon.ico")
                    : System.Drawing.SystemIcons.Application;
            }
            catch
            {
                notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            }

            notifyIcon.Visible = true;
            notifyIcon.Text = "StackBar";

            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            contextMenu.Items.Add("設定を開く", null, (s, ea) => MainWindow?.Activate());
            contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            contextMenu.Items.Add("終了", null, (s, ea) => Shutdown());
            notifyIcon.ContextMenuStrip = contextMenu;

            notifyIcon.DoubleClick += (s, ea) => MainWindow?.Activate();

            Exit += (s, ea) =>
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
                NamedPipeServer.StopListening();
            };

            NamedPipeServer.StartListening();

            if (isGroupLaunch && groupId != null)
            {
                var popup = new PopupWindow(groupId, groups);
                popup.Show();
                popup.Topmost = true;
                popup.Activate();
                popup.Focus();
            }
            else
            {
                if (MainWindow == null)
                {
                    MainWindow = new MainWindow();
                }
                MainWindow.Show();
            }
        }
    }
}
```

---

## ブロックごとの解説

### ① using と名前空間のエイリアス

```csharp
using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using SystemApplication = System.Windows.Application;
```

`using` はC#で「このクラス群を使います」と宣言するもの。  
これがないと `Environment` や `Window` などを毎回フルパスで書く必要がある。

最後の行が特殊：

```csharp
using SystemApplication = System.Windows.Application;
```

これは**エイリアス（別名）**の定義。  
このファイルでは `System.Windows.Forms` と `System.Windows` の両方を使っているため、  
`Application` というクラス名が**2つのライブラリに存在して衝突する**。  
それを避けるために `System.Windows.Application` に `SystemApplication` という別名をつけている。

---

### ② クラス定義

```csharp
public partial class App : SystemApplication
```

#### `partial` とは

1つのクラスを**複数のファイルに分けて書ける**C#の機能。  
`App.xaml.cs` と `App.xaml` が合わさって、はじめて完全な `App` クラスになる。

#### `: SystemApplication` とは

`SystemApplication`（= `System.Windows.Application`）を**継承**している。  
継承とは「親クラスの機能をそのまま引き継ぎつつ、追加・上書きできる」仕組み。  
WPFアプリの本体クラスは必ず `Application` を継承して作る。

---

### ③ OnStartup（起動時に呼ばれるメソッド）

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);
```

- `OnStartup` は親クラス（`Application`）が持つメソッドで、**アプリ起動時に自動で呼ばれる**
- `override` は親クラスのメソッドを**上書き**するキーワード
- `protected` は「このクラスと子クラスからしか呼べない」アクセス修飾子
- `base.OnStartup(e)` は「親クラスの元の処理も実行する」という意味。これを忘れると親の初期化処理が走らない

---

### ④ 起動引数の取得

```csharp
string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray();
```

タスクバーのショートカットから起動されたとき、引数として `--group {groupId}` が渡される。  
この行でその引数を取り出している。

- `Environment.GetCommandLineArgs()` → 起動時の引数を配列で取得。ただし**最初の要素はexeのパス自体**
- `.Skip(1)` → 最初の要素（exeパス）を飛ばす。`Linq` の機能
- `.ToArray()` → `Skip` の結果は配列ではないので、配列に変換する

#### Linqとは

C#でコレクション（配列やリストなど）を操作するための機能群。  
`.Where()`, `.Skip()`, `.Select()` などのメソッドチェーンで直感的に書ける。

```csharp
// Linqなし
var list = new List<string>();
for (int i = 1; i < allArgs.Length; i++)
    list.Add(allArgs[i]);
string[] args = list.ToArray();

// Linqあり（同じ意味）
string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray();
```

---

### ⑤ シャットダウンモードの設定

```csharp
ShutdownMode = ShutdownMode.OnExplicitShutdown;
```

WPFはデフォルトで「最後のウィンドウが閉じられたらアプリ終了」という動作をする。  
しかしStackBarはタスクトレイに常駐させたいので、ウィンドウを閉じてもアプリが終了しては困る。

`OnExplicitShutdown` にすることで「`Shutdown()` を明示的に呼ぶまで終了しない」動作になる。

| モード | 動作 |
|---|---|
| `OnLastWindowClose`（デフォルト） | 最後のウィンドウが閉じたら終了 |
| `OnMainWindowClose` | メインウィンドウが閉じたら終了 |
| `OnExplicitShutdown` | `Shutdown()` を呼ぶまで終了しない |

---

### ⑥ 起動種別の判定

```csharp
bool isGroupLaunch = args.Length >= 2 && args[0] == "--group";
string? groupId = null;
```

- `args.Length >= 2 && args[0] == "--group"` → 引数が2つ以上あり、かつ最初が `--group` ならグループ起動
- `&&` は「かつ（AND）」の意味
- `string?` の `?` は**null許容型**。通常のstringはnullを入れられないが、`?` をつけるとnullを許容できる

#### null許容型とは

```csharp
string name = null;   // ❌ コンパイルエラー（nullを入れられない）
string? name = null;  // ✅ OK（nullを許容している）
```

C#はデフォルトでnullを厳しくチェックする。  
「この変数はnullになるかもしれない」という意思表示として `?` をつける。

---

### ⑦ 設定の読み込みとキャッシュ

```csharp
var configManager = new ConfigManager();
var groups = configManager.LoadGroups();
NamedPipeServer.SetCachedGroups(groups);
```

- `ConfigManager` はJSONファイルの読み書きを担当するクラス（別ファイル）
- `LoadGroups()` でJSONからグループ一覧を読み込む
- `NamedPipeServer.SetCachedGroups(groups)` でサーバー側にキャッシュしておく

**なぜキャッシュするか：**  
ポップアップはショートカット経由で何度も起動される。  
そのたびにJSONを読むより、メモリ上にキャッシュしておいて即座に返す方が速い。

---

### ⑧ タスクトレイアイコンの初期化

```csharp
var notifyIcon = new System.Windows.Forms.NotifyIcon();
try
{
    notifyIcon.Icon = System.IO.File.Exists("taskbar_icon.ico")
        ? new System.Drawing.Icon("taskbar_icon.ico")
        : System.Drawing.SystemIcons.Application;
}
catch
{
    notifyIcon.Icon = System.Drawing.SystemIcons.Application;
}

notifyIcon.Visible = true;
notifyIcon.Text = "StackBar";
```

`NotifyIcon` はWindowsのシステムトレイ（右下の通知領域）にアイコンを表示するクラス。  
WPFではなく `WindowsForms` の機能だが、WPFから使うことができる。

アイコンの設定部分：

```csharp
notifyIcon.Icon = System.IO.File.Exists("taskbar_icon.ico")
    ? new System.Drawing.Icon("taskbar_icon.ico")
    : System.Drawing.SystemIcons.Application;
```

#### 三項演算子とは

```csharp
条件 ? trueのとき : falseのとき
```

`if-else` を1行で書ける書き方。

```csharp
// 三項演算子
notifyIcon.Icon = ファイルがある ? カスタムアイコン : デフォルトアイコン;

// 同じ意味のif-else
if (ファイルがある)
    notifyIcon.Icon = カスタムアイコン;
else
    notifyIcon.Icon = デフォルトアイコン;
```

`try-catch` でアイコン読み込みを囲んでいるのは、  
ファイルが壊れていたりアクセスできないときでも**アプリがクラッシュしないようにするため**。

---

### ⑨ 右クリックメニューの作成

```csharp
var contextMenu = new System.Windows.Forms.ContextMenuStrip();
contextMenu.Items.Add("設定を開く", null, (s, ea) => MainWindow?.Activate());
contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
contextMenu.Items.Add("終了", null, (s, ea) => Shutdown());
notifyIcon.ContextMenuStrip = contextMenu;
```

タスクトレイアイコンを右クリックしたときのメニューを作っている。

#### ラムダ式とは

```csharp
(s, ea) => MainWindow?.Activate()
```

これは**ラムダ式**と呼ばれる、その場で書く小さな関数。  
「クリックされたとき（s, eaを受け取って）、MainWindowをアクティブにする」という意味。

```csharp
// ラムダ式
(s, ea) => MainWindow?.Activate()

// 同じ意味の普通のメソッド
void OnMenuClick(object s, EventArgs ea)
{
    MainWindow?.Activate();
}
```

#### `?.`（null条件演算子）とは

```csharp
MainWindow?.Activate()
```

`MainWindow` が `null` でなければ `Activate()` を呼ぶ。  
`null` のときは何もしない（エラーにならない）。

```csharp
// null条件演算子なし
if (MainWindow != null)
    MainWindow.Activate();

// null条件演算子あり（同じ意味）
MainWindow?.Activate();
```

---

### ⑩ ダブルクリックとアプリ終了時の処理

```csharp
notifyIcon.DoubleClick += (s, ea) => MainWindow?.Activate();

Exit += (s, ea) =>
{
    notifyIcon.Visible = false;
    notifyIcon.Dispose();
    NamedPipeServer.StopListening();
};
```

#### `+=` イベントへの登録とは

WPFやC#では「何かが起きたとき（クリック、終了など）に実行する処理」を**イベント**として登録する。  
`+=` でイベントに処理を追加できる。複数追加も可能。

```csharp
notifyIcon.DoubleClick += 処理A;
notifyIcon.DoubleClick += 処理B;
// ダブルクリックで処理A・処理B両方が実行される
```

`Exit` イベントはアプリ終了時に発火する。  
ここで `notifyIcon.Dispose()` を呼ばないと、タスクトレイのアイコンがゴーストとして残り続けてしまう。

`Dispose()` は「このオブジェクトが使っているリソースを解放する」メソッド。  
C#のガベージコレクタは自動でメモリを解放するが、アイコンのようなOS側のリソースは手動で解放が必要。

---

### ⑪ 表示するウィンドウの決定

```csharp
if (isGroupLaunch && groupId != null)
{
    var popup = new PopupWindow(groupId, groups);
    popup.Show();
    popup.Topmost = true;
    popup.Activate();
    popup.Focus();
}
else
{
    if (MainWindow == null)
    {
        MainWindow = new MainWindow();
    }
    MainWindow.Show();
}
```

- グループ起動（`--group {id}` あり）→ `PopupWindow` を表示
- 通常起動 → `MainWindow`（設定画面）を表示

`popup.Topmost = true` は「常に最前面に表示する」設定。  
ポップアップが他のウィンドウの裏に隠れないようにするため。

---

## まとめ

| ブロック | やっていること |
|---|---|
| using・エイリアス | 使うライブラリの宣言・名前衝突の回避 |
| `partial class App` | Applicationを継承したアプリ本体クラス |
| `OnStartup` | 起動時に自動で呼ばれる処理 |
| 引数の取得・判定 | ポップアップ起動か設定画面起動かの判断 |
| `ShutdownMode` | タスクトレイ常駐のための設定 |
| `ConfigManager` | JSONから設定を読み込んでキャッシュ |
| `NotifyIcon` | タスクトレイアイコンの初期化 |
| `ContextMenuStrip` | 右クリックメニューの作成 |
| `Exit` イベント | アプリ終了時のリソース解放 |
| ウィンドウの表示 | 起動種別に応じて表示するウィンドウを切り替え |

## 登場したC#の重要概念

| 概念 | 書き方 | 意味 |
|---|---|---|
| エイリアス | `using A = B.C` | クラスに別名をつける |
| 継承 | `: 親クラス` | 親の機能を引き継ぐ |
| override | `override メソッド` | 親のメソッドを上書き |
| null許容型 | `string?` | nullを許容する型 |
| 三項演算子 | `条件 ? A : B` | 1行のif-else |
| ラムダ式 | `(s, e) => 処理` | その場で書く小さな関数 |
| null条件演算子 | `obj?.メソッド()` | nullなら何もしない |
| イベント登録 | `イベント += 処理` | 何かが起きたときの処理を登録 |
| Dispose | `obj.Dispose()` | OSリソースの解放 |