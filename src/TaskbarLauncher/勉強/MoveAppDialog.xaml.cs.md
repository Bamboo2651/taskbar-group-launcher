# MoveAppDialog.xaml.cs

## このファイルの役割

**`MoveAppDialog.xaml` のロジックを書いたコードビハインド。**

グループ一覧をリストに表示し、ユーザーが選択した移動先グループを呼び出し元に返す。  
`InputDialog.xaml.cs` が文字列を返すのに対し、こちらは **`GroupConfig` オブジェクトそのもの**を返す点が異なる。

---

## `SelectedGroup` プロパティ

```csharp
public GroupConfig? SelectedGroup { get; private set; }
```

ユーザーが選択した移動先グループを保持するプロパティ。

**`GroupConfig?` の `?` とは？**  
`?` は「nullを許容する型」を表す。  
ダイアログを開いた直後はまだ何も選ばれていないため、初期値は `null` になる。  
`?` を付けることで「選ばれていない状態（null）もあり得る」とコンパイラに伝えられる。

**`private set` の理由**  
`InputDialog` の `Answer` と同じ理由。  
移動先グループは「移動ボタンを押したとき」だけセットされるべきものなので、外から書き換えられないように `private set` で守っている。

---

## コンストラクタ

```csharp
public MoveAppDialog(List<GroupConfig> groups)
{
    InitializeComponent();
    GroupListBox.ItemsSource = groups;
    GroupListBox.DisplayMemberPath = "Name";
}
```

ダイアログを生成するときにグループ一覧を受け取り、ListBoxに表示する。

### `ItemsSource` でリストを一括バインド

```csharp
GroupListBox.ItemsSource = groups;
```

`ItemsSource` はListBoxに表示するデータの元となるコレクションを指定するプロパティ。  
`groups`（`List<GroupConfig>`）をそのままセットするだけで、リストの項目が自動的に生成される。

`InputDialog` のように1件ずつ `Items.Add(...)` で追加する必要がなく、リスト全体を一括で渡せる。

### `DisplayMemberPath` で表示するプロパティを指定

```csharp
GroupListBox.DisplayMemberPath = "Name";
```

`ItemsSource` に `GroupConfig` オブジェクトのリストを渡すと、そのままでは `TaskbarLauncher.Models.GroupConfig` のような型名が表示されてしまう。  
`DisplayMemberPath = "Name"` を指定することで、**各 `GroupConfig` の `Name` プロパティの値だけを表示**するようになる。

```
// DisplayMemberPath なし → "TaskbarLauncher.Models.GroupConfig" が並ぶ
// DisplayMemberPath = "Name" → "開発ツール", "ブラウザ" ... が並ぶ
```

---

## Move_Click()

```csharp
private void Move_Click(object sender, RoutedEventArgs e)
{
    if (GroupListBox.SelectedItem is GroupConfig selected)
    {
        SelectedGroup = selected;
        DialogResult = true;
    }
    else
    {
        MessageBox.Show(
            "移動先のグループを選択してください。",
            "未選択",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }
}
```

移動ボタンを押したときの処理。

### パターンマッチング

```csharp
if (GroupListBox.SelectedItem is GroupConfig selected)
```

`is` を使ったパターンマッチングと呼ばれる書き方。  
`SelectedItem`（選択された項目）が `GroupConfig` 型であれば `true` になり、同時に `selected` という変数に代入される。

何も選ばれていない場合 `SelectedItem` は `null` になるため、`is GroupConfig` の条件が `false` になる。  
`null` チェックと型変換を1行でまとめて書ける便利な構文。

### 選択あり：移動先を保存して閉じる

```csharp
SelectedGroup = selected;
DialogResult = true;
```

選択された `GroupConfig` を `SelectedGroup` に保存し、`DialogResult = true` でダイアログを閉じる。  
呼び出し元では以下のように結果を受け取る。

```csharp
var dialog = new MoveAppDialog(groups);
if (dialog.ShowDialog() == true)
{
    var targetGroup = dialog.SelectedGroup; // 移動先グループ
}
```

### 選択なし：警告メッセージを表示

```csharp
MessageBox.Show(
    "移動先のグループを選択してください。",
    "未選択",
    MessageBoxButton.OK,
    MessageBoxImage.Warning);
```

何も選ばずに移動ボタンを押した場合は警告ダイアログを出してダイアログを閉じない。  
`InputDialog` が入力欄を赤くする方法を取ったのに対し、こちらは MessageBox で伝える方法を取っている。

| 引数 | 値 | 意味 |
|---|---|---|
| 第1引数 | `"移動先のグループを選択してください。"` | メッセージ本文 |
| 第2引数 | `"未選択"` | ダイアログのタイトル |
| 第3引数 | `MessageBoxButton.OK` | OKボタンのみ表示 |
| 第4引数 | `MessageBoxImage.Warning` | 警告アイコンを表示 |

---

## Cancel_Click()

```csharp
private void Cancel_Click(object sender, RoutedEventArgs e)
{
    DialogResult = false;
}
```

`InputDialog` と同じ。`DialogResult = false` でダイアログを閉じ、`SelectedGroup` は `null` のまま。  
呼び出し元の `ShowDialog() == true` が成立しないため、移動処理は行われない。