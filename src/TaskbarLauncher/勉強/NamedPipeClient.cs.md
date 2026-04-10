# NamedPipeClient.cs

## このファイルの役割

**新しく起動した StackBar プロセスが、常駐しているメインプロセスにメッセージを送るクライアント側クラス。**

タスクバーのショートカットをクリックすると新しい StackBar プロセスが起動する。  
そのとき「すでに常駐しているプロセスがあるか」を確認し、あればメッセージを送って自分は終了する。  
このクラスはその「送る側」を担当する。

---

## フィールド変数

```csharp
private const string PipeName = "StackBar_MutualExclusion";
private const int TimeoutMs = 1000;
```

| フィールド | 役割 |
|---|---|
| `PipeName` | 接続先のパイプ名。`NamedPipeServer` と同じ名前を指定することで繋がる |
| `TimeoutMs` | 接続を待つ最大時間（ミリ秒）。1000ms = 1秒でタイムアウト |

**なぜタイムアウトが必要なのか？**  
常駐プロセスが起動していない場合、パイプに接続しようとしても永遠に待ち続けてしまう。  
1秒待って繋がらなければ「起動していない」と判断して処理を切り上げるための上限時間。

---

## SendGroupIdToRunningInstance()

```csharp
public static bool SendGroupIdToRunningInstance(string groupId)
```

常駐プロセスにグループIDを送信するメソッド。  
戻り値で「常駐プロセスが起動していたか」を呼び出し元に伝える。

### ① パイプへの接続を試みる

```csharp
using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.None))
{
    var connectTask = client.ConnectAsync(TimeoutMs);
    if (!connectTask.Wait(TimeoutMs))
    {
        return false; // タイムアウト → 常駐プロセスなし
    }
```

`"."` はローカルPC（自分自身）を意味する。  
`ConnectAsync` で非同期に接続を試み、`Wait(TimeoutMs)` で最大1秒待つ。  
1秒以内に繋がらなければ `false` を返して終了。

### ② グループIDを送信

```csharp
using (var writer = new StreamWriter(client))
{
    writer.WriteLine(groupId);
    writer.Flush();
}
return true;
```

接続できたらグループIDを1行送信して終了。  
`Flush()` でバッファに残ったデータを確実に送り出してから閉じる。

**戻り値の意味**

| 戻り値 | 意味 | 呼び出し元の動作 |
|---|---|---|
| `true` | 常駐プロセスが応答した | 新しいプロセスは自分の役割を果たしたので終了 |
| `false` | 常駐プロセスが起動していない | 新しいプロセスがそのままメインとして起動する |

### ③ 例外処理

```csharp
catch (TimeoutException)
{
    return false;
}
catch (Exception ex)
{
    return false;
}
```

タイムアウト例外・その他の例外どちらでも `false` を返す。  
エラーの種類に関わらず「常駐プロセスなし」として扱い、呼び出し元が正しく動き続けられるようにする。

---

## SendMessageToRunningInstance()

```csharp
public static bool SendMessageToRunningInstance(string message)
{
    try
    {
        using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.None))
        {
            if (!client.ConnectAsync(TimeoutMs).Wait(TimeoutMs)) return false;
            using (var writer = new StreamWriter(client))
            {
                writer.WriteLine(message);
                writer.Flush();
            }
            return true;
        }
    }
    catch { return false; }
}
```

グループID以外の任意のメッセージを送る汎用メソッド。  
処理の流れは `SendGroupIdToRunningInstance` とほぼ同じだが、引数が `string message` になっていて何でも送れる。

**使われる場面**  
二重起動時に `"--open-settings"` を送り、常駐プロセスの設定画面を前面に出す処理で使われる。

**`SendGroupIdToRunningInstance` との違い**

| | SendGroupIdToRunningInstance | SendMessageToRunningInstance |
|---|---|---|
| 用途 | グループID専用 | 汎用（任意のメッセージ） |
| コメント・ログ | 詳細あり | 最小限 |
| 使われる場面 | ショートカットからの起動 | 二重起動時の設定画面呼び出し |

---

## ServerとClientの関係まとめ

```
【常駐プロセス（最初に起動した StackBar）】
NamedPipeServer.StartListening() で待ち受け開始
    ↓ 待機中...

【新しいプロセス（ショートカットをクリックで起動）】
NamedPipeClient.SendGroupIdToRunningInstance(groupId) を呼ぶ
    ↓ 接続成功 → グループIDを送信 → true を返す → 新プロセス終了

【常駐プロセス（受け取り側）】
NamedPipeServer がメッセージを受信
    ↓
グループIDならポップアップ表示
--open-settings なら設定画面を前面に
```

クライアントは「送って終わり」、サーバーは「常に待ち続ける」という役割分担になっている。