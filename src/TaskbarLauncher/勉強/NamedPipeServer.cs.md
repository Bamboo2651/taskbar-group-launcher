# NamedPipeServer.cs

## このファイルの役割

**常駐しているメインプロセスが、別プロセスからのメッセージを受け取るサーバー側クラス。**

タスクバーのショートカットをクリックすると新しい StackBar プロセスが起動する。  
そのとき「すでに常駐しているプロセス」にメッセージを渡す仕組みが Named Pipe であり、  
このクラスはその受け取り側（サーバー）を担当する。

受け取ったメッセージに応じて、ポップアップの表示または設定画面の呼び出しを行う。

---

## Named Pipe とは？

同じPC上の**異なるプロセス間でデータをやり取りする仕組み**（プロセス間通信）。

StackBar では以下の流れで使われている。

```
タスクバーのショートカットをクリック
    ↓
新しい StackBar プロセスが起動（クライアント側）
    ↓
Named Pipe 経由でグループIDを送信
    ↓
常駐している StackBar プロセスが受け取る（サーバー側）
    ↓
対応するグループのポップアップを表示
```

ファイルのように「名前付きのパイプ」を通じてデータを流すイメージ。

---

## フィールド変数

```csharp
private const string PipeName = "StackBar_MutualExclusion";
private static bool _isListening = false;
private static List<GroupConfig> _cachedGroups = new List<GroupConfig>();
```

| フィールド | 役割 |
|---|---|
| `PipeName` | パイプの名前。サーバーとクライアントで同じ名前を使うことで接続できる |
| `_isListening` | 現在待ち受け中かどうかのフラグ。二重起動防止に使う |
| `_cachedGroups` | ポップアップ表示に使うグループ一覧のキャッシュ |

`static` にしているのは、インスタンスを作らずクラス単体で状態を管理するため。

---

## SetCachedGroups()

```csharp
public static void SetCachedGroups(List<GroupConfig> groups)
{
    _cachedGroups = groups;
}
```

外部からグループ一覧をサーバーに渡して更新するメソッド。

**なぜキャッシュが必要なのか？**  
ポップアップを表示するとき、グループIDだけでは「どんなアプリが入っているか」が分からない。  
サーバー側があらかじめグループ一覧を持っておくことで、IDを受け取った瞬間にポップアップを開ける。

設定アプリでグループを編集するたびにこのメソッドを呼び、最新状態を反映させている。

---

## StartListening()

```csharp
public static void StartListening()
{
    if (_isListening) return;
    _isListening = true;
    Task.Run(() => WaitForNextConnection());
}
```

パイプサーバーの待ち受けを開始するメソッド。

**`if (_isListening) return;` とは？**  
すでに待ち受け中なのに再度呼ばれた場合に二重起動しないためのガード。

**`Task.Run` で別スレッドに投げる理由**  
パイプの待ち受けは接続が来るまでブロックする処理。  
UIスレッドで実行するとアプリ全体が固まってしまうため、別スレッドで動かす必要がある。

---

## WaitForNextConnection()

接続を非同期で待ち受け、来たら処理して次の待ち受けを再起動するメソッド。

### 非同期待ち受けの開始

```csharp
var server = new NamedPipeServerStream(
    PipeName, PipeDirection.In,
    NamedPipeServerStream.MaxAllowedServerInstances,
    PipeTransmissionMode.Byte,
    PipeOptions.Asynchronous);

server.BeginWaitForConnection(ar => { ... }, null);
```

`BeginWaitForConnection` は接続が来るまで「待ちながらも他の処理をブロックしない」非同期パターン。  
`MaxAllowedServerInstances` を指定することで、複数の接続を同時に受け付けられる。

### 接続後すぐに次の待ち受けを開始

```csharp
server.EndWaitForConnection(ar);
Task.Run(() => WaitForNextConnection()); // ← 即座に次の待ち受けへ
```

接続を受け取ったら**先に次の待ち受けを再起動**してから処理する。  
こうすることで、今の処理中に別の接続が来ても取りこぼさない。

---

## メッセージの振り分け

```csharp
string message = reader.ReadLine();
if (message == "--open-settings")
{
    // 設定画面を開く
}
else
{
    ShowPopup(message);
}
```

受け取ったメッセージの内容で処理を分岐する。

| メッセージ | 処理 |
|---|---|
| `"--open-settings"` | 設定画面を開く（または前面に出す） |
| それ以外（グループID） | 対応するグループのポップアップを表示 |

---

## Dispatcher.Invoke とは？

```csharp
System.Windows.Application.Current.Dispatcher.Invoke(() =>
{
    // UIの操作
});
```

WPF では**UIの操作はUIスレッドからしか行えない**というルールがある。  
パイプの待ち受けは別スレッドで動いているため、そのままウィンドウを操作するとエラーになる。

`Dispatcher.Invoke` を使うと「UIスレッドにお願いして代わりに実行してもらう」ことができる。  
別スレッドからUIを安全に操作するための橋渡し役。

---

## ShowPopup()

```csharp
private static void ShowPopup(string groupId)
{
    if (_currentPopup != null)
    {
        try { _currentPopup.Close(); } catch { }
        _currentPopup = null;
    }

    var popup = new PopupWindow(groupId, _cachedGroups);
    _currentPopup = popup;

    popup.Closed += (s, e) =>
    {
        if (_currentPopup == popup)
            _currentPopup = null;
    };

    popup.Show();
    popup.Topmost = true;
    popup.Activate();
    popup.Focus();
}
```

グループIDに対応するポップアップウィンドウを表示するメソッド。

**既存ポップアップを先に閉じる理由**  
同じグループのショートカットを連続クリックしたとき、ポップアップが重複して開くのを防ぐ。  
`try { } catch { }` で囲んでいるのは、すでに閉じかけている場合のエラーを無視するため。

**`popup.Closed` イベント**  
ポップアップが閉じられたとき `_currentPopup` を `null` にリセットする。  
これで次に開くときに「前のポップアップが残っている」と誤判定されない。

**`Topmost`・`Activate`・`Focus` の役割**

| 設定 | 意味 |
|---|---|
| `Topmost = true` | 常に最前面に表示（他のウィンドウに隠れない） |
| `Activate()` | ウィンドウをアクティブ状態にする |
| `Focus()` | キーボードフォーカスを当てる（Escキーで閉じるなどに必要） |

---

## StopListening()

```csharp
public static void StopListening()
{
    _isListening = false;
}
```

待ち受けを停止するメソッド。

**フラグを折るだけでよい理由**  
`WaitForNextConnection()` の先頭で `if (!_isListening) return;` をチェックしている。  
`_isListening` を `false` にすれば、次の再帰呼び出し時に自然に停止する。  
現在処理中の接続があっても強制終了せず、安全に止まる。