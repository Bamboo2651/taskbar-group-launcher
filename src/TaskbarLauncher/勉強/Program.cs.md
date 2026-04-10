# Program.cs

## このファイルの役割

**アプリの起点（エントリーポイント）。起動引数によって処理を振り分けるクラス。**

StackBar は用途によって2通りの起動をする。

| 起動パターン | 条件 | 処理 |
|---|---|---|
| ポップアップ起動 | `--group [ID]` 引数あり | 常駐プロセスに通知して即終了 |
| 通常起動 | 引数なし、または常駐プロセスなし | WPFアプリとして立ち上げる |

この振り分けを担うのが `Program.cs` の `Main()` メソッド。

---

## `[STAThread]` とは？

```csharp
[STAThread]
public static void Main(string[] args)
```

WPF アプリを起動するために**必須の属性**。

`STA` は「Single-Threaded Apartment」の略で、UIに関わる処理を1つのスレッドで管理するモデル。  
WPF はこのモデルを前提に設計されているため、`Main()` に必ずこの属性をつける必要がある。  
付け忘れると WPF の初期化時にエラーが発生する。

---

## 引数チェックをWPF起動前に行う理由

```csharp
// 引数チェック（WPFの重いライブラリを読み込む前に行う！）
bool isGroupLaunch = args.Length >= 2 && args[0] == "--group";
```

コメントにある通り、**WPFの初期化より先に引数を確認している**。

**なぜか？**  
ポップアップ起動（`--group [ID]` あり）の場合、常駐プロセスへ通知して即終了するだけでよい。  
WPFの重いライブラリをわざわざ読み込む必要がないため、引数チェックを最初に行うことで**超高速な終了**を実現している。

ユーザーがタスクバーのショートカットをクリックするたびに新しいプロセスが起動・終了するため、この速度が体感に直結する。

---

## isGroupLaunch の判定

```csharp
bool isGroupLaunch = args.Length >= 2 && args[0] == "--group";
```

ショートカットの引数は `--group [グループID]` という形式で渡される。

| 条件 | 意味 |
|---|---|
| `args.Length >= 2` | 引数が2つ以上ある（`--group` と `ID` の両方がある） |
| `args[0] == "--group"` | 最初の引数が `--group` である |

両方を満たす場合だけ `isGroupLaunch = true` になる。  
どちらか一方だけでは不正な引数とみなし、通常起動に fallback する。

---

## ポップアップ起動の流れ

```csharp
if (isGroupLaunch)
{
    string groupId = args[1];

    if (NamedPipeClient.SendGroupIdToRunningInstance(groupId))
    {
        return; // WPFを一切起動せずに終了
    }
}
```

**流れ**

```
引数からグループIDを取得（args[1]）
    ↓
常駐プロセスへ Named Pipe で通知
    ↓
通知成功（true） → return で即終了（WPF起動なし）
通知失敗（false） → 常駐プロセスがいないので通常起動へ
```

通知に成功した場合は `return` するだけ。  
WPFのウィンドウは一切開かず、ユーザーには見えないまま終了する。

通知に失敗した場合（常駐プロセスが起動していない）は、  
このプロセス自身がメインとして通常起動する。

---

## 通常起動の流れ

```csharp
var app = new App();
//app.InitializeComponent();
app.Run();
```

WPF アプリケーションとして起動する。

**`new App()` とは？**  
`App.xaml.cs` で定義した `App` クラスのインスタンスを作成する。  
StackBar のタスクトレイ常駐・Named Pipe サーバー起動などの初期化処理がここで動く。

**`app.Run()` とは？**  
WPF のメインループを開始する。  
ここに処理が入るとアプリは「起動中」の状態になり、終了命令が来るまでイベントを待ち続ける。

**`app.InitializeComponent()` がコメントアウトされている理由**  
通常 WPF では `App.xaml` の内容（リソースやスタートアップURIなど）を読み込むために呼ぶメソッド。  
StackBar では `App.xaml` に `StartupUri` を設定せず、起動処理を `App.xaml.cs` の `OnStartup()` で独自に制御しているため不要になっている。