# InputDialog.xaml.cs

## このファイルの役割

**`InputDialog.xaml` の見た目に対応するロジックを書いたファイル（コードビハインド）。**

WPF では UI の見た目を `.xaml`、処理を `.xaml.cs` に分けて書く。  
この `.xaml.cs` を「コードビハインド」と呼ぶ。

このファイルでは以下を担当する。
- ダイアログを開くときにタイトル文をセットする
- OK ボタン押下時の入力バリデーション
- 入力値を呼び出し元に返す

---

## `partial class` とは？

```csharp
public partial class InputDialog : Window
```

`partial` は「このクラスの定義は複数のファイルに分かれている」という宣言。

`InputDialog.xaml` をビルドすると、XAMLの内容をC#に変換したコードが自動生成される。  
その自動生成コードと、このファイルのコードが合わさって**1つの `InputDialog` クラス**になる。

`partial` を付けることで、手書きのコードと自動生成コードを別ファイルに分けながら1つのクラスとして扱える。

---

## `Answer` プロパティ

```csharp
public string Answer { get; private set; } = "";
```

ユーザーが入力してOKを押したテキストを保持するプロパティ。

**`private set` とは？**  
`get`（読み取り）は外から自由にできるが、`set`（書き込み）はこのクラス内からしかできない設定。  
呼び出し元は `dialog.Answer` で入力値を読めるが、勝手に書き換えることはできない。

**なぜこの設計なのか？**  
ダイアログの入力値はOKボタンを押したときだけセットされるべきもの。  
外から書き換えられると「ユーザーが入力した値」という信頼性が崩れるため、`private set` で守っている。

---

## コンストラクタ

```csharp
public InputDialog(string title)
{
    InitializeComponent();
    TitleText.Text = title;
}
```

ダイアログを生成するときに呼ばれる初期化処理。

**`InitializeComponent()` とは？**  
`InputDialog.xaml` に書いたUI要素（TextBlock・TextBox・Buttonなど）を実際に生成して画面に表示する処理。  
これを呼ばないとXAMLで定義した要素が存在しない状態になり、`TitleText.Text` などへのアクセスがエラーになる。  
必ず最初に呼ぶ必要がある。

**`TitleText.Text = title` とは？**  
`InputDialog.xaml` で `x:Name="TitleText"` と名前をつけた TextBlock に、引数で受け取ったタイトル文をセットしている。  
こうすることで同じダイアログを「グループ名を入力」「アプリ名を入力」など**用途に応じて使い回せる**。

---

## OK_Click()

```csharp
private void OK_Click(object sender, RoutedEventArgs e)
{
    if (string.IsNullOrWhiteSpace(InputBox.Text))
    {
        InputBox.BorderBrush = System.Windows.Media.Brushes.Red;
        return;
    }
    Answer = InputBox.Text.Trim();
    DialogResult = true;
}
```

OKボタンを押したときの処理。

### ① 空白チェック

```csharp
if (string.IsNullOrWhiteSpace(InputBox.Text))
{
    InputBox.BorderBrush = System.Windows.Media.Brushes.Red;
    return;
}
```

`IsNullOrWhiteSpace` は `null`・空文字・スペースのみの文字列をまとめてチェックする。  
スペースだけ入力してOKを押しても弾けるため、`IsNullOrEmpty` より適切。

バリデーション失敗時は入力欄の枠を赤くしてユーザーに知らせ、`return` でダイアログを閉じずに留まる。

### ② 入力値の取得と保存

```csharp
Answer = InputBox.Text.Trim();
```

`Trim()` で文字列の前後のスペースを除去してから `Answer` に保存する。  
「　グループA　」のように前後に空白が入っていても、綺麗な「グループA」として保存できる。

### ③ ダイアログを閉じる

```csharp
DialogResult = true;
```

`DialogResult` はダイアログの結果を表すプロパティ。  
`true` をセットすると「OKで閉じた」としてダイアログが閉じる。

呼び出し元では以下のように結果を受け取る。

```csharp
var dialog = new InputDialog("グループ名を入力");
if (dialog.ShowDialog() == true)
{
    string name = dialog.Answer; // ユーザーが入力した値
}
```

`ShowDialog()` がダイアログを開き、`DialogResult = true` になるまで呼び出し元の処理を止める。

---

## Cancel_Click()

```csharp
private void Cancel_Click(object sender, RoutedEventArgs e)
{
    DialogResult = false;
}
```

キャンセルボタンを押したときの処理。

`DialogResult = false` をセットするだけでダイアログが閉じる。  
`Answer` には何もセットしないため、呼び出し元で `ShowDialog() == true` の条件が成立せず、入力値は使われない。

**これだけでよい理由**  
キャンセルは「何もしない」が正解。  
`Answer` を空のままにしておくことで、呼び出し元が `if (dialog.ShowDialog() == true)` のチェックだけで安全に処理を分岐できる。